# Users API

[![.NET](https://github.com/iskorotkov/users-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/iskorotkov/users-api/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/iskorotkov/users-api/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/iskorotkov/users-api/actions/workflows/codeql-analysis.yml)

The service provides functionality to create/read/update/delete users via Web API. All data is stored in SQL Server 2019.

## Run

To run both SQL Server and Users API run the following:

```shell
echo 'SA_PASSWORD=SomePassword123' > .env
docker-compose up
```

Then open your browser at `http://localhost:5000/swagger/index.html` for Swagger UI or use Postman/curl to access API.

## Test

Execute the following to run tests using the SQLite In-Memory database:

```shell
dotnet test --filter Sqlite
```

To test using SQL Server use the following. Note that these tests will take much longer and may fail when run concurrently.

```shell
# Show list of tests.
dotnet test -t
# Execute tests from class SqlServerUsersControllerTests.
dotnet test --filter SqlServerUsersControllerTests
```
