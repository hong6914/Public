using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using AlbumService.BusinessLogic;
using AlbumService.Models;
using AlbumService.Utils;
using AlbumService.DataAccess;
using AlbumService.DataAccess.DataRepositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ApiAlbumController : ApiController
    {
        private IDataRepository _dataReposity;        
        private IServiceManager _serviceManager;

        public ApiAlbumController( )
        {
            // DIRTY code ...
            AppSettings.GetApplicationSettings( );
            if ( AppSettings.useLocalDatabase == true )
            {
                _dataReposity = DatabaseRepository.Instance;
            }
            else
            {
                _dataReposity = XMLRepository.Instance;
            }

            _serviceManager = new AlbumServiceManager( _dataReposity );
        }

        #region artist

        /// <summary>
        /// add one artist
        /// </summary>
        /// <param name="Artist"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        [Route( "artist" )]
        public HttpResponseMessage AddArtist( [FromBody]Artist ct )
        {
            if ( null == ct || string.IsNullOrWhiteSpace( ct.Name ) )
            {
                throw new HttpException( "Invalid data of Artist" );
            }

            var result = _serviceManager.AddArtist( ct );

            if( HttpStatusCode.OK == result )
            {
                HttpResponseMessage response = Request.CreateResponse( result, new { artistId = ct.ArtistId } );
                return response;
            }
            else
            {
                throw new HttpException( "Failed to add new artist" );
            }
        }


        /// <summary>
        /// retrieve one artist settings
        /// </summary>
        /// <param name="artistId"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [Route( "artist/{artistId}" )]
        public Artist GetArtist( int artistId )
        {
            Artist ct = null;
            var result = HttpStatusCode.OK;

            if ( HttpStatusCode.OK != ( result = _serviceManager.GetArtist( artistId, out ct ) ) )
            {
                throw new HttpException( "Failed to retrieve artist based on the ID: " + artistId );
            }

            return ct;
        }


        /// <summary>
        /// update one artist
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updatedArtist"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPut]
        [Route( "artist/{artistId}" )]
        public HttpResponseMessage SetArtist( int artistId, [FromBody]Artist updatedArtist )
        {
            if ( null == updatedArtist || artistId <= 0 || string.IsNullOrWhiteSpace( updatedArtist.Name ) )
            {
                Request.CreateResponse( HttpStatusCode.NotAcceptable, "Invalid data of artist" );
            }

            var result = HttpStatusCode.BadRequest;

            updatedArtist.ArtistId = artistId;

            if ( HttpStatusCode.OK != ( result = _serviceManager.SetArtist( updatedArtist ) ) )
            {
                throw new HttpException( "Failed to update artist settings: ID: " + artistId );
            }

            return Request.CreateResponse( result );
        }


        /// <summary>
        /// delete one artist
        /// </summary>
        /// <param name="id"></param>
        [System.Web.Http.HttpDelete]
        [Route( "artist/{artistId}" )]
        public HttpResponseMessage DeleteArtist( int artistId )
        {
            Artist ct = null;
            var result = HttpStatusCode.BadRequest;

            if ( artistId <= 0 )
            {
                throw new HttpException( "Invalid artist ID: " + artistId );
            }

            if ( HttpStatusCode.OK == ( result = _serviceManager.GetArtist( artistId, out ct ) ) )
            {

                if( HttpStatusCode.OK != (result = _serviceManager.DeleteArtist( ct ) ) )
                {
                    throw new HttpException( "Failed to delete artist with ID: " + artistId );
                }
            }
            else
            {
                throw new HttpException( "Failed to find artist with ID: " + artistId );
            }

            return Request.CreateResponse( result );
        }

        /// <summary>
        /// get all artists
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [Route( "artist" )]
        public HttpResponseMessage GetArtistList( )
        {
            var ctl = _serviceManager.GetAllArtists( );
            return Request.CreateResponse( HttpStatusCode.OK, ctl );
        }

        #endregion


        #region album

        /// <summary>
        /// add one album
        /// </summary>
        [System.Web.Http.HttpPost]
        [Route( "album" )]
        public HttpResponseMessage AddAlbum( [FromBody]Album ct )
        {
            if ( null == ct || string.IsNullOrWhiteSpace( ct.Title ) )
            {
                throw new HttpException( "Invalid data" );
            }

            var result = _serviceManager.AddAlbum( ct );

            if ( HttpStatusCode.OK == result )
            {
                HttpResponseMessage response = Request.CreateResponse( result, new { title = ct.Title } );
                return response;
            }
            else
            {
                throw new HttpException( "Failed to add new artist" );
            }
        }


        /// <summary>
        /// retrieve one album settings
        /// </summary>
        [System.Web.Http.HttpGet]
        [Route( "album" )]
        public object GetAlbum( string title )
        {
            Album ct = null;
            var result = HttpStatusCode.OK;

            if ( HttpStatusCode.OK != ( result = _serviceManager.GetAlbum( title, out ct ) ) )
            {
                throw new HttpException( "Failed to retrieve artist based on the title: " + title );
            }

            List<Song> allSongs = _serviceManager.GetAllSongs( );

            return new { 
                album = ct,
                allSongsInAlbum = allSongs.Where( x => x.Album.Equals( title, StringComparison.InvariantCultureIgnoreCase ) ),
            };
        }



        /// <summary>
        /// get all albums
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [Route( "albums" )]
        public HttpResponseMessage GetAlbumList( )
        {
            var ctl = _serviceManager.GetAllAlbums( );
            return Request.CreateResponse( HttpStatusCode.OK, ctl );
        }

        #endregion


        #region song

        /// <summary>
        /// add one song
        /// </summary>
        [System.Web.Http.HttpPost]
        [Route( "song" )]
        public HttpResponseMessage AddSong( [FromBody]Song ct )
        {
            if ( null == ct || string.IsNullOrWhiteSpace( ct.Title ) )
            {
                throw new HttpException( "Invalid data" );
            }

            var result = _serviceManager.AddSong( ct );

            if ( HttpStatusCode.OK == result )
            {
                HttpResponseMessage response = Request.CreateResponse( result, new { title = ct.Title } );
                return response;
            }
            else
            {
                throw new HttpException( "Failed to add new artist" );
            }
        }


        /// <summary>
        /// retrieve one song settings
        /// </summary>
        [System.Web.Http.HttpGet]
        [Route( "song" )]
        public Song GetSong( string title )
        {
            Song ct = null;
            var result = HttpStatusCode.OK;

            if ( HttpStatusCode.OK != ( result = _serviceManager.GetSong( title, out ct ) ) )
            {
                throw new HttpException( "Failed to retrieve artist based on the title: " + title );
            }

            return ct;
        }



        /// <summary>
        /// get all albums
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        [Route( "songs" )]
        public HttpResponseMessage GetSongList( )
        {
            var ctl = _serviceManager.GetAllSongs( );
            return Request.CreateResponse( HttpStatusCode.OK, ctl );
        }

        #endregion


    }
}
