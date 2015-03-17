using System;
using Newtonsoft.Json;

namespace AlbumService.Models
{
    public class Artist
    {
        [JsonProperty( PropertyName = "ArtistId" )]
        public long ArtistId { get; set; }

        [JsonProperty( PropertyName = "Name" )]
        public string Name { get; set; }
    }


    public class AlbumDataModel
    {
        public long AlbumId { get; set; }

        public string Title { get; set; }

        public long ArtistId { get; set; }
    }


    public class Album                                                          // the logical model
    {
        [JsonProperty( PropertyName = "Title" )]
        public string Title { get; set; }

        [JsonProperty( PropertyName = "Artist" )]
        public string Artist { get; set; }
    }


    public class SongDataModel
    {
        public long SongId { get; set; }

        public string Title { get; set; }

        public string Length { get; set; }                                      // in minutes:seconds

        public int? TrackNumber { get; set; }

        public string Genre { get; set; }

        public DateTime DateAdded { get; set; }

        public DateTime DateModified { get; set; }

        public long AlbumId { get; set; }
    }


    public class Song                                                           // the logical model
    {
        [JsonProperty( PropertyName = "Title" )]
        public string Title { get; set; }

        [JsonProperty( PropertyName = "Length" )]
        public string Length { get; set; }

        [JsonProperty( PropertyName = "TrackNumber" )]
        public int? TrackNumber { get; set; }

        [JsonProperty( PropertyName = "Genre" )]
        public string Genre { get; set; }

        [JsonProperty( PropertyName = "DateAdded" )]
        public DateTime DateAdded { get; set; }

        [JsonProperty( PropertyName = "DateModified" )]
        public DateTime DateModified { get; set; }

        [JsonProperty( PropertyName = "AlbumName" )]
        public string Album { get; set; }
    }



}
