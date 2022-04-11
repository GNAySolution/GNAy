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
    public class TradeType
    {
        public enum Enum
        {
            [Description("ROD")]
            ROD, //0

            [Description("IOC")]
            IOC, //1

            [Description("FOK")]
            FOK, //2
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.ROD.GetDescription(),
            Enum.IOC.GetDescription(),
            Enum.FOK.GetDescription(),
        }.AsReadOnly();
    }
}
