using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET47
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public int Index { get; private set; }
        public string StringFormat { get; private set; }

        public ColumnAttribute(string name, string shortName, int index, string stringFormat)
        {
            Name = name;
            ShortName = shortName;
            Index = index;
            StringFormat = stringFormat;
        }

        public ColumnAttribute(string name, string shortName, int index) : this(name, shortName, index, string.Empty)
        {
            //
        }

        public ColumnAttribute(string name, int index, string stringFormat) : this(name, name, index, stringFormat)
        {
            //
        }

        public ColumnAttribute(string name, int index) : this(name, name, index, string.Empty)
        {
            //
        }

        public ColumnAttribute() : this(string.Empty, string.Empty, -1, string.Empty)
        {
            //
        }
    }
}
