using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace Resonance
{
    /// <summary>
    /// PRPD类
    /// </summary>
    public class Prp
    {
        /// <summary>
        /// 将PRPD信息写入文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="prpLis"></param>
        public static void WriteFile(FileInfo file, List<Point>[] prpLis)
        {
            using (BinaryWriter bw = new BinaryWriter(file.Create()))
            {
                for (int i = 0; i < 3; i++)
                {
                    bw.Write(prpLis[i].Count);
                    byte[] temp = new byte[prpLis[i].Count * 16];
                    for (int j = 0; j < prpLis[i].Count; j++)
                    {
                        Array.Copy(BitConverter.GetBytes(prpLis[i][j].X), 0, temp, j * 16, 8);
                        Array.Copy(BitConverter.GetBytes(prpLis[i][j].Y), 0, temp, j * 16 + 8, 8);
                    }
                    bw.Write(temp);
                }
            }
        }

        /// <summary>
        /// 从文件中读取PRPD信息
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static List<Point>[] ReadFile(FileInfo file)
        {
            List<Point>[] prpList = new List<Point>[3];
            using (BinaryReader br = new BinaryReader(file.OpenRead()))
            {
                for (int i = 0; i < 3; i++)
                {
                    prpList[i] = new List<Point>();
                    int len = br.ReadInt32();
                    for (int j = 0; j < len; j++)
                    {
                        double x = br.ReadDouble();
                        double y = br.ReadDouble();
                        prpList[i].Add(new Point(x, y));
                    }
                }
            }
            return prpList;
        }
    }
}
