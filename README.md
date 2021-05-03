# Users API

[![.NET](https://github.com/iskorotkov/users-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/iskorotkov/users-api/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/iskorotkov/users-api/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/iskorotkov/users-api/actions/workflows/codeql-analysis.yml)

The service provides functionality to create/read/update/delete users via Web API. All data is stored in SQL Server 2019.

- [Users API](#users-api)
  - [Run](#run)
  - [Test](#test)
  - [Project structure](#project-structure)

## Run

To run both SQL Server and Users API run the following:

```shell
echo 'SA_PASSWORD=SomePassword123' > .env
docker-compose up
```

Then open your browser at `http://localhost:5000/swagger/index.html` for Swagger UI or use Postman/curl to access API.

## Test

Execute the following to run all tests using the SQLite In-Memory database:

```shell
dotnet test --filter Mapper
dotnet test --filter Sqlite
```

To test using SQL Server use the following. Note that these tests will take much longer and may fail when run concurrently.

```shell
# Show list of tests.
dotnet test -t
# Execute tests from class SqlServerUsersControllerTests.
dotnet test --filter SqlServerUsersControllerTests
```

## Project structure

- Admin - admin group and changing a user to be in the admin group.
- Auth - password hashing for newly created users.
- Db - DbContext and migrations.
- Models - entities and enums.
- Signup - timeout after user creation for other requests to create a user with the same login.
- WebApi - controllers and configuration.
