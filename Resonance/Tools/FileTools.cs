using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tools
{
    /// <summary>
    /// 负责打开文件对话框
    /// </summary>
    class FileTools
    {
        /// <summary>
        /// 读属性文件
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ReadProperties(FileInfo fileInfo)
        {
            using (StreamReader sr = fileInfo.OpenText())
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] str = s.Split('=');
                    if (str.Length > 1)
                    {
                        dic.Add(str[0].Trim(), str[1].Trim());
                    }
                }
                sr.Close();
                return dic;
            }
        }

        /// <summary>
        /// 写属性文件
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="content"></param>
        public static void WriteProperties(FileInfo fileInfo, Dictionary<string, string> content)
        {
            using (StreamWriter sw = fileInfo.CreateText())
            {
                foreach (var item in content)
                {
                    sw.WriteLine(item.Key + "=" + item.Value);
                }
            }
        }

        ////打开文件对话框，默认程序当前目录
        //public static FileInfo OpenFile(string type)
        //{
        //    return OpenFile(Settings.DataDirectory, type);
        //}

        //打开文件对话框

        //public static FileInfo OpenFile(string initialDir, string type)

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="type">后缀名</param>
        /// <returns></returns>
        public static FileInfo OpenFile(string name, string type)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = type + " files (" + name + "." + type + ")|" + name + "." + type;
            openFileDialog.FilterIndex = 1;
            openFileDialog.FileName = name + "." + type;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "导入数据";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                return fileInfo;
            }
            return null;
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="type">后缀名</param>
        /// <returns></returns>
        public static FileInfo OpenFile(string type)
        {
            return OpenFile("*", type);
        }

        /// <summary>
        /// 打开多个文件
        /// </summary>
        /// <param name="type">后缀名</param>
        /// <returns></returns>
        public static FileInfo[] OpenFiles(string type)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            //openFileDialog.InitialDirectory = initialDir;
            openFileDialog.Filter = type + " files (*." + type + ")|*." + type;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "导入数据";


            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                FileInfo[] fileinfos = new FileInfo[openFileDialog.FileNames.Length];
                for (int i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    fileinfos[i] = new FileInfo(openFileDialog.FileNames[i]);
                }
                return fileinfos;
            }
            return null;
        }

        /// <summary>
        /// 打开文件夹，返回目录字符串
        /// </summary>
        /// <returns></returns>
        public static string OpenDirectory()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            //fbd.SelectedPath = Settings.DataDirectory;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            return null;
        }

        /// <summary>
        /// 打开保存对话框
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FileInfo SaveFile(string name, string filter)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            //saveFileDialog.InitialDirectory = "";
            saveFileDialog.Filter = filter + " files (*." + filter + ")|*." + filter;
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = name;//这里只是文件名
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return new FileInfo(saveFileDialog.FileName);//这里会自动变为完整路径加文件名，挺不错
            }
            return null;
        }

        /// <summary>
        /// 序列化文件
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="obj"></param>
        public static void Serialize(FileInfo fileInfo, object obj)
        {
            if (!fileInfo.Exists)
            {
                using (FileStream fs = fileInfo.Create())
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    serializer.Serialize(fs, obj);
                }
            }
            else
            {
                using (FileStream fs = fileInfo.OpenWrite())
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    serializer.Serialize(fs, obj);
                }
            }
        }

        /// <summary>
        /// 反序列化文件
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static object Deserialize(FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                using (FileStream fs = fileInfo.OpenRead())
                {
                    BinaryFormatter deserializer = new BinaryFormatter();
                    object obj = deserializer.Deserialize(fs);
                    return obj;
                }
            }
            return null;
        }

    }//end class
}//end namespace
