CREATE PROCEDURE pUpdateAlbum
(
    @albumId    INT,
    @artistId   INT,
    @albumTitle NVARCHAR(50)
)
AS
BEGIN
    UPDATE [Album]
    SET [Title] = @albumTitle,
        [ArtistId] = @artistId
    WHERE AlbumId = @albumId
END