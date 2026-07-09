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
20. Trade-offs
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

# Architecture

The project follows **Clean Architecture**.

```
Presentation (API)

↓

Application

↓

Domain

↓

Infrastructure
```

## Why Clean Architecture?

Clean Architecture was chosen to separate business logic from infrastructure concerns.

Each layer has a single responsibility:

### API

Responsible for:

- HTTP endpoints
- Dependency Injection
- Middleware
- Swagger configuration

### Application

Contains all business rules.

Responsibilities:

- Import sensor readings
- Aggregate sensor data
- Validation
- Interfaces
- DTOs

The Application layer does not depend on EF Core or SQL Server.

### Domain

Contains the core business model.

Responsibilities:

- Entities
- Value Objects

The Domain layer has no dependency on any external library.

### Infrastructure

Responsible for implementation details.

Examples:

- EF Core
- SQL Server
- Repository
- File Reader

This layer depends on the Application layer but not vice versa.

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

The project includes unit tests for the application's core business logic.

The tests are focused on validating the behavior of the service layer rather than framework-specific components such as controllers, middleware, or Entity Framework.

The following scenarios are covered:



## ReadingImporter

### 1. ImportAsync_Should_Store_Valid_Reading

**Purpose**

Verifies that a valid sensor reading is successfully imported and stored.

**Scenario**

- A valid JSON line is provided.
- The reading passes all business validation rules.
- The reading does not already exist in the database.

**Expected Result**

- The reading is added to the repository.
- SaveChangesAsync is called exactly once.
- ImportReport reports:
  - StoredReadings = 1
  - DuplicateReadings = 0
  - InvalidReadings = 0

---

### 2. ImportAsync_Should_Skip_Duplicate_Reading

**Purpose**

Verifies that duplicate readings are detected and skipped.

**Scenario**

- The imported reading already exists in the database.
- The duplicate is identified using:
  - DeviceId
  - Metric
  - Timestamp
  - Sequence

**Expected Result**

- The reading is not stored.
- AddRangeAsync is never called.
- SaveChangesAsync is never called.
- ImportReport reports:
  - StoredReadings = 0
  - DuplicateReadings = 1
  - InvalidReadings = 0

---

### 3. ImportAsync_Should_Count_Invalid_Json

**Purpose**

Verifies that malformed JSON records do not interrupt the import process.

**Scenario**

- The input contains an invalid JSON document.
- JSON deserialization throws a JsonException.

**Expected Result**

- The invalid record is skipped.
- The import process continues.
- No data is stored.
- ImportReport reports:
  - StoredReadings = 0
  - DuplicateReadings = 0
  - InvalidReadings = 1

---

## AggregationService

### 4. AggregateAsync_Should_Return_Correct_Aggregation

**Purpose**

Verifies that sensor readings are correctly aggregated into time buckets.

**Scenario**

A bucket contains multiple readings.

The service calculates:

- Count
- Average
- Minimum
- Maximum

**Expected Result**

The returned aggregation contains the correct statistical values for the bucket.

---

### 5. AggregateAsync_Should_Return_Empty_Bucket_When_No_Readings_Exist

**Purpose**

Verifies that empty time buckets are still returned even when no readings exist.

**Scenario**

- No readings are returned from the repository.
- The requested time range still contains one or more buckets.

**Expected Result**

Each bucket is returned with:

- Count = 0
- Average = null
- Minimum = null
- Maximum = null

This ensures that consumers always receive a continuous timeline, even when there is no sensor data available.

---

# Test Strategy

The testing strategy focuses on validating business behavior rather than implementation details.

The following principles were followed:

- External dependencies are mocked using **Moq**.
- Assertions are written using **FluentAssertions**.
- Each test follows the **Arrange – Act – Assert (AAA)** pattern.
- Repository interactions are verified using `Verify()`.
- Only the service layer is unit tested, since it contains the application's business logic.
- Controllers, middleware, and Entity Framework Core are intentionally excluded from unit testing because they primarily delegate work or rely on framework functionality.

This approach keeps the tests isolated, deterministic, fast, and easy to maintain.

------------------------------------------------------------------------

# Trade-offs

For this coding exercise:

-   Simplicity over premature optimization
-   In-memory aggregation after loading filtered records
-   HashSet chosen to reduce duplicate lookup cost

------------------------------------------------------------------------

# Author

**MohammadHossein Nazari**
