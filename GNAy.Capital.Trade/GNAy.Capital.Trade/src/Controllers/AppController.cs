using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class AppController
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        public readonly MainWindow MainForm;

        public readonly string ProcessName;
        public readonly int ProcessID;

        private readonly ObservableCollection<AppLogInDataGrid> _appLogCollection;

        public readonly AppConfig Config;
        public AppSettings Settings => Config.Settings;

        public CapitalCenterController CAPCenter { get; private set; }
        public CapitalQuoteController CAPQuote { get; private set; }
        public TriggerController Trigger { get; private set; }
        public CapitalOrderController CAPOrder { get; private set; }
        public OpenInterestController OpenInterest { get; private set; }
        public OrderDetailController OrderDetail { get; private set; }
        public StrategyController Strategy { get; private set; }

        private readonly System.Timers.Timer _timerBG;

        public AppController(MainWindow mainForm)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(AppController).Replace("Controller", "Ctrl");
            MainForm = mainForm;

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

            Process ps = Process.GetCurrentProcess();
            ProcessName = ps.ProcessName.Replace(".vshost", string.Empty);
            ProcessID = ps.Id;

            mainForm.DataGridAppLog.SetHeadersByBindings(AppLogInDataGrid.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _appLogCollection = mainForm.DataGridAppLog.SetAndGetItemsSource<AppLogInDataGrid>();

            Config = LoadSettings();

            AppSettings newSetting = new AppSettings();
            Version newVer = new Version(newSetting.Version);
            if (Config.Version < newVer)
            {
                LogError($"設定檔({Config.Archive.Name})版本過舊({Config.Version} < {newVer})", UniqueName);
                //TODO: Migrate old config to new version.
            }
            if (Config.TriggerFolder != null && string.IsNullOrWhiteSpace(Settings.TriggerFileFormat))
            {
                Settings.TriggerFileFormat = newSetting.TriggerFileFormat;
            }

            if (!Debugger.IsAttached)
            {
                ps.PriorityClass = (ProcessPriorityClass)Settings.ProcessPriority;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            CAPCenter = null;
            CAPQuote = null;
            Trigger = null;
            CAPOrder = null;
            OrderDetail = null;
            Strategy = null;
            OpenInterest = null;

            SignalTimeBG = DateTime.MinValue;

            _secondsToQueryOpenInterest = 10;
            _lastTimeToSaveQuote = DateTime.Now;

            _timerBG = new System.Timers.Timer(Settings.TimerIntervalBackground);
            _timerBG.Elapsed += OnTimedEvent;
            _timerBG.AutoReset = true;
            _timerBG.Enabled = true;
        }

        protected AppController() : this(null)
        { }

        private void AppendLog(LogLevel level, string msg, int lineNumber, string memberName)
        {
            AppLogInDataGrid log = new AppLogInDataGrid()
            {
                Project = ProcessName,
                Level = level.Name.ToUpper(),
                ThreadID = Thread.CurrentThread.ManagedThreadId,
                Message = msg,
                CallerLineNumber = lineNumber,
                CallerMemberName = memberName,
            };

            MainForm.InvokeAsync(delegate
            {
                try
                {
                    if (level == LogLevel.Warn || level == LogLevel.Error)
                    {
                        MainForm.TabControlCA.SelectedIndex = 0;
                    }

                    _appLogCollection.Add(log);

                    while (_appLogCollection.Count > Settings.DataGridAppLogRowsMax)
                    {
                        _appLogCollection.RemoveAt(0);
                    }

                    if (!MainForm.DataGridAppLog.IsMouseOver)
                    {
                        MainForm.DataGridAppLog.ScrollToBorderEnd();
                    }
                }
                catch
                { }
            });
        }

        public void LogTrace(string msg, string uniqueName, TimeSpan? elapsed = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            if (elapsed.HasValue)
            {
                msg = string.Format("ts={0}{1}{2}", elapsed.Value.ToString("ss'.'ffffff"), string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", msg);
            }
            memberName = $"{uniqueName}.{memberName}";
            _logger.Trace(string.Join("|", msg, lineNumber, memberName));
            AppendLog(LogLevel.Trace, msg, lineNumber, memberName);
        }

        public void LogTrace(DateTime startTime, string msg, string uniqueName, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            LogTrace(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public DateTime StartTrace()
        {
            return DateTime.Now;
        }

        public DateTime StartTrace(string msg, string uniqueName, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            DateTime now = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(msg))
            {
                LogTrace(msg, uniqueName, null, lineNumber, memberName);
            }
            return now;
        }

        public void EndTrace(DateTime startTime, string uniqueName, string msg = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            LogTrace(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void LogDebug(string msg, string uniqueName, TimeSpan? elapsed = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            if (elapsed.HasValue)
            {
                msg = string.Format("ts={0}{1}{2}", elapsed.Value.ToString("ss'.'ffffff"), string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", msg);
            }
            memberName = $"{uniqueName}.{memberName}";
            _logger.Debug(string.Join("|", msg, lineNumber, memberName));
            AppendLog(LogLevel.Debug, msg, lineNumber, memberName);
        }

        public void LogDebug(DateTime startTime, string msg, string uniqueName, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            LogDebug(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void LogInfo(string msg, string uniqueName, TimeSpan? elapsed = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            if (elapsed.HasValue)
            {
                msg = string.Format("ts={0}{1}{2}", elapsed.Value.ToString("ss'.'ffffff"), string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", msg);
            }
            memberName = $"{uniqueName}.{memberName}";
            _logger.Info(string.Join("|", msg, lineNumber, memberName));
            AppendLog(LogLevel.Info, msg, lineNumber, memberName);
        }

        public void LogInfo(DateTime startTime, string msg, string uniqueName, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            LogInfo(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void LogWarn(string msg, string uniqueName, TimeSpan? elapsed = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            if (elapsed.HasValue)
            {
                msg = string.Format("ts={0}{1}{2}", elapsed.Value.ToString("ss'.'ffffff"), string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", msg);
            }
            memberName = $"{uniqueName}.{memberName}";
            _logger.Warn(string.Join("|", msg, lineNumber, memberName));
            AppendLog(LogLevel.Warn, msg, lineNumber, memberName);
        }

        public void LogWarn(DateTime startTime, string msg, string uniqueName, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            LogWarn(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void LogError(string msg, string uniqueName, TimeSpan? elapsed = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            if (elapsed.HasValue)
            {
                msg = string.Format("ts={0}{1}{2}", elapsed.Value.ToString("ss'.'ffffff"), string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", msg);
            }
            memberName = $"{uniqueName}.{memberName}";
            _logger.Error(string.Join("|", msg, lineNumber, memberName));
            AppendLog(LogLevel.Error, msg, lineNumber, memberName);
        }

        public void LogError(DateTime startTime, string msg, string uniqueName, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            LogError(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void LogException(Exception ex, string stackTrace, TimeSpan? elapsed = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string msg = string.Join("|", ex.Message, ex.GetType().Name, $"{Environment.NewLine}{stackTrace}");
            if (elapsed.HasValue)
            {
                msg = string.Format("ts={0}|{1}", elapsed.Value.ToString("ss'.'ffffff"), msg);
            }
            _logger.Error(msg);
            AppendLog(LogLevel.Error, msg, lineNumber, memberName);
        }

        public void LogException(DateTime startTime, Exception ex, string stackTrace, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            LogException(ex, stackTrace, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void Log(int statusCode, string msg, string uniqueName, TimeSpan? elapsed = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            if (statusCode <= StatusCode.BaseTraceValue)
            {
                LogError(msg, uniqueName, elapsed, lineNumber, memberName);
            }
            else if (statusCode % StatusCode.BaseTraceValue == 0)
            {
                LogError(msg, uniqueName, elapsed, lineNumber, memberName);
            }
            else if (statusCode < StatusCode.BaseDebugValue)
            {
                LogTrace(msg, uniqueName, elapsed, lineNumber, memberName);
            }
            else if (statusCode < StatusCode.BaseInfoValue)
            {
                LogDebug(msg, uniqueName, elapsed, lineNumber, memberName);
            }
            else if (statusCode < StatusCode.BaseWarnValue)
            {
                LogInfo(msg, uniqueName, elapsed, lineNumber, memberName);
            }
            else if (statusCode < StatusCode.BaseErrorValue)
            {
                LogWarn(msg, uniqueName, elapsed, lineNumber, memberName);
            }
            else
            {
                LogError(msg, uniqueName, elapsed, lineNumber, memberName);
            }
        }

        public void Log(LogLevel level, string msg, string uniqueName, TimeSpan? elapsed = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            if (level == null || level == LogLevel.Trace)
            {
                LogTrace(msg, uniqueName, elapsed, lineNumber, memberName);
            }
            else if (level == LogLevel.Debug)
            {
                LogDebug(msg, uniqueName, elapsed, lineNumber, memberName);
            }
            else if (level == LogLevel.Info)
            {
                LogInfo(msg, uniqueName, elapsed, lineNumber, memberName);
            }
            else if (level == LogLevel.Warn)
            {
                LogWarn(msg, uniqueName, elapsed, lineNumber, memberName);
            }
            else
            {
                LogError(msg, uniqueName, elapsed, lineNumber, memberName);
            }
        }

        private AppConfig LoadSettings()
        {
            FileInfo configFile = new FileInfo($"{ProcessName}.appsettings.json");
            AppConfig config = null;

            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (!string.IsNullOrWhiteSpace(arg) && arg.StartsWith("-AppSettings=", StringComparison.OrdinalIgnoreCase))
                {
                    string sub = arg.Substring("-AppSettings=".Length);
                    if (File.Exists(sub))
                    {
                        configFile = new FileInfo(sub);
                    }
                    break;
                }
            }

            if (!configFile.Exists)
            {
                using (StreamWriter sw = new StreamWriter(configFile.FullName, false, TextEncoding.UTF8WithoutBOM))
                {
                    sw.Write(JsonConvert.SerializeObject(new AppSettings(), Formatting.Indented));
                }
                //configFile.Refresh();
            }

            using (StreamReader sr = new StreamReader(configFile.FullName, TextEncoding.UTF8WithoutBOM))
            {
                config = new AppConfig(JsonConvert.DeserializeObject<AppSettings>(sr.ReadToEnd()), configFile);
            }

            return config;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException(ex, ex.StackTrace);
            }
        }

        /// <summary>
        /// https://dotblogs.com.tw/sean_liao/2018/01/09/taskexceptionshandling
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();

            if (e.Exception != null)
            {
                LogException(e.Exception, e.Exception.StackTrace);

                if (e.Exception.InnerExceptions != null)
                {
                    foreach (Exception ex in e.Exception.InnerExceptions)
                    {
                        LogException(ex, ex.StackTrace);
                    }
                }
            }
        }

        public void Exit(string msg = "", LogLevel level = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            DateTime start = StartTrace(msg, UniqueName);
            int exitCode = lineNumber + StatusCode.WinError + StatusCode.BaseErrorValue;

            try
            {
                _timerBG.Enabled = false;

                if (level == null || level == LogLevel.Trace)
                {
                    exitCode = lineNumber + StatusCode.WinError + StatusCode.BaseTraceValue;
                }
                else if (level == LogLevel.Debug)
                {
                    exitCode = lineNumber + StatusCode.WinError + StatusCode.BaseDebugValue;
                }
                else if (level == LogLevel.Info)
                {
                    exitCode = lineNumber + StatusCode.WinError + StatusCode.BaseInfoValue;
                }
                else if (level == LogLevel.Warn)
                {
                    exitCode = lineNumber + StatusCode.WinError + StatusCode.BaseWarnValue;
                }

                Log(level, string.IsNullOrWhiteSpace(msg) ? $"exitCode={exitCode}" : $"exitCode={exitCode}|{msg}", UniqueName, null, lineNumber, memberName);

                if (CAPQuote != null)
                {
                    if (!string.IsNullOrWhiteSpace(Settings.QuoteFileClosePrefix))
                    {
                        CAPQuote.SaveData(Config.QuoteFolder, false, Settings.QuoteFileClosePrefix);
                    }
                    CAPQuote.Disconnect();
                }

                //TODO: Send info mail.

                EndTrace(start, UniqueName);

                Thread.Sleep(3 * 1000);
                Environment.Exit(exitCode);
            }
            catch (Exception ex)
            {
                LogException(start, ex, ex.StackTrace);

                Thread.Sleep(3 * 1000);
                Environment.Exit(exitCode);
            }
        }

        public bool InitialCapital()
        {
            DateTime start = StartTrace();

            try
            {
                if (CAPCenter == null)
                {
                    CAPCenter = new CapitalCenterController(this);
                    CAPQuote = new CapitalQuoteController(this);

                    Trigger = new TriggerController(this);
                    MainForm.TextBoxTriggerPrimaryKey.Text = $"{Trigger.Count + 1}";

                    CAPOrder = new CapitalOrderController(this);
                    OpenInterest = new OpenInterestController(this);
                    OrderDetail = new OrderDetailController(this);

                    Strategy = new StrategyController(this);
                    MainForm.TextBoxStrategyPrimaryKey.Text = $"{Strategy.Count + 1}";

                    MainForm.DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.OrderReport)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.DealPrice)].Item1.WPFDisplayIndex].Header = OpenInterestData.PropertyMap[nameof(OpenInterestData.AveragePrice)].Item1.WPFName;
                    MainForm.DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.DealQty)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.DealReport)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;

                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopLossBefore)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopLossAfterStr)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopWinBefore)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopWinAfter)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.MoveStopWinBefore)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.MoveStopWinAfter)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.OpenTriggerAfterStopLoss)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.OpenStrategyAfterStopLoss)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.OpenTriggerAfterStopWin)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.OpenStrategyAfterStopWin)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.CloseTriggerAfterStopWin)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.CloseStrategyAfterStopWin)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.WinCloseQty)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.WinCloseTime)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.LossCloseQty)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.LossCloseTime)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                EndTrace(start, UniqueName);
            }

            return false;
        }
    }
}
