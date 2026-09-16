# Solution Architecture

## Purpose

This document describes the architecture I implemented for the Top Scorers solution and explains how the main responsibilities are separated across file ingestion, business logic, persistence, and API access.

The solution contains two executable .NET 10 applications:

1. `Score.ConsoleApp` - responsible for file ingestion, score evaluation, STDOUT output, and optional database publication.
2. `Score.Api` - responsible for secured HTTP access to the stored score data.

Both applications use the same logical SQL Server datastore.

---

## Component architecture

```mermaid
flowchart TB
    subgraph Input
        CSV[TestData.csv / configurable plain-text CSV]
        ConsoleConfig[Console appsettings.json]
    end

    subgraph ConsoleApp[Score.ConsoleApp]
        Reader[File.ReadAllLines]
        Parser[String-based CSV parser]
        Scores[In-memory score collection]
        Rank[Maximum score + tie filtering + alphabetical sort]
        Publisher[SQLPublisher]
    end

    subgraph ApiApp[Score.Api]
        Swagger[Swagger UI]
        Auth[JWT authentication and authorization]
        Controller[ScoresController]
        Dtos[Request / response DTOs]
        ApiDb[ScoreDbContext]
    end

    DB[(SQL Server - Scores)]
    Stdout[STDOUT]

    CSV --> Reader
    ConsoleConfig --> Reader
    Reader --> Parser --> Scores --> Rank --> Stdout
    Scores -->|InsertIntoDatabase = true| Publisher --> DB

    Swagger --> Auth --> Controller
    Controller --> Dtos
    Controller --> ApiDb --> DB
```

---

## Console application flow

The console application executes the core scoring requirement independently from the API.

```mermaid
flowchart TD
    Start([Start]) --> Config[Load appsettings.json]
    Config --> Path[Read CSV path, DB connection and insert toggle]
    Path --> Exists{CSV file exists?}

    Exists -- No --> Missing[Write file-not-found message]
    Missing --> End([End])

    Exists -- Yes --> Read[Read plain-text file]
    Read --> Skip[Skip header and empty rows]
    Skip --> Parse[Map each row to a score object]
    Parse --> List[Build in-memory list]

    List --> Any{Valid score data?}
    Any -- No --> NoData[Write no-data message]
    NoData --> End

    Any -- Yes --> Max[Find maximum score]
    Max --> Filter[Keep every record at maximum]
    Filter --> Sort[Order by FirstName then SecondName]
    Sort --> Output[Write names and score to STDOUT]

    Output --> Insert{InsertIntoDatabase enabled?}
    Insert -- No --> End
    Insert -- Yes --> Persist[EF Core AddRangeAsync + SaveChangesAsync]
    Persist --> End
```

### Why this design

I kept the scoring logic in the console process because the file-processing program is a distinct requirement. Database persistence is an optional secondary action rather than a prerequisite for calculating the result. This allows the core behaviour to be exercised even when SQL Server is not available.

---

## API architecture

The API exposes stored scores through authenticated REST endpoints.

```mermaid
flowchart LR
    Caller[Client / Swagger UI] -->|HTTPS + JWT| Middleware[ASP.NET Core middleware]
    Middleware --> AuthN[JWT authentication]
    AuthN --> AuthZ[Authorization policy]
    AuthZ --> Controller[ScoresController]
    Controller --> Mapping[DTO projection / mapping]
    Mapping --> EF[Entity Framework Core]
    EF --> DB[(SQL Server)]
```

The API boundary is separated from the persistence model through request and response DTOs. This prevents database-specific fields from becoming the write contract and provides a clear place for validation.

---

## API endpoint flow

### Search by person name

```mermaid
sequenceDiagram
    participant C as Client
    participant A as ASP.NET Core Auth
    participant S as ScoresController
    participant E as EF Core
    participant D as SQL Server

    C->>A: GET /api/scores/search/{search} + Bearer JWT
    A->>A: Validate token and authorization
    A->>S: Authorized request
    S->>E: Query FirstName or SecondName contains search
    E->>D: SELECT matching scores
    D-->>E: Matching rows
    E-->>S: Project to ScoreResponse
    S-->>C: 200 OK + matching score data
```

The string-based search is intentionally aligned to the concept of locating a person by name rather than requiring the caller to know an internal database identifier.

### Highest-score lookup

```mermaid
sequenceDiagram
    participant C as Client
    participant S as ScoresController
    participant E as EF Core
    participant D as SQL Server

    C->>S: GET /api/scores/highest
    S->>E: Determine maximum Score
    E->>D: Query score data
    D-->>E: Maximum value
    S->>E: Select every row matching maximum
    E->>D: Query tied records ordered by name
    D-->>E: Top scorers
    E-->>S: Project responses
    S-->>C: 200 OK + top scorer collection
```

### Score creation

```mermaid
sequenceDiagram
    participant C as Client
    participant A as Authorization
    participant S as ScoresController
    participant E as EF Core
    participant D as SQL Server

    C->>A: POST /api/scores + admin JWT
    A->>A: Enforce WriteScores policy
    A->>S: Authorized request
    S->>S: Validate and map CreateScoreRequest
    S->>E: Add score entity
    E->>D: INSERT record
    D-->>E: Persisted record
    E-->>S: SaveChangesAsync complete
    S-->>C: 201 Created + response DTO
```

---

## Data model

```mermaid
erDiagram
    SCORES {
        int Id PK
        string FirstName
        string SecondName
        int Score
    }
```

The three business data fields originate from the CSV input. `Id` is a surrogate database key introduced to provide stable relational/EF identity without treating a person's name as guaranteed unique.

---

## Responsibility boundaries

### `Score.ConsoleApp`

I assigned the following responsibilities to the console application:

- Load runtime configuration.
- Resolve the input file location.
- Read the plain-text input.
- Parse CSV row values without a CSV package.
- Build the in-memory score collection.
- Determine the highest score.
- Preserve every tie.
- Alphabetically order tied top scorers.
- Write the required result to STDOUT.
- Optionally publish the imported data to SQL Server.

### `Score.Api`

I assigned the following responsibilities to the API:

- Authenticate callers using JWT Bearer authentication.
- Apply authorization rules before controller execution.
- Validate POST request payloads.
- Expose score retrieval and creation operations.
- Search score data by first or second name.
- Retrieve all highest-scoring records.
- Separate HTTP DTOs from EF Core entities.
- Expose secured Swagger UI for development-time exploration.

### SQL Server

SQL Server is the shared persistence layer between the file-ingestion path and API path. Entity Framework Core provides the database abstraction and migration support.

---

## Architectural qualities demonstrated

### Maintainability

Each project has a focused responsibility, and configuration values are externalised from business logic.

### Evolvability

The file importer can later move to a scheduled service, Azure Function, Container Apps Job, or worker service without changing the API contract.

### Security

Authentication is required by default, with more restrictive authorization applied to data mutation.

### Testability

The business rules are explicit and small enough to isolate into unit-tested services as the solution evolves.

### Operational simplicity

The solution does not introduce unnecessary infrastructure. It uses managed framework capabilities and a conventional relational database that can move directly from local SQL Server to Azure SQL.
