CREATE PROCEDURE pUpdateArtist
(
    @artistId int,
    @artistName NVARCHAR(50)
)
AS
BEGIN
    UPDATE [Artist]
    SET [ArtistName] = @artistName
    WHERE ArtistId = @artistId
END