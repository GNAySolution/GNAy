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

        private int _secondsToQueryOpenInterest;
        private DateTime _lastTimeToSaveQuote;

        /// <summary>
        /// https://docs.microsoft.com/zh-tw/dotnet/api/system.timers.timer?view=net-6.0
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _timerBG.Stop();
            SignalTimeBG = e.SignalTime;

            try
            {
                if (OpenInterest != null && (e.SignalTime - OpenInterest.QuerySent.Item1).TotalSeconds >= _secondsToQueryOpenInterest)
                {
                    OpenInterest.SendNextQuery(e.SignalTime);

                    if (Capital.OrderAccCount > 0 && OpenInterest.QuerySent.Item4 != 0)
                    {
                        ++_secondsToQueryOpenInterest;
                        LogTrace(e.SignalTime, $"_secondsToQueryOpenInterest={_secondsToQueryOpenInterest}", UniqueName);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(e.SignalTime, ex, ex.StackTrace);
            }

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
