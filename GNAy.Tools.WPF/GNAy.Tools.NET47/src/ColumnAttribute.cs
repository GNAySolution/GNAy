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
        public string StringFormat { get; set; }

        public ColumnAttribute(string name, string shortName, int index)
        {
            Name = name;
            ShortName = shortName;
            Index = index;
            StringFormat = String.Empty;
        }

        public ColumnAttribute(string name, int index) : this(name, name, index)
        {
            //
        }

        public ColumnAttribute() : this(string.Empty, string.Empty, -1)
        {
            //
        }
    }
}
