using System.Collections.Generic;
using AlbumService.Models;

namespace AlbumService.DataAccess.DataRepositories
{
    public interface IDataRepository
    {
        bool Initialized { get; }
        void Initialize( );

        bool AddArtist( Artist data );
        bool UpdateArtist( Artist data );
        bool DeleteArtist( Artist data );
        List<Artist> GetArtists( );

        bool AddAlbum( Album data );
        bool UpdateAlbum( Album data );
        //bool DeleteAlbum( Album data );
        List<AlbumDataModel> GetAlbums( );

        bool AddSong( Song data );
        bool UpdateSong( Song data );
        //void DeleteSong( SongDataModel data );
        List<SongDataModel> GetSongs( );
    }
}