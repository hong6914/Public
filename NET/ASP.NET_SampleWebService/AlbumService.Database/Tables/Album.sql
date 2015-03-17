CREATE TABLE [Album] (
    [AlbumId]       INT             IDENTITY (1, 1) NOT NULL,
    [Title]         NVARCHAR(50)    NOT NULL,
    [ArtistId]      INT             NOT NULL,
    CONSTRAINT [PK_AlbumId] PRIMARY KEY CLUSTERED ([AlbumId] ASC),
    CONSTRAINT [FK_ArtistId] FOREIGN KEY ([ArtistId]) REFERENCES [Artist] ([ArtistId])
);

