CREATE TABLE [Song] (
    [SongId]        INT             IDENTITY (1, 1) NOT NULL,
    [Title]         NVARCHAR(50)    NOT NULL,
    [Length]        NVARCHAR(10)    NOT NULL,                               -- in minutes:seconds
    [TrackNumber]   INT             NOT NULL,
    [Genre]         NVARCHAR(50)    NULL,
    [DateAdded]     DATETIME        NULL,
    [DateModified]  DATETIME        NULL,
    [AlbumId]       INT             NOT NULL,
    CONSTRAINT [PK_SongId] PRIMARY KEY CLUSTERED ([SongId] ASC),
    CONSTRAINT [FK_AlbumId] FOREIGN KEY ([AlbumId]) REFERENCES [Album] ([AlbumId])
);

