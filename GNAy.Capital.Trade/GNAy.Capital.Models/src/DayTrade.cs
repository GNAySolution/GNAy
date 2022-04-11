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
    public class DayTrade
    {
        public enum Enum
        {
            [Description("否")]
            No, //0

            [Description("是")]
            Yes, //1
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.No.GetDescription(),
            Enum.Yes.GetDescription(),
        }.AsReadOnly();
    }
}
