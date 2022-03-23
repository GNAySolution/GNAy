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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class MainWindowController
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public readonly DateTime CreatedTime;
        public readonly string ProcessName;

        public readonly AppConfig Config;
        public AppSettings Settings => Config.Settings;

        private readonly ObservableCollection<AppLogInDataGrid> AppLogCollection;

        public MainWindowController()
        {
            CreatedTime = DateTime.Now;

            ProcessName = Process.GetCurrentProcess().ProcessName.Replace(".vshost", string.Empty);
            Config = LoadSettings();

            MainWindow.Instance.DataGridAppLog.SetHeadersByBindings(AppLogInDataGrid.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            AppLogCollection = MainWindow.Instance.DataGridAppLog.SetAndGetItemsSource<AppLogInDataGrid>();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

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

            MainWindow.Instance.InvokeRequired(delegate
            {
                try
                {
                    if (level == LogLevel.Warn || level == LogLevel.Error)
                    {
                        MainWindow.Instance.TabControlBA.SelectedIndex = 0;
                    }

                    AppLogCollection.Add(log);

                    while (AppLogCollection.Count > Settings.DataGridAppLogRowsMax)
                    {
                        AppLogCollection.RemoveAt(0);
                    }

                    if (!MainWindow.Instance.DataGridAppLog.IsMouseOver)
                    {
                        MainWindow.Instance.DataGridAppLog.ScrollToBorder();
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

            //AppConfig rawConfig = new AppConfig();
            //if (rawConfig.Version > config.Version)
            //{
            //    //TODO: Migrate old config to new version.
            //}

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

                Log(level, String.IsNullOrWhiteSpace(msg) ? $"exitCode={exitCode}" : $"{msg}|exitCode={exitCode}", lineNumber, memberName);

                if (MainWindow.CapitalCtrl != null)
                {
                    MainWindow.CapitalCtrl.Disconnect();
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
    }
}
