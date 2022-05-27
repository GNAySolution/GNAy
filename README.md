# GNAy
* Yang的專案開發紀錄
* 儲存庫裡的程式碼都是自己寫或參考網路上的，參考都有在註解內附上連結，表示感謝
* 技術細節整理放在Blog
* https://gnaysolution.blogspot.com/

## https://github.com/GNAySolution/GNAy/tree/main/GNAy.Capital.Trade/GNAy.Capital.Trade/GNAy.Capital.Trade.csproj
![image](https://github.com/GNAySolution/GNAy/blob/main/GNAy.Capital.Trade/GNAy.Capital.Trade/docs/GNAy.Capital.Trade.22050811.jpg)
* 交易輔助APP，策略交易框架與範例，提供多樣化的素材，方便user自行組合與測試交易策略
* 基於群益API(SKCOM.dll)，需先申請群益證券帳戶和期貨帳戶才能使用，目前專案參考API版本2.13.37_x64
* https://www.capital.com.tw/Service2/download/api.asp
* Demo放在以下頻道
* https://youtube.com/playlist?list=PLOGS4yeidG_YCRn2hZjwbP4ffqnL33VuH
* (投資一定有風險，不同時間進場，將有不同的投資績效，電子下單也可能面對使用者操作不當、程式錯誤、作業系統錯誤、硬體錯誤、病毒攻擊、時間延遲、網路壅塞、斷線、停電等風險)

###### vcredist_x64.exe & CAP-*.pfx
* 使用群益API的前置條件，要先安裝憑證(CAP-*.pfx)，以及微軟Visual C++ 2010 Service Pack 1 MFC可轉散發套件安全性更新(vcredist_x64.exe)
* https://docs.microsoft.com/zh-tw/cpp/windows/latest-supported-vc-redist?view=msvc-170

###### https://github.com/GNAySolution/GNAy/tree/main/GNAy.Capital.Trade/GNAy.Capital.Trade/docs/GNAy.Capital.Trade.22050811.jpg
* APP(開發中)操作介面示意圖
- [x] 交易輔助APP，自定義台灣期貨、單隻腳選擇權，多樣化的觸價條件與停損停利策略
- [x] 短期目標是幫助user快速做當沖或隔日沖
- [x] 可設定模擬下單(SendRealOrder = false)，方便同時跑多種策略，觀察比較優劣
- [x] 全自動(或半自動)運行，適合放在雲端主機上運行
- [x] 直播模式(LiveMode = true)，隱藏隱私資料，只呈現損益，方便user實況(或錄影)播放自己的交易策略
- [ ] 台股上市、上櫃股票，待實作
- [ ] 國外期貨、選擇權，待實作
- [ ] 中期目標是連續IOC等多樣化的CD單，並串接其他券商的API
- [ ] 長期目標是把程式移植到iOS平台上，用iPhone或Mac操作

###### https://github.com/GNAySolution/GNAy/blob/main/GNAy.Capital.Trade/GNAy.Capital.Trade/.config/GNAy.Capital.Trade.dwp.config
* 讀取帳號密碼的基本範例，專案不對帳密做儲存或特別處理，有需要請自行修改程式碼

###### https://github.com/GNAySolution/GNAy/blob/main/GNAy.Capital.Trade/GNAy.Capital.Trade/.config/holidaySchedule_{yyy}.csv
* 臺灣證券交易所市場開休市日期
* https://www.twse.com.tw/zh/holidaySchedule/holidaySchedule

###### https://github.com/GNAySolution/GNAy/blob/main/GNAy.Capital.Trade/GNAy.Capital.Trade/docs/Futures_MTX_1DayK.csv
* 範例參考期交所的小台日K資訊，整理使其更易於發想交易策略，以及回測驗證多年績效
* https://www.taifex.com.tw/cht/3/dlFutDailyMarketView

###### https://github.com/GNAySolution/GNAy/blob/main/GNAy.Capital.Trade/GNAy.Capital.Trade/.config/TriggerData/0404_1830.csv
* 觸價範例，方便demo用的，用來快速測試停損停利等各項功能，是容易賠錢的設定，請勿在正式環境使用

###### https://github.com/GNAySolution/GNAy/blob/main/GNAy.Capital.Trade/GNAy.Capital.Trade/.config/StrategyData/0504_1442.csv
* 策略範例，方便demo用的，用來快速測試停損停利等各項功能，是容易賠錢的設定，請勿在正式環境使用

## https://github.com/GNAySolution/GNAy/tree/main/GNAy.Capital.Trade/GNAy.Capital.Models/GNAy.Capital.Models.csproj
* 策略交易框架，相關模型、資料、設定

## https://github.com/GNAySolution/GNAy/tree/main/GNAy.Capital.Trade/GNAy.Capital.Monitor/GNAy.Capital.Monitor.csproj
* 針對GNAy.Capital.Trade的監控和功能測試，空專案，尚未實作

## https://github.com/GNAySolution/GNAy/tree/main/GNAy.Tools.WPF/GNAy.Tools.WPF/GNAy.Tools.WPF.csproj
* WPF共用工具

## https://github.com/GNAySolution/GNAy/tree/main/GNAy.Tools.WPF/GNAy.Tools.NET47/GNAy.Tools.NET47.csproj
* .NET Framework 4.7共用工具

