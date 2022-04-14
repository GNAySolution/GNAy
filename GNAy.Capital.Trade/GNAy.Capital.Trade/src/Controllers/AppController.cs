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

        public CapitalController Capital { get; private set; }
        public TriggerController Trigger { get; private set; }

        private ObservableCollection<TradeColumnTrigger> _triggerColumnCollection;

        private readonly System.Timers.Timer _timerBG;
        private readonly System.Timers.Timer _timerTrigger;

        public AppController(MainWindow mainForm)
        {
            CreatedTime = DateTime.Now;
            UniqueName = GetType().Name.Replace("Controller", "Ctrl");
            MainForm = mainForm;

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

            Process p = Process.GetCurrentProcess();
            ProcessName = p.ProcessName.Replace(".vshost", string.Empty);
            ProcessID = p.Id;

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

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Capital = null;
            Trigger = null;

            _triggerColumnCollection = null;

            _lastTimeToSaveQuote = DateTime.Now;

            _timerBG = new System.Timers.Timer(Settings.TimerIntervalBackground);
            _timerBG.Elapsed += OnTimedEvent;
            _timerBG.AutoReset = true;
            _timerBG.Enabled = true;

            _timerTrigger = new System.Timers.Timer(Settings.TimerIntervalTrigger);
            Task.Factory.StartNew(() =>
            {
                SpinWait.SpinUntil(() => Trigger != null);
                _timerTrigger.Elapsed += OnTimedTrigger;
                _timerTrigger.AutoReset = true;
                _timerTrigger.Enabled = true;
            });
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

            MainForm.InvokeRequired(delegate
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

        public DateTime StartTrace()
        {
            return DateTime.Now;
        }

        public DateTime StartTrace(string uniqueName, string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            DateTime now = DateTime.Now;
            LogTrace(msg, uniqueName, null, lineNumber, memberName);
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
            DateTime start = StartTrace();
            int exitCode = lineNumber + StatusCode.WinError + StatusCode.BaseErrorValue;

            try
            {
                _timerBG.Enabled = false;
                _timerTrigger.Enabled = false;

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

                if (Capital != null)
                {
                    if (!string.IsNullOrWhiteSpace(Settings.QuoteFileClosePrefix))
                    {
                        Capital.SaveQuotes(Config.QuoteFolder, false, Settings.QuoteFileClosePrefix);
                    }
                    Capital.Disconnect();
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
                if (Capital == null)
                {
                    Capital = new CapitalController(this);
                    Trigger = new TriggerController(this);
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

        public bool SetTriggerRule()
        {
            DateTime start = StartTrace();

            try
            {
                if (_triggerColumnCollection == null || _triggerColumnCollection.Count <= 0)
                {
                    MainForm.ComboBoxTriggerProduct.ItemsSource = MainForm.DataGridQuoteSubscribed.ItemsSource;
                    _triggerColumnCollection = MainForm.ComboBoxTriggerColumn.SetAndGetItemsSource<TradeColumnTrigger>();

                    foreach (TradeColumnTrigger column in TriggerData.QuoteColumnTriggerMap.Values)
                    {
                        _triggerColumnCollection.Add(column);
                    }
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
