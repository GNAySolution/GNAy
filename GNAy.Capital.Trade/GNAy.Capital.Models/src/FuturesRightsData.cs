using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class FuturesRightsData : NotifyPropertyChanged
    {
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(FuturesRightsData).GetColumnAttrMapByProperty<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        private string _creator;
        [Column("建立者")]
        public string Creator
        {
            get { return _creator; }
            set { OnPropertyChanged(ref _creator, value); }
        }

        [Column("日期", CSVIndex = -1)]
        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
        [Column("時間", CSVStringFormat = "yyyy/MM/dd HH:mm:ss.ffffff")]
        public DateTime CreatedTime
        {
            get { return _createdTime; }
            set { OnPropertiesChanged(ref _createdTime, value, nameof(CreatedTime), nameof(CreatedDate)); }
        }

        private string _updater;
        [Column("更新者", WPFDisplayIndex = 0)]
        public string Updater
        {
            get { return _updater; }
            set { OnPropertyChanged(ref _updater, value); }
        }

        [Column("更新日", CSVIndex = -1)]
        public DateTime UpdateDate => UpdateTime.Date;

        private DateTime _updateTime;
        [Column("更新時", CSVStringFormat = "yyyy/MM/dd HH:mm:ss.ffffff", WPFDisplayIndex = 1, WPFStringFormat = "{0:HH:mm:ss.fff}")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set { OnPropertiesChanged(ref _updateTime, value, nameof(UpdateTime), nameof(UpdateDate)); }
        }

        public string RawInfo;

        //+0000000000000,
        private decimal _f0;
        [Column("帳戶餘額", CSVIndex = -1, WPFDisplayIndex = 2, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F0
        {
            get { return _f0; }
            set { OnPropertyChanged(ref _f0, value); }
        }

        //+0000000000000,
        private decimal _f1;
        [Column("浮動損益", CSVIndex = -1, WPFDisplayIndex = 3, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F1
        {
            get { return _f1; }
            set { OnPropertyChanged(ref _f1, value); }
        }

        //+0000000000000,
        private decimal _f2;
        [Column("已實現費用", CSVIndex = -1, WPFDisplayIndex = 4, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F2
        {
            get { return _f2; }
            set { OnPropertyChanged(ref _f2, value); }
        }

        //+0000000000000,
        private decimal _f3;
        [Column("交易稅", CSVIndex = -1, WPFDisplayIndex = 5, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F3
        {
            get { return _f3; }
            set { OnPropertyChanged(ref _f3, value); }
        }

        //+0000000000000,
        private decimal _f4;
        [Column("預扣權利金", CSVIndex = -1, WPFDisplayIndex = 6, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F4
        {
            get { return _f4; }
            set { OnPropertyChanged(ref _f4, value); }
        }

        //+0000000000000,
        private decimal _f5;
        [Column("權利金收付", CSVIndex = -1, WPFDisplayIndex = 7, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F5
        {
            get { return _f5; }
            set { OnPropertyChanged(ref _f5, value); }
        }

        //+0000000000000,
        private decimal _f6;
        [Column("權益數", CSVIndex = -1, WPFDisplayIndex = 8, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F6
        {
            get { return _f6; }
            set { OnPropertyChanged(ref _f6, value); }
        }

        //+0000000000000,
        private decimal _f7;
        [Column("超額保證金", CSVIndex = -1, WPFDisplayIndex = 9, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F7
        {
            get { return _f7; }
            set { OnPropertyChanged(ref _f7, value); }
        }

        //+0000000000000,
        private decimal _f8;
        [Column("存提款", CSVIndex = -1, WPFDisplayIndex = 10, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F8
        {
            get { return _f8; }
            set { OnPropertyChanged(ref _f8, value); }
        }

        //+0000000000000,
        private decimal _f9;
        [Column("買方市值", CSVIndex = -1, WPFDisplayIndex = 11, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F9
        {
            get { return _f9; }
            set { OnPropertyChanged(ref _f9, value); }
        }

        //+0000000000000,
        private decimal _f10;
        [Column("賣方市值", CSVIndex = -1, WPFDisplayIndex = 12, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F10
        {
            get { return _f10; }
            set { OnPropertyChanged(ref _f10, value); }
        }

        //+0000000000000,
        private decimal _f11;
        [Column("期貨平倉損益", CSVIndex = -1, WPFDisplayIndex = 13, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F11
        {
            get { return _f11; }
            set { OnPropertyChanged(ref _f11, value); }
        }

        //+0000000000000,
        private decimal _f12;
        [Column("盤中未實現", CSVIndex = -1, WPFDisplayIndex = 14, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F12
        {
            get { return _f12; }
            set { OnPropertyChanged(ref _f12, value); }
        }

        //+0000000000000,
        private decimal _f13;
        [Column("原始保證金1", CSVIndex = -1, WPFDisplayIndex = 15, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F13
        {
            get { return _f13; }
            set { OnPropertyChanged(ref _f13, value); }
        }

        //+0000000000000,
        private decimal _f14;
        [Column("維持保證金", CSVIndex = -1, WPFDisplayIndex = 16, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F14
        {
            get { return _f14; }
            set { OnPropertyChanged(ref _f14, value); }
        }

        //+0000000000000,
        private decimal _f15;
        [Column("部位原始保證金", CSVIndex = -1, WPFDisplayIndex = 17, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F15
        {
            get { return _f15; }
            set { OnPropertyChanged(ref _f15, value); }
        }

        //+0000000000000,
        private decimal _f16;
        [Column("部位維持保證金", CSVIndex = -1, WPFDisplayIndex = 18, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F16
        {
            get { return _f16; }
            set { OnPropertyChanged(ref _f16, value); }
        }

        //+0000000000000,
        private decimal _f17;
        [Column("委託保證金", CSVIndex = -1, WPFDisplayIndex = 19, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F17
        {
            get { return _f17; }
            set { OnPropertyChanged(ref _f17, value); }
        }

        //+0000000000000,
        private decimal _f18;
        [Column("超額最佳保證金", CSVIndex = -1, WPFDisplayIndex = 20, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F18
        {
            get { return _f18; }
            set { OnPropertyChanged(ref _f18, value); }
        }

        //+0000000000000,
        private decimal _f19;
        [Column("權利總值", CSVIndex = -1, WPFDisplayIndex = 21, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F19
        {
            get { return _f19; }
            set { OnPropertyChanged(ref _f19, value); }
        }

        //+0000000000000,
        private decimal _f20;
        [Column("預扣費用", CSVIndex = -1, WPFDisplayIndex = 22, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F20
        {
            get { return _f20; }
            set { OnPropertyChanged(ref _f20, value); }
        }

        //+0000000000000,
        private decimal _f21;
        [Column("原始保證金2", CSVIndex = -1, WPFDisplayIndex = 23, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F21
        {
            get { return _f21; }
            set { OnPropertyChanged(ref _f21, value); }
        }

        //+0000000000000,
        private decimal _f22;
        [Column("昨日餘額", CSVIndex = -1, WPFDisplayIndex = 24, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F22
        {
            get { return _f22; }
            set { OnPropertyChanged(ref _f22, value); }
        }

        //Y ,
        private string _f23;
        [Column("選擇權組合單加不加收保證金", CSVIndex = -1, WPFDisplayIndex = 25, WPFHorizontalAlignment = WPFHorizontalAlignment.Center)]
        public string F23
        {
            get { return _f23; }
            set { OnPropertyChanged(ref _f23, value); }
        }

        //000000000,
        private decimal _f24;
        [Column("維持率", CSVIndex = -1, WPFDisplayIndex = 26, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F24
        {
            get { return _f24; }
            set { OnPropertyChanged(ref _f24, value); }
        }

        //NTD,
        private string _currency;
        [Column("幣別", CSVIndex = -1, WPFDisplayIndex = 27, WPFHorizontalAlignment = WPFHorizontalAlignment.Center)]
        public string Currency
        {
            get { return _currency; }
            set { OnPropertyChanged(ref _currency, value); }
        }

        //+0000000000000,
        private decimal _f26;
        [Column("足額原始保證金", CSVIndex = -1, WPFDisplayIndex = 28, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F26
        {
            get { return _f26; }
            set { OnPropertyChanged(ref _f26, value); }
        }

        //+0000000000000,
        private decimal _f27;
        [Column("足額維持保證金", CSVIndex = -1, WPFDisplayIndex = 29, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F27
        {
            get { return _f27; }
            set { OnPropertyChanged(ref _f27, value); }
        }

        //+0000000000000,
        private decimal _f28;
        [Column("足額可用", CSVIndex = -1, WPFDisplayIndex = 30, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F28
        {
            get { return _f28; }
            set { OnPropertyChanged(ref _f28, value); }
        }

        //+0000000000000,
        private decimal _f29;
        [Column("抵繳金額", CSVIndex = -1, WPFDisplayIndex = 31, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F29
        {
            get { return _f29; }
            set { OnPropertyChanged(ref _f29, value); }
        }

        //+0000000000000,
        private decimal _f30;
        [Column("有價可用", CSVIndex = -1, WPFDisplayIndex = 32, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F30
        {
            get { return _f30; }
            set { OnPropertyChanged(ref _f30, value); }
        }

        //+0000000000000,
        private decimal _f31;
        [Column("可用餘額", CSVIndex = -1, WPFDisplayIndex = 33, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F31
        {
            get { return _f31; }
            set { OnPropertyChanged(ref _f31, value); }
        }

        //+0000000000000,
        private decimal _f32;
        [Column("足額現金可用", CSVIndex = -1, WPFDisplayIndex = 34, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F32
        {
            get { return _f32; }
            set { OnPropertyChanged(ref _f32, value); }
        }

        //+0000000000000,
        private decimal _f33;
        [Column("有價價值", CSVIndex = -1, WPFDisplayIndex = 35, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F33
        {
            get { return _f33; }
            set { OnPropertyChanged(ref _f33, value); }
        }

        //100      ,
        private decimal _f34;
        [Column("風險指標", CSVIndex = -1, WPFDisplayIndex = 36, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F34
        {
            get { return _f34; }
            set { OnPropertyChanged(ref _f34, value); }
        }

        //+0000000000000,
        private decimal _f35;
        [Column("選擇權到期差異", CSVIndex = -1, WPFDisplayIndex = 37, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F35
        {
            get { return _f35; }
            set { OnPropertyChanged(ref _f35, value); }
        }

        //+0000000000000,
        private decimal _f36;
        [Column("選擇權到期差損", CSVIndex = -1, WPFDisplayIndex = 38, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F36
        {
            get { return _f36; }
            set { OnPropertyChanged(ref _f36, value); }
        }

        //+0000000000000,
        private decimal _f37;
        [Column("期貨到期損益", CSVIndex = -1, WPFDisplayIndex = 39, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F37
        {
            get { return _f37; }
            set { OnPropertyChanged(ref _f37, value); }
        }

        //+0000000000000
        private decimal _f38;
        [Column("加收保證金", CSVIndex = -1, WPFDisplayIndex = 40, WPFStringFormat = "{0:0.00}", WPFHorizontalAlignment = WPFHorizontalAlignment.Right)]
        public decimal F38
        {
            get { return _f38; }
            set { OnPropertyChanged(ref _f38, value); }
        }

        private string _userID;
        [Column("會員帳號", WPFDisplayIndex = 41)]
        public string UserID
        {
            get { return _userID; }
            set { OnPropertyChanged(ref _userID, value); }
        }

        private string _account;
        [Column("下單帳號", WPFDisplayIndex = 42)]
        public string Account
        {
            get { return _account; }
            set { OnPropertyChanged(ref _account, value); }
        }

        public FuturesRightsData(string raw, [CallerMemberName] string memberName = "")
        {
            Creator = memberName;
            CreatedTime = DateTime.Now;
            Updater = string.Empty;
            UpdateTime = DateTime.MaxValue;

            RawInfo = raw;

            string[] cells = raw.Split(',');

            if (cells.Length < 41)
            {
                throw new ArgumentException($"{cells.Length}|{raw}");
            }

            F0 = decimal.Parse(cells[0]) / 100;
            F1 = decimal.Parse(cells[1]) / 100;
            F2 = decimal.Parse(cells[2]) / 100;
            F3 = decimal.Parse(cells[3]) / 100;
            F4 = decimal.Parse(cells[4]) / 100;
            F5 = decimal.Parse(cells[5]) / 100;
            F6 = decimal.Parse(cells[6]) / 100;
            F7 = decimal.Parse(cells[7]) / 100;
            F8 = decimal.Parse(cells[8]) / 100;
            F9 = decimal.Parse(cells[9]) / 100;
            F10 = decimal.Parse(cells[10]) / 100;
            F11 = decimal.Parse(cells[11]) / 100;
            F12 = decimal.Parse(cells[12]) / 100;
            F13 = decimal.Parse(cells[13]) / 100;
            F14 = decimal.Parse(cells[14]) / 100;
            F15 = decimal.Parse(cells[15]) / 100;
            F16 = decimal.Parse(cells[16]) / 100;
            F17 = decimal.Parse(cells[17]) / 100;
            F18 = decimal.Parse(cells[18]) / 100;
            F19 = decimal.Parse(cells[19]) / 100;
            F20 = decimal.Parse(cells[20]) / 100;
            F21 = decimal.Parse(cells[21]) / 100;
            F22 = decimal.Parse(cells[22]) / 100;
            F23 = cells[23].Trim();
            F24 = decimal.Parse(cells[24]);
            Currency = cells[25].Trim();
            F26 = decimal.Parse(cells[26]) / 100;
            F27 = decimal.Parse(cells[27]) / 100;
            F28 = decimal.Parse(cells[28]) / 100;
            F29 = decimal.Parse(cells[29]) / 100;
            F30 = decimal.Parse(cells[30]) / 100;
            F31 = decimal.Parse(cells[31]) / 100;
            F32 = decimal.Parse(cells[32]) / 100;
            F33 = decimal.Parse(cells[33]) / 100;
            F34 = decimal.Parse(cells[34]);
            F35 = decimal.Parse(cells[35]) / 100;
            F36 = decimal.Parse(cells[36]) / 100;
            F37 = decimal.Parse(cells[37]) / 100;
            F38 = decimal.Parse(cells[38]) / 100;
            UserID = cells[39].Trim();
            Account = cells[40].Trim();
        }
    }
}
