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
        public const string GROUPLISTDATAEXT = ".mgroups";
        public const string GROUPLISTDATAFILTER = "groupsdata files (*.mgroups)|*.mgroups";
        public const string CSVEXT = ".csv";
        public const string CSVFILTER = "CSV file (*.csv)|*.csv";
        public Encoding CSVENCODE = Encoding.UTF8;

        public const string WORD_CSV = ",";
        public const string NAME_ID = "ID";
        public const string NAME_DATE = "日付";
        public const string NAME_GROUP = "項目";
        public const string NAME_TITLE = "内容";
        public const string NAME_PAID = "支出";
        public const string NAME_INCOME = "収益";

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

        /// <summary>
        /// 会計リストとグループリスト全てをファイルとして保存します。
        /// </summary>
        /// <param name="datas"></param>
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

        /// <summary>
        /// グループリストをデータとして保存します。
        /// </summary>
        /// <param name="groupslist"></param>
        public void SaveGroupListData(ref List<GROUP> groupslist)
        {
            var sd = new SaveFileDialog();
            sd.InitialDirectory = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            sd.DefaultExt = GROUPLISTDATAEXT;
            sd.Filter = GROUPLISTDATAFILTER;
            if((bool)sd.ShowDialog())
            {
                var se = new XmlSerializer(typeof(List<GROUP>));
                using (var fs = new FileStream(sd.FileName, FileMode.Create))
                {
                    se.Serialize(fs, groupslist);
                }
                Console.WriteLine("Saved");
            }
        }

        /// <summary>
        /// 会計データをCSV形式で保存します。
        /// </summary>
        /// <param name="accounts"></param>
        public void GenerateCSV(ref List<ACCOUNTDATA> accounts)
        {
            string res = "";
            res += NAME_ID + WORD_CSV;
            res += NAME_DATE + WORD_CSV;
            res += NAME_GROUP + WORD_CSV;
            res += NAME_TITLE + WORD_CSV;
            res += NAME_PAID + WORD_CSV;
            res += NAME_INCOME + Environment.NewLine;

            foreach (var ac in accounts)
            {
                res += ac.Id.ToString() + WORD_CSV;
                res += ac.Date.ToString() + WORD_CSV;
                res += ac.Group.ToString() + WORD_CSV;
                res += ac.Title + WORD_CSV;
                res += ac.Paid.ToString() + WORD_CSV;
                res += ac.Income.ToString() + Environment.NewLine;
            }

            var sd = new SaveFileDialog();
            sd.InitialDirectory = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            sd.DefaultExt = CSVEXT;
            sd.Filter = CSVFILTER;
            if((bool)sd.ShowDialog())
            {
                var se = new SaveFileDialog();
                using (var sw = new StreamWriter(sd.FileName,false,CSVENCODE))
                {
                    sw.Write(res);
                }
            }
        }

        /// <summary>
        /// 保存された会計データファイルを読み込みます。
        /// </summary>
        /// <param name="datas"></param>
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
        /// グループリストのデータを読み込みます。
        /// </summary>
        /// <param name="groups"></param>
        public void LoadGroupListData(ref List<GROUP> groups)
        {
            var fd = new OpenFileDialog();
            fd.InitialDirectory = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            fd.DefaultExt = GROUPLISTDATAEXT;
            fd.Filter = GROUPLISTDATAFILTER;
            if((bool)fd.ShowDialog())
            {
                var se = new XmlSerializer(typeof(List<GROUP>));
                using (var fs = new FileStream(fd.FileName,FileMode.Open))
                {
                    groups = (List<GROUP>)se.Deserialize(fs);
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

        /// <summary>
        /// メニュー内のボタン押された時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuElements_Clicked(object sender, RoutedEventArgs e)
        {
            if(sender as MenuItem == MI_LoadData)
            {
                this.LoadData(ref this.accounts);
                this.ShowDatas(ref this.accounts);
                this.ShowGroups(ref this.groups);
            }
            else if(sender as MenuItem == MI_LoadGroups)
            {
                this.LoadGroupListData(ref this.groups);
                this.ShowGroups(ref this.groups);
            }
            else if(sender as MenuItem == MI_SaveData)
            {
                this.SaveData(ref this.accounts);
            }
            else if(sender as MenuItem == MI_SaveGroups)
            {
                this.SaveGroupListData(ref this.groups);
            }
            else if(sender as MenuItem == MI_ExportCSV)
            {
                this.GenerateCSV(ref this.accounts);
            }
        }
    }
}
