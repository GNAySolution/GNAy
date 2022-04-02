using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class AppController
    {
        private DateTime _lastTimeToSaveQuote;

        /// <summary>
        /// https://docs.microsoft.com/zh-tw/dotnet/api/system.timers.timer?view=net-6.0
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            //Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
            //                  e.SignalTime);

            _timer.Stop();

            DateTime now = DateTime.Now;

            try
            {
                if (Settings.QuoteSaveInterval > 0 && (now - _lastTimeToSaveQuote).TotalSeconds >= Settings.QuoteSaveInterval && Capital != null && !string.IsNullOrWhiteSpace(Settings.QuoteFileClosePrefix))
                {
                    _lastTimeToSaveQuote = now;
                    Capital.SaveQuotes(Config.QuoteFolder, false, Settings.QuoteFileClosePrefix);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, ex.StackTrace);
            }

            try
            {
                Trigger?.UpdateStatus();
            }
            catch (Exception ex)
            {
                LogException(ex, ex.StackTrace);
            }

            _timer.Start();
        }
    }
}
