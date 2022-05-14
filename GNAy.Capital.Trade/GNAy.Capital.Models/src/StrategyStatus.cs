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
    /// <summary>
    /// 策略狀態
    /// </summary>
    public class StrategyStatus
    {
        public enum Enum
        {
            [Description("等待中")]
            Waiting, //0

            [Description("已取消")]
            Cancelled, //1

            [Description("委託送出")]
            OrderSent, //2

            [Description("委託回報")]
            OrderReport, //3

            [Description("委託成交")]
            DealReport, //4

            [Description("委託錯誤")]
            OrderError, //5

            [Description("停損送出")]
            StopLossSent, //6,

            [Description("停損回報")]
            StopLossOrderReport, //7,

            [Description("停損成交")]
            StopLossDealReport, //8,

            [Description("停損錯誤")]
            StopLossError, //9,

            [Description("停利送出")]
            StopWinSent, //10,

            [Description("停利回報")]
            StopWinOrderReport, //11,

            [Description("停利成交")]
            StopWinDealReport, //12,

            [Description("停利錯誤")]
            StopWinError, //13,

            [Description("移動停利送出")]
            MoveStopWinSent, //14,

            [Description("移動停利回報")]
            MoveStopWinOrderReport, //15,

            [Description("移動停利成交")]
            MoveStopWinDealReport, //16,

            [Description("移動停利錯誤")]
            MoveStopWinError, //17,

            [Description("收盤送出")]
            MarketClosingSent, //18,

            [Description("收盤回報")]
            MarketClosingOrderReport, //19,

            [Description("收盤成交")]
            MarketClosingDealReport, //20,

            [Description("收盤錯誤")]
            MarketClosingError, //21,
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.Waiting.GetDescription(),
            Enum.Cancelled.GetDescription(),
            //
            Enum.OrderSent.GetDescription(),
            Enum.OrderReport.GetDescription(),
            Enum.DealReport.GetDescription(),
            Enum.OrderError.GetDescription(),
            //
            Enum.StopLossSent.GetDescription(),
            Enum.StopLossOrderReport.GetDescription(),
            Enum.StopLossDealReport.GetDescription(),
            Enum.StopLossError.GetDescription(),
            //
            Enum.StopWinSent.GetDescription(),
            Enum.StopWinOrderReport.GetDescription(),
            Enum.StopWinDealReport.GetDescription(),
            Enum.StopWinError.GetDescription(),
            //
            Enum.MoveStopWinSent.GetDescription(),
            Enum.MoveStopWinOrderReport.GetDescription(),
            Enum.MoveStopWinDealReport.GetDescription(),
            Enum.MoveStopWinError.GetDescription(),
            //
            Enum.MarketClosingSent.GetDescription(),
            Enum.MarketClosingOrderReport.GetDescription(),
            Enum.MarketClosingDealReport.GetDescription(),
            Enum.MarketClosingError.GetDescription(),
        }.AsReadOnly();
    }
}
