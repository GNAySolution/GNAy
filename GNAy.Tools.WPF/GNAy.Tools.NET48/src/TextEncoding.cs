using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET47
{
    public class TextEncoding
    {
        /// <summary>
        /// https://stackoverflow.com/questions/2502990/create-text-file-without-bom
        /// </summary>
        public static readonly Encoding UTF8WithoutBOM = new UTF8Encoding(false);
    }
}
