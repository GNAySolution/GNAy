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

        private readonly ObservableCollection<AppLog> AppLogCollection;

        public MainWindowController()
        {
            CreatedTime = DateTime.Now;

            ProcessName = Process.GetCurrentProcess().ProcessName;
            if (ProcessName.EndsWith(".vshost"))
            {
                ProcessName = ProcessName.Remove(ProcessName.Length - ".vshost".Length);
            }

            Config = LoadSettings();

            MainWindow.Current.DataGridAppLog.SetHeadersByBindings(AppLog.PropertyDescriptionMap);
            AppLogCollection = MainWindow.Current.DataGridAppLog.SetItemsSource(new ObservableCollection<AppLog>());

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void AppandLog(string level, string msg)
        {
            int threadID = Thread.CurrentThread.ManagedThreadId;

            MainWindow.Current.InvokeRequired(delegate
            {
                try
                {
                    AppLogCollection.Add(new AppLog()
                    {
                        Level = level,
                        Message = $"{threadID}|{msg}",
                    });

                    while (AppLogCollection.Count > Settings.DataGridAppLogRowsMax)
                    {
                        AppLogCollection.RemoveAt(0);
                    }

                    MainWindow.Current.DataGridAppLog.ZxtScrollToEnd();
                }
                catch
                { }
            });
        }

        public void LogTrace(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string _msg = string.Join("|", msg, lineNumber, memberName);
            _logger.Trace(_msg);
            AppandLog("TRACE", _msg);
        }

        public void LogDebug(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string _msg = string.Join("|", msg, lineNumber, memberName);
            _logger.Debug(_msg);
            AppandLog("DEBUG", _msg);
        }

        public void LogInfo(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string _msg = string.Join("|", msg, lineNumber, memberName);
            _logger.Info(_msg);
            AppandLog("INFO", _msg);
        }

        public void LogWarn(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string _msg = string.Join("|", msg, lineNumber, memberName);
            _logger.Warn(_msg);
            AppandLog("WARN", _msg);
        }

        public void LogError(string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            string _msg = string.Join("|", msg, lineNumber, memberName);
            _logger.Error(_msg);
            AppandLog("ERROR", _msg);
        }

        public void LogException(Exception ex, string stackTrace)
        {
            string _msg = string.Join("|", ex.Message, ex.GetType().Name, $"{Environment.NewLine}{stackTrace}");
            _logger.Error(_msg);
            AppandLog("ERROR", _msg);
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
                configFile.Refresh();
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
        /// <param name="lineNumber"></param>
        /// <param name="memberName"></param>
        public void Exit([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            int exitCode = lineNumber < 16000 ? 16000 + lineNumber : lineNumber;

            LogTrace(exitCode.ToString(), lineNumber, memberName);
            Thread.Sleep(1 * 1000);
            Environment.Exit(exitCode);
        }
    }
}
