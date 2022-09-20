using System;
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

using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Win32;

namespace AccountingProcessingSystem_GUI
{
    /// <summary>
    /// 会計データ
    /// </summary>
    public partial class ACCOUNTDATA : IEquatable<ACCOUNTDATA>
    {
        public int Id = 0;
        public DateTime? Date = null;
        public List<GROUP> Groups = null;
        public GROUP Group = null;
        public string Title = string.Empty;
        public int Paid = 0;
        public int Income = 0;

        public override string ToString()
        {
            string res = "";
            res += "Id:     " + this.Id.ToString() + Environment.NewLine;
            res += "Date:   " + this.Date.ToString() + Environment.NewLine;
            if(this.Group == null) res += "Group:  null" + Environment.NewLine;
            else res += "Group:  " + this.Group.ToString() + Environment.NewLine;
            res += "Title:  " + this.Title + Environment.NewLine;
            res += "Paid:   " + this.Paid.ToString() + Environment.NewLine;
            res += "Income: " + this.Income.ToString() + Environment.NewLine;

            return res;
        }

        public bool Equals(ACCOUNTDATA other)
        {
            return this.Id == other.Id;
        }
    }

    /// <summary>
    /// 品目のグループ
    /// </summary>
    public partial class GROUP : IEquatable<GROUP>
    {
        public string Name = string.Empty;
        public override string ToString()
        {
            return this.Name;
        }

        //同型のオブジェクトについて、何を持って等価であるかを定義
        public bool Equals(GROUP other)
        {
            return this.Name == other.Name;
        }

        public GROUP()
        {

        }

        public GROUP(string name)
        {
            this.Name = name;
        }
    }

    /// <summary>
    /// ListViewで表示する内容を定義したクラス
    /// </summary>
    public partial class ACCOUNTDATASHOWS
    {
        public int ID { get; set; }
        public DateTime? Date { get; set; }
        public GROUP Group { get; set; }
        public string Title { get; set; }
        public int Paid { get; set; }
        public int Income { get; set; }
        public ACCOUNTDATA Owner = null;

        public ACCOUNTDATASHOWS(ACCOUNTDATA data)
        {
            this.ID = data.Id;
            this.Date = data.Date;
            this.Group = data.Group;
            this.Title = data.Title;
            this.Paid = data.Paid;
            this.Income = data.Income;
            this.Owner = data;
        }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string DATAFILTER = "data files (*.mdata)|*.mdata";
        public const string DATAEXT = ".mdata";
        public List<ACCOUNTDATA> accounts = new List<ACCOUNTDATA>();
        public List<GROUP> groups = new List<GROUP>();
        public bool IsSelected
        {
            get { return this.LV_Datas.SelectedItem != null; }
        }
        public MainWindow()
        {
            InitializeComponent();
            this.B_Edit.IsEnabled = this.IsSelected;
            this.B_Del.IsEnabled = this.IsSelected;
        }

        /// <summary>
        /// 会計データを追加します。
        /// </summary>
        /// <returns>追加に成功したかどうか</returns>
        public bool AddNewAccount(ref List<ACCOUNTDATA> datas,bool debug=false)
        {
            if(DP_date.SelectedDate == null) return false;
            var current = new ACCOUNTDATA();
            current.Id = datas.Count;
            current.Date = DP_date.SelectedDate;
            current.Groups = this.groups;
            if(CB_Group.SelectedItem == null) current.Group = null;
            else current.Group = CB_Group.SelectedItem as GROUP;
            current.Title = TB_Title.Text;
            current.Paid = int.Parse(TB_Paid.Text);
            current.Income = int.Parse(TB_Income.Text);
            datas.Add(current);
            if(debug) Console.WriteLine(current.ToString());
            return true;
        }

        /// <summary>
        /// 会計一覧をListViewに表示します。
        /// </summary>
        /// <param name="datas"></param>
        public void ShowDatas(ref List<ACCOUNTDATA> datas)
        {
            var list = new ObservableCollection<ACCOUNTDATASHOWS>();
            foreach(var data in datas)
            {
                list.Add(new ACCOUNTDATASHOWS(data));
            }
            LV_Datas.ItemsSource = list;
        }

        /// <summary>
        /// グループをConboBoxに表示します。
        /// </summary>
        /// <param name="groups"></param>
        public void ShowGroups(ref List<GROUP> groups)
        {
            var list = new ObservableCollection<GROUP>();
            foreach(var i in groups)
            {
                list.Add(i);
            }
            CB_Group.ItemsSource = list;
        }

