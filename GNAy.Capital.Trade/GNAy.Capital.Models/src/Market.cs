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
    public class Market
    {
        public enum EType
        {
            [Description("TS,證券")]
            Stock, //0

            [Description("TF,期貨")]
            Futures, //1

            [Description("TO,選擇權")]
            Option, //2

            [Description("OS,複委託")]
            OverseaStock, //3

            [Description("OF,海外期貨")]
            OverseaFutures, //4

            [Description("OO,海外選擇權")]
            OverseaOption, //5
        }

        public static ReadOnlyCollection<string> CodeDescription = new List<string>()
        {
            EType.Stock.GetDescription().Split(',')[0],
            EType.Futures.GetDescription().Split(',')[0],
            EType.Option.GetDescription().Split(',')[0],
            EType.OverseaStock.GetDescription().Split(',')[0],
            EType.OverseaFutures.GetDescription().Split(',')[0],
            EType.OverseaOption.GetDescription().Split(',')[0],
        }.AsReadOnly();

        public static Dictionary<string, EType> CodeMap = CodeDescription.ToDictionary(x => x, x => (EType)CodeDescription.IndexOf(x));

        public static ReadOnlyCollection<string> NameDescription = new List<string>()
        {
            EType.Stock.GetDescription().Split(',')[1],
            EType.Futures.GetDescription().Split(',')[1],
            EType.Option.GetDescription().Split(',')[1],
            EType.OverseaStock.GetDescription().Split(',')[1],
            EType.OverseaFutures.GetDescription().Split(',')[1],
            EType.OverseaOption.GetDescription().Split(',')[1],
        }.AsReadOnly();

        public static Dictionary<string, EType> NameMap = NameDescription.ToDictionary(x => x, x => (EType)NameDescription.IndexOf(x));

        public enum EGroup
        {
            [Description("上市")]
            TSE, //0

            [Description("上櫃")]
            OTC, //1

            [Description("期貨")]
            Futures, //2

            [Description("選擇權")]
            Option, //3

            [Description("興櫃")]
            Emerging, //4
        }

        public static ReadOnlyCollection<string> GroupDescription = new List<string>()
        {
            EGroup.TSE.GetDescription(),
            EGroup.OTC.GetDescription(),
            EGroup.Futures.GetDescription(),
            EGroup.Option.GetDescription(),
            EGroup.Emerging.GetDescription(),
        }.AsReadOnly();

        public enum EDayNight
        {
            [Description("夜盤")]
            PM, //0

            [Description("日盤")]
            AM, //1
        }

        public static ReadOnlyCollection<string> DayNightDescription = new List<string>()
        {
            EDayNight.PM.GetDescription(),
            EDayNight.AM.GetDescription(),
        }.AsReadOnly();
    }
}
