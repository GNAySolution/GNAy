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
            SentOrder, //2

            [Description("委託回報")]
            ReturnedOrder, //3

            [Description("委託錯誤")]
            OrderError, //4

            [Description("停損送出")]
            SentStopLoss, //5,

            [Description("停損回報")]
            ReturnedStopLoss, //6,

            [Description("停損錯誤")]
            StopLossError, //7,

            [Description("停利送出")]
            SentStopWin, //8,

            [Description("停利回報")]
            ReturnedStopWin, //9,

            [Description("停利錯誤")]
            StopWinError, //10,

            [Description("移動停利送出")]
            SentMoveStopWin, //11,

            [Description("移動停利回報")]
            ReturnedMoveStopWin, //12,

            [Description("移動停利錯誤")]
            MoveStopWinError, //13,
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.Waiting.GetDescription(),
            Enum.Cancelled.GetDescription(),
            Enum.SentOrder.GetDescription(),
            Enum.ReturnedOrder.GetDescription(),
            Enum.OrderError.GetDescription(),
            Enum.SentStopLoss.GetDescription(),
            Enum.ReturnedStopLoss.GetDescription(),
            Enum.StopLossError.GetDescription(),
            Enum.SentStopWin.GetDescription(),
            Enum.ReturnedStopWin.GetDescription(),
            Enum.StopWinError.GetDescription(),
            Enum.SentMoveStopWin.GetDescription(),
            Enum.ReturnedMoveStopWin.GetDescription(),
            Enum.MoveStopWinError.GetDescription(),
        }.AsReadOnly();
    }
}
