# Sensor Reading Service

> A .NET 8 Web API for importing sensor readings from JSONL files,
> storing them in SQL Server, removing duplicates, and exposing
> time-based aggregation endpoints.

------------------------------------------------------------------------

# Table of Contents

1.  Project Overview
2.  Features
3.  Architecture
4.  Project Structure
5.  Technologies
6.  Design Decisions
7.  Data Flow
8.  Database
9.  Configuration
10. Installation
11. Running the Application
12. API Endpoints
13. Validation Rules
14. Duplicate Detection
15. Aggregation Logic
16. Logging
17. Error Handling
18. Unit Tests
19. Performance Considerations
20. Trade-offs
21. Future Improvements
23. Author

------------------------------------------------------------------------

# Project Overview

Sensor Reading Service is a backend service developed with **ASP.NET
Core 8** following **Clean Architecture** principles.

The service imports sensor readings from a JSONL file, validates each
record, removes duplicates, stores valid data in SQL Server, and exposes
an API for aggregating sensor values into configurable time buckets.

The implementation focuses on:

-   Clean Architecture
-   Separation of Concerns
-   Testability
-   Maintainability
-   Readability
-   Production-ready coding practices

------------------------------------------------------------------------

# Features

-   Import readings from JSONL files
-   Validation of incoming records
-   Skip invalid JSON records
-   Duplicate detection
-   SQL Server persistence using EF Core
-   Time-based aggregation
-   Configurable bucket size
-   Structured logging
-   Global exception handling
-   Swagger/OpenAPI
-   Unit tests

------------------------------------------------------------------------

# Architecture

                    +----------------------+
                    |      API Layer       |
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    |   Application Layer  |
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    |    Domain Layer      |
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    | Infrastructure Layer |
                    +----------------------+

### Responsibilities

**API** - Controllers - Dependency Injection - Middleware - Swagger

**Application** - Business Logic - Import Service - Aggregation
Service - DTOs - Interfaces

**Domain** - Entities - Value Objects

**Infrastructure** - EF Core - SQL Server - Repository - File Reader

------------------------------------------------------------------------

# Project Structure

``` text
 SensorReadingService/
 ├── SensorReadingService.API
 ├── SensorReadingService.Application
 ├── SensorReadingService.Domain
 └── SensorReadingService.Infrastructure

tests/
 └── SensorReadingService.Tests
```

------------------------------------------------------------------------

# Technologies

-   .NET 8
-   ASP.NET Core Web API
-   Entity Framework Core
-   SQL Server
-   xUnit
-   Moq
-   FluentAssertions
-   Swagger
-   Microsoft.Extensions.Logging

------------------------------------------------------------------------

# Design Decisions

## Why Clean Architecture?

-   Better separation of concerns
-   Easier testing
-   Lower coupling
-   Easier maintenance

## Why Repository Pattern?

-   Decouples business logic from EF Core
-   Simplifies unit testing
-   Easier replacement of persistence implementation

## Why SQL Server?

-   Excellent EF Core integration
-   Reliable relational database
-   Strong transaction support

## Why CancellationToken?

Repository methods support cancellation for long-running operations and
future scalability.

------------------------------------------------------------------------

# Data Flow

## Import Flow

    JSONL File
         |
         v
    FileReader
         |
         v
    ReadingImporter
         |
         +--> JSON Validation
         |
         +--> Business Validation
         |
         +--> Duplicate Detection
         |
         v
    Repository
         |
         v
    SQL Server

## Aggregation Flow

    HTTP Request
          |
          v
    AggregationService
          |
          v
    Repository
          |
          v
    Readings
          |
          v
    Bucketing
          |
          v
    Statistics
          |
          v
    HTTP Response

------------------------------------------------------------------------

# Database

Entity:

Reading

-   DeviceId
-   Metric
-   Timestamp
-   Value
-   Sequence

Duplicate key consists of:

-   DeviceId
-   Metric
-   Timestamp
-   Sequence

------------------------------------------------------------------------

# Configuration

Example:

``` json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SensorReadingDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "ImportSettings": {
    "FilePath": "Data/readings.jsonl"
  }
}
```

------------------------------------------------------------------------

# Installation

``` bash
dotnet restore
dotnet build
dotnet ef database update
dotnet run --project src/SensorReadingService.API
```

Swagger:

`https://localhost:xxxx/swagger`

------------------------------------------------------------------------

# API Endpoints

## Import

POST `/api/import`

Returns:

-   TotalLines
-   StoredReadings
-   DuplicateReadings
-   InvalidReadings

------------------------------------------------------------------------

## Aggregation

GET `/api/readings/aggregate`

Parameters:

-   deviceId
-   metric
-   from
-   to
-   bucketSizeInMinutes

------------------------------------------------------------------------

# Validation Rules

A reading is valid when:

-   DeviceId is not empty
-   Metric is not empty
-   Sequence \>= 0
-   JSON is valid

Invalid records are skipped.

------------------------------------------------------------------------

# Duplicate Detection

Existing records are loaded into a HashSet`<ReadingIdentity>`{=html}
before importing.

Advantages:

-   Single database query
-   O(1) duplicate lookup
-   Avoid repeated database round trips

Identity fields:

-   DeviceId
-   Metric
-   Timestamp
-   Sequence

------------------------------------------------------------------------

# Aggregation Logic

Data is grouped into configurable time buckets.

Each bucket returns:

-   BucketStart
-   BucketEnd
-   Count
-   Average
-   Min
-   Max

Empty buckets are returned with:

-   Count = 0
-   Average = null
-   Min = null
-   Max = null

------------------------------------------------------------------------

# Logging

The application logs:

-   Import started
-   Invalid JSON
-   Invalid records
-   Duplicate records
-   Import completed
-   Unexpected exceptions

------------------------------------------------------------------------

# Error Handling

Global exception middleware returns consistent HTTP responses.

-   400 → Validation errors
-   404 → Resource not found (if applicable)
-   500 → Unexpected server errors

------------------------------------------------------------------------

# Unit Tests

Implemented unit tests:

  Service              Scenario
  -------------------- -------------------------
  ReadingImporter      Valid reading import
  ReadingImporter      Duplicate detection
  ReadingImporter      Invalid JSON
  AggregationService   Aggregation calculation
  AggregationService   Empty bucket

Run tests:

``` bash
dotnet test
```

------------------------------------------------------------------------

# Performance Considerations

Current implementation prioritizes readability.

Possible future optimizations:

-   SQL-side aggregation
-   Bulk insert
-   Streaming import
-   Background processing
-   Parallel processing
-   Caching

------------------------------------------------------------------------

# Trade-offs

For this coding exercise:

-   Simplicity over premature optimization
-   In-memory aggregation after loading filtered records
-   HashSet chosen to reduce duplicate lookup cost

------------------------------------------------------------------------

# Future Improvements

-   Integration Tests
-   File Upload API


------------------------------------------------------------------------

# Author

**MohammadHossein Nazari**