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

            [Description("委託錯誤")]
            SentOrderError, //3

            [Description("已停損")]
            StoppedLoss, //4,

            [Description("停損錯誤")]
            StoppedLossError, //5,

            [Description("已停利")]
            StoppedWin, //6,

            [Description("停利錯誤")]
            StoppedWinError, //7,

            [Description("已移動停利")]
            MoveStoppedWin, //8,

            [Description("移動停利錯誤")]
            MoveStoppedWinError, //9,
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.Waiting.GetDescription(),
            Enum.Cancelled.GetDescription(),
            Enum.SentOrder.GetDescription(),
            Enum.SentOrderError.GetDescription(),
            Enum.StoppedLoss.GetDescription(),
            Enum.StoppedLossError.GetDescription(),
            Enum.StoppedWin.GetDescription(),
            Enum.StoppedWinError.GetDescription(),
            Enum.MoveStoppedWin.GetDescription(),
            Enum.MoveStoppedWinError.GetDescription(),
        }.AsReadOnly();
    }
}
