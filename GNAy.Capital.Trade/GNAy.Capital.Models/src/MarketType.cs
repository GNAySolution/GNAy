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
    public class MarketType
    {
        public enum Enum
        {
            [Description("TS,證券")]
            Stock, //0

            [Description("TF,期貨")]
            Futures, //1

            [Description("TO,選擇權")]
            Options, //2

            [Description("OS,複委託")]
            OverseaStock, //3

            [Description("OF,海外期貨")]
            OverseaFutures, //4

            [Description("OO,海外選擇權")]
            OverseaOptions, //5
        }

        public static ReadOnlyCollection<string> DescriptionCode = new List<string>()
        {
            Enum.Stock.GetDescription().Split(',')[0],
            Enum.Futures.GetDescription().Split(',')[0],
            Enum.Options.GetDescription().Split(',')[0],
            Enum.OverseaStock.GetDescription().Split(',')[0],
            Enum.OverseaFutures.GetDescription().Split(',')[0],
            Enum.OverseaOptions.GetDescription().Split(',')[0],
        }.AsReadOnly();

        public static Dictionary<string, Enum> CodeMap = DescriptionCode.ToDictionary(x => x, x => (Enum)DescriptionCode.IndexOf(x));

        public static ReadOnlyCollection<string> DescriptionName = new List<string>()
        {
            Enum.Stock.GetDescription().Split(',')[1],
            Enum.Futures.GetDescription().Split(',')[1],
            Enum.Options.GetDescription().Split(',')[1],
            Enum.OverseaStock.GetDescription().Split(',')[1],
            Enum.OverseaFutures.GetDescription().Split(',')[1],
            Enum.OverseaOptions.GetDescription().Split(',')[1],
        }.AsReadOnly();

        public static Dictionary<string, Enum> NameMap = DescriptionName.ToDictionary(x => x, x => (Enum)DescriptionName.IndexOf(x));
    }
}
