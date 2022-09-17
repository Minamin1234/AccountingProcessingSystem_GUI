﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace AccountingProcessingSystem_GUI
{
    /// <summary>
    /// 会計データ
    /// </summary>
    public partial class ACCOUNTDATA
    {
        public DateTime? Date = null;
        public string Group = string.Empty;
        public string Title = string.Empty;
        public int Paid = 0;
        public int Income = 0;

        public override string ToString()
        {
            string res = "";
            res += "Date:   " + this.Date.ToString() + Environment.NewLine;
            res += "Group:  " + this.Group + Environment.NewLine;
            res += "Title:  " + this.Title + Environment.NewLine;
            res += "Paid:   " + this.Paid.ToString() + Environment.NewLine;
            res += "Income: " + this.Income.ToString() + Environment.NewLine;

            return res;
        }
    }
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<ACCOUNTDATA> accounts = new List<ACCOUNTDATA>();
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 会計データを追加します。
        /// </summary>
        /// <returns>追加に成功したかどうか</returns>
        public bool AddNewAccount(ref List<ACCOUNTDATA> datas,bool debug=false)
        {
            if(DP_date.SelectedDate == null) return false;
            var current = new ACCOUNTDATA();
            current.Date = DP_date.SelectedDate;
            if(CB_Group.SelectedItem == null) current.Group = String.Empty;
            else current.Group = CB_Group.SelectedItem.ToString();
            current.Title = TB_Title.Text;
            current.Paid = int.Parse(TB_Paid.Text);
            current.Income = int.Parse(TB_Income.Text);
            datas.Add(current);
            if(debug) Console.WriteLine(current.ToString());
            return true;
        }

        private void Add_Clicked(object sender, RoutedEventArgs e)
        {
            AddNewAccount(ref this.accounts);
        }
    }
}
