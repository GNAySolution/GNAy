using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public static class Definition
    {
        public const int MarketTSE = 0;
        public const int MarketOTC = 1;
        public const int MarketFutures = 2;
        public const int MarketOptions = 3;
        public const int MarketEmerging = 4;

        //public const int SimulateTrade = 1;
        public const int RealTrade = 0;

        /// <summary>
        /// 試撮
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsSimulating(this int obj)
        {
            return obj > RealTrade;
        }

        public static bool IsRealTrading(this int obj)
        {
            return obj == RealTrade;
        }

        /// <summary>
        /// 大於或等於
        /// </summary>
        public const string IsGreaterThanOrEqualTo = ">=";

        /// <summary>
        /// 大於
        /// </summary>
        public const string IsGreaterThan = ">";

        /// <summary>
        /// 等於
        /// </summary>
        public const string IsEqualTo = "=";

        /// <summary>
        /// 小於或等於
        /// </summary>
        public const string IsLessThanOrEqualTo = "<=";

        /// <summary>
        /// 小於
        /// </summary>
        public const string IsLessThan = "<";

        /// <summary>
        /// 觸價狀態，等待中
        /// </summary>
        public static (int, string) TriggerStatus0 = (0, "等待中");

        /// <summary>
        /// 觸價狀態，已取消
        /// </summary>
        public static (int, string) TriggerStatus1 = (1, "已取消");

        /// <summary>
        /// 觸價狀態，監控中
        /// </summary>
        public static (int, string) TriggerStatus2 = (2, "監控中");

        /// <summary>
        /// 觸價狀態，已觸發
        /// </summary>
        public static (int, string) TriggerStatus3 = (3, "已觸發");

        /// <summary>
        /// https://stackoverflow.com/questions/4680035/read-only-list-in-c-sharp
        /// </summary>
        public static ReadOnlyCollection<string> TriggerStatusKinds = new List<string>()
        {
            TriggerStatus0.Item2,
            TriggerStatus1.Item2,
            TriggerStatus2.Item2,
            TriggerStatus3.Item2,
        }.AsReadOnly();

        /// <summary>
        /// 觸價後取消監控，同帳號同代碼同欄位
        /// </summary>
        public static (int, string) TriggerCancel0 = (0, "同帳號同代碼同欄位");

        /// <summary>
        /// 觸價後取消監控，同帳號同代碼全欄位
        /// </summary>
        public static (int, string) TriggerCancel1 = (1, "同帳號同代碼全欄位");

        /// <summary>
        /// 觸價後取消監控，同帳號全代碼
        /// </summary>
        public static (int, string) TriggerCancel2 = (2, "同帳號全代碼");

        /// <summary>
        /// 觸價後取消監控，全帳號同代碼同欄位
        /// </summary>
        public static (int, string) TriggerCancel3 = (3, "全帳號同代碼同欄位");

        /// <summary>
        /// 觸價後取消監控，全帳號同代碼全欄位
        /// </summary>
        public static (int, string) TriggerCancel4 = (4, "全帳號同代碼全欄位");

        /// <summary>
        /// 觸價後取消監控，全帳號全代碼
        /// </summary>
        public static (int, string) TriggerCancel5 = (5, "全帳號全代碼");

        public static ReadOnlyCollection<string> TriggerCancelKinds = new List<string>()
        {
            TriggerCancel0.Item2,
            TriggerCancel1.Item2,
            TriggerCancel2.Item2,
            TriggerCancel3.Item2,
            TriggerCancel4.Item2,
            TriggerCancel5.Item2,
        }.AsReadOnly();
    }
}
