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
    public class PositionKind
    {
        public enum Enum
        {
            [Description("新倉")]
            Open, //0

            [Description("平倉")]
            Close, //1

            [Description("自動")]
            Auto, //2
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.Open.GetDescription(),
            Enum.Close.GetDescription(),
            Enum.Auto.GetDescription(),
        }.AsReadOnly();
    }
}
