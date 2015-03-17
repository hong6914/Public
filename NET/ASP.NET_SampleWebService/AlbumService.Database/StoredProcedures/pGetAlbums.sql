CREATE PROCEDURE [pGetAlbums]
AS
BEGIN
    --Get all the albums in use
    SELECT	a.ArtistId,
            a.Title,
            a.AlbumId
    FROM Album a
END