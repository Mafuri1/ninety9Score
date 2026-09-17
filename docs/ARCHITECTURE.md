# Architecture

## Design objective

The solution separates business rules from frameworks and transport concerns. `Score.Core` owns the score model, custom CSV parsing, top-scorer calculation and repository contract. EF Core/SQL Server is isolated in `Score.Infrastructure`, while the console app and REST API are independent delivery mechanisms.

```mermaid
flowchart TD
    subgraph Delivery
        Console[Score.ConsoleApp]
        API[Score.Api]
    end

    subgraph Core
        Parser[ScoreCsvParser]
        Model[ScoreRecord / TopScorersResult]
        Contract[IScoreRepository]
    end

    subgraph Infrastructure
        Repository[EfScoreRepository]
        Context[ScoreDbContext]
        SQL[(SQL Server)]
    end

    Console --> Parser
    Console --> Model
    Console --> Contract
    API --> Contract
    Contract --> Repository
    Repository --> Context
    Context --> SQL
```

## Console flow

```mermaid
sequenceDiagram
    participant C as Console App
    participant F as Plain-text file
    participant P as ScoreCsvParser
    participant R as IScoreRepository
    participant D as SQL Server

    C->>F: ReadAllTextAsync
    F-->>C: CSV string
    C->>P: Parse(csv)
    P-->>C: ScoreRecord[]
    C->>C: Calculate top scorers
    C->>C: Write result to STDOUT
    alt InsertIntoDatabase = true
        C->>R: AddRangeAsync(records)
        R->>D: INSERT records
    end
```

## API flow

```mermaid
sequenceDiagram
    participant Client
    participant Auth as JWT/Auth policy
    participant API as ScoresController
    participant Repo as IScoreRepository
    participant DB as SQL Server

    Client->>Auth: HTTP + Bearer token
    Auth->>API: Authorized request
    API->>Repo: Domain-oriented repository call
    Repo->>DB: EF Core query/command
    DB-->>Repo: Data
    Repo-->>API: ScoreRecord(s)
    API-->>Client: HTTP response DTO
```
