using System;
using System.Collections.Generic;
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
        /// 等於
        /// </summary>
        public const string IsEqualTo = "=";

        /// <summary>
        /// 小於或等於
        /// </summary>
        public const string IsLessThanOrEqualTo = "<=";

        /// <summary>
        /// 觸價後取消監控，取消自己
        /// </summary>
        public static (int, string) TriggerCancel0 = (0, "取消自己");

        /// <summary>
        /// 觸價後取消監控，取消同帳號同代碼
        /// </summary>
        public static (int, string) TriggerCancel1 = (1, "取消同帳號同代碼");

        /// <summary>
        /// 觸價後取消監控，取消同帳號全代碼
        /// </summary>
        public static (int, string) TriggerCancel2 = (2, "取消同帳號全代碼");

        /// <summary>
        /// 觸價後取消監控，取消不同帳號同代碼
        /// </summary>
        public static (int, string) TriggerCancel3 = (3, "取消不同帳號同代碼");

        /// <summary>
        /// 觸價後取消監控，取消全帳號全代碼
        /// </summary>
        public static (int, string) TriggerCancel4 = (4, "取消全帳號全代碼");
    }
}
