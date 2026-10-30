# Auto Deren

A simple car rental website and API built with ASP.NET Core and SQL Server.

## Run

Start the web project:

```bash
dotnet run --project KooliProjekt.WebAPI
```

The site is served by the API. In development, the API documentation is available at `/swagger`.

## Rental flow

1. Register an account or sign in.
2. Choose an available car and start a rental.
3. Finish the rental and enter the kilometers driven.
4. Review the invoice. An administrator can mark it as paid.

Rental cost is based on the time rate and kilometers driven.

## Administrator

Set `Admin__Email` and `Admin__Password` environment variables before starting the API to create the administrator account. The password must be at least 12 characters.

## Database

The application applies Entity Framework migrations on startup. Configure `ConnectionStrings:DefaultConnection` in `KooliProjekt.WebAPI/appsettings.json` for your SQL Server instance.
