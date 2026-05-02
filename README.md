# SmartAppt — Appointment Management System

**SmartAppt** high-performance, microservice-based scheduling system that allows businesses to manage their services, working schedules, and customer bookings. Customers can reserve time slots for services, while business owners manage availability, pricing, and operational constraints. 

## Overview

### Main API (Business Engine)
The Main API is a high-performance service focused strictly on scheduling and service management. It follows a Layered Architecture to maintain a clean flow of data and logic.

- **API Layer** — Defines the RESTful endpoints for business operations, service management, and booking lifecycles.
- **Business Layer** — core logic, service rules for business and customer
- **Data Layer** — Optimized for performance using ADO.NET for full control over data mapping and near-zero overhead using stored procedures.
- **Common Layer** — shared utilities, libraries

This structure ensures a predictable flow: API → Business → Data, with zero cross-contamination between layers.

### Auth Service (Identity Provider)
The Auth-Service is a dedicated microservice built using Clean Architecture principles to manage the security perimeter of the system. It is completely decoupled from the Main API to allow for independent scaling and security management.

*   **API Layer:** Acts as the entry point for authentication requests, handling login, registration, and token distribution.
*   **Application Layer:** Contains the use-case logic, including command handling for user registration and the generation of security tokens.
*   **Domain Layer:** Defines the core identity entities, constants, and enterprise-wide security specifications.
*   **Infrastructure Layer:** Implements the technical details, using Microsoft Identity and EF Core for persistence. It handles secure password hashing, database migrations, and the physical storage of user data.
*   **Security Architecture:** Implements a full JWT (JSON Web Token) lifecycle. This includes the issuance of access tokens for stateless authorization and Refresh Tokens to maintain persistent sessions across client applications.

### Technical Stack Summary
*   **Framework:** .NET 8 (ASP.NET Core)
*   **Persistence:** ADO.NET (Main API) and EF Core (Auth-Service)
*   **Database:** SQL Server (MSSQL)
*   **Identity:** Microsoft Identity Framework
*   **Authorization:** Stateless JWT-based communication
