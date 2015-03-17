CREATE PROCEDURE pDeleteAlbum
(
    @albumId int
)
AS
BEGIN
    DELETE [Album]
    WHERE AlbumId = @albumId
END