CREATE TRIGGER TR_OpeningHours_Validate
ON core.OpeningHours
INSTEAD OF INSERT
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE OpenTime >= CloseTime
    )
    BEGIN
        RAISERROR('OpenTime must be before CloseTime', 16, 1);
        RETURN;
    END

    INSERT INTO core.OpeningHours (BusinessId, DayOfWeek, OpenTime, CloseTime)
    SELECT BusinessId, DayOfWeek, OpenTime, CloseTime
    FROM inserted;
END;