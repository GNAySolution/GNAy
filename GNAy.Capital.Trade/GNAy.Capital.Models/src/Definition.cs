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
        /// 觸價後取消監控，同代碼同欄位
        /// </summary>
        public static (int, string) TriggerCancel0 = (0, "同代碼同欄位");

        /// <summary>
        /// 觸價後取消監控，同代碼全欄位
        /// </summary>
        public static (int, string) TriggerCancel1 = (1, "同代碼全欄位");

        /// <summary>
        /// 觸價後取消監控，全代碼同欄位
        /// </summary>
        public static (int, string) TriggerCancel2 = (2, "全代碼同欄位");

        /// <summary>
        /// 觸價後取消監控，全代碼全欄位
        /// </summary>
        public static (int, string) TriggerCancel3 = (3, "全代碼全欄位");

        public static ReadOnlyCollection<string> TriggerCancelKinds = new List<string>()
        {
            TriggerCancel0.Item2,
            TriggerCancel1.Item2,
            TriggerCancel2.Item2,
            TriggerCancel3.Item2,
        }.AsReadOnly();
    }
}
