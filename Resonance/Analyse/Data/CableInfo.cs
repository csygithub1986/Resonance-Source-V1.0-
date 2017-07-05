using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Resonance
{
    /// <summary>
    /// 电缆信息
    /// </summary>
    public class CableInfo
    {
        /// <summary>
        /// 变电站
        /// </summary>
        public string Station { get; set; }

        /// <summary>
        /// 测试日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 电缆长度
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// 电压等级
        /// </summary>
        public double U0 { get; set; }

        /// <summary>
        /// 接头
        /// </summary>
        public List<double> Joints { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 保存路径
        /// </summary>
        public DirectoryInfo Path { get; set; }

        /// <summary>
        /// 三相的谐振频率
        /// </summary>
        public float[] Freqs { get; set; }

        /// <summary>
        /// 根据额定电压U0，算出相电压最大值
        /// </summary>
        public double Upp
        {
            get
            {
                //2017-7更改，不除以根号3
                return U0 * Math.Sqrt(2);
            }
        }

        /// <summary>
        /// 构造函数初始化
        /// </summary>
        public CableInfo()
        {
            Date = DateTime.Now;
            Freqs = new float[3];
            Joints = new List<double>();
        }

        /// <summary>
        /// 从文件读入电缆信息
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static CableInfo ReadFile(FileInfo fileInfo)
        {
            CableInfo info = new CableInfo();
            if (fileInfo.Exists == false)
            {
                return null;
            }
            using (StreamReader sr = new StreamReader(fileInfo.FullName, Encoding.Default))
            {
                string str = sr.ReadLine();
                info.Station = str;
                str = sr.ReadLine();
                info.Date = DateTime.FromBinary(long.Parse(str));
                str = sr.ReadLine();
                info.Length = double.Parse(str);
                //str = sr.ReadLine();
                //string[] vels = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                //for (int i = 0; i < vels.Length; i++)
                //{
                //    info.Velocity[i] = double.Parse(vels[i]);
                //}
                //str = sr.ReadLine();
                //string[] atts = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                //for (int i = 0; i < atts.Length; i++)
                //{
                //    info.Attenuation[i] = double.Parse(atts[i]);
                //}

                //str = sr.ReadLine();
                //string[] dis = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                //for (int i = 0; i < dis.Length; i++)
                //{
                //    info.DischargeRate[i] = double.Parse(atts[i]);
                //}

                str = sr.ReadLine();
                info.U0 = double.Parse(str);
                str = sr.ReadLine();
                string[] fres = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < fres.Length; i++)
                {
                    info.Freqs[i] = float.Parse(fres[i]);
                }
                str = sr.ReadLine();
                string[] strs = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in strs)
                {
                    info.Joints.Add(double.Parse(item));
                }
                return info;
            }
        }

        /// <summary>
        /// 将电缆信息写入文件
        /// </summary>
        /// <param name="info"></param>
        /// <param name="fileInfo"></param>
        public static void WriteFile(CableInfo info, FileInfo fileInfo)
        {
            //本函数是新建电缆时调用，此文件肯定不存在
            using (FileStream fs = fileInfo.Create())
            {
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                sw.WriteLine(info.Station);
                sw.WriteLine(info.Date.ToBinary());
                sw.WriteLine(info.Length);
                //sw.WriteLine(info.Velocity[0] + ";" + info.Velocity[1] + ";" + info.Velocity[2]);
                //sw.WriteLine(info.Attenuation[0] + ";" + info.Attenuation[1] + ";" + info.Attenuation[2]);
                //sw.WriteLine(info.DischargeRate[0] + ";" + info.DischargeRate[1] + ";" + info.DischargeRate[2]);
                sw.WriteLine(info.U0);
                sw.WriteLine(info.Freqs[0] + ";" + info.Freqs[1] + ";" + info.Freqs[2]);

                foreach (var item in info.Joints)
                {
                    sw.Write(item + ";");
                }
                sw.WriteLine();
                sw.Write(info.Comment);
                sw.Flush();
            }
        }
    }
}
