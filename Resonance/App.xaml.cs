using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;


namespace Resonance
{
    /// <summary>
    /// 程序入口类
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 注册全局异常捕获事件
        /// </summary>
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        /// <summary>
        /// 全局异常捕获处理，写入error.log日志
        /// </summary>
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("程序异常" + Environment.NewLine + e.Exception.Message);

            //记录日志
            string appDir = System.Environment.CurrentDirectory;
            FileInfo errorLog = new FileInfo(appDir + "/error.log");
            StreamWriter sw = null;
            sw = new StreamWriter(errorLog.FullName, true);
            sw.Write(DateTime.Now.ToShortDateString() + "  " + DateTime.Now.ToShortTimeString() + "\t");
            sw.WriteLine(e.Exception.GetType().ToString() + "  " + e.Exception.Message);
            string[] strs = e.Exception.StackTrace.Split('\n');
            foreach (var item in strs)
            {
                if (item.Contains("Resonance"))
                {
                    sw.WriteLine("\t" + item);
                }
            }
            sw.Close();

            e.Handled = true;
        }
    }
}
