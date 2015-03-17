

CREATE PROCEDURE pAddArtist
(
    @artistName NVARCHAR(50)
)
AS
BEGIN
    INSERT INTO [Artist]
        (
            [ArtistName]
        )
        VALUES
        (
            @artistName
        )

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS LAST_IDENTITY
END

