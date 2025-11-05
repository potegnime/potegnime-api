# [potegni.me](https://potegni.me) backend

To learn more about the project, visit [GitHub organization](https://github.com/potegnime)

## Overview

- **Framework**: ASP.NET Core 8.0
- **Database:** PostgreSQL with Entity Framework Core
- **Authentication:** JWT
- **Password Security:** BCrypt for password hashing (ans salting)
- **Email Service:** SendGrid for email notifications and password resets

## Development

Prerequisites:

- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) - runtime and SDK
- Database client (eg: pgAdmin)

To run the backend, you need to set secrets inside the `.env` file. See `example.env` file regarding file contents. Contact code owners if you need access to app secrets.

Running the app:

```
cd PotegniMe/
dotnet restore
dotnet build
dotnet run
```

API runs on http://localhost:5194. Swagger is available at http://localhost:5194/swagger/index.html

## Development guidelines

- TODO

## Deployment

Deployed on [Render](https://render.com/). Currently on a free tier (0.1 CPU, 512MB RAM) - this is the main reason app is so slow at the moment. Plan is to migrate to AWS/solid on-prem server soon.

## Folder structure

```
TODO
```
