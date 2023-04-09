using GNAy.Tools.NET48;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    /// <summary>
    /// 當沖
    /// </summary>
    public class OrderDayTrade
    {
        public enum Enum
        {
            [Description("否")]
            No, //0

            [Description("是")]
            Yes, //1
        }

        public static readonly string[] Description =
        {
            Enum.No.GetDescription(),
            Enum.Yes.GetDescription(),
        };
    }
}
