using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AlbumService.Utils;

namespace AlbumService.Utils
{
    //TODO: Clean up the LogError
    public class AlbumServiceException : Exception
    {
        public AlbumServiceException( )
            : base( )
        {
            throw new HttpResponseException( HttpStatusCode.NotAcceptable );
        }

        public AlbumServiceException( HttpStatusCode theCode, string logMessage = null, string displayMessage = null )
            : base( logMessage )
        {
            throw ( string.IsNullOrWhiteSpace( logMessage ) ?
                        new HttpResponseException( theCode ) :
                        new HttpResponseException( new HttpResponseMessage( theCode ) { Content = new StringContent( displayMessage ?? logMessage ) } ) );
        }
    }
}
