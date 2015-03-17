using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;
using AlbumService.Models;
using AlbumService.Utils;
using System.Xml;
using System.Xml.Linq;

namespace AlbumService.DataAccess.DataRepositories
{
    public class XMLRepository : IDataRepository
    {
        private static XMLRepository instance;
        private static object _lockObj = new object();

        private List<Artist> _artists = new List<Artist> ( );
        private List<AlbumDataModel> _albums = new List<AlbumDataModel>( );
        private List<SongDataModel> _songs = new List<SongDataModel>( );
        private bool _initialized;
        private XmlDocument xDoc;


        public static XMLRepository Instance
        {
            get
            {
                if ( instance == null )
                {
                    lock ( _lockObj )
                    {
                        instance = new XMLRepository( );
                    }
                }

                return instance;
            }
        }

        public bool Initialized
        {
            get { return _initialized; }
        }

        private XMLRepository( )
        {
            Initialize( );
            _initialized = true;
        }


        public void Initialize( )
        {
            xDoc = new XmlDocument( );
            xDoc.Load( AppSettings.localXMLFile );

            lock (_lockObj)
            {
                InitializeData( );
            }
        }


        #region Artists

        private void InitializeData( )
        {
            _artists.Clear( );
            _albums.Clear( );
            _songs.Clear( );

            XmlNodeList xnl = xDoc.SelectNodes( "/music/*" );
            foreach ( XmlNode oneArtist in xnl )
            {
                Artist a = new Artist( );
                a.ArtistId = _artists.Count + 1;
                if ( oneArtist.Attributes[ "name" ] != null )
                { a.Name = oneArtist.Attributes[ "name" ].Value; }

                _artists.Add( a );

                var albumsDoc = new XmlDocument( );
                albumsDoc.LoadXml( oneArtist.InnerXml );
                XmlNodeList anl = albumsDoc.SelectNodes( "/artist/*" );

                foreach ( XmlNode oneAlbum in anl )
                {
                    AlbumDataModel b = new AlbumDataModel( );
                    b.AlbumId = _albums.Count + 1;
                    b.ArtistId = a.ArtistId;
                    if ( oneAlbum.Attributes[ "title" ] != null )
                    { b.Title = oneAlbum.Attributes[ "title" ].Value; }

                    _albums.Add( b );

                    var songsDoc = new XmlDocument( );
                    songsDoc.LoadXml( oneAlbum.InnerXml );
                    XmlNodeList snl = songsDoc.SelectNodes( "/song/*" );

                    foreach ( XmlNode oneSong in snl )
                    {
                        SongDataModel c = new SongDataModel( );
                        c.SongId = _songs.Count + 1;
                        c.AlbumId = b.AlbumId;
                        c.DateAdded = c.DateModified = DateTime.UtcNow;
                        if ( oneSong.Attributes[ "title" ] != null )
                        { c.Title = oneSong.Attributes[ "title" ].Value; }
                        if ( oneSong.Attributes[ "genre" ] != null )
                        { c.Genre = oneSong.Attributes[ "genre" ].Value; }
                        if ( oneSong.Attributes[ "length" ] != null )
                        { c.Length = oneSong.Attributes[ "length" ].Value; }
                        if ( oneSong.Attributes[ "tracknumber" ] != null )
                        { c.TrackNumber = Convert.ToInt32( oneSong.Attributes[ "tracknumber" ].Value ); }

                        _songs.Add( c );
                    }
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
                entity.ArtistId = _artists.Count + 1;
                _artists.Add( entity );
                SaveNewArtist( entity );
            }
            return true;
        }

        private bool SaveNewArtist( Artist entity )
        {
            XDocument xroot = XDocument.Parse( AppSettings.localXMLFile );
            xroot.Descendants( "artist" ).FirstOrDefault( ).Add( entity );
            xroot.Save( AppSettings.localXMLFile );
            return true;
        }


        
        public bool UpdateArtist( Artist entity )
        {
            var index = _artists.ToList( ).FindIndex( x => x.ArtistId == entity.ArtistId );
            if ( index == -1 )
                return false; //TODO: throw exception?

            lock ( _lockObj )
            {
                entity.ArtistId = index;
                _artists[ index ] = entity;
                SaveUpdatedArtist( entity );
            }

            return true;
        }

        private bool SaveUpdatedArtist( Artist entity )
        {
            throw new NotImplementedException( );
        }

        
        public bool DeleteArtist( Artist entity )
        {
            var index = _artists.ToList( ).FindIndex( x => x.ArtistId == entity.ArtistId );
            if ( index == -1 )
                return false; //TODO: throw exception?

            lock ( _lockObj )
            {
                _artists.RemoveAt( index );
                SaveDeletedArtist( entity );
            }
            return true;
        }

        private bool SaveDeletedArtist( Artist entity )
        {
            throw new NotImplementedException( );
        }

        
        public List<Artist> GetArtists( )
        {
            return _artists;
        }
        
        #endregion


        #region Albums

        public bool AddAlbum( Album entity )
        {
            if ( entity == null )
                return false;

            AlbumDataModel adm = DataConvertor.BusinessModelToDataModel( this, entity );
            if ( adm == null ) return false;

            lock ( _lockObj )
            { 
                adm.AlbumId = _albums.Count + 1;
                _albums.Add( adm );
                SaveNewAlbum( entity );
            }
            return true;
        }

        private bool SaveNewAlbum( Album entity )
        {
            XDocument xroot = XDocument.Parse( AppSettings.localXMLFile );
            xroot.Descendants( "album" ).FirstOrDefault( x => x.Parent.Attribute( "name" ).Equals( entity.Artist ) ).Add( entity );
            xroot.Save( AppSettings.localXMLFile );
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
                _albums[ index ] = adm;
                SaveUpdatedAlbum( entity, adm );
            }

            return true;
        }

