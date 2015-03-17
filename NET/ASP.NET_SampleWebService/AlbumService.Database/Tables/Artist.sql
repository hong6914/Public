CREATE TABLE [Artist] (
    [ArtistId]      INT             IDENTITY (1, 1) NOT NULL,
    [ArtistName]    NVARCHAR(50)    NOT NULL,
    CONSTRAINT [PK_ArtistId] PRIMARY KEY CLUSTERED ([ArtistId] ASC)
);

