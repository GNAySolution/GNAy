using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public class AppConfig
    {
        public readonly AppSettings Settings;

        public readonly Version Version;
        public readonly Encoding Big5Encoding;

        public readonly FileInfo File;

        public AppConfig(AppSettings settings, FileInfo file)
        {
            if (file == null && settings == null)
            {
                settings = new AppSettings();
            }

            Settings = settings;
            Version = new Version(settings.Version);
            Big5Encoding = Encoding.GetEncoding(settings.Big5EncodingCodePage);

            File = file;
        }

        public AppConfig() : this(null, null)
        {
            //
        }
    }
}
