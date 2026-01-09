# [potegni.me](https://potegni.me) backend

To learn more about the project, visit [GitHub organization](https://github.com/potegnime)

## Overview

- **Framework:** ASP.NET Core 8.0
- **Database:** PostgreSQL with Entity Framework Core
- **Authentication:** JWT
- **Password Security:** BCrypt for password hashing (ans salting)
- **Email Service:** SendGrid for email notifications and password resets

## Development

Prerequisites:

- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) - runtime and SDK
- Database client (eg: pgAdmin)

### Running the API

1. Set secrets<br>
    To run the backend, you need to set secrets inside the `.env` file. See `example.env` file regarding file contents. Contact code owners if you need access to app secrets.
2. Generate RS256 key pair<br>
    You will need `.env`, `public.pem` and `private.pem` in the same location where the app runs. This is usually `potegnime-api/PotegniMe/bin/Debug/net8.0`. Create `.env` file and folder `keys` with the keys mentioned above in this folder. You can generate key pair by running:<br><br>
    ```
    openssl genpkey -algorithm RSA -out private.pem -pkeyopt rsa_keygen_bits:2048
    openssl rsa -pubout -in private.pem -out public.pem
    ```
    If you want to run [potegnime-scraper](https://github.com/potegnime/potegnime-scraper) locally as well, make sure the `public.pem` key is the same for the API and the scraper.
3. Run the API:<br><br>
    ```
    cd PotegniMe/
    dotnet restore
    dotnet build
    dotnet run
    ```
    API runs on http://localhost:5194. Swagger is available at http://localhost:5194/swagger/index.html

## Development guidelines

- Use primary constructors when creating new classes
- Check for unused using statements before committing
- Use [file scoped](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/file-scoped-namespaces) namespaces

## Deployment

For deployment info consult internal [potegnime-wiki](https://github.com/potegnime/potegnime-wiki)

If you want to try out the production build locally run:
```
# build the app
dotnet publish -c Release -o PotegniMe/publish

# create .env file in the same location where the Potegni.me dll lives
nano .env # copy contents from your .env

# create keys folder with public.pem and private.pem
mkdir keys
cd keys
nano public.pem
nano private.pem

dotnet PotegniMe/publish/PotegniMe.dll --urls "http://127.0.0.1:5194"
```
