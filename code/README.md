# API Gateway

This is an ASP.NET Core 8 API Gateway implementation that includes authentication, authorization, rate limiting, and logging features.

## Features

- JWT-based Authentication
- Role-based Authorization
- Rate Limiting (100 requests per minute per user)
- Structured Logging with Serilog
- Swagger/OpenAPI Documentation

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or Visual Studio Code

## Getting Started

1. Clone the repository
2. Update the JWT configuration in `appsettings.json` with your own secret key
3. Run the application:
   ```bash
   dotnet run --project ApiGateway.API
   ```

## API Documentation

The API documentation is available through Swagger UI when running the application in development mode:
- https://localhost:7001/swagger

## Authentication

The API uses JWT (JSON Web Token) for authentication. To access protected endpoints:

1. Obtain a JWT token from your authentication service
2. Include the token in the Authorization header:
   ```
   Authorization: Bearer <your-token>
   ```

## Rate Limiting

The API implements rate limiting with the following rules:
- 100 requests per minute per user
- Rate limit headers are included in the response

## Logging

Logs are written to:
- Console
- Daily rolling file logs in the `logs` directory

## Configuration

Key configuration settings in `appsettings.json`:
- JWT settings (Key, Issuer, Audience)
- Logging levels
- Rate limiting parameters 