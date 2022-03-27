using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public class TradeColumnTouched : NotifyPropertyChanged
    {
        public string PropertyName { get; private set; }

        private TradeColumnAttribute _kernel { get; set; }

        public string Name => _kernel.Name;
        public string ShortName => _kernel.ShortName;
        public int Index => _kernel.Index;
        public bool TouchedAlert => _kernel.TouchedAlert;

        public string DisplayName => $"{Name},{PropertyName}";
        public string ToolTip => ToString();

        public TradeColumnTouched(string propertyName, TradeColumnAttribute kernel)
        {
            PropertyName = propertyName;
            _kernel = kernel;
        }

        private TradeColumnTouched() : this(string.Empty, null)
        { }

        public override string ToString()
        {
            return string.Join(",", PropertyName, Name, ShortName, Index, TouchedAlert);
        }
    }
}
