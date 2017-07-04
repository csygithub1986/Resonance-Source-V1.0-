using System;
using System.Collections.Generic;
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
using System.IO;
using Tools;

namespace Resonance
{
    /// <summary>
    /// 填写电缆参数的窗口
    /// </summary>
    public partial class CableInfoWin : Window
    {
        public CableInfoWin()
        {
            InitializeComponent();
        }

        public CableInfo Info { get; set; }
        public bool Ok;

        /// <summary>
        /// 确认按钮
        /// </summary>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (Info != null)
            {
                //string filename = "start.info";    //文件名称
                //string folder = "振荡波测试 " + txtStation.Text + " " + DateTime.Now.ToString("yyyy-MM-dd");     //文件夹名称
                //Info.Path = new DirectoryInfo(System.IO.Path.Combine(txtDirectory.Text, folder));
                //FileInfo fileInfo = new FileInfo(System.IO.Path.Combine(Info.Path.FullName, filename));
                //if (!fileInfo.Exists)
                //{
                //    fileInfo.Directory.Create();
                //}
                ////以前做的用户便捷性的设计工作都是白费，不做了，map不存
                //CableInfo.WriteFile(Info, fileInfo);
                Ok = true;
                this.Close();
                return;
            }
            if (FillCableInfo())
            {
                try
                {
                    string filename = "start.info";    //文件名称
                    FileInfo fileInfo = new FileInfo(System.IO.Path.Combine(Info.Path.FullName, filename));
                    if (!fileInfo.Exists)
                    {
                        fileInfo.Directory.Create();
                    }
                    //以前做的用户便捷性的设计工作都是白费，不做了，map不存
                    CableInfo.WriteFile(Info, fileInfo);
                    Ok = true;
                    this.Close();
                }
                catch (Exception)
                {
                    MessageBox.Show("保存路径不存在", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    Info = null;
                }

            }
        }

        /// <summary>
        /// 装配CableInfo对象
        /// </summary>
        /// <returns>如果格式都ok，返回true</returns>
        private bool FillCableInfo()
        {
            try
            {
                //float[] freqs = new float[3];
                //if (Info != null)
                //{
                //    freqs = Info.Freqs;
                //}
                Info = new CableInfo();
                //Info.Freqs = freqs;
                string folder = "振荡波测试 " + txtStation.Text + " " + DateTime.Now.ToString("yyyy-MM-dd");     //文件夹名称
                Info.Station = txtStation.Text;
                Info.Path = new DirectoryInfo(System.IO.Path.Combine(txtDirectory.Text, folder));
                Info.Length = double.Parse(txtLength.Text);
                //Info.Velocity = double.Parse(txtVelocity.Text);
                Info.U0 = double.Parse(txtU0.Text);
                if (jointBox.Items.Count > 0)
                {
                    foreach (var item in jointBox.Items)
                    {
                        Info.Joints.Add((double)item);
                    }
                    if (Info.Joints.Min() <= 0 || Info.Joints.Max() >= Info.Length)
                    {
                        MessageBox.Show("接头位置错误");
                        return false;
                    }
                    Info.Joints.Add(Info.Length);
                    Info.Joints.Insert(0, 0);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("填写的数据格式错误");
                Info = null;
                return false;
            }

            Info.Joints.Sort();
            return true;
        }

        /// <summary>
        /// 取消按钮
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Info = null;
            this.Close();
        }

        /// <summary>
        /// 导入按钮
        /// </summary>
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            FileInfo fileInfo = FileTools.OpenFile("start", "info");
            if (fileInfo == null)
            {
                return;
            }
            Info = CableInfo.ReadFile(fileInfo);
            Info.Path = fileInfo.Directory;
            DisPlayInfo();
        }

        /// <summary>
        /// 导入后显示
        /// </summary>
        private void DisPlayInfo()
        {
            txtDirectory.Text = Info.Path.FullName;
            txtStation.Text = Info.Station;
            txtLength.Text = Info.Length.ToString();
            //txtVelocity.Text = Info.Velocity.ToString();
            txtU0.Text = Info.U0.ToString();
            txtComment.Text = Info.Comment;

            jointBox.Items.Clear();
            for (int i = 1; i < Info.Joints.Count - 1; i++)
            {
                jointBox.Items.Add(Info.Joints[i]);
            }
        }

        /// <summary>
        /// 浏览保存目录
        /// </summary>
        private void btnChooseDire_Click(object sender, RoutedEventArgs e)
        {
            string dire = FileTools.OpenDirectory();
            if (dire == null)
                return;
            txtDirectory.Text = dire;
        }

        /// <summary>
        /// 添加接头
        /// </summary>
        private void btnAddJoint_Click(object sender, RoutedEventArgs e)
        {
            double result;
            if (double.TryParse(txtJoint.Text, out result))
            {
                jointBox.Items.Add(result);
            }
            txtJoint.Text = "";
        }

        /// <summary>
        /// 删除接头
        /// </summary>
        private void Menu_Del(object sender, RoutedEventArgs e)
        {
            jointBox.Items.Remove(jointBox.SelectedItem);
        }
    }
}
