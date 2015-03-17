CREATE PROCEDURE [pGetArtists]
AS
BEGIN
    --Get all the artists in use
    SELECT	a.ArtistId,
            a.ArtistName
    FROM Artist a
END