using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using Newtonsoft.Json;

namespace AlbumService.Utils
{

    public class AppSettings
    {

        private static Configuration rootWebConfig = null;
        public static string DBConnectionString { get; set; }
        public static bool useLocalDatabase { get; set; }
        public static string localXMLFile { get; set; }

        public static void GetApplicationSettings( )
        {
            rootWebConfig = WebConfigurationManager.OpenWebConfiguration( "/AlbumService" );
            if ( rootWebConfig.ConnectionStrings.ConnectionStrings.Count > 0 )
            {
                DBConnectionString = rootWebConfig.ConnectionStrings.ConnectionStrings[ "AlbumServiceDB" ].ToString( );
            }

            if ( rootWebConfig.AppSettings.Settings.Count > 0 )
            {
                KeyValueConfigurationElement customSetting = rootWebConfig.AppSettings.Settings[ "UseLocalDatabase" ];
                if ( null != customSetting )
                {
                    useLocalDatabase = Convert.ToBoolean( customSetting.Value );
                }

                if ( false == useLocalDatabase )
                {
                    customSetting = rootWebConfig.AppSettings.Settings[ "LocalXMLFilePath" ];
                    if ( null != customSetting )
                    {
                        localXMLFile = customSetting.Value;
                    }
                }
            }
        }
    }
    


}