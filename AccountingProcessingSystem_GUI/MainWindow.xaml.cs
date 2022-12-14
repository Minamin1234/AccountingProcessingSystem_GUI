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
        public const string CACHEFILENAME = "cache.mcache";

        public const string WORD_CSV = ",";
        public const string NAME_ID = "ID";
        public const string NAME_DATE = "日付";
        public const string NAME_GROUP = "項目";
        public const string NAME_TITLE = "内容";
        public const string NAME_PAID = "支出";
        public const string NAME_INCOME = "収益";

        public List<ACCOUNTDATA> accounts = new List<ACCOUNTDATA>();
        public List<GROUP> groups = new List<GROUP>();
        public CACHE cache = null;
        public bool IsCacheExsist = false;
        public bool IsSelected
        {
            get { return this.LV_Datas.SelectedItem != null; }
        }
        public MainWindow()
        {
            InitializeComponent();
            this.B_Edit.IsEnabled = this.IsSelected;
            this.B_Del.IsEnabled = this.IsSelected;
            if (this.IsCacheExsist = CheckCache()) LoadCache(ref this.cache);
            else this.cache = new CACHE();
        }

        /// <summary>
        /// キャッシュの配置場所を返します。(アプリケーションと同じ場所に配置されます)
        /// </summary>
        /// <returns></returns>
        public string GetCachePath()
        {
            return System.IO.Path.GetDirectoryName(
                Environment.GetCommandLineArgs()[0])
                + System.IO.Path.DirectorySeparatorChar
                + CACHEFILENAME;
        }

        /// <summary>
        /// キャッシュが存在するかどうかを返します。
        /// </summary>
        /// <returns></returns>
        public bool CheckCache()
        {
            if (System.IO.File.Exists(System.IO.Path.GetDirectoryName(
                Environment.GetCommandLineArgs()[0])
                + System.IO.Path.DirectorySeparatorChar
                + CACHEFILENAME)) return true;
            return false;
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
                this.cache.DataFilePath = sd.FileName;
                this.Title = System.IO.Path.GetFileNameWithoutExtension(sd.FileName);
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
                this.cache.GroupsDataFilePath = sd.FileName;
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
                using (var sw = new StreamWriter(sd.FileName,false,CSVENCODE))
                {
                    sw.Write(res);
                }
            }
        }

        /// <summary>
        /// キャッシュを生成します。
        /// </summary>
        /// <param name="cache"></param>
        public void GenerateCache(ref CACHE cache)
        {
            var se = new XmlSerializer(typeof(CACHE));
            using (var fs = new FileStream(this.GetCachePath(),FileMode.Create))
            {
                se.Serialize(fs, cache);
                Console.WriteLine("Saved:" + this.GetCachePath());
            }
        }

        /// <summary>
        /// キャッシュからデータファイルをロードします。
        /// </summary>
        /// <param name="cache"></param>
        public void LoadCache(ref CACHE cache)
        {
            var se = new XmlSerializer(typeof(CACHE));
            using (var fs = new FileStream(this.GetCachePath(),FileMode.Open))
            {
                cache = (CACHE)se.Deserialize(fs);
                if (cache.DataFilePath != String.Empty) this.LoadData(cache.DataFilePath, ref this.accounts);
                else if (cache.GroupsDataFilePath != String.Empty) this.LoadGroupListData(cache.GroupsDataFilePath, ref this.groups);
                this.ShowDatas(ref this.accounts);
                this.ShowGroups(ref this.groups);
                Console.WriteLine("Loaded");
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
                this.LoadData(fd.FileName, ref datas);
                this.cache.DataFilePath = fd.FileName;
            }
        }

        /// <summary>
        /// 指定した場所のファイルから会計データを読み込みます。
        /// </summary>
        /// <param name="filedir"></param>
        /// <param name="datas"></param>
        public void LoadData(string filedir,ref List<ACCOUNTDATA> datas)
        {
            if(System.IO.File.Exists(filedir))
            {
                var se = new XmlSerializer(typeof(List<ACCOUNTDATA>));
                using (var fs = new FileStream(filedir, FileMode.Open))
                {
                    datas = (List <ACCOUNTDATA>)se.Deserialize(fs);
                    if (datas.Count != 0) this.groups = datas[0].Groups;
                }
                this.Title = System.IO.Path.GetFileNameWithoutExtension(filedir);
                foreach (var d in datas)
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
                this.LoadGroupListData(fd.FileName,ref groups);
                this.cache.GroupsDataFilePath = fd.FileName;
            }
        }

        /// <summary>
        /// 指定したファイルの場所からグループリストデータをロードします。
        /// </summary>
        /// <param name="filedir"></param>
        /// <param name="groups"></param>
        public void LoadGroupListData(string filedir,ref List<GROUP> groups)
        {
            var se = new XmlSerializer(typeof(List<GROUP>));
            using (var fs = new FileStream(filedir,FileMode.Open))
            {
                groups = (List <GROUP>)se.Deserialize(fs);
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

        /// <summary>
        /// ウィンドウが閉じられる時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void On_Close(object sender, EventArgs e)
        {
            this.GenerateCache(ref this.cache);
        }
    }
}
