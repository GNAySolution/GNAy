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

        public DateTime IsExiting { get; private set; }

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
        public FuturesRightsController FuturesRights { get; private set; }

        private readonly Task _timerBG;
        public bool CallTimedEventFromBG => _timerBG != null;
        public TaskStatus TimerBGStatus => _timerBG == null ? TaskStatus.Canceled : _timerBG.Status;
        public DateTime SignalTimeBG { get; private set; }

        public AppController(in MainWindow mainForm, in Process ps)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(AppController).Replace("Controller", "Ctrl");
            MainForm = mainForm;

            IsExiting = DateTime.MinValue;

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

            mainForm.DataGridAppLog.SetColumns(AppLogInDataGrid.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _appLogCollection = mainForm.DataGridAppLog.SetViewAndGetObservation<AppLogInDataGrid>();

            Config = LoadSettings();

            AppSettings newSetting = new AppSettings();
            Version newVer = new Version(newSetting.Version);

            if (Config.Version < newVer)
            {
                LogWarn($"設定檔({Config.Archive.Name})版本過舊({Config.Version} < {newVer})", UniqueName);
                //TODO: Migrate old config to new version.
            }

            ps.PriorityClass = Settings.ProcessPriority.ConvertTo<ProcessPriorityClass>();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            CAPCenter = null;
            CAPQuote = null;
            Trigger = null;
            CAPOrder = null;
            OpenInterest = null;
            OrderDetail = null;
            Strategy = null;
            FuturesRights = null;

            SignalTimeBG = DateTime.MinValue;

            _openInterestInterval = Settings.OpenInterestInterval;
            _futuresRightsInterval = Settings.FuturesRightsInterval;

            _lastTimeToSaveQuote = DateTime.Now;

            if (Settings.TimerIntervalBackground > 0)
            {
                _timerBG = Task.Factory.StartNew(() =>
                {
                    SpinWait.SpinUntil(() =>
                    {
                        Thread.Sleep(Settings.TimerIntervalBackground);

                        if (IsExiting != DateTime.MinValue)
                        {
                            return LoopResult.Break;
                        }
                        else if (CAPCenter == null)
                        {
                            return LoopResult.Continue;
                        }

                        SignalTimeBG = DateTime.Now;

                        OnTimedEvent(SignalTimeBG);

                        return LoopResult.Continue;
                    });
                });
            }
            else
            {
                _timerBG = null;
                Settings.TimerIntervalBackground = Settings.TimerIntervalUI1;
            }
        }

        protected AppController() : this(null, null)
        { }

        private void AppendLog(LogLevel level, in string msg, in int lineNumber, in string memberName)
        {
            AppLogInDataGrid log = new AppLogInDataGrid()
            {
                Project = MainForm.ProcessName,
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

        public void LogTrace(string msg, in string uniqueName, in TimeSpan? elapsed = null, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            if (elapsed.HasValue)
            {
                msg = string.Format("{0}={1}{2}{3}", nameof(elapsed), elapsed.Value.ToString("ss'.'ffffff"), string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", msg);
            }

            _logger.Trace(string.Join("|", msg, lineNumber, $"{uniqueName}.{memberName}"));
            AppendLog(LogLevel.Trace, msg, lineNumber, $"{uniqueName}.{memberName}");
        }

        public void LogTrace(in DateTime startTime, in string msg, in string uniqueName, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            LogTrace(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public DateTime StartTrace()
        {
            return DateTime.Now;
        }

        public DateTime StartTrace(in string msg, in string uniqueName, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            DateTime now = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(msg))
            {
                LogTrace(msg, uniqueName, null, lineNumber, memberName);
            }

            return now;
        }

        public void EndTrace(in DateTime startTime, in string uniqueName, in string msg = "", [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            LogTrace(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void LogDebug(string msg, in string uniqueName, in TimeSpan? elapsed = null, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            if (elapsed.HasValue)
            {
                msg = string.Format("{0}={1}{2}{3}", nameof(elapsed), elapsed.Value.ToString("ss'.'ffffff"), string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", msg);
            }

            _logger.Debug(string.Join("|", msg, lineNumber, $"{uniqueName}.{memberName}"));
            AppendLog(LogLevel.Debug, msg, lineNumber, $"{uniqueName}.{memberName}");
        }

        public void LogDebug(in DateTime startTime, in string msg, in string uniqueName, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            LogDebug(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void LogInfo(string msg, in string uniqueName, in TimeSpan? elapsed = null, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            if (elapsed.HasValue)
            {
                msg = string.Format("{0}={1}{2}{3}", nameof(elapsed), elapsed.Value.ToString("ss'.'ffffff"), string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", msg);
            }

            _logger.Info(string.Join("|", msg, lineNumber, $"{uniqueName}.{memberName}"));
            AppendLog(LogLevel.Info, msg, lineNumber, $"{uniqueName}.{memberName}");
        }

        public void LogInfo(in DateTime startTime, in string msg, in string uniqueName, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            LogInfo(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void LogWarn(string msg, in string uniqueName, in TimeSpan? elapsed = null, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            if (elapsed.HasValue)
            {
                msg = string.Format("{0}={1}{2}{3}", nameof(elapsed), elapsed.Value.ToString("ss'.'ffffff"), string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", msg);
            }

            _logger.Warn(string.Join("|", msg, lineNumber, $"{uniqueName}.{memberName}"));
            AppendLog(LogLevel.Warn, msg, lineNumber, $"{uniqueName}.{memberName}");
        }

        public void LogWarn(in DateTime startTime, in string msg, in string uniqueName, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            LogWarn(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void LogError(string msg, in string uniqueName, in TimeSpan? elapsed = null, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            if (elapsed.HasValue)
            {
                msg = string.Format("{0}={1}{2}{3}", nameof(elapsed), elapsed.Value.ToString("ss'.'ffffff"), string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", msg);
            }

            _logger.Error(string.Join("|", msg, lineNumber, $"{uniqueName}.{memberName}"));
            AppendLog(LogLevel.Error, msg, lineNumber, $"{uniqueName}.{memberName}");
        }

        public void LogError(in DateTime startTime, in string msg, in string uniqueName, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            LogError(msg, uniqueName, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void LogException(in Exception ex, in string stackTrace, in TimeSpan? elapsed = null, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            string msg = string.Join("|", ex.Message, ex.GetType().Name, $"{Environment.NewLine}{stackTrace}");

            if (elapsed.HasValue)
            {
                msg = string.Format("{0}={1}|{2}", nameof(elapsed), elapsed.Value.ToString("ss'.'ffffff"), msg);
            }

            _logger.Error(msg);
            AppendLog(LogLevel.Error, msg, lineNumber, memberName);
        }

        public void LogException(in DateTime startTime, in Exception ex, in string stackTrace, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
        {
            LogException(ex, stackTrace, (DateTime.Now - startTime), lineNumber, memberName);
        }

        public void Log(in int statusCode, in string msg, in string uniqueName, in TimeSpan? elapsed = null, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
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

        public void Log(in LogLevel level, in string msg, in string uniqueName, in TimeSpan? elapsed = null, [CallerLineNumber] in int lineNumber = 0, [CallerMemberName] in string memberName = "")
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
            FileInfo configFile = new FileInfo($"{MainForm.ProcessName}.appsettings.json");
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

        public void ExitAsync(string msg = "", LogLevel level = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            DateTime start = StartTrace(msg, UniqueName);
            int exitCode = lineNumber + StatusCode.WinError + StatusCode.BaseErrorValue;

            IsExiting = start;

            MainForm.ButtonScreenshot_Click($"{start:yyMMdd_HHmmss}", null);

            Task.Factory.StartNew(() =>
            {
                try
                {
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

                    Log(level, string.IsNullOrWhiteSpace(msg) ? $"{nameof(exitCode)}={exitCode}" : $"{nameof(exitCode)}={exitCode}|{msg}", UniqueName, null, lineNumber, memberName);

                    if (CAPQuote != null)
                    {
                        if (!string.IsNullOrWhiteSpace(Settings.QuoteFileClosePrefix))
                        {
                            CAPQuote.SaveData(Config.QuoteFolder, false, Settings.QuoteFileClosePrefix);
                        }

                        CAPQuote.Disconnect();
                    }

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
            });
        }

        public void SelfTest()
        {
            DateTime start = StartTrace();

            try
            {
                LogTrace(start, $"{nameof(AppSettings.ProcessPriority)}={Settings.ProcessPriority}|{Settings.ProcessPriority.ConvertTo<ProcessPriorityClass>()}", UniqueName);
                LogTrace(start, $"{ProcessPriorityClass.AboveNormal}|{ProcessPriorityClass.AboveNormal.ToString().ConvertTo<ProcessPriorityClass>()}|{((int)ProcessPriorityClass.AboveNormal).ToString().ConvertTo<ProcessPriorityClass>()}", UniqueName);
                LogTrace(start, $"{ProcessPriorityClass.BelowNormal}|{ProcessPriorityClass.BelowNormal.ToString().ConvertTo<ProcessPriorityClass>()}|{((int)ProcessPriorityClass.BelowNormal).ToString().ConvertTo<ProcessPriorityClass>()}", UniqueName);
                LogTrace(start, $"{"Sunday".ConvertTo<DayOfWeek>()}", UniqueName);
                LogTrace(start, $"{"monday".ConvertTo<DayOfWeek>()}", UniqueName);
                LogTrace(start, $"{"Tue".ConvertTo<DayOfWeek>()}", UniqueName);
                LogTrace(start, $"{"Wed.".ConvertTo<DayOfWeek>()}", UniqueName);
                LogTrace(start, $"{"tHURSDAY".ConvertTo<DayOfWeek>()}", UniqueName);
                LogTrace(start, $"{"f".ConvertTo<DayOfWeek>()}", UniqueName);
                LogTrace(start, $"{"6".ConvertTo<DayOfWeek>()}", UniqueName);
            }
            catch (Exception ex)
            {
                LogException(start, ex, ex.StackTrace);
            }

            try
            {
                LogTrace(start, $"{CreatedTime.AddDays(-3):MM/dd HH:mm}|{CreatedTime.AddDays(-3).DayOfWeek}|{nameof(AppConfig.IsHoliday)}={Config.IsHoliday(CreatedTime.AddDays(-3))}", UniqueName);
                LogTrace(start, $"{CreatedTime.AddDays(-2):MM/dd HH:mm}|{CreatedTime.AddDays(-2).DayOfWeek}|{nameof(AppConfig.IsHoliday)}={Config.IsHoliday(CreatedTime.AddDays(-2))}", UniqueName);
                LogTrace(start, $"{CreatedTime.AddDays(-1):MM/dd HH:mm}|{CreatedTime.AddDays(-1).DayOfWeek}|{nameof(AppConfig.IsHoliday)}={Config.IsHoliday(CreatedTime.AddDays(-1))}", UniqueName);
                LogTrace(start, $"{CreatedTime.AddDays(0):MM/dd HH:mm}|{CreatedTime.AddDays(+0).DayOfWeek}|{nameof(AppConfig.IsHoliday)}={Config.IsHoliday(CreatedTime.AddDays(0))}|Today", UniqueName);
                LogTrace(start, $"{CreatedTime.AddDays(1):MM/dd HH:mm}|{CreatedTime.AddDays(+1).DayOfWeek}|{nameof(AppConfig.IsHoliday)}={Config.IsHoliday(CreatedTime.AddDays(1))}", UniqueName);
                LogTrace(start, $"{CreatedTime.AddDays(2):MM/dd HH:mm}|{CreatedTime.AddDays(+2).DayOfWeek}|{nameof(AppConfig.IsHoliday)}={Config.IsHoliday(CreatedTime.AddDays(2))}", UniqueName);
                LogTrace(start, $"{CreatedTime.AddDays(3):MM/dd HH:mm}|{CreatedTime.AddDays(+3).DayOfWeek}|{nameof(AppConfig.IsHoliday)}={Config.IsHoliday(CreatedTime.AddDays(3))}", UniqueName);
            }
            catch (Exception ex)
            {
                LogException(start, ex, ex.StackTrace);
            }

            try
            {
                LogTrace(start, $"Limit|{OrderPrice.Parse("10050", 10100, 10000, 0, 0)}", UniqueName);
                LogTrace(start, $"M|{OrderPrice.Parse("M", 10100, 10000, 0, 0)}", UniqueName);
                LogTrace(start, $"P|{OrderPrice.Parse("P", 10100, 10000, 0, 0)}", UniqueName);
                LogTrace(start, $"M+50|{OrderPrice.Parse("M+50", 10100, 10000, 0, 0)}", UniqueName);
                LogTrace(start, $"M-50|{OrderPrice.Parse("M-50", 10100, 10000, 0, 0)}", UniqueName);
                LogTrace(start, $"P+0.5%|{OrderPrice.Parse("P+0.5%", 10100, 10000, 0, 0)}", UniqueName);
                LogTrace(start, $"P-0.5%|{OrderPrice.Parse("P-0.5%", 10100, 10000, 0, 0)}", UniqueName);
            }
            catch (Exception ex)
            {
                LogException(start, ex, ex.StackTrace);
            }

            try
            {
                LogTrace(start, $"{Config.GetDateToChangeFutures(CreatedTime.AddMonths(-1)):MM/dd (dddd)}", UniqueName);
                LogTrace(start, $"{Config.GetDateToChangeFutures(CreatedTime.AddMonths(+0)):MM/dd (dddd)}|This Month", UniqueName);
                LogTrace(start, $"{Config.GetDateToChangeFutures(CreatedTime.AddMonths(+1)):MM/dd (dddd)}", UniqueName);
                LogTrace(start, $"{Config.GetDateToChangeFutures(CreatedTime.AddMonths(+2)):MM/dd (dddd)}", UniqueName);
                LogTrace(start, $"{Config.GetDateToChangeFutures(CreatedTime.AddMonths(+3)):MM/dd (dddd)}", UniqueName);
            }
            catch (Exception ex)
            {
                LogException(start, ex, ex.StackTrace);
            }

            EndTrace(start, UniqueName);
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

                    FuturesRights = new FuturesRightsController(this);

                    MainForm.DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.OrderReport)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.DealPrice)].Item1.WPFDisplayIndex].Header = OpenInterestData.PropertyMap[nameof(OpenInterestData.AveragePrice)].Item1.WPFName;
                    MainForm.DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.DealQty)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.DealReport)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;

                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.BestClosePrice)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopLossBefore)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopLossAfter)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopWinPriceABefore)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopWinPriceAAfter)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopWin1Before)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopWin1After)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopWin2Before)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StopWin2After)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.ClosedProfitTotal)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
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
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.AccountsWinLossClose)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                    MainForm.DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.StartTimesMax)].Item1.WPFDisplayIndex].Visibility = System.Windows.Visibility.Collapsed;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogException(start, ex, ex.StackTrace);

                if (Config.AutoRun)
                {
                    Thread.Sleep(1 * 1000);
                    ExitAsync(ex.Message, LogLevel.Error);
                }
            }
            finally
            {
                EndTrace(start, UniqueName);
            }

            return false;
        }
    }
}
