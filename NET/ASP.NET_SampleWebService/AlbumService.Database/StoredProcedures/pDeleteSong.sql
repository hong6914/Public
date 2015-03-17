CREATE PROCEDURE pDeleteSong
(
    @songId int
)
AS
BEGIN
    DELETE [Song]
    WHERE SongId = @songId
END