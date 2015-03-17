

CREATE PROCEDURE pAddAlbum
(
    @albumTitle NVARCHAR(50),
    @artistId   INT
)
AS
BEGIN
    INSERT INTO [Album]
        (
            [Title],
            [ArtistId] 
        )
        VALUES
        (
            @albumTitle,
            @artistId
        )

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS LAST_IDENTITY
END

