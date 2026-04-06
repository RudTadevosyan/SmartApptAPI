CREATE TRIGGER TR_Business_NoDelete
ON core.Business
INSTEAD OF DELETE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN core.Booking b ON b.BusinessId = d.BusinessId
    )
    BEGIN
        RAISERROR('Cannot delete business with existing bookings', 16, 1);
        RETURN;
    END

    DELETE b
    FROM core.Business b
    JOIN deleted d ON b.BusinessId = d.BusinessId;
END;