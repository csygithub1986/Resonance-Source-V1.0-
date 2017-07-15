using System;
using System.Collections.Generic;
using System.IO;
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
using Tools;
using Resonance;

namespace 数据格式转换
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            FileInfo file = FileTools.OpenFile("*");
            if (file != null)
            {
                if (file.Extension.Contains("cal"))
                {
                    CalibrationInfo calib = CalibrationInfo.ReadFile(file);
                    FileInfo txtFile = new FileInfo(file.Directory.FullName + "\\" + file.Name.Substring(0, file.Name.Length - 4) + ".txt");
                    using (StreamWriter sw = new StreamWriter(txtFile.OpenWrite()))
                    {
                        foreach (var item in calib.AllData)
                        {
                            sw.WriteLine(item * Params.PD_Coeffi);
                        }
                    }
                    MessageBox.Show("OK");
                    return;
                }
                else if (file.Extension.Contains("zdb"))
                {
                    double[] hv;
                    double[] pd;
                    DataInfo.Read(file, out hv, out pd);
                    FileInfo txtHVFile = new FileInfo(file.Directory.FullName + "\\" + file.Name.Substring(0, file.Name.Length - 4) + "高压.txt");
                    FileInfo txtPDFile = new FileInfo(file.Directory.FullName + "\\" + file.Name.Substring(0, file.Name.Length - 4) + "局放.txt");
                    using (StreamWriter sw = new StreamWriter(txtHVFile.OpenWrite()))
                    {
                        foreach (var item in hv)
                        {
                            sw.WriteLine(item);
                        }
                    }
                    using (StreamWriter sw = new StreamWriter(txtPDFile.OpenWrite()))
                    {
                        foreach (var item in pd)
                        {
                            sw.WriteLine(item);
                        }
                    }
                    MessageBox.Show("OK");
                    return;
                }
                else
                {
                    MessageBox.Show("文件格式不正确");
                }
            }
        }
    }
}