        /// <summary>
        /// グループの管理ウィンドウを開きます。
        /// </summary>
        /// <param name="groups"></param>
        public void OpenGroupManager(ref List<GROUP> groups)
        {
            var window = new GroupManager(this,ref groups);
            window.Show();
        }

        public void SaveData(ref List<ACCOUNTDATA> datas)
        {
            var sd = new SaveFileDialog();
            sd.InitialDirectory = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            sd.DefaultExt = DATAEXT;
            sd.Filter = DATAFILTER;
            if((bool)sd.ShowDialog())
            {
                var se = new XmlSerializer(typeof(List<ACCOUNTDATA>));
                using (var fs = new FileStream(sd.FileName, FileMode.Create))
                {
                    se.Serialize(fs, datas);
                }
                Console.WriteLine("Saved");
            }
        }

        public void LoadData(ref List<ACCOUNTDATA> datas)
        {
            var fd = new OpenFileDialog();
            fd.InitialDirectory = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            fd.DefaultExt = DATAEXT;
            fd.Filter = DATAFILTER;
            if((bool)fd.ShowDialog())
            {
                var se = new XmlSerializer(typeof(List<ACCOUNTDATA>));
                using (var fs = new FileStream(fd.FileName, FileMode.Open))
                {
                    datas = (List<ACCOUNTDATA>)se.Deserialize(fs);
                    if (datas.Count != 0) this.groups = datas[0].Groups;
                }

                foreach(var d in datas)
                {
                    Console.WriteLine(d);
                }
            }
        }

        /// <summary>
        /// 開いていたグループマネージャーが閉じられた際に呼ばれます。
        /// </summary>
        public void On_ClosedGroupManager()
        {
            this.ShowGroups(ref this.groups);
        }

        /// <summary>
        /// 追加ボタンをクリックした際に呼ばれます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Clicked(object sender, RoutedEventArgs e)
        {
            AddNewAccount(ref this.accounts);
            ShowDatas(ref this.accounts);
        }

        /// <summary>
        /// 編集ボタンがクリックされた際に呼ばれます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Edit_clicked(object sender, RoutedEventArgs e)
        {
            if (this.LV_Datas.SelectedItem == null) return;
            var target = (this.LV_Datas.SelectedItem as ACCOUNTDATASHOWS).Owner;
            if (target == null) return;
            target.Date = this.DP_date.SelectedDate;
            target.Group = this.CB_Group.SelectedItem as GROUP;
            target.Title = this.TB_Title.Text;
            target.Paid = int.Parse(this.TB_Paid.Text);
            target.Income = int.Parse(this.TB_Income.Text);
            this.ShowDatas(ref this.accounts);
            this.B_Edit.IsEnabled = this.IsSelected;
        }

        /// <summary>
        /// グループマネージャーのボタンがクリックされた際に呼ばれます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupManager_Clicked(object sender, RoutedEventArgs e)
        {
            this.OpenGroupManager(ref this.groups);
        }

        /// <summary>
        /// 削除ボタンがクリックされた際に呼ばれます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Del_Clicked(object sender, RoutedEventArgs e)
        {
            if (this.LV_Datas.SelectedItem as ACCOUNTDATASHOWS == null) return;
            this.accounts.Remove((this.LV_Datas.SelectedItem as ACCOUNTDATASHOWS).Owner);
            this.ShowDatas(ref this.accounts);
            this.B_Del.IsEnabled = this.IsSelected;
        }

        /// <summary>
        /// データが選択された際に呼ばれます。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ElementSelected(object sender, SelectionChangedEventArgs e)
        {
            if (this.LV_Datas.SelectedItem as ACCOUNTDATASHOWS == null) return;
            var target = (this.LV_Datas.SelectedItem as ACCOUNTDATASHOWS).Owner;
            this.DP_date.SelectedDate = target.Date;
            this.CB_Group.SelectedItem = target.Group;
            this.TB_Title.Text = target.Title;
            this.TB_Paid.Text = target.Paid.ToString();
            this.TB_Income.Text = target.Income.ToString();
            this.B_Edit.IsEnabled = this.IsSelected;
            this.B_Del.IsEnabled = this.IsSelected;
            Console.WriteLine((this.LV_Datas.SelectedItem as ACCOUNTDATASHOWS).Owner.ToString());
        }

        private void MenuElements_Clicked(object sender, RoutedEventArgs e)
        {
            if(sender as MenuItem == MI_LoadData)
            {
                this.LoadData(ref this.accounts);
                this.ShowDatas(ref this.accounts);
                this.ShowGroups(ref this.groups);
            }
            else if(sender as MenuItem == MI_SaveData)
            {
                this.SaveData(ref this.accounts);
            }
            else if(sender as MenuItem == MI_ExportCSV)
            {

            }
        }
    }
}
