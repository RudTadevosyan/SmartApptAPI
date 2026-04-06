CREATE TRIGGER TR_Customer_UniqueEmail
ON core.Customer
INSTEAD OF INSERT
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN core.Customer c 
          ON i.Email = c.Email
    )
    BEGIN
        RAISERROR('Email already exists', 16, 1);
        RETURN;
    END

    INSERT INTO core.Customer (BusinessId, FullName, Email, Phone)
    SELECT BusinessId, FullName, Email, Phone
    FROM inserted;
END;