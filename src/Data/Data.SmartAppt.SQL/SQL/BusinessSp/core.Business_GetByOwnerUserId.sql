CREATE PROCEDURE core.Business_GetByOwnerUserId
    @OwnerUserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM core.Business
    WHERE OwnerUserId = @OwnerUserId;
END