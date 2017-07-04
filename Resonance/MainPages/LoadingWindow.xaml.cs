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
using System.Threading;
using System.Windows.Threading;

//using Sense4;

namespace Resonance
{
    /// <summary>
    /// 欢迎界面，因未加载Matlab库，所以实际上很快一闪而过
    /// </summary>
    public partial class LoadingWindow : Window
    {
        /// <summary>
        /// 构造函数，并在多线程中加载算法库
        /// </summary>
        public LoadingWindow()
        {
            InitializeComponent();
            new Thread(LoadingOperate).Start();
        }


        /// <summary>
        /// 加载matlab算法库，需要2~3秒（弃用）
        /// </summary>
        private void LoadingOperate()
        {
            //Algorithm.InitAlgorithm();//加载算法

            //转到MainWindow
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render,
            new Action(() =>
            {
                MainWindow w = new MainWindow();
                w.Show();
                this.Close();
            }));
        }
    }
}
