CREATE PROCEDURE [core].[Customer_GetByUserId]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM core.Customer
    WHERE UserId = @UserId;
END