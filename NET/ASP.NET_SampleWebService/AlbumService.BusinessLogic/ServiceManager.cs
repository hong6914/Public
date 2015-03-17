using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using AlbumService.DataAccess;
using AlbumService.Models;
using AlbumService.Utils;
using AlbumService.DataAccess.DataRepositories;

namespace AlbumService.BusinessLogic
{
    public interface IServiceManager
    {
        HttpStatusCode AddArtist( Artist ct );
        HttpStatusCode GetArtist( long ArtistId, out Artist ct );
        HttpStatusCode SetArtist( Artist ct );
        HttpStatusCode DeleteArtist( Artist ct );
        List<Artist>   GetAllArtists( );

        HttpStatusCode AddAlbum( Album ct );
        HttpStatusCode GetAlbum( long AlbumId, out Album ct );
        HttpStatusCode GetAlbum( string albumTitle, out Album ct );
        List<Album> GetAllAlbums( );

        HttpStatusCode AddSong( Song ct );
        HttpStatusCode GetSong( long SongId, out Song ct );
        HttpStatusCode GetSong( string songTitle, out Song ct );
        HttpStatusCode SetSong( Song ct );
        List<Song> GetAllSongs( );
    }


    public class AlbumServiceManager : IServiceManager
    {
        private IDataRepository _DataRepository;


        private ReaderWriterLockSlim _opLock = new ReaderWriterLockSlim( );


        public AlbumServiceManager( IDataRepository artistDataRepository )
        {
            _DataRepository = artistDataRepository;
        }


        #region code that deals with Artist

        /// <summary>
        /// add new artist
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public HttpStatusCode AddArtist( Artist ct )
        {
            HttpStatusCode result = HttpStatusCode.NotAcceptable;


            _opLock.EnterWriteLock( );
            try
            {
                if ( false == _DataRepository.AddArtist( ct ) )
                {
                    throw new AlbumServiceException( result, "data=Artist action=add ok=false", "Error creating artist" );
                }

                result = HttpStatusCode.OK;
            }
            finally
            {
                _opLock.ExitWriteLock( );
            }

            return result;
        }


        /// <summary>
        /// get artist
        /// </summary>
        /// <param name="artistId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public HttpStatusCode GetArtist( long artistId, out Artist ct )
        {
            if ( artistId <= 0 )
            {
                ct = null;
                throw new AlbumServiceException( HttpStatusCode.BadRequest, string.Format( "data=Artist action=get ok=false error=bad parameter Id={0}", artistId ),
                                                                            string.Format( "Error retrieving Artist: bad parameter Id={0}", artistId ) );
            }

            _opLock.EnterReadLock( );
            try
            {
                List<Artist> theList = _DataRepository.GetArtists( );
                ct = theList.FirstOrDefault( x => x.ArtistId == artistId );
            }
            finally
            {
                _opLock.ExitReadLock( );
            }

            return ( null == ct ? HttpStatusCode.NotFound : HttpStatusCode.OK );
        }


        /// <summary>
        /// update Artist
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public HttpStatusCode SetArtist( Artist ct )
        {
            HttpStatusCode result = HttpStatusCode.NotAcceptable;

            _opLock.EnterWriteLock( );
            try
            {
                if( false == _DataRepository.UpdateArtist( ct ) )
                {
                    throw new AlbumServiceException( result, "data=Artist action=update ok=false", "Error updating artist" );
                }

                result = HttpStatusCode.OK;
            }
            finally
            {
                _opLock.ExitWriteLock( );
            }

            return result;
        }


        /// <summary>
        /// remove artist from list
        /// </summary>
        /// <param name="artistId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public HttpStatusCode DeleteArtist( Artist ct )
        {
            HttpStatusCode result = HttpStatusCode.NotAcceptable;

            _opLock.EnterWriteLock( );
            try
            {
                if ( false == _DataRepository.DeleteArtist( ct ) )
                {
                    throw new AlbumServiceException( result, "data=Artist action=delete ok=false", "Error deleting artist" );
                }

                result = HttpStatusCode.OK;
            }
            finally
            {
                _opLock.ExitWriteLock( );
            }

            return result;
        }


        /// <summary>
        /// get the list of artists
        /// </summary>
        /// <returns></returns>
        public List<Artist> GetAllArtists( )
        {
            var _artistDataList = _DataRepository.GetArtists( );
            return _artistDataList;
        }

        #endregion


        #region code that deals with Album

        public HttpStatusCode AddAlbum( Album ct )
        {
            HttpStatusCode result = HttpStatusCode.NotAcceptable;


            _opLock.EnterWriteLock( );
            try
            {
                if ( false == _DataRepository.AddAlbum( ct ) )
                {
                    throw new AlbumServiceException( result, "data=Album action=add ok=false", "Error creating album" );
                }

                result = HttpStatusCode.OK;
            }
            finally
            {
                _opLock.ExitWriteLock( );
            }

            return result;
        }


