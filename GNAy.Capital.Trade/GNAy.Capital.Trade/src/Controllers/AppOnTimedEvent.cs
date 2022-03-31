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

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            //Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
            //                  e.SignalTime);

            _bgTimer.Stop();

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
            finally
            {
                _bgTimer.Start();
            }
        }
    }
}
