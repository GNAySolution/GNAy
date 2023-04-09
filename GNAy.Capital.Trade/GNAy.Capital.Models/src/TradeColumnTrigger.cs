using GNAy.Tools.NET48.Models;
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

        public string DisplayName => string.Join(",", Attribute.CSVName, Property.PropertyType.Name, Attribute.TriggerFormat);
        public string ToolTip => ToString();

        public TradeColumnTrigger(in TradeColumnAttribute attr, in PropertyInfo property)
        {
            Attribute = attr;
            Property = property;
        }

        private TradeColumnTrigger() : this(null, null)
        { }

        public override string ToString()
        {
            return string.Join(",", Property.Name, Property.PropertyType.Name, Attribute.CSVName, Attribute.WPFName, Attribute.TriggerFormat);
        }
    }
}