        public HttpStatusCode GetAlbum( long albumId, out Album ct )
        {
            if ( albumId <= 0 )
            {
                ct = null;
                throw new AlbumServiceException( HttpStatusCode.BadRequest, string.Format( "data=Album action=get ok=false error=bad parameter Id={0}", albumId ),
                                                                            string.Format( "Error retrieving Album: bad parameter Id={0}", albumId ) );
            }

            _opLock.EnterReadLock( );
            try
            {
                List<AlbumDataModel> theList = _DataRepository.GetAlbums( );
                ct = DataConvertor.DataModelToBusinessModel( _DataRepository, theList.FirstOrDefault( x => x.ArtistId == albumId ) );
            }
            finally
            {
                _opLock.ExitReadLock( );
            }

            return ( null == ct ? HttpStatusCode.NotFound : HttpStatusCode.OK );
        }


        public HttpStatusCode GetAlbum( string albumTitle, out Album ct )
        {
            if ( string.IsNullOrWhiteSpace( albumTitle ) )
            {
                ct = null;
                throw new AlbumServiceException( HttpStatusCode.BadRequest, "data=Album action=get ok=false error=bad parameter albumTitle",
                                                                            "Error retrieving Album: bad parameter albumTitle" );
            }

            _opLock.EnterReadLock( );
            try
            {
                List<AlbumDataModel> theList = _DataRepository.GetAlbums( );
                ct = DataConvertor.DataModelToBusinessModel( _DataRepository, theList.FirstOrDefault( x => x.Title.Equals( albumTitle, StringComparison.InvariantCultureIgnoreCase) ) );
            }
            finally
            {
                _opLock.ExitReadLock( );
            }

            return ( null == ct ? HttpStatusCode.NotFound : HttpStatusCode.OK );
        }


        public List<Album> GetAllAlbums( )
        {
            var _albumDataList = _DataRepository.GetAlbums( );
            var _albumList = new List<Album>( );

            foreach ( AlbumDataModel adm in _albumDataList )
            {
                _albumList.Add( DataConvertor.DataModelToBusinessModel( _DataRepository, adm ) );
            }

            return _albumList;
        }

        #endregion


        #region code that deals with Song

        public HttpStatusCode AddSong( Song ct )
        {
            HttpStatusCode result = HttpStatusCode.NotAcceptable;


            _opLock.EnterWriteLock( );
            try
            {
                if ( false == _DataRepository.AddSong( ct ) )
                {
                    throw new AlbumServiceException( result, "data=Song action=add ok=false", "Error creating Song" );
                }

                result = HttpStatusCode.OK;
            }
            finally
            {
                _opLock.ExitWriteLock( );
            }

            return result;
        }


        public HttpStatusCode GetSong( long songId, out Song ct )
        {
            if ( songId <= 0 )
            {
                ct = null;
                throw new AlbumServiceException( HttpStatusCode.BadRequest, string.Format( "data=Song action=get ok=false error=bad parameter Id={0}", songId ),
                                                                            string.Format( "Error retrieving Song: bad parameter Id={0}", songId ) );
            }

            _opLock.EnterReadLock( );
            try
            {
                List<SongDataModel> theList = _DataRepository.GetSongs( );
                ct = DataConvertor.DataModelToBusinessModel( _DataRepository, theList.FirstOrDefault( x => x.SongId == songId ) );
            }
            finally
            {
                _opLock.ExitReadLock( );
            }

            return ( null == ct ? HttpStatusCode.NotFound : HttpStatusCode.OK );
        }


        public HttpStatusCode GetSong( string songTitle, out Song ct )
        {
            if ( string.IsNullOrWhiteSpace( songTitle ) )
            {
                ct = null;
                throw new AlbumServiceException( HttpStatusCode.BadRequest, "data=Song action=get ok=false error=bad parameter songTitle",
                                                                            "Error retrieving Song: bad parameter albumTitle" );
            }

            _opLock.EnterReadLock( );
            try
            {
                List<SongDataModel> theList = _DataRepository.GetSongs( );
                ct = DataConvertor.DataModelToBusinessModel( _DataRepository, theList.FirstOrDefault( x => x.Title.Equals( songTitle, StringComparison.InvariantCultureIgnoreCase ) ) );
            }
            finally
            {
                _opLock.ExitReadLock( );
            }

            return ( null == ct ? HttpStatusCode.NotFound : HttpStatusCode.OK );
        }

        
        public HttpStatusCode SetSong( Song ct )
        {
            _opLock.EnterReadLock( );
            try
            {
                if( false == _DataRepository.UpdateSong( ct ) )
                {
                    throw new AlbumServiceException( HttpStatusCode.BadRequest, "data=Song action=set ok=false",
                                                                                "Error updating Song" );
                }

                return HttpStatusCode.OK;
            }
            finally
            {
                _opLock.ExitReadLock( );
            }
        }


        public List<Song> GetAllSongs( )
        {
            var _songDataList = _DataRepository.GetSongs( );
            var _songList = new List<Song>( );

            foreach ( SongDataModel adm in _songDataList )
            {
                _songList.Add( DataConvertor.DataModelToBusinessModel( _DataRepository, adm ) );
            }

            return _songList;
        }

        #endregion
    }
}
