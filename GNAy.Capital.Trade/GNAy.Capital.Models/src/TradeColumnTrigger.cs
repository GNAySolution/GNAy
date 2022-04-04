using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class TradeColumnTrigger : NotifyPropertyChanged
    {
        public TradeColumnAttribute Attribute { get; private set; }
        public PropertyInfo Property { get; private set; }

        public string DisplayName => string.Join(",", Attribute.Name, Property.PropertyType.Name, Attribute.ValueFormat);
        public string ToolTip => ToString();

        public TradeColumnTrigger(TradeColumnAttribute attr, PropertyInfo property)
        {
            Attribute = attr;
            Property = property;
        }

        private TradeColumnTrigger() : this(null, null)
        { }

        public override string ToString()
        {
            return string.Join(",", Property.Name, Property.PropertyType.Name, Attribute.Name, Attribute.ShortName, Attribute.ValueFormat);
        }
    }
}
