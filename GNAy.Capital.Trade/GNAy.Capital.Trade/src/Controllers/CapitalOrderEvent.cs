using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class CapitalController
    {
        /// <summary>
        /// 帳號資訊。透過呼叫GetUserAccount後，帳號資訊由該事件回傳
        /// </summary>
        /// <param name="bstrLogInID"></param>
        /// <param name="bstrAccountData"></param>
        private void m_OrderObj_OnAccount(string bstrLogInID, string bstrAccountData)
        {
            MainWindow.AppCtrl.LogTrace($"SKAPI|bstrLogInID={bstrLogInID}|bstrAccountData={bstrAccountData}");
            AppandReply(bstrLogInID, bstrAccountData);

            //string[] strValues;
            //string strAccount;

            //strValues = bstrAccountData.Split(',');
            //strAccount = bstrLogInID + " " + strValues[1] + strValues[3];

            //if (strValues[0] == "TS")
            //{
            //    boxStockAccount.Items.Add(strAccount);

            //    //boxExecutionAccount.Items.Add("證券 " + strAccount);
            //}
            //else if (strValues[0] == "TF")
            //{
            //    boxFutureAccount.Items.Add(strAccount);
            //    withDrawInOutControl1.UserAccountTF = strValues[1] + strValues[3];
            //    //boxExecutionAccount.Items.Add("期貨 " + strAccount);
            //}
            //else if (strValues[0] == "OF")
            //{
            //    boxOSFutureAccount.Items.Add(strAccount);
            //    withDrawInOutControl1.UserAccountOF = strValues[1] + strValues[3];
            //}
            //else if (strValues[0] == "OS")
            //{
            //    boxOSStockAccount.Items.Add(strAccount);
            //}

            //if (boxStockAccount.Items.Count > 0)
            //    boxStockAccount.SelectedIndex = 0;
            //if (boxFutureAccount.Items.Count > 0)
            //    boxFutureAccount.SelectedIndex = 0;
            //if (boxOSFutureAccount.Items.Count > 0)
            //    boxOSFutureAccount.SelectedIndex = 0;
            //if (boxOSStockAccount.Items.Count > 0)
            //    boxOSStockAccount.SelectedIndex = 0;
        }
    }
}
