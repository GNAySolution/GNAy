using GNAy.Tools.NET47;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    /// <summary>
    /// 買賣別
    /// </summary>
    public class OrderBS
    {
        public enum Enum
        {
            [Description("Buy")]
            Buy, //0

            [Description("Sell")]
            Sell, //1
        }

        public static readonly string[] Description =
        {
            Enum.Buy.GetDescription(),
            Enum.Sell.GetDescription(),
        };
    }
}
