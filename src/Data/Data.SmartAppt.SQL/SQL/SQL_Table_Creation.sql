CREATE SCHEMA core;

CREATE TABLE core.Business (
  BusinessId      INT IDENTITY(1,1) PRIMARY KEY,
  Name            NVARCHAR(200) NOT NULL,
  Email           NVARCHAR(320) NULL,
  Phone           NVARCHAR(50) NULL,
  TimeZoneIana    NVARCHAR(100) NOT NULL,       -- e.g., 'Asia/Yerevan'
  SettingsJson    NVARCHAR(MAX) NULL,           -- flexible per-business flags
  CreatedAtUtc    DATETIME2(3) NOT NULL CONSTRAINT DF_Business_Created DEFAULT SYSUTCDATETIME()
);

SettingsJson
{
  "AllowSms": true,
  "BookingWindowDays": 30,
  "CancelPolicyHours": 2,
  "SendEmailNotifications": false
}


CREATE TABLE core.Service (
  ServiceId     INT IDENTITY PRIMARY KEY,
  BusinessId    INT NOT NULL REFERENCES core.Business(BusinessId),
  Name          NVARCHAR(200) NOT NULL,
  DurationMin   INT NOT NULL CHECK (DurationMin BETWEEN 5 AND 480),
  Price         DECIMAL(10,2) NOT NULL CHECK (Price >= 0),
  IsActive      BIT NOT NULL CONSTRAINT DF_Service_Active DEFAULT 1,
  UNIQUE (BusinessId, Name) -- alternate key per business
);

CREATE TABLE core.Customer (
  CustomerId    INT IDENTITY PRIMARY KEY,
  BusinessId    INT NOT NULL REFERENCES core.Business(BusinessId),
  FullName      NVARCHAR(200) NOT NULL,
  Email         NVARCHAR(320) NULL,
  Phone         NVARCHAR(50) NULL,
  CreatedAtUtc  DATETIME2(3) NOT NULL CONSTRAINT DF_Customer_Created DEFAULT SYSUTCDATETIME(),
  UNIQUE (BusinessId, Email)  -- allow nulls to repeat; MSSQL treats NULL as distinct
);

-- BookingStatus: 'Pending','Confirmed','Cancelled'
CREATE TABLE core.Booking (
  BookingId       INT IDENTITY PRIMARY KEY,
  BusinessId      INT NOT NULL REFERENCES core.Business(BusinessId),
  ServiceId       INT NOT NULL REFERENCES core.Service(ServiceId),
  CustomerId      INT NOT NULL REFERENCES core.Customer(CustomerId),
  StartAtUtc      DATETIME2(3) NOT NULL,
  EndAtUtc        DATETIME2(3) NOT NULL,
  Status          VARCHAR(12)  NOT NULL CHECK (Status IN ('Pending','Confirmed','Cancelled')),
  Notes           NVARCHAR(500) NULL,
  RowVer          ROWVERSION,
  CreatedAtUtc    DATETIME2(3) NOT NULL CONSTRAINT DF_Booking_Created DEFAULT SYSUTCDATETIME(),
  CHECK (EndAtUtc > StartAtUtc)
);

-- Double-booking guard (Confirmed only) at the same business/service and start time
CREATE UNIQUE INDEX UX_Booking_NoOverlap_Exact
ON core.Booking(BusinessId, ServiceId, StartAtUtc)
WHERE Status = 'Confirmed';

CREATE UNIQUE INDEX IX_Business_OwnerUserId
ON core.Business(OwnerUserId);

CREATE UNIQUE INDEX IX_Customer_UserId
ON core.Customer(UserId);

CREATE TABLE core.OpeningHours (
  OpeningHoursId  INT IDENTITY PRIMARY KEY,
  BusinessId      INT NOT NULL REFERENCES core.Business(BusinessId),
  DayOfWeek       TINYINT NOT NULL CHECK (DayOfWeek BETWEEN 0 AND 6), -- 0=Sun
  OpenTime        TIME(0) NOT NULL,
  CloseTime       TIME(0) NOT NULL,
  CHECK (CloseTime > OpenTime),
  UNIQUE (BusinessId, DayOfWeek)
);

CREATE TABLE core.Holiday (
  HolidayId     INT IDENTITY PRIMARY KEY,
  BusinessId    INT NOT NULL REFERENCES core.Business(BusinessId),
  HolidayDate   DATE NOT NULL,
  Reason        NVARCHAR(200) NULL,
  UNIQUE (BusinessId, HolidayDate)
);
