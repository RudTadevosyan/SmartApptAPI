--------------------------------------------------
-- Create Roles
--------------------------------------------------

CREATE ROLE CustomerRole;
CREATE ROLE BusinessRole;

--------------------------------------------------
-- Customer Restricted Permissions
--------------------------------------------------

-- Booking
GRANT EXECUTE ON OBJECT::core.Booking_SafeCreate TO CustomerRole;
GRANT EXECUTE ON OBJECT::core.Booking_GetById TO CustomerRole;
GRANT EXECUTE ON OBJECT::core.Booking_GetBookingsByRange TO CustomerRole;
GRANT EXECUTE ON OBJECT::core.Booking_GetBookingsCountByBusinessAndRange TO CustomerRole;
GRANT EXECUTE ON OBJECT::core.Booking_Cancel TO CustomerRole;

-- Business (read-only)
GRANT EXECUTE ON OBJECT::core.Business_GetAll TO CustomerRole;
GRANT EXECUTE ON OBJECT::core.Business_GetById TO CustomerRole;

-- Services
GRANT EXECUTE ON OBJECT::core.Service_GetAll TO CustomerRole;
GRANT EXECUTE ON OBJECT::core.Service_GetByBusinessId TO CustomerRole;
GRANT EXECUTE ON OBJECT::core.Service_GetById TO CustomerRole;

-- Opening hours
GRANT EXECUTE ON OBJECT::core.OpeningHours_GetByBusinessId TO CustomerRole;
GRANT EXECUTE ON OBJECT::core.OpeningHours_GetByBusinessIdAndDow TO CustomerRole;

-- Holidays
GRANT EXECUTE ON OBJECT::core.Holiday_GetByBusinessId TO CustomerRole;
GRANT EXECUTE ON OBJECT::core.Holiday_GetAllByMonth TO CustomerRole;

--------------------------------------------------
-- Business Permissions
--------------------------------------------------

-- Full access to all procedures in schema
GRANT EXECUTE ON SCHEMA::core TO BusinessRole;

--------------------------------------------------
-- Create login users
--------------------------------------------------

CREATE LOGIN customer_demo WITH PASSWORD = 'Customer123!$';
CREATE LOGIN business_demo WITH PASSWORD = 'Business123!$';

USE SmartAppt;

--------------------------------------------------
-- Create database users
--------------------------------------------------

CREATE USER customer_demo FOR LOGIN customer_demo;
CREATE USER business_demo FOR LOGIN business_demo;

--------------------------------------------------
-- Assigne users to role
--------------------------------------------------

ALTER ROLE CustomerRole ADD MEMBER customer_demo;
ALTER ROLE BusinessRole ADD MEMBER business_demo;