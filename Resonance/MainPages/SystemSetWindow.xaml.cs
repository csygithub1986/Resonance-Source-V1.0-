using Resonance.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Resonance
{
    /// <summary>
    /// SystemSetWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SystemSetWindow : Window
    {
        public SystemSetWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //加压等级
            VoltageList = new ObservableCollection<string>(Settings.Default.VoltageList.Cast<string>().ToArray());
            //扫频窗口
            ckboxShowSweepWindow.IsChecked = Settings.Default.EnableSweepWindow;
        }

        #region 加压等级

        public ObservableCollection<string> VoltageList
        {
            get { return (ObservableCollection<string>)GetValue(VoltageListProperty); }
            set { SetValue(VoltageListProperty, value); }
        }

        public static readonly DependencyProperty VoltageListProperty =
            DependencyProperty.Register("VoltageList", typeof(ObservableCollection<string>), typeof(SystemSetWindow), new PropertyMetadata(null));

        public string SelectedVoltage
        {
            get { return (string)GetValue(SelectedVoltageProperty); }
            set { SetValue(SelectedVoltageProperty, value); }
        }
        public static readonly DependencyProperty SelectedVoltageProperty = DependencyProperty.Register("SelectedVoltage", typeof(string), typeof(SystemSetWindow), new PropertyMetadata(null));



        private void btnDeleteVoltage_Click(object sender, RoutedEventArgs e)
        {
            VoltageList.Remove(SelectedVoltage);
        }

        private void btnAddVoltage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtAddVoltage.Text))
            {
                MessageBox.Show("请填写电压倍数");
                return;
            }
            double vol;
            if (double.TryParse(txtAddVoltage.Text, out vol) == false)
            {
                MessageBox.Show("请填写正确的数字格式");
            }
            if (vol < 0.1 || vol > 3)
            {
                MessageBox.Show("加压倍数应介于0.1到3.0之间");
            }
            VoltageList.Add(txtAddVoltage.Text);
            VoltageList = new ObservableCollection<string>(VoltageList.OrderBy(p => p));
            txtAddVoltage.Text = "";
        }

        private void listboxVoltage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDeleteVoltage.IsEnabled = listboxVoltage.SelectedItem != null;
        }

        #endregion

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            //加压等级
            var vlist = new System.Collections.Specialized.StringCollection();
            vlist.AddRange(VoltageList.ToArray());
            //扫频窗口
            Settings.Default.EnableSweepWindow = (bool)ckboxShowSweepWindow.IsChecked;

            Settings.Default.Save();
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
