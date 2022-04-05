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
        public readonly DateTime StartTime;
        private readonly AppController _appCtrl;

        private readonly DispatcherTimer _timer1;
        private readonly DispatcherTimer _timer2;

        public MainWindow()
        {
            InitializeComponent();

            StartTime = DateTime.Now;
            _appCtrl = new AppController(this);

            //https://www.796t.com/post/MWV3bG0=.html
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            Title = $"{version.Comments} ({version.FileVersion})";
            if (version.FileMajorPart <= 0 || version.FilePrivatePart % 2 == 1)
            {
                Title = $"{Title}(BETA)";
            }
            Title = $"{Title} ({version.ProductName})({version.LegalCopyright}) (PID:{_appCtrl.ProcessID})({_appCtrl.Settings.Description})";
            if (Debugger.IsAttached)
            {
                Title = $"{Title}(附加偵錯)";
            }

            _timer1 = new DispatcherTimer(DispatcherPriority.ContextIdle)
            {
                Interval = TimeSpan.FromMilliseconds(_appCtrl.Settings.TimerIntervalUI1),
            };
            _timer1.Tick += Timer1_Tick;
            _timer2 = new DispatcherTimer(DispatcherPriority.Send)
            {
                Interval = TimeSpan.FromMilliseconds(_appCtrl.Settings.TimerIntervalUI2),
            };
            _timer2.Tick += Timer2_Tick;

            StatusBarItemAA1.Text = StartTime.ToString("MM/dd HH:mm");
            TextBoxQuoteFolderTest.Text = _appCtrl.Settings.QuoteFolderPath;
            StatusBarItemAB2.Text = $"Subscribed={_appCtrl.Config.QuoteSubscribed.Count}|Live={_appCtrl.Settings.QuoteLive.Count}";

            _appCtrl.LogTrace(Title);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _appCtrl.LogTrace("Start");
            _timer1.Stop();
            _timer2.Stop();
            _appCtrl.Exit();
            _appCtrl.LogTrace("End");
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void Window_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_GotMouseCapture(object sender, MouseEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                if (!_appCtrl.Config.StartOnTime)
                {
                    _appCtrl.LogWarn($"程式沒有在正常時間啟動");
                }

                _appCtrl.LogTrace($"{StartTime.AddDays(-3):MM/dd HH:mm}|{StartTime.AddDays(-3).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(-3))}");
                _appCtrl.LogTrace($"{StartTime.AddDays(-2):MM/dd HH:mm}|{StartTime.AddDays(-2).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(-2))}");
                _appCtrl.LogTrace($"{StartTime.AddDays(-1):MM/dd HH:mm}|{StartTime.AddDays(-1).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(-1))}");
                _appCtrl.LogTrace($"{StartTime.AddDays(0):MM/dd HH:mm}|{StartTime.AddDays(+0).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(0))}|Today");
                _appCtrl.LogTrace($"{StartTime.AddDays(1):MM/dd HH:mm}|{StartTime.AddDays(+1).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(1))}");
                _appCtrl.LogTrace($"{StartTime.AddDays(2):MM/dd HH:mm}|{StartTime.AddDays(+2).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(2))}");
                _appCtrl.LogTrace($"{StartTime.AddDays(3):MM/dd HH:mm}|{StartTime.AddDays(+3).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(3))}");

                _appCtrl.LogTrace($"{_appCtrl.Config.Archive.FullName}");
                _appCtrl.LogTrace($"{_appCtrl.Config.Archive.Name}|Version={_appCtrl.Config.Version}|Exists={_appCtrl.Config.Archive.Exists}");

                _appCtrl.LogTrace($"AutoRun={_appCtrl.Settings.AutoRun}");

                if (!_appCtrl.Config.Archive.Exists)
                {
                    //https://docs.microsoft.com/zh-tw/dotnet/desktop/wpf/windows/how-to-open-message-box?view=netdesktop-6.0

                    string caption = $"第一次產生設定檔{_appCtrl.Config.Archive.Name}";
                    string messageBoxText = $"請確認檔案內容\r\n{_appCtrl.Config.Archive.FullName}";

                    MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);

                    _appCtrl.Exit(caption, LogLevel.Warn);
                    return;
                }
                else if (_appCtrl.Settings.AutoRun)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(1 * 1000);
                        this.InvokeRequired(delegate { Timer2_Tick(null, null); });
                    });
                }

                _timer1.Start();

                if (!_appCtrl.Settings.AutoRun)
                {
                    _timer2.Start();
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_LostMouseCapture(object sender, MouseEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //https://stackoverflow.com/questions/24214804/does-wpf-have-mouse-wheel-scrolling-up-and-down-event
                if (e.Delta == 0)
                {
                    return;
                }

                ComboBox cb = null;

                if (ComboBoxStockAccs.IsMouseOver && !ComboBoxStockAccs.IsFocused)
                {
                    cb = ComboBoxStockAccs;
                }
                else if (ComboBoxFuturesAccs.IsMouseOver && !ComboBoxFuturesAccs.IsFocused)
                {
                    cb = ComboBoxFuturesAccs;
                }
                else if (ComboBoxTriggerProduct.IsMouseOver && !ComboBoxTriggerProduct.IsFocused)
                {
                    cb = ComboBoxTriggerProduct;
                }
                else if (ComboBoxTriggerColumn.IsMouseOver && !ComboBoxTriggerColumn.IsFocused)
                {
                    cb = ComboBoxTriggerColumn;
                }
                else if (ComboBoxTriggerCancel.IsMouseOver && !ComboBoxTriggerCancel.IsFocused)
                {
                    cb = ComboBoxTriggerCancel;
                }

                if (cb == null)
                {
                    return;
                }

                int offset = cb.SelectedIndex + ((e.Delta > 0) ? -1 : 1);

                if (offset >= 0 && offset < cb.Items.Count)
                {
                    cb.SelectedIndex = offset;
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //_appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.LogTrace("End");
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void Window_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void Window_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                //https://stackoverflow.com/questions/9565740/display-duration-in-milliseconds
                string elapsed = (now - StartTime).ToString("hh':'mm':'ss");

                StatusBarItemAA2.Text = $"{elapsed}";

                //if (Application.Current.MainWindow.IsMouseOver)
                if (IsMouseOver)
                {
                    //https://stackoverflow.com/questions/29822020/how-to-get-mouse-position-on-screen-in-wpf
                    //Point pt = Mouse.GetPosition(Application.Current.MainWindow);
                    Point pt = Mouse.GetPosition(this);
                    StatusBarItemAA3.Text = $"({Width},{Height},{pt.X},{pt.Y})";
                }

                if (TabControlBA.SelectedIndex == 0 && DataGridAPIReply.ItemsSource != null)
                {
                    StatusBarItemBA1.Text = $"({DataGridAPIReply.Columns.Count},{DataGridAPIReply.Items.Count})";
                }

                if (TabControlCA.SelectedIndex == 0 && DataGridAppLog.ItemsSource != null)
                {
                    StatusBarItemCA1.Text = $"({DataGridAppLog.Columns.Count},{DataGridAppLog.Items.Count})";
                }

                if (_appCtrl.Capital != null)
                {
                    StatusBarItemBA2.Text = $"{_appCtrl.Capital.AccountTimer.Item1:mm:ss}|{_appCtrl.Capital.AccountTimer.Item2}";
                    StatusBarItemAB5.Text = _appCtrl.Capital.QuoteStatusStr;
                    StatusBarItemAB3.Text = $"{_appCtrl.Capital.QuoteTimer.Item1:mm:ss.fff}|{_appCtrl.Capital.QuoteTimer.Item2}|{_appCtrl.Capital.QuoteTimer.Item3}";

                    elapsed = (now - _appCtrl.Capital.QuoteTimer.Item1).ToString("mm':'ss'.'fff");
                    StatusBarItemAA2.Text = $"{StatusBarItemAA2.Text}|{elapsed}";
                }

                if (TabControlAB.SelectedIndex == 0 && DataGridQuoteSubscribed.ItemsSource != null)
                {
                    StatusBarItemAB1.Text = $"({DataGridQuoteSubscribed.Columns.Count},{DataGridQuoteSubscribed.Items.Count})";
                }

                if (TabControlBB.SelectedIndex == 0 && DataGridTriggerRule.ItemsSource != null)
                {
                    StatusBarItemBB1.Text = $"({DataGridTriggerRule.Columns.Count},{DataGridTriggerRule.Items.Count})";
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            _timer2.Stop();

            DateTime now = DateTime.Now;
            int reConnect = 0;

            try
            {
                string msg = $"{now:MM/dd HH:mm.ss}|IsHoliday={_appCtrl.Config.IsHoliday(now)}";
                //_appCtrl.LogTrace(msg);
                StatusBarItemCA2.Text = msg;

                ButtonSaveQuotesTest_Click(null, null);

                foreach (DateTime timeToExit in _appCtrl.Settings.TimeToExit)
                {
                    if (now.Hour == timeToExit.Hour && now.Minute >= timeToExit.Minute && now.Minute <= (timeToExit.Minute + 2))
                    {
                        _timer1.Stop();
                        Thread.Sleep(3 * 1000);
                        _appCtrl.Exit($"Time to exit.");
                        return;
                    }
                }

                if (_appCtrl.Settings.AutoRun && _appCtrl.Capital == null)
                {
                    reConnect = 1 + StatusCode.BaseTraceValue;
                }
                //3002 SK_SUBJECT_CONNECTION_DISCONNECT 斷線
                //3021 SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK 連線失敗(網路異常等)
                //3022 SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL Solace底層連線錯誤
                //3033 SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR Solace Sessio down錯誤
                else if (_appCtrl.Capital != null && (_appCtrl.Capital.QuoteStatus == 3002 || _appCtrl.Capital.QuoteStatus == 3021 || _appCtrl.Capital.QuoteStatus == 3022 || _appCtrl.Capital.QuoteStatus == 3033))
                {
                    reConnect = _appCtrl.Capital.QuoteStatus + StatusCode.BaseErrorValue;
                }
                else if (_appCtrl.Capital != null && _appCtrl.Capital.QuoteStatus > StatusCode.BaseTraceValue)
                {
                    reConnect = _appCtrl.Capital.QuoteStatus;
                }

                if (reConnect == 0)
                {
                    _timer2.Start();
                    return;
                }

                _appCtrl.Log(reConnect, $"Retry to connect quote service.|reConnect={reConnect}");
                Task.Factory.StartNew(() =>
                {
                    if (_appCtrl.Capital != null)
                    {
                        Thread.Sleep(1 * 1000);
                        _appCtrl.Capital.Disconnect();
                    }

                    Thread.Sleep(3 * 1000);
                    this.InvokeRequired(delegate
                    {
                        ButtonLoginAccount_Click(null, null);
                        ButtonLoginQuote_Click(null, null);
                        ButtonReadCertification_Click(null, null);
                    });

                    Thread.Sleep(3 * 1000);
                    SpinWait.SpinUntil(() => _appCtrl.Capital.QuoteStatus == StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY, 2 * 60 * 1000);
                    if (_appCtrl.Capital.QuoteStatus != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY) //Timeout
                    {
                        //TODO: Send alert mail.
                        _appCtrl.Capital.Disconnect();
                        this.InvokeRequired(delegate { _timer2.Start(); }); //Retry to connect quote service.
                        return;
                    }

                    Thread.Sleep(1 * 1000);
                    this.InvokeRequired(delegate
                    {
                        ButtonIsConnected_Click(null, null);
                        ButtonGetProductInfo_Click(null, null);
                        ButtonSubQuotes_Click(null, null);
                        ButtonGetOrderAccs_Click(null, null);
                    });

                    Thread.Sleep(3 * 1000);
                    this.InvokeRequired(delegate { _timer2.Start(); });
                });
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
                _timer2.Start();
            }
        }

        private void ButtonLoginAccount_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                if (string.IsNullOrWhiteSpace(TextBoxAccount.Text) && string.IsNullOrWhiteSpace(DWPBox.Password))
                {
                    FileInfo dwpFile = new FileInfo($"{_appCtrl.ProcessName}.dwp.config");

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

                _appCtrl.InitialCapital();
                _appCtrl.Capital.LoginAccount(TextBoxAccount.Text, DWPBox.Password);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonLoginQuote_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                _appCtrl.Capital.LoginQuoteAsync(DWPBox.Password);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonIsConnected_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                StatusBarItemAB4.Text = _appCtrl.Capital.IsConnected();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                _appCtrl.Capital.Disconnect();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonPrintProductList_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                _appCtrl.Capital.PrintProductList();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonGetProductInfo_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                _appCtrl.Capital.GetProductInfo();
                StatusBarItemAB2.Text = $"Sub={_appCtrl.Config.QuoteSubscribed.Count}|Live={_appCtrl.Settings.QuoteLive.Count}|QuoteFile={_appCtrl.Capital.QuoteFileNameBase}";
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonSubQuotes_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                _appCtrl.Capital.SubQuotesAsync();
                _appCtrl.SetTriggerRule();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonRecoverQuotes_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                _appCtrl.Capital.RecoverQuotesAsync(TextBoxRecoverQuotes.Text);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonRequestKLine_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                _appCtrl.Capital.RequestKLine();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonSaveQuotesTest_Click(object sender, RoutedEventArgs e)
        {
            if (_appCtrl.Capital == null)
            {
                return;
            }
            else if (string.IsNullOrWhiteSpace(TextBoxQuoteFolderTest.Text))
            {
                return;
            }

            _appCtrl.LogTrace("Start");

            try
            {
                DirectoryInfo folder = new DirectoryInfo(TextBoxQuoteFolderTest.Text);
                folder.Create();
                folder.Refresh();
                _appCtrl.Capital.SaveQuotesAsync(quoteFolder: folder);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonReadCertification_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                _appCtrl.Capital.ReadCertification();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonGetOrderAccs_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                _appCtrl.Capital.GetGetOrderAccs();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonUnlockOrder_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                _appCtrl.Capital.UnlockOrder(0);
                _appCtrl.Capital.UnlockOrder(1);
                _appCtrl.Capital.UnlockOrder(2);
                _appCtrl.Capital.UnlockOrder(3);
                _appCtrl.Capital.UnlockOrder(4);
                _appCtrl.Capital.UnlockOrder(5);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonGetOpenInterest_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                OrderAccData acc = (OrderAccData)ComboBoxFuturesAccs.SelectedItem;
                _appCtrl.Capital.GetOpenInterest(acc.FullAccount);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonSaveTriggerRule_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                _appCtrl.Trigger.AddRule();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonFuturesOrderTest_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }

        private void ButtonOptionsOrderTest_Click(object sender, RoutedEventArgs e)
        {
            _appCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.LogTrace("End");
            }
        }
    }
}
