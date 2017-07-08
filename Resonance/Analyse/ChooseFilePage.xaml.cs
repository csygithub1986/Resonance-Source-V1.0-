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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;


namespace Resonance
{
    /// <summary>
    /// 选择分析数据页面
    /// </summary>
    public partial class ChooseFilePage : Page
    {
        /// <summary>
        /// 记录当前选中路径
        /// </summary>
        DirectoryInfo _currentFolder;

        /// <summary>
        /// 桌面所在路径
        /// </summary>
        DirectoryInfo _desktop;

        public ChooseFilePage()
        {
            InitializeComponent();
        }

        private void Page_Initialized(object sender, EventArgs e)
        {
            KeepAlive = true;//记忆文件夹状态
            _desktop = new DirectoryInfo(System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            TraverseRoot();
            //Settings.ReadConfig();
        }

        #region  文件浏览相关
        /// <summary>
        /// 搜索根目录
        /// </summary>
        private void TraverseRoot()
        {
            string[] roots = System.IO.Directory.GetLogicalDrives();//
            List<FolderPathImage> listFolder = new List<FolderPathImage>();
            listFolder.Add(new FolderPathImage() { ImageUrl = "Images/desktop.png", Path = "桌面" });
            foreach (var item in roots)
            {
                DriveInfo dr = new DriveInfo(item.Substring(0, 1));
                if (dr.DriveType == DriveType.Fixed || dr.DriveType == DriveType.Removable)
                {
                    listFolder.Add(new FolderPathImage() { ImageUrl = "Images/harddisk.png", Path = item });
                }
            }
            this.listViewFiles.DataContext = listFolder;
            txtPath.Text = "我的电脑";
        }

        /// <summary>
        /// 搜索文件夹
        /// </summary>
        /// <returns>电缆信息</returns>
        private List<CableInfo> TraverseFolder()
        {
            DirectoryInfo[] folders = _currentFolder.GetDirectories();

            List<FolderPathImage> listFolder = new List<FolderPathImage>();//所有文件夹
            List<CableInfo> listValidData = new List<CableInfo>();//合法的数据文件夹
            CableInfo ci = FileDetector.GetValid(_currentFolder);
            if (ci != null)
            {
                listValidData.Add(ci);
            }
            foreach (var item in folders)
            {
                ci = null;
                if (item.Name.Contains(Params.FolderExName))
                {
                    try
                    {
                        ci = FileDetector.GetValid(item);
                        if (ci != null)
                        {
                            listValidData.Add(ci);
                        }
                    }
                    catch (Exception)
                    {
                        //目前以异常的方式，判断文件是否拒绝访问
                        continue;
                    }
                }
                //显示文件夹
                string imageUrl;
                if (ci == null)
                {
                    imageUrl = "Images/folder.png";
                }
                else
                {
                    imageUrl = "Images/folder_new.png";
                }
                listFolder.Add(new FolderPathImage() { ImageUrl = imageUrl, Path = item.Name });
            }
            this.listViewFiles.DataContext = listFolder;
            txtPath.Text = _currentFolder.FullName;
            return listValidData;
        }

        /// <summary>
        /// 双击目录
        /// </summary>
        private void ListViewFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentFolder == null)
            {
                string path = (listViewFiles.SelectedItem as FolderPathImage).Path;
                if (path == "桌面")
                    _currentFolder = _desktop;
                else
                    _currentFolder = new DirectoryInfo((listViewFiles.SelectedItem as FolderPathImage).Path);
            }
            else
            {
                _currentFolder = _currentFolder.CreateSubdirectory((listViewFiles.SelectedItem as FolderPathImage).Path);
            }
            //显示合法的搜索结果
            List<CableInfo> listFD = TraverseFolder();
            listViewChecked.DataContext = listFD;

            btnFolderUp.IsEnabled = true;
        }

        /// <summary>
        /// 返回上一层按钮
        /// </summary>
        private void BtnFolderUp_Click(object sender, RoutedEventArgs e)
        {
            if (_currentFolder.FullName == _desktop.FullName)
                _currentFolder = null;
            else
                _currentFolder = _currentFolder.Parent;

            List<CableInfo> listFD;
            if (_currentFolder == null)
            {
                TraverseRoot();
                listFD = null;
                btnFolderUp.IsEnabled = false;
            }
            else
            {
                txtPath.Text = _currentFolder.FullName;
                listFD = TraverseFolder();
            }

            //显示合法的搜索结果
            listViewChecked.DataContext = listFD;
        }
        #endregion

        #region 进入分析页面
        private void ListViewValid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ImportData();
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            ImportData();
        }

        private void ImportData()
        {
            CableInfo cinfo = listViewChecked.SelectedItem as CableInfo;
            if (cinfo == null)
            {
                return;
            }
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("ss:fff"));
            if (InitData(cinfo))
            {
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("ss:fff"));
                WholePage w = new WholePage();//用时24ms
                NavigationService.Navigate(w);
            }

        }
        #endregion

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="ci">电缆信息</param>
        /// <returns>是否加载成功</returns>
        private bool InitData(CableInfo ci)
        {
            AnalyseState gd = new AnalyseState();
            MeasureState.CableInfo = ci;
            gd.Path = ci.Path;
            FileInfo[] fis = ci.Path.GetFiles("*.zdb", SearchOption.TopDirectoryOnly);
            if (fis.Length == 0)
            {
                MessageBox.Show("无数据文件", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            foreach (var item in fis)
            {
                DataInfo di = DataInfo.ReadInfoOnly(item);
                gd.DataInfos.Add(item.Name, di);
            }
            gd.HvParam = HvParam.GetHvParam();

            FileInfo mapFile = new FileInfo(gd.Path.FullName + "\\result.map");
            if (mapFile.Exists)
            {
                AnalyseState.Instance.AllMapResults = PulsePair.ReadFile(mapFile);
            }

            //标定信息
            for (int i = 0; i < 3; i++)
            {
                string file = "ABC".Substring(i, 1) + ".cal";

                FileInfo fileInfo = new FileInfo(MeasureState.CableInfo.Path.FullName + "/" + file);
                if (!fileInfo.Exists)
                {
                    continue;
                }
                gd.CalibrationInfos[i] = CalibrationInfo.ReadFile(fileInfo);
                if (Properties.Settings.Default.DischargeUnit == 0)//mV
                {
                    Params.mVTopC[i] = 1;
                }
                else//pC
                {
                    Params.mVTopC[i] = gd.CalibrationInfos[i].PcPerMv;
                }
            }
            //读PRP
            FileInfo prpFile = new FileInfo(MeasureState.CableInfo.Path.FullName + "/prpd.dat");
            if (prpFile.Exists)
            {
                AnalyseState.Instance.Prp = Prp.ReadFile(prpFile);
            }

            return true;
        }

        /// <summary>
        /// 供ListView数据绑定的类，文件夹图片及路径
        /// </summary>
        private class FolderPathImage
        {
            public string ImageUrl { get; set; }
            public string Path { get; set; }
        }

    }//end class
}
