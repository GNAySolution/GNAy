using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class AppController
    {
        public DateTime SignalTimeBG { get; private set; }

        private DateTime _lastTimeToSaveQuote;

        /// <summary>
        /// https://docs.microsoft.com/zh-tw/dotnet/api/system.timers.timer?view=net-6.0
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            _timerBG.Stop();
            SignalTimeBG = e.SignalTime;

            try
            {
                Strategy?.UpdateStatus(e.SignalTime);
            }
            catch (Exception ex)
            {
                LogException(e.SignalTime, ex, ex.StackTrace);
            }

            try
            {
                Trigger?.UpdateStatus(e.SignalTime);
            }
            catch (Exception ex)
            {
                LogException(e.SignalTime, ex, ex.StackTrace);
            }

            try
            {
                if (Settings.QuoteSaveInterval > 0 && (e.SignalTime - _lastTimeToSaveQuote).TotalSeconds >= Settings.QuoteSaveInterval && Capital != null && !string.IsNullOrWhiteSpace(Settings.QuoteFileClosePrefix))
                {
                    _lastTimeToSaveQuote = e.SignalTime;
                    Capital.SaveQuotes(Config.QuoteFolder, false, Settings.QuoteFileClosePrefix);
                }
            }
            catch (Exception ex)
            {
                LogException(e.SignalTime, ex, ex.StackTrace);
            }

            _timerBG.Start();
        }
    }
}
