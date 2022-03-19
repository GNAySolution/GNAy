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

            ProcessName = Process.GetCurrentProcess().ProcessName;
            if (ProcessName.EndsWith(".vshost"))
            {
                ProcessName = ProcessName.Remove(ProcessName.Length - ".vshost".Length);
            }

            Config = LoadSettings();

            MainWindow.Instance.DataGridAppLog.SetHeadersByBindings(AppLogInDataGrid.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1.ShortName));
            AppLogCollection = MainWindow.Instance.DataGridAppLog.SetAndGetItemsSource<AppLogInDataGrid>();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void AppandLog(LogLevel level, string msg, int lineNumber, string memberName)
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
                    AppLogCollection.Add(log);

                    while (AppLogCollection.Count > Settings.DataGridAppLogRowsMax)
                    {
                        AppLogCollection.RemoveAt(0);
                    }

                    MainWindow.Instance.DataGridAppLog.ZxtScrollToEnd();
                }
                catch
                { }
            });
        }

        public void LogTrace(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            _logger.Trace(string.Join("|", msg, lineNumber, memberName));
            AppandLog(LogLevel.Trace, msg, lineNumber, memberName);
        }

        public void LogDebug(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            _logger.Debug(string.Join("|", msg, lineNumber, memberName));
            AppandLog(LogLevel.Debug, msg, lineNumber, memberName);
        }

        public void LogInfo(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            _logger.Info(string.Join("|", msg, lineNumber, memberName));
            AppandLog(LogLevel.Info, msg, lineNumber, memberName);
        }

        public void LogWarn(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            _logger.Warn(string.Join("|", msg, lineNumber, memberName));
            AppandLog(LogLevel.Warn, msg, lineNumber, memberName);
        }

        public void LogError(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            _logger.Error(string.Join("|", msg, lineNumber, memberName));
            AppandLog(LogLevel.Error, msg, lineNumber, memberName);
        }

        public void LogException(Exception ex, string stackTrace, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string _msg = string.Join("|", ex.Message, ex.GetType().Name, $"{Environment.NewLine}{stackTrace}");
            _logger.Error(_msg);
            AppandLog(LogLevel.Error, _msg, lineNumber, memberName);
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
                if (!string.IsNullOrWhiteSpace(arg) && arg.StartsWith("-AppSettings="))
                {
                    string sub = arg.Substring("-AppSettings=".Length).Trim();
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

        /// <summary>
        /// https://docs.microsoft.com/zh-tw/windows/win32/debug/system-error-codes
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="level"></param>
        /// <param name="lineNumber"></param>
        /// <param name="memberName"></param>
        public void Exit(string msg = "", LogLevel level = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            LogTrace("Start");

            int exitCode = lineNumber < 16000 ? 16000 + lineNumber : lineNumber;

            try
            {
                if (MainWindow.CapitalCtrl != null)
                {
                    MainWindow.CapitalCtrl.Disconnect();
                }

                Log(level, String.IsNullOrWhiteSpace(msg) ? $"exitCode={exitCode}" : $"{msg}|exitCode={exitCode}", lineNumber, memberName);

                //TODO: Send info mail.

                Thread.Sleep(3 * 1000);
                Environment.Exit(exitCode);
            }
            catch (Exception ex)
            {
                LogException(ex, ex.StackTrace);

                Thread.Sleep(3 * 1000);
                Environment.Exit(16000 + 1);
            }
            finally
            {
                LogTrace("End");
            }
        }
    }
}
