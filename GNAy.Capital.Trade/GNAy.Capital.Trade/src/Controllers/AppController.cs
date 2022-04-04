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
using System.Reflection;
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

        public readonly MainWindow MainForm;

        public readonly string ProcessName;
        public readonly int ProcessID;

        private readonly ObservableCollection<AppLogInDataGrid> _appLogCollection;

        public readonly AppConfig Config;
        public AppSettings Settings => Config.Settings;

        public CapitalController Capital { get; private set; }
        public TriggerController Trigger { get; private set; }

        private ObservableCollection<TradeColumnTrigger> _triggerColumnCollection;

        private readonly System.Timers.Timer _timer;

        public AppController(MainWindow mainForm)
        {
            CreatedTime = DateTime.Now;

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

            MainForm = mainForm;

            Process p = Process.GetCurrentProcess();
            ProcessName = p.ProcessName.Replace(".vshost", string.Empty);
            ProcessID = p.Id;

            mainForm.DataGridAppLog.SetHeadersByBindings(AppLogInDataGrid.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _appLogCollection = mainForm.DataGridAppLog.SetAndGetItemsSource<AppLogInDataGrid>();

            Config = LoadSettings();

            Version newVer = new Version(new AppSettings().Version);
            if (Config.Version < newVer)
            {
                LogError($"設定檔({Config.Archive.Name})版本過舊({Config.Version} < {newVer})");
                //TODO: Migrate old config to new version.
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Capital = null;
            Trigger = null;

            _triggerColumnCollection = null;

            _lastTimeToSaveQuote = DateTime.Now;

            _timer = new System.Timers.Timer(Settings.TimerIntervalBackground);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
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
                        MainForm.TabControlBA.SelectedIndex = 0;
                    }

                    _appLogCollection.Add(log);

                    while (_appLogCollection.Count > Settings.DataGridAppLogRowsMax)
                    {
                        _appLogCollection.RemoveAt(0);
                    }

                    if (!MainForm.DataGridAppLog.IsMouseOver)
                    {
                        MainForm.DataGridAppLog.ScrollToBorder();
                    }
                }
                catch
                { }
            });
        }

        public void LogTrace(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            _logger.Trace(string.Join("|", msg, lineNumber, memberName));
            AppendLog(LogLevel.Trace, msg, lineNumber, memberName);
        }

        public void LogDebug(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            _logger.Debug(string.Join("|", msg, lineNumber, memberName));
            AppendLog(LogLevel.Debug, msg, lineNumber, memberName);
        }

        public void LogInfo(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            _logger.Info(string.Join("|", msg, lineNumber, memberName));
            AppendLog(LogLevel.Info, msg, lineNumber, memberName);
        }

        public void LogWarn(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            _logger.Warn(string.Join("|", msg, lineNumber, memberName));
            AppendLog(LogLevel.Warn, msg, lineNumber, memberName);
        }

        public void LogError(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            _logger.Error(string.Join("|", msg, lineNumber, memberName));
            AppendLog(LogLevel.Error, msg, lineNumber, memberName);
        }

        public void LogException(Exception ex, string stackTrace, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string _msg = string.Join("|", ex.Message, ex.GetType().Name, $"{Environment.NewLine}{stackTrace}");
            _logger.Error(_msg);
            AppendLog(LogLevel.Error, _msg, lineNumber, memberName);
        }

        public void Log(int statusCode, string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            if (statusCode <= StatusCode.BaseTraceValue)
            {
                LogError(msg, lineNumber, memberName);
            }
            else if (statusCode % StatusCode.BaseTraceValue == 0)
            {
                LogError(msg, lineNumber, memberName);
            }
            else if (statusCode < StatusCode.BaseDebugValue)
            {
                LogTrace(msg, lineNumber, memberName);
            }
            else if (statusCode < StatusCode.BaseInfoValue)
            {
                LogDebug(msg, lineNumber, memberName);
            }
            else if (statusCode < StatusCode.BaseWarnValue)
            {
                LogInfo(msg, lineNumber, memberName);
            }
            else if (statusCode < StatusCode.BaseErrorValue)
            {
                LogWarn(msg, lineNumber, memberName);
            }
            else
            {
                LogError(msg, lineNumber, memberName);
            }
        }

        private void Log(LogLevel level, string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            if (level == null || level == LogLevel.Trace)
            {
                LogTrace(msg, lineNumber, memberName);
            }
            else if (level == LogLevel.Debug)
            {
                LogDebug(msg, lineNumber, memberName);
            }
            else if (level == LogLevel.Info)
            {
                LogInfo(msg, lineNumber, memberName);
            }
            else if (level == LogLevel.Warn)
            {
                LogWarn(msg, lineNumber, memberName);
            }
            else
            {
                LogError(msg, lineNumber, memberName);
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
            LogTrace("Start");

            int exitCode = lineNumber + StatusCode.WinError + StatusCode.BaseErrorValue;

            try
            {
                _timer.Enabled = false;

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

                Log(level, String.IsNullOrWhiteSpace(msg) ? $"exitCode={exitCode}" : $"exitCode={exitCode}|{msg}", lineNumber, memberName);

                if (Capital != null)
                {
                    if (!string.IsNullOrWhiteSpace(Settings.QuoteFileClosePrefix))
                    {
                        Capital.SaveQuotes(Config.QuoteFolder, false, Settings.QuoteFileClosePrefix);
                    }
                    Capital.Disconnect();
                }

                //TODO: Send info mail.

                Thread.Sleep(3 * 1000);
                Environment.Exit(exitCode);
            }
            catch (Exception ex)
            {
                LogException(ex, ex.StackTrace);

                Thread.Sleep(3 * 1000);
                Environment.Exit(exitCode);
            }
            finally
            {
                LogTrace("End");
            }
        }

        public bool InitialCapital()
        {
            LogTrace("Start");

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
                LogException(ex, ex.StackTrace);
            }
            finally
            {
                LogTrace("End");
            }

            return false;
        }

        public bool SetTriggerRule()
        {
            LogTrace("Start");

            try
            {
                if (_triggerColumnCollection == null || _triggerColumnCollection.Count <= 0)
                {
                    MainForm.ComboBoxTriggerProduct.ItemsSource = MainForm.DataGridQuoteSubscribed.ItemsSource;
                    _triggerColumnCollection = MainForm.ComboBoxTriggerColumn.SetAndGetItemsSource<TradeColumnTrigger>();

                    foreach ((TradeColumnAttribute, PropertyInfo) value in QuoteData.PropertyMap.Values)
                    {
                        if (value.Item1.IsTrigger)
                        {
                            _triggerColumnCollection.Add(new TradeColumnTrigger(value.Item1, value.Item2));
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogException(ex, ex.StackTrace);
            }
            finally
            {
                LogTrace("End");
            }

            return false;
        }
    }
}
