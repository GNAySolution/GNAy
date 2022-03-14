using GNAy.Capital.Trade.Controllers;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
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
        public static MainWindow Current { get; private set; }
        public static MainWindowController AppCtrl => Current.AppControl;
        public static CapitalController CapitalCtrl => Current.CapitalControl;

        public readonly DateTime StartTime;

        public readonly MainWindowController AppControl;
        public CapitalController CapitalControl { get; private set; }

        private readonly DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            StartTime = DateTime.Now;
            StatusBarItemAA1.Text = StartTime.ToString("MM/dd HH:mm");

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

            Current = this;

            AppControl = new MainWindowController();
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

            _timer = new DispatcherTimer(DispatcherPriority.Send)
            {
                Interval = TimeSpan.FromMilliseconds(AppCtrl.Settings.TimerInterval),
            };
            _timer.Tick += Timer_Tick;

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

        private void Window_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
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

        private void Window_KeyUp(object sender, KeyEventArgs e)
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                AppCtrl.LogTrace($"{AppCtrl.Config.File.FullName}|Exists={AppCtrl.Config.File.Exists}");

                if (!AppCtrl.Config.File.Exists)
                {
                    //https://docs.microsoft.com/zh-tw/dotnet/desktop/wpf/windows/how-to-open-message-box?view=netdesktop-6.0

                    string caption = $"第一次產生設定檔{AppCtrl.Config.File.Name}";
                    string messageBoxText = $"請確認檔案內容\r\n{AppCtrl.Config.File.FullName}";

                    MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);

                    AppCtrl.Exit(caption);
                    return;
                }
                else if (AppCtrl.Settings.AutoRun)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(1 * 1000);
                        this.InvokeRequired(delegate
                        {
                            AppCtrl.LogTrace("Start");

                            try
                            {
                                ButtonLoginAccount_Click(null, null);
                                ButtonLoginQuote_Click(null, null);
                            }
                            catch (Exception ex)
                            {
                                AppCtrl.LogException(ex, ex.StackTrace);
                            }
                            finally
                            {
                                AppCtrl.LogTrace("End");
                            }
                        });

                        Thread.Sleep(1 * 1000);
                        //TODO: ButtonIsConnected_Click
                    });
                }

                _timer.Start();
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
            //finally
            //{
            //    AppCtrl.LogTrace("End");
            //}
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
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

        private void Window_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
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

        private void Window_MouseLeave(object sender, MouseEventArgs e)
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
            //finally
            //{
            //    AppCtrl.LogTrace("End");
            //}
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
            //finally
            //{
            //    AppCtrl.LogTrace("End");
            //}
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                string duration = (now - StartTime).ToString(@"hh\:mm\:ss");

                StatusBarItemAA2.Text = $"{now:HH:mm:ss} ({duration})";

                //if (Application.Current.MainWindow.IsMouseOver)
                if (IsMouseOver)
                {
                    //https://stackoverflow.com/questions/29822020/how-to-get-mouse-position-on-screen-in-wpf
                    //Point pt = Mouse.GetPosition(Application.Current.MainWindow);
                    Point pt = Mouse.GetPosition(this);
                    StatusBarItemAA3.Text = $"({pt.X},{pt.Y})";
                }

                if (DataGridAppLog.ItemsSource != null)
                {
                    StatusBarItemBA1.Text = $"({DataGridAppLog.Columns.Count},{DataGridAppLog.Items.Count})";
                }

                if (CapitalCtrl != null)
                {
                    StatusBarItemAB2.Text = CapitalCtrl.LoginQuoteStatusStr;
                }
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
        }

        private void ButtonLoginAccount_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                FileInfo dwpFile = new FileInfo($"{AppCtrl.ProcessName}.dwp.config");
                if (dwpFile.Exists)
                {
                    foreach (string line in File.ReadAllLines(dwpFile.FullName, TextEncoding.UTF8WithoutBOM))
                    {
                        if (line.StartsWith("account=", StringComparison.OrdinalIgnoreCase))
                        {
                            TextBoxAccount.Text = line.Substring("account=".Length).Trim();
                        }
                        else if (line.StartsWith("dwp=", StringComparison.OrdinalIgnoreCase))
                        {
                            DWPBox.Password = line.Substring("dwp=".Length).Trim();
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

        private void ButtonGetProductList_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                CapitalCtrl.GetProductList();
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
