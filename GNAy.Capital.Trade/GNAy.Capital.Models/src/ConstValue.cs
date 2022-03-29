using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public static class ConstValue
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
    }
}
