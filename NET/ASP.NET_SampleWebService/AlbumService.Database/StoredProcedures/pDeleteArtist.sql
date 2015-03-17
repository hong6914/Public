CREATE PROCEDURE pDeleteArtist
(
    @artistId int
)
AS
BEGIN
    DELETE [Artist]
    WHERE ArtistId = @artistId
END