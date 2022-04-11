using GNAy.Tools.NET47;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public class BS
    {
        public enum Enum
        {
            [Description("Buy")]
            Buy, //0

            [Description("Sell")]
            Sell, //1
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.Buy.GetDescription(),
            Enum.Sell.GetDescription(),
        }.AsReadOnly();
    }
}
