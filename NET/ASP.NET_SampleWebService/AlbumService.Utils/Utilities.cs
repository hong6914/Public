using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AlbumService.Utils
{
    public static class Utilities
    {
        public static string SerializeObject<T>( this T toSerialize )
        {
            XmlSerializer xmlSerializer = new XmlSerializer( toSerialize.GetType( ) );
            StringWriter textWriter = new StringWriter( );

            xmlSerializer.Serialize( textWriter, toSerialize );
            return textWriter.ToString( );
        }
    }
}
