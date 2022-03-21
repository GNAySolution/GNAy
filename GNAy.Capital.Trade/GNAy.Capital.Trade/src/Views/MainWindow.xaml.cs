using GNAy.Capital.Models;
using GNAy.Capital.Trade.Controllers;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GNAy.Capital.Trade
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public static MainWindowController AppCtrl => Instance.AppControl;
        public static CapitalController CapitalCtrl => Instance.CapitalControl;

        public readonly DateTime StartTime;

        private readonly MainWindowController AppControl;
        private CapitalController CapitalControl;

        private readonly DispatcherTimer _timer1;
        private readonly DispatcherTimer _timer2;

        public MainWindow()
        {
            InitializeComponent();

            StartTime = DateTime.Now;
            StatusBarItemAA1.Text = StartTime.ToString("MM/dd HH:mm");

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

            Instance = this;

            AppControl = new MainWindowController();
            StatusBarItemAB2.Text = $"Subscribed={AppControl.Config.QuoteSubscribed.Count}|Live={AppControl.Settings.QuoteLive.Count}";

            CapitalControl = null;

            //https://www.796t.com/post/MWV3bG0=.html
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            Title = $"{version.Comments} ({version.FileVersion})";
            if (version.FileMajorPart <= 0 || version.FilePrivatePart % 2 == 1)
            {
                Title = $"{Title}(BETA)";
            }
            Title = $"{Title} ({version.ProductName})({version.LegalCopyright}) ({AppCtrl.Settings.Description})";
            if (Debugger.IsAttached)
            {
                Title = $"{Title}(附加偵錯)";
            }

            _timer1 = new DispatcherTimer(DispatcherPriority.Send)
            {
                Interval = TimeSpan.FromMilliseconds(AppCtrl.Settings.TimerInterval1),
            };
            _timer1.Tick += Timer1_Tick;
            _timer2 = new DispatcherTimer(DispatcherPriority.Send)
            {
                Interval = TimeSpan.FromMilliseconds(AppCtrl.Settings.TimerInterval2),
            };
            _timer2.Tick += Timer2_Tick;

            AppCtrl.LogTrace(Title);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppCtrl.LogTrace("Start");
            _timer1.Stop();
            _timer2.Stop();
            AppCtrl.Exit();
            AppCtrl.LogTrace("End");
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_GotMouseCapture(object sender, MouseEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                AppCtrl.LogTrace($"{StartTime.AddDays(-3):MM/dd HH:mm}|{StartTime.AddDays(-3).DayOfWeek}|IsHoliday={AppCtrl.Config.IsHoliday(StartTime.AddDays(-3))}");
                AppCtrl.LogTrace($"{StartTime.AddDays(-2):MM/dd HH:mm}|{StartTime.AddDays(-2).DayOfWeek}|IsHoliday={AppCtrl.Config.IsHoliday(StartTime.AddDays(-2))}");
                AppCtrl.LogTrace($"{StartTime.AddDays(-1):MM/dd HH:mm}|{StartTime.AddDays(-1).DayOfWeek}|IsHoliday={AppCtrl.Config.IsHoliday(StartTime.AddDays(-1))}");
                AppCtrl.LogTrace($"{StartTime.AddDays(0):MM/dd HH:mm}|{StartTime.AddDays(+0).DayOfWeek}|IsHoliday={AppCtrl.Config.IsHoliday(StartTime.AddDays(0))}");
                AppCtrl.LogTrace($"{StartTime.AddDays(1):MM/dd HH:mm}|{StartTime.AddDays(+1).DayOfWeek}|IsHoliday={AppCtrl.Config.IsHoliday(StartTime.AddDays(1))}");
                AppCtrl.LogTrace($"{StartTime.AddDays(2):MM/dd HH:mm}|{StartTime.AddDays(+2).DayOfWeek}|IsHoliday={AppCtrl.Config.IsHoliday(StartTime.AddDays(2))}");
                AppCtrl.LogTrace($"{StartTime.AddDays(3):MM/dd HH:mm}|{StartTime.AddDays(+3).DayOfWeek}|IsHoliday={AppCtrl.Config.IsHoliday(StartTime.AddDays(3))}");

                AppCtrl.LogTrace($"{AppCtrl.Config.Archive.FullName}|Exists={AppCtrl.Config.Archive.Exists}");

                if (!AppCtrl.Config.Archive.Exists)
                {
                    //https://docs.microsoft.com/zh-tw/dotnet/desktop/wpf/windows/how-to-open-message-box?view=netdesktop-6.0

                    string caption = $"第一次產生設定檔{AppCtrl.Config.Archive.Name}";
                    string messageBoxText = $"請確認檔案內容\r\n{AppCtrl.Config.Archive.FullName}";

                    MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);

                    AppCtrl.Exit(caption, LogLevel.Warn);
                    return;
                }
                else if (AppCtrl.Settings.AutoRun)
                {
                    AppCtrl.LogTrace($"AutoRun={AppCtrl.Settings.AutoRun}");

                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(1 * 1000);
                        this.InvokeRequired(delegate { Timer2_Tick(null, null); });
                    });
                }

                _timer1.Start();

                if (!AppCtrl.Settings.AutoRun)
                {
                    _timer2.Start();
                }
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_LostMouseCapture(object sender, MouseEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                StatusBarItemAA3.Text = $"({Width},{Height})";
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                string duration = (now - StartTime).ToString(@"hh\:mm\:ss");

                StatusBarItemAA2.Text = $"{duration}";

                //if (Application.Current.MainWindow.IsMouseOver)
                if (IsMouseOver)
                {
                    //https://stackoverflow.com/questions/29822020/how-to-get-mouse-position-on-screen-in-wpf
                    //Point pt = Mouse.GetPosition(Application.Current.MainWindow);
                    Point pt = Mouse.GetPosition(this);
                    StatusBarItemAA4.Text = $"({pt.X},{pt.Y})";
                }

                if (TabControlBA.SelectedIndex == 0 && DataGridAppLog.ItemsSource != null)
                {
                    StatusBarItemBA1.Text = $"({DataGridAppLog.Columns.Count},{DataGridAppLog.Items.Count})";
                }
                else if (TabControlBA.SelectedIndex == 1 && DataGridAPIReply.ItemsSource != null)
                {
                    StatusBarItemBA1.Text = $"({DataGridAPIReply.Columns.Count},{DataGridAPIReply.Items.Count})";
                }

                if (CapitalCtrl != null)
                {
                    StatusBarItemAB4.Text = CapitalCtrl.QuoteStatusStr;
                    StatusBarItemBA3.Text = $"{CapitalCtrl.AccountTimer.Item1:mm:ss}|{CapitalCtrl.AccountTimer.Item2}";
                    StatusBarItemBA4.Text = $"{CapitalCtrl.QuoteTimer.Item1:mm:ss}|{CapitalCtrl.QuoteTimer.Item2}";
                }

                if (DataGridQuoteSubscribed.ItemsSource != null)
                {
                    StatusBarItemAB1.Text = $"({DataGridQuoteSubscribed.Columns.Count},{DataGridQuoteSubscribed.Items.Count})";
                }
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            _timer2.Stop();

            DateTime now = DateTime.Now;
            int reConnect = 0;

            try
            {
                string msg = $"{now:MM/dd HH:mm.ss}|IsHoliday={AppCtrl.Config.IsHoliday(now)}";
                AppCtrl.LogTrace(msg);
                StatusBarItemBA2.Text = msg;

                if (AppCtrl.Config.QuoteFolder != null && CapitalCtrl != null)
                {
                    CapitalCtrl.SaveQuotesAsync();
                }

                foreach (DateTime timeToExit in AppCtrl.Settings.TimeToExit)
                {
                    if (now.Hour == timeToExit.Hour && now.Minute >= timeToExit.Minute && now.Minute <= (timeToExit.Minute + 2))
                    {
                        _timer1.Stop();
                        AppCtrl.Exit($"Time to exit.");
                        break;
                    }
                }

                if (AppCtrl.Settings.AutoRun && CapitalCtrl == null)
                {
                    reConnect = 1 + StatusCode.BaseTraceValue;
                }
                //3002 SK_SUBJECT_CONNECTION_DISCONNECT 斷線
                //3021 SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK 連線失敗(網路異常等)
                //3022 SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL Solace底層連線錯誤
                //3033 SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR Solace Sessio down錯誤
                else if (CapitalCtrl != null && (CapitalCtrl.QuoteStatus == 3002 || CapitalCtrl.QuoteStatus == 3021 || CapitalCtrl.QuoteStatus == 3022 || CapitalCtrl.QuoteStatus == 3033))
                {
                    reConnect = CapitalCtrl.QuoteStatus + StatusCode.BaseErrorValue;
                }
                else if (CapitalCtrl != null && CapitalCtrl.QuoteStatus > StatusCode.BaseTraceValue)
                {
                    reConnect = CapitalCtrl.QuoteStatus;
                }

                if (reConnect == 0)
                {
                    _timer2.Start();
                    return;
                }

                AppCtrl.Log(reConnect, $"Retry to connect quote service.|reConnect={reConnect}");
                Task.Factory.StartNew(() =>
                {
                    if (CapitalCtrl != null)
                    {
                        Thread.Sleep(1 * 1000);
                        CapitalCtrl.Disconnect();
                    }

                    Thread.Sleep(3 * 1000);
                    this.InvokeRequired(delegate
                    {
                        ButtonLoginAccount_Click(null, null);
                        ButtonLoginQuote_Click(null, null);
                    });

                    Thread.Sleep(3 * 1000);
                    SpinWait.SpinUntil(() => CapitalCtrl.QuoteStatus == 3003, 2 * 60 * 1000); //3003 SK_SUBJECT_CONNECTION_STOCKS_READY 報價商品載入完成
                    if (CapitalCtrl.QuoteStatus != 3003) //Timeout
                    {
                        //TODO: Send alert mail.
                        CapitalCtrl.Disconnect();
                        this.InvokeRequired(delegate { _timer2.Start(); }); //Retry to connect quote service.
                        return;
                    }

                    Thread.Sleep(1 * 1000);
                    this.InvokeRequired(delegate
                    {
                        ButtonIsConnected_Click(null, null);
                        ButtonSubQuotes_Click(null, null);
                    });

                    Thread.Sleep(3 * 1000);
                    this.InvokeRequired(delegate { _timer2.Start(); });
                });
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
                _timer2.Start();
            }
        }

        private void ButtonLoginAccount_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                if (string.IsNullOrWhiteSpace(TextBoxAccount.Text) && string.IsNullOrWhiteSpace(DWPBox.Password))
                {
                    FileInfo dwpFile = new FileInfo($"{AppCtrl.ProcessName}.dwp.config");

                    if (dwpFile.Exists)
                    {
                        foreach (string line in File.ReadAllLines(dwpFile.FullName, TextEncoding.UTF8WithoutBOM))
                        {
                            if (line.StartsWith("account=", StringComparison.OrdinalIgnoreCase))
                            {
                                TextBoxAccount.Text = line.Substring("account=".Length).Trim().ToUpper();
                            }
                            else if (line.StartsWith("dwp=", StringComparison.OrdinalIgnoreCase))
                            {
                                DWPBox.Password = line.Substring("dwp=".Length).Trim();
                            }
                        }
                    }
                }

                if (CapitalCtrl == null)
                {
                    CapitalControl = new CapitalController();
                    CapitalCtrl.LoginAccount(TextBoxAccount.Text, DWPBox.Password);
                }
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void ButtonLoginQuote_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                CapitalCtrl.LoginQuote(DWPBox.Password);
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void ButtonIsConnected_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                StatusBarItemAB3.Text = CapitalCtrl.IsConnected();
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                CapitalCtrl.Disconnect();
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void ButtonPrintProductList_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                CapitalCtrl.PrintProductList();
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void ButtonSubQuotes_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                CapitalCtrl.SubQuotes();
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void ButtonQueryQuotes_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void ButtonFuturesOrderTest_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void ButtonOptionsOrderTest_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }
    }
}