        private bool SaveUpdatedAlbum( Album entity, AlbumDataModel adm )
        {
            XDocument xroot = XDocument.Parse( AppSettings.localXMLFile );
            xroot.Descendants( "album" ).FirstOrDefault( x => x.Parent.Attribute( "name" ).Equals( entity.Artist ) && x.Attribute( "Id" ).Value.Equals( adm.AlbumId.ToString( ) ) ).Attribute( "title" ).SetValue( entity.Title );
            xroot.Save( AppSettings.localXMLFile );
            return true;
        }


        public List<AlbumDataModel> GetAlbums( )
        {
            return _albums;
        }

        #endregion


        #region Songs

        public bool AddSong( Song entity )
        {
            if ( entity == null )
                return false;

            SongDataModel adm = DataConvertor.BusinessModelToDataModel( this, entity );
            if ( adm == null ) return false;

            lock ( _lockObj )
            {
                adm.SongId = _songs.Count + 1;
                _songs.Add( adm );
                SaveNewSong( entity );
            }

            return true;
        }

        private bool SaveNewSong( Song entity )
        {
            XDocument xroot = XDocument.Parse( AppSettings.localXMLFile );
            xroot.Descendants( "song" ).FirstOrDefault( x => x.Parent.Attribute( "title" ).Equals( entity.Album ) ).Add( entity );
            xroot.Save( AppSettings.localXMLFile );
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
                adm.SongId = index;
                adm.AlbumId = _songs[ index ].AlbumId;
                adm.DateModified = DateTime.UtcNow;
                _songs[ index ] = adm;
                SaveUpdatedSong( entity, adm );
            }
       
            return true;
        }

        private bool SaveUpdatedSong( Song entity, SongDataModel adm )
        {
            XDocument xroot = XDocument.Parse( AppSettings.localXMLFile );
            var node = xroot.Descendants( "song" ).FirstOrDefault( x => x.Parent.Attribute( "title" ).Equals( entity.Album ) && x.Attribute( "Id" ).Value.Equals( adm.SongId.ToString( ) ) );
            node.Attribute( "DateAdded" ).SetValue( entity.DateAdded.ToString( ) );
            node.Attribute( "DateModified" ).SetValue( entity.DateModified.ToString( ) );
            if ( entity.Genre != null )
                node.Attribute( "Genre" ).SetValue( entity.Genre.ToString( ) );
            if ( entity.Length != null )
                node.Attribute( "Length" ).SetValue( entity.Length.ToString( ) );
            if ( entity.TrackNumber != null )
                node.Attribute( "TrackNumber" ).SetValue( entity.TrackNumber.ToString( ) );

            xroot.Save( AppSettings.localXMLFile );
            return true;
        }


        public List<SongDataModel> GetSongs( )
        {
            return _songs;
        }

        #endregion
    
    }
}