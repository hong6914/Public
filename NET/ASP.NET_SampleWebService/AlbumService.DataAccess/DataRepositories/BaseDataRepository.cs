using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;
using AlbumService.Models;
using AlbumService.Utils;

namespace AlbumService.DataAccess.DataRepositories
{
    public class DatabaseRepository : IDataRepository
    {
        private static DatabaseRepository instance;
        private DBHelper _dbHelper = null;
        private static object _lockObj = new object();

        private List<Artist> _artists = new List<Artist> ( );
        private List<AlbumDataModel> _albums = new List<AlbumDataModel>( );
        private List<SongDataModel> _songs = new List<SongDataModel>( );
        private bool _initialized;


        public static DatabaseRepository Instance
        {
            get 
            {
                if ( instance == null )
                {
                    lock ( _lockObj )
                    {
                        instance = new DatabaseRepository( );
                    }
                }

                return instance;
            }
        }
        
        public bool Initialized
        {
            get { return _initialized; }
        }

        private DatabaseRepository( )
        {
            Initialize( );
            _initialized = true;
        }


        public void Initialize( )
        {
            AppSettings.GetApplicationSettings( );
            _dbHelper = new DBHelper( AppSettings.DBConnectionString );

            lock (_lockObj)
            {
                InitializeArtists( );
                InitializeAlbums( );
                InitializeSongs( );
            }
        }


        #region Artists

        private void InitializeArtists( )
        {
            if ( _artists.Count > 0 )
            {
                _artists.Clear( );
            }

            using ( var dataReader = _dbHelper.ExecuteReader( "pGetArtists", null, false ) ) 
            {
                while ( dataReader.Read( ) )
                {
                    _artists.Add(
                        new Artist( ) 
                        {
                            ArtistId = (int) dataReader[ "ArtistId" ],
                            Name = (string) dataReader[ "ArtistName" ]
                        }
                    );
                }
            }

            if ( false == _initialized )
            {
                _initialized = true;
            }
        }

      
        public bool AddArtist( Artist entity )
        {
            if ( entity == null )
                return false;

            var index = _artists.ToList( ).FindIndex( x => x.Name.Equals( entity.Name, StringComparison.InvariantCultureIgnoreCase ) );
            if ( index == -1 )
                return false; //TODO: throw exception?

            lock ( _lockObj )
            {
                var id = _dbHelper.ExecuteScalar( "pAddArtist",
                                                  new List<SqlParameter>( )
                                                     {
                                                         new SqlParameter( "@artistName", entity.Name )
                                                     },
                                                  false );
                entity.ArtistId = (int) id;
                _artists.Add( entity );
            }
            
            return true;
        }

        
        public bool UpdateArtist( Artist entity )
        {
            var index = _artists.ToList( ).FindIndex( x => x.ArtistId == entity.ArtistId );
            if ( index == -1 )
                return false; //TODO: throw exception?

            lock ( _lockObj )
            {
                _dbHelper.ExecuteNonQuery( "pUpdateArtist",
                                           new List<SqlParameter>( )
                                              {
                                                  new SqlParameter( "@artistId", entity.ArtistId ),
                                                  new SqlParameter( "@artistName", entity.Name )
                                              },
                                           false );

                _artists[index] = entity;
            }
            
            return true;
        }

        
        public bool DeleteArtist( Artist entity )
        {
            var index = _artists.ToList( ).FindIndex( x => x.ArtistId == entity.ArtistId );
            if ( index == -1 )
                return false; //TODO: throw exception?

            lock ( _lockObj )
            {
                _dbHelper.ExecuteNonQuery( "pDeleteArtist",
                                           new List<SqlParameter>( )
                                              {
                                                  new SqlParameter( "@artistId", entity.ArtistId )
                                              },
                                           false );
                _artists.RemoveAt( index );
            }

            return true;
        }

        
        public List<Artist> GetArtists( )
        {
            return _artists;
        }
        
        #endregion


        #region Albums

        private void InitializeAlbums( )
        {
            if ( _albums.Count > 0 )
            {
                _albums.Clear( );
            }

            using ( var dataReader = _dbHelper.ExecuteReader( "pGetAlbums", null, false ) )
            {
                while ( dataReader.Read( ) )
                {
                    _albums.Add(
                        new AlbumDataModel( )
                        {
                            AlbumId = (int) dataReader[ "AlbumId" ],
                            ArtistId = (int) dataReader[ "ArtistId" ],
                            Title = (string) dataReader[ "Title" ]
                        }
                    );
                }
            }

            if ( false == _initialized )
            {
                _initialized = true;
            }
        }


        public bool AddAlbum( Album entity )
        {
            if ( entity == null )
                return false;

            AlbumDataModel adm = DataConvertor.BusinessModelToDataModel( this, entity );
            if ( adm == null ) return false;

            lock ( _lockObj )
            {
                var id = _dbHelper.ExecuteScalar( "pAddAlbum",
                                                  new List<SqlParameter>( )
                                                     {
                                                         new SqlParameter( "@albumTitle", adm.Title ),
                                                         new SqlParameter( "@artistId", adm.ArtistId )
                                                     },
                                                  false );
                adm.AlbumId = (int) id;
                _albums.Add( adm );
            }

            return true;
        }


        public bool UpdateAlbum( Album entity )
        {
            if ( entity == null )
                return false;

            AlbumDataModel adm = DataConvertor.BusinessModelToDataModel( this, entity );
            if ( adm == null ) return false;

            var index = _albums.ToList( ).FindIndex( x => x.AlbumId == adm.AlbumId );
            if ( index == -1 )
                return false; //TODO: throw exception?

            lock ( _lockObj )
            {
                var id = _dbHelper.ExecuteNonQuery( "pUpdateAlbum",
                                                  new List<SqlParameter>( )
                                                     {
                                                         new SqlParameter( "@albumId", adm.AlbumId ),
                                                         new SqlParameter( "@artistId", adm.ArtistId ),
                                                         new SqlParameter( "@albumTitle", adm.Title )
                                                     },
                                                  false );
                _albums[index] = adm;
            }

            return true;
        }


        public List<AlbumDataModel> GetAlbums( )
        {
            return _albums;
        }

        #endregion


        #region Songs

        private void InitializeSongs( )
        {
            if ( _songs.Count > 0 )
            {
                _songs.Clear( );
            }

            using ( var dataReader = _dbHelper.ExecuteReader( "pGetSongs", null, false ) )
            {
                while ( dataReader.Read( ) )
                {
                    _songs.Add(
                        new SongDataModel( )
                        {
                            SongId = (int) dataReader[ "SongId" ],
                            AlbumId = (int) dataReader[ "AlbumId" ],
                            DateAdded = (DateTime) dataReader[ "DateAdded" ],
                            DateModified = (DateTime) dataReader[ "DateModified" ],
                            Genre = (string) dataReader[ "Genre" ],
                            Length = (string) dataReader[ "Length" ],
                            TrackNumber = (int) dataReader[ "TrackNumber" ],
                            Title = (string) dataReader[ "Title" ]
                        }
                    );
                }
            }

            if ( false == _initialized )
            {
                _initialized = true;
            }
        }


        public bool AddSong( Song entity )
        {
            if ( entity == null )
                return false;

            SongDataModel adm = DataConvertor.BusinessModelToDataModel( this, entity );
            if ( adm == null ) return false;

            lock ( _lockObj )
            {
                var id = _dbHelper.ExecuteScalar( "pAddSong",
                                                  new List<SqlParameter>( )
                                                     {
                                                         new SqlParameter( "@title", adm.Title ),
                                                         new SqlParameter( "@length", adm.Length ),
                                                         new SqlParameter( "@trackNumber", adm.TrackNumber ),
                                                         new SqlParameter( "@genre", adm.Genre ),
                                                         new SqlParameter( "@albumId", adm.AlbumId )
                                                     },
                                                  false );
                adm.SongId = (int) id;
                _songs.Add( adm );
            }

            return true;
        }


        public bool UpdateSong( Song entity )
        {
            if ( entity == null )
                return false;

            SongDataModel adm = DataConvertor.BusinessModelToDataModel( this, entity );
            if ( adm == null ) return false;

            var index = _songs.ToList( ).FindIndex( x => x.SongId == adm.SongId );
            if ( index == -1 )
                return false; //TODO: throw exception?

            lock ( _lockObj )
            {
                  _dbHelper.ExecuteNonQuery( "pUpdateSong",
                                                  new List<SqlParameter>( )
                                                     {
                                                         new SqlParameter( "@songId", adm.SongId ),
                                                         new SqlParameter( "@title", adm.Title ),
                                                         new SqlParameter( "@length", adm.Length ),
                                                         new SqlParameter( "@trackNumber", adm.TrackNumber ),
                                                         new SqlParameter( "@genre", adm.Genre ),
                                                         new SqlParameter( "@albumId", adm.AlbumId )
                                                     },
                                                  false );
                _songs[index]= adm;
          }

            return true;
        }


        public List<SongDataModel> GetSongs( )
        {
            return _songs;
        }

        #endregion
    
    }
}