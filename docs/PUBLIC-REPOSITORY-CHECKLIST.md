# Public GitHub Repository Checklist

This checklist captures the final verification and sanitisation steps I would complete before publishing the solution to a public GitHub repository.

## Source control hygiene

- [ ] Remove generated `bin/` and `obj/` directories from source control.
- [ ] Add a standard Visual Studio/.NET `.gitignore`.
- [ ] Remove IDE/user-specific files such as `*.csproj.user` where appropriate.
- [ ] Confirm there are no credentials, tokens, certificates, or secrets in the repository or Git history.
- [ ] Keep the supplied `TestData.csv` in source control as reproducible demonstration data.
- [ ] Keep EF Core migrations in source control if migrations remain the selected schema-management mechanism.

## Configuration hygiene

- [ ] Replace absolute developer-machine CSV paths with portable example values or clear local setup instructions.
- [ ] Replace machine-specific SQL connection strings with safe examples.
- [ ] Ensure `ScoreDbContextFactory` does not contain a personal machine-specific connection string.
- [ ] Use .NET user secrets or environment variables for developer secrets.
- [ ] Document the production secret strategy rather than committing secrets.

## Build verification

From a clean clone:

- [ ] Run `dotnet restore` successfully.
- [ ] Run `dotnet build` successfully.
- [ ] Apply the database migration successfully against a clean local database.
- [ ] Run the console application successfully.
- [ ] Confirm the supplied dataset produces the documented top-scorer output.
- [ ] Run the API successfully.
- [ ] Confirm Swagger opens in Development.

## API verification

- [ ] Confirm unauthenticated requests return `401`.
- [ ] Confirm an authenticated non-admin caller can execute GET endpoints.
- [ ] Confirm an authenticated non-admin caller receives `403` for POST.
- [ ] Confirm an admin JWT can execute POST successfully.
- [ ] Confirm `/api/scores/search/{search}` returns matching person data.
- [ ] Confirm `/api/scores/highest` returns every tied top scorer in alphabetical order.

## Documentation verification

- [ ] Confirm every Markdown link resolves correctly from GitHub.
- [ ] Confirm Mermaid diagrams render in GitHub.
- [ ] Confirm endpoint names in documentation match the final controller routes.
- [ ] Confirm configuration examples contain no personal paths, machine names, or credentials.
- [ ] Confirm README instructions work from a clean clone.

## Recommended repository layout

```text
/
|-- README.md
|-- Score.slnx
|-- Score.ConsoleApp/
|-- Score.Api/
|-- docs/
|   |-- ARCHITECTURE.md
|   |-- CLOUD-HOSTING.md
|   |-- DESIGN-DECISIONS.md
|   |-- PUBLIC-REPOSITORY-CHECKLIST.md
|   |-- REQUIREMENTS-TRACEABILITY.md
|   |-- SECURITY.md
|   `-- SUBMISSION-EVIDENCE.md
`-- .gitignore
```

## Suggested GitHub repository description

> .NET 10 Top Scorers solution demonstrating custom plain-text CSV parsing, LINQ business logic, EF Core persistence, secured ASP.NET Core REST APIs, JWT/Swagger integration, and an Azure deployment design.

## Suggested release tag

```text
v1.0-submission
```

A release tag is optional, but it creates a stable snapshot of the exact version being presented.
