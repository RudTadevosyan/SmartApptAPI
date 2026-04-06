# SmartAppt — Appointment Management System

**SmartAppt** is a modular appointment-booking system that allows businesses to manage their services, working schedules, and customer bookings. Customers can reserve time slots for services, while business owners manage availability, pricing, and operational constraints. 
The project uses **C#, ASP.NET 8, and ADO.NET with SQL Server** (MSSQL), relying on stored procedures for all database operations.

The system follows a strict **layered architecture**, keeping every concern cleanly separated and maintainable:

## Overview

- **API Layer** — ASP.NET 8 Web API
- **Business Layer** — core logic, service rules for business and customer
- **Data Layer** — repositories, data access via ADO.NET, stored procedures
- **Common Layer** — shared utilities, libraries

This structure ensures a predictable flow: API → Business → Data, with zero cross-contamination between layers.
