using GNAy.Tools.NET48;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public class OrderPosition
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

        public static readonly string[] Description =
        {
            Enum.Open.GetDescription(),
            Enum.Close.GetDescription(),
            Enum.Auto.GetDescription(),
        };
    }
}
