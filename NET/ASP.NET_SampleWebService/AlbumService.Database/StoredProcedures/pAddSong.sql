

CREATE PROCEDURE pAddSong
(
    @title          NVARCHAR(50),
    @length         NVARCHAR(10),
    @trackNumber    INT,
    @genre          NVARCHAR(50),
    @albumId        INT
)
AS
BEGIN
    INSERT INTO [Song]
        (
            [Title],
            [Length],
            [TrackNumber],
            [Genre],
            [DateAdded],
            [DateModified],
            [AlbumId]
        )
        VALUES
        (
            @title,
            @length,
            @trackNumber,
            @genre,
            SYSDATETIME(),
            SYSDATETIME(),
            @albumId
        )

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS LAST_IDENTITY
END

