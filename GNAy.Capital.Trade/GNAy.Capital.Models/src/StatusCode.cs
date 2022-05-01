using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public class StatusCode
    {
        /// <summary>
        /// Precheck 失敗(EX:RCode)
        /// </summary>
        public const int SK_WARNING_PRECHECK_RESULT_FAIL = 2020;

        /// <summary>
        /// Precheck結果回傳空值
        /// </summary>
        public const int SK_WARNING_PRECHECK_RESULT_EMPTY = 2021;

        /// <summary>
        /// 斷線
        /// </summary>
        public const int SK_SUBJECT_CONNECTION_DISCONNECT = 3002;

        /// <summary>
        /// 報價商品載入完成
        /// </summary>
        public const int SK_SUBJECT_CONNECTION_STOCKS_READY = 3003;

        /// <summary>
        /// 連線失敗(網路異常等)
        /// </summary>
        public const int SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK = 3021;

        /// <summary>
        /// Solace底層連線錯誤
        /// </summary>
        public const int SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL = 3022;

        /// <summary>
        /// Solace Sessio down錯誤
        /// </summary>
        public const int SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR = 3033;

        /// <summary>
        /// https://docs.microsoft.com/zh-tw/windows/win32/debug/system-error-codes
        /// </summary>
        public const int WinError = 16000;

        public const int BaseTraceValue = 100000;
        public const int BaseDebugValue = 200000;
        public const int BaseInfoValue = 300000;
        public const int BaseWarnValue = 400000;
        public const int BaseErrorValue = 500000;
    }
}
