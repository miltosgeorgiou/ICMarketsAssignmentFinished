# ICMarkets Assignment – Blockchain Symbols API

## Overview
This project is a .NET 8 Web API that fetches blockchain data from the BlockCypher public APIs (ETH, BTC, DASH, LTC), 
stores the raw JSON responses in a SQLite database, and exposes endpoints to retrieve historical data.

The application is built with clean architecture principles, SOLID design, asynchronous patterns, and includes Unit, Integration, and Functional tests.



---

## Technologies
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- FluentValidation
- Swagger (OpenAPI)
- xUnit (Unit, Integration, Functional tests)
- Moq (unit testing)
- In-memory SQLite for tests

---

## Architecture
The solution follows a layered architecture:

- **Controllers** – HTTP endpoints
- **Services** – business logic
- **Repositories** – data access
- **HttpClients** – external API access
- **Entities** – domain models
- **DTOs / Validators** – input validation
- **Tests**
  - Unit tests (services)
  - Integration tests (EF Core + SQLite)
  - Functional tests (full HTTP pipeline)

---

## External APIs Used
Data is fetched from BlockCypher public endpoints:

- https://api.blockcypher.com/v1/eth/main
- https://api.blockcypher.com/v1/dash/main
- https://api.blockcypher.com/v1/btc/main
- https://api.blockcypher.com/v1/btc/test3
- https://api.blockcypher.com/v1/ltc/main

---

## Profiles
- The application uses .NET runtime profiles defined in launchSettings.json to control ports and environments.
- Production
- Development

## Database
- SQLite is used as the database engine. To re-create the database file just delete the migrations folder and from the Package Manager Console type :
	
	Add-Migration InitialCreate
	Update-Database

- Raw JSON responses are stored as they come.
- Each record includes a `CreatedAt` timestamp
- History is returned ordered by `CreatedAt DESC`

---

## SWAGGER
Swagger UI will be available at (depends on your profile) Development is 7299:

https://localhost:7299/swagger

---

## API Endpoints

### Fetch all symbols
POST https://localhost:7299/api/symbols/fetch/all?showResults=true

### Fetch a single symbol
POST https://localhost:7299/api/symbols/fetch/{symbolName}

### Get history for a symbol
GET https://localhost:7299/api/symbols/history/{symbolName}?limit=50 ( You can ommit the limit)

### Health Check

GET https://localhost:7299/api/symbols/health

---

### CORS
- A basic permissive CORS policy is enabled:
- Allows any origin
- Allows any header
- Allows any method
