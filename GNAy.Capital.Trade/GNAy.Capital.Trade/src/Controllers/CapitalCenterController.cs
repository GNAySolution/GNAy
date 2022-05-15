using GNAy.Capital.Models;
using GNAy.Tools.WPF;
using NLog;
using SKCOMLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class CapitalCenterController
    {
        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private SKCenterLib m_pSKCenter;
        private SKReplyLib m_pSKReply;

        public int LoginUserResult { get; private set; }
        public string UserID { get; private set; }

        public (DateTime, string) UserIDTimer { get; private set; }

        private readonly Dictionary<int, APIReplyData> _apiReplyMap;
        private readonly ObservableCollection<APIReplyData> _apiReplyCollection;

        public CapitalCenterController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(CapitalCenterController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            LoginUserResult = -1;
            UserID = string.Empty;

            UserIDTimer = (DateTime.MinValue, string.Empty);

            _apiReplyMap = new Dictionary<int, APIReplyData>();
            _appCtrl.MainForm.DataGridAPIReply.SetHeadersByBindings(APIReplyData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _apiReplyCollection = _appCtrl.MainForm.DataGridAPIReply.SetAndGetItemsSource<APIReplyData>();
        }

        private CapitalCenterController() : this(null)
        { }

        public string GetAPIMessage(int nCode)
        {
            if (nCode <= 0)
            {
                return $"nCode={nCode}";
            }

            string lastLog = m_pSKCenter.SKCenterLib_GetLastLogInfo(); //取得最後一筆LOG內容
            string codeMessage = m_pSKCenter.SKCenterLib_GetReturnCodeMessage(nCode % StatusCode.BaseTraceValue); //取得定義代碼訊息文字

            return $"nCode={nCode}|{codeMessage}|{lastLog}";
        }

        public (LogLevel, string) LogAPIMessage(int nCode, string msg = "", TimeSpan? elapsed = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            msg = string.Format("{0}{1}{2}", msg, string.IsNullOrWhiteSpace(msg) ? string.Empty : "|", GetAPIMessage(nCode));

            if (nCode < 0)
            {
                _appCtrl.LogError(msg, UniqueName, elapsed, lineNumber, memberName);
                return (LogLevel.Error, msg);
            }

            int _code = nCode % StatusCode.BaseTraceValue;

            if (_code == 0)
            {
                _appCtrl.LogTrace(msg, UniqueName, elapsed, lineNumber, memberName);
                return (LogLevel.Trace, msg);
            }
            else if (_code < 2000 || _code == StatusCode.SK_WARNING_PRECHECK_RESULT_FAIL ||
                _code == StatusCode.SK_WARNING_PRECHECK_RESULT_EMPTY ||
                _code == StatusCode.SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK ||
                _code == StatusCode.SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL ||
                _code == StatusCode.SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR)
            {
                _appCtrl.LogError(msg, UniqueName, elapsed, lineNumber, memberName);
                return (LogLevel.Error, msg);
            }
            else if (_code < 3000)
            {
                _appCtrl.LogWarn(msg, UniqueName, elapsed, lineNumber, memberName);
                return (LogLevel.Warn, msg);
            }
            else
            {
                _appCtrl.LogTrace(msg, UniqueName, elapsed, lineNumber, memberName);
                return (LogLevel.Trace, msg);
            }
        }

        public (LogLevel, string) LogAPIMessage(DateTime start, int nCode, string msg = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            return LogAPIMessage(nCode, msg, DateTime.Now - start, lineNumber, memberName);
        }

        public void AppendReply(string userID, string msg, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
        {
            APIReplyData replay = new APIReplyData()
            {
                ThreadID = Thread.CurrentThread.ManagedThreadId,
                UserID = userID,
                Message = msg,
                CallerLineNumber = lineNumber,
                CallerMemberName = memberName,
            };

            _appCtrl.MainForm.InvokeAsync(delegate
            {
                try
                {
                    _appCtrl.MainForm.TabControlBA.SelectedIndex = 0;

                    _apiReplyCollection.Add(replay);

                    //while (_apiReplyCollection.Count > _appCtrl.Settings.DataGridAppLogRowsMax * 2)
                    //{
                    //    _apiReplyCollection.RemoveAt(0);
                    //}

                    if (!_appCtrl.MainForm.DataGridAPIReply.IsMouseOver)
                    {
                        _appCtrl.MainForm.DataGridAPIReply.ScrollToBorderEnd();
                    }
                }
                catch
                { }
            });
        }

        public int LoginUser(string userID, string dwp)
        {
            userID = userID.Trim().ToUpper();
            dwp = dwp.Trim();

            DateTime start = _appCtrl.StartTrace($"userID={userID}|dwp=********", UniqueName);

            try
            {
                if (m_pSKCenter != null)
                {
                    return LoginUserResult;
                }

                m_pSKReply = new SKReplyLib();
                m_pSKReply.OnReplyMessage += SKReply_OnAnnouncement;
                m_pSKReply.OnConnect += OnConnect;
                m_pSKReply.OnDisconnect += OnDisconnect;
                m_pSKReply.OnSolaceReplyConnection += OnSolaceReplyConnection;
                m_pSKReply.OnSolaceReplyDisconnect += OnSolaceReplyDisconnect;
                m_pSKReply.OnComplete += OnComplete;
                m_pSKReply.OnNewData += OnNewData;
                m_pSKReply.OnReportCount += m_SKReplyLib_OnReportCount;
                m_pSKReply.OnReplyClear += OnClear;
                m_pSKReply.OnReplyClearMessage += OnClearMessage;

                m_pSKCenter = new SKCenterLib();
                m_pSKCenter.SKCenterLib_SetAuthority(0); //SGX 專線屬性：關閉／開啟：0／1
                m_pSKCenter.OnTimer += SKCenter_OnTimer;

                LoginUserResult = m_pSKCenter.SKCenterLib_Login(userID, dwp); //元件初始登入。在使用此 Library 前必須先通過使用者的雙因子(憑證綁定)身份認證，方可使用

                if (LoginUserResult == 0)
                {
                    _appCtrl.LogTrace(start, $"LoginUserResult={LoginUserResult}|雙因子登入成功", UniqueName);
                    UserID = userID;
                }
                else if (LoginUserResult >= 600 && LoginUserResult <= 699)
                {
                    _appCtrl.LogTrace(start, $"LoginUserResult={LoginUserResult}|雙因子登入成功|未使用雙因子登入成功, 請在強制雙因子實施前確認憑證是否有效", UniqueName);
                    UserID = userID;
                }
                else
                {
                    //1097 SK_ERROR_TELNET_LOGINSERVER_FAIL Telnet登入主機失敗，請確認您的環境(Firewall及hosts…等)
                    //1098 SK_ERROR_TELNET_AGREEMENTSERVER_FAIL Telnet同意書查詢主機失敗，請確認您的環境(Firewall及hosts…等)
                    (LogLevel, string) apiMsg = LogAPIMessage(start, LoginUserResult);
                    m_pSKCenter = null;
                    _appCtrl.Exit(apiMsg.Item2, apiMsg.Item1);
                    return LoginUserResult;
                }

                //if (LoginUserResult == 0 || (LoginUserResult >= 600 && LoginUserResult <= 699))
                //{
                //    int nCode = m_pSKReply.SKReplyLib_ConnectByID(userID); //指定回報連線的使用者登入帳號
                //
                //    if (nCode != 0)
                //    {
                //        LogAPIMessage(start, nCode);
                //    }
                //}

                string version = m_pSKCenter.SKCenterLib_GetSKAPIVersionAndBit(userID); //取得目前註冊SKAPI 版本及位元
                _appCtrl.LogTrace(start, $"SKAPIVersionAndBit={version}", UniqueName);
                _appCtrl.MainForm.StatusBarItemBA2.Text = $"SKAPIVersionAndBit={version}";
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }

            return LoginUserResult;
        }

        public int LoginQuote(string dwp, bool startQuoteMonitor = true)
        {
            DateTime start = _appCtrl.StartTrace($"UserID={UserID}|dwp=********|startQuoteMonitor={startQuoteMonitor}", UniqueName);

            dwp = dwp.Trim();

            int status = m_pSKCenter.SKCenterLib_LoginSetQuote(UserID, dwp, startQuoteMonitor ? "Y" : "N"); //Y:啟用報價 N:停用報價

            if (status == 0 || (status >= 600 && status <= 699))
            {
                _appCtrl.LogTrace(start, $"status={status}|登入成功", UniqueName);
            }
            else
            {
                LogAPIMessage(start, status);
            }

            return status;
        }

        public (LogLevel, string) IsConnected()
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                int nCode = m_pSKReply.SKReplyLib_IsConnectedByID(UserID); //檢查輸入的帳號目前連線狀態 //正式環境： 0表示斷線。1表示連線中。2表示下載中

                if (nCode != 0 && nCode != 1 && nCode != 2)
                {
                    LogAPIMessage(start, nCode);
                }

                return (LogLevel.Trace, nCode.ToString());
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);

                return (LogLevel.Error, ex.Message);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }
    }
}
