

CREATE PROCEDURE pUpdateSong
(
    @songId         INT,
    @title          NVARCHAR(50),
    @length         NVARCHAR(10),
    @trackNumber    INT,
    @genre          NVARCHAR(50),
    @albumId        INT
)
AS
BEGIN
    UPDATE [Song]
    SET
        [Title] = @title,
        [Length] = @length,
        [TrackNumber] = @trackNumber,
        [Genre] = @genre,
        [DateModified] = SYSDATETIME(),
        [AlbumId] = @albumId

    WHERE SongId = @songId
END

