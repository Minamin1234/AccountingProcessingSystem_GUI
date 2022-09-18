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
using System.Windows.Shapes;

using System.Collections.ObjectModel;

namespace AccountingProcessingSystem_GUI
{
    /// <summary>
    /// GroupManager.xaml の相互作用ロジック
    /// </summary>
    public partial class GroupManager : Window
    {
        public MainWindow owner = null;
        public List<GROUP> groups = new List<GROUP>();
        public GroupManager()
        {
            InitializeComponent();
        }

        public GroupManager(MainWindow owner,ref List<GROUP> groups)
        {
            InitializeComponent();
            this.owner = owner;
            this.groups = groups;
            this.ShowGroups(ref this.groups);
        }

        public void AddNewGroup()
        {
            var current = new GROUP(this.TB_Name.Text);
            if (this.groups.Contains(current)) return;
            this.groups.Add(current);
        }

        public GROUP GetSelectedGroup()
        {
            if(this.LV_Groups.SelectedItem == null) return null;
            var target = this.LV_Groups.SelectedItem as GROUP;
            if(target == null) return null;
            return target;
        }

        public void LoadGroup()
        {
            var target = this.GetSelectedGroup();
            if (target == null) return;
            this.TB_Name.Text = target.Name;
        }

        public void ShowGroups(ref List<GROUP> groups)
        {
            var list = new ObservableCollection<GROUP>();
            foreach(var i in groups)
            {
                list.Add(i);
            }
            this.LV_Groups.ItemsSource = list;
        }

        private void Add_Clicked(object sender, RoutedEventArgs e)
        {
            this.AddNewGroup();
            this.ShowGroups(ref this.groups);
        }

        private void Edit_Clicked(object sender, RoutedEventArgs e)
        {
            var target = this.GetSelectedGroup();
            if(target == null) return;
            target.Name = this.TB_Name.Text;
            Console.WriteLine(target.Name);
            this.ShowGroups(ref this.groups);
        }

        private void Del_Clicked(object sender, RoutedEventArgs e)
        {
            if(this.LV_Groups.SelectedItem as GROUP == null) return;
            this.groups.Remove(this.LV_Groups.SelectedItem as GROUP);
            this.ShowGroups(ref this.groups);
        }

        private void Close_Clicked(object sender, RoutedEventArgs e)
        {
            this.owner.On_ClosedGroupManager();
            this.Close();
        }

        private void EditElementSelected(object sender, MouseButtonEventArgs e)
        {
            this.LoadGroup();
        }
    }
}
