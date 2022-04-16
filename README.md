# GNAy
* Yang的專案開發紀錄

## GNAy.Capital.Trade.csproj
![image](https://drive.google.com/file/d/1QS-zJQSLCRWLUpvG5ev7ooPWyBnSj58i/view?usp=sharing)
* 策略交易框架與範例，交易輔助APP，基於群益API(SKCOM.dll)，開發中
* https://www.capital.com.tw/Service2/download/api.asp
* 需要先申請群益證券帳戶和期貨帳戶才能使用，目前專案參考API版本2.13.37_x64

###### vcredist_x64.exe & CAP-*.pfx
* 使用群益API的前置條件，要先安裝憑證(CAP-*.pfx)，以及微軟Visual C++ 2010 Service Pack 1 MFC可轉散發套件安全性更新(vcredist_x64.exe)
* https://docs.microsoft.com/zh-tw/cpp/windows/latest-supported-vc-redist?view=msvc-170

###### GNAy.Capital.Trade.dwp.config
* 讀取帳號密碼的基本範例，專案不對帳密做儲存或特別處理，有需要請自行修改程式碼

###### holidaySchedule_{yyy}.csv
* 臺灣證券交易所市場開休市日期
* https://www.twse.com.tw/zh/holidaySchedule/holidaySchedule

###### Futures_MTX_1DayK.csv
* 範例參考期交所的小台日K資訊，整理使其更易於發想交易策略，以及回測驗證多年績效
* https://www.taifex.com.tw/cht/3/dlFutDailyMarketView

###### 0404_1830.csv
* 觸價條件範例與測試

## GNAy.Capital.Models.csproj
* 策略交易框架，相關模型、資料、設定

## GNAy.Capital.Monitor.csproj
* 針對GNAy.Capital.Trade的監控和功能測試，空專案，尚未實作

## GNAy.Tools.WPF.csproj
* WPF共用工具

## GNAy.Tools.NET47.csproj
* .NET Framework 4.7共用工具

