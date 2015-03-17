using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumService.Models;
using AlbumService.DataAccess.DataRepositories;

namespace AlbumService.DataAccess
{
    public class DataConvertor
    {
        public static Album DataModelToBusinessModel( IDataRepository repo, AlbumDataModel adm )
        {
            if ( adm == null ) return null;
            
            List<Artist> artistList = repo.GetArtists( );
            var index = artistList.FirstOrDefault( x => x.ArtistId == adm.ArtistId);
            if ( index == null ) return null;

            Album a = new Album() 
            {
                Title = adm.Title,
                Artist = index.Name,
            };

            return a;
        }


        public static AlbumDataModel BusinessModelToDataModel( IDataRepository repo, Album a )
        {
            if ( a == null ) return null;

            List<Artist> artistList = repo.GetArtists( );
            var index = artistList.FirstOrDefault( x => x.Name.Equals( a.Artist, StringComparison.InvariantCultureIgnoreCase ) );
            if ( index == null ) return null;

            AlbumDataModel adm = new AlbumDataModel( )
            {
                Title = a.Title,
                ArtistId = index.ArtistId,
            };

            return adm;
        }


        public static Song DataModelToBusinessModel( IDataRepository repo, SongDataModel adm )
        {
            if ( adm == null ) return null;

            List<AlbumDataModel> albumList = repo.GetAlbums( );
            var index = albumList.FirstOrDefault( x => x.AlbumId == adm.AlbumId );
            if ( index == null ) return null;

            Song a = new Song( )
            {
                Title = adm.Title,
                Album = index.Title,
                DateAdded = adm.DateAdded,
                DateModified = adm.DateModified,
                Genre = adm.Genre,
                Length = adm.Length,
                TrackNumber = adm.TrackNumber,
            };

            return a;
        }


        public static SongDataModel BusinessModelToDataModel( IDataRepository repo, Song a )
        {
            if ( a == null ) return null;

            List<AlbumDataModel> albumList = repo.GetAlbums( );
            var index = albumList.FirstOrDefault( x => x.Title.Equals( a.Album, StringComparison.InvariantCultureIgnoreCase ) );
            if ( index == null ) return null;

            SongDataModel adm = new SongDataModel( )
            {
                Title = a.Title,
                AlbumId = index.AlbumId,
                DateAdded = a.DateAdded,
                DateModified = a.DateModified,
                Genre = a.Genre,
                Length = a.Length,
                TrackNumber = a.TrackNumber,
            };

            return adm;
        }



    }
}
