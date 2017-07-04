using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Resonance
{
    /// <summary>
    /// 记录高压和局放数据的类
    /// </summary>
    public class DataInfo
    {
        /// <summary>
        /// 相序，0,1,2
        /// </summary>
        public int Phase;

        /// <summary>
        /// 加压等级，几倍U0
        /// </summary>
        public double VoltageLevel;

        /// <summary>
        /// 测试日期
        /// </summary>
        public DateTime TestDate;

        /// <summary>
        /// indexs[0]=0，高压周期起始点
        /// </summary>
        public int[] Indexs;

        /// <summary>
        /// 最大放电幅值
        /// </summary>
        public double MaxPd;

        //高压参数
        public double Fre;
        public double Cap;
        public double Damp;
        public double Tanδ;

        /// <summary>
        /// 量程
        /// </summary>
        public int RangeIndex;

        /// <summary>
        /// 将数据写入文件
        /// </summary>
        /// <param name="file">文件名</param>
        /// <param name="d">测试数据信息</param>
        /// <param name="hvData">高压数据</param>
        /// <param name="pdData">局放数据</param>
        public static void Save(FileInfo file, DataInfo d, short[] hvData, short[] pdData)
        {
            using (BinaryWriter bw = new BinaryWriter(file.Create()))
            {
                bw.Write(d.RangeIndex);
                bw.Write(d.Phase);
                bw.Write(d.VoltageLevel);
                bw.Write(d.Fre);
                bw.Write(d.TestDate.ToBinary());
                bw.Write(d.Cap);
                bw.Write(d.MaxPd);

                bw.Write(d.Indexs.Length);
                foreach (var item in d.Indexs)
                {
                    bw.Write(item);
                }
                byte[] temp = new byte[hvData.Length * 2];
                byte[] t = new byte[2];
                for (int i = 0; i < hvData.Length; i++)
                {
                    t = BitConverter.GetBytes(hvData[i]);
                    temp[2 * i] = t[0];
                    temp[2 * i + 1] = t[1];
                }
                bw.Write(hvData.Length);
                bw.Write(temp);
                temp = new byte[pdData.Length * 2];
                for (int i = 0; i < pdData.Length; i++)
                {
                    t = BitConverter.GetBytes(pdData[i]);
                    temp[2 * i] = t[0];
                    temp[2 * i + 1] = t[1];
                }
                bw.Write(pdData.Length);
                bw.Write(temp);
            }
        }

        /// <summary>
        /// 从文件读入测试数据
        /// </summary>
        /// <param name="file">文件</param>
        /// <param name="hvData">高压数据</param>
        /// <param name="pdData">局放数据</param>
        /// <returns>DataInfo对象</returns>
        public static DataInfo Read(FileInfo file, out short[] hvData, out short[] pdData)
        {
            DataInfo info = null;
            using (BinaryReader br = new BinaryReader(file.OpenRead()))
            {
                info = ReadInfo(br);
                //hvdata
                int len = br.ReadInt32();
                hvData = new short[len];
                byte[] buffer = new byte[len * 2];
                br.Read(buffer, 0, buffer.Length);
                for (int i = 0; i < len; i++)
                {
                    hvData[i] = (short)(buffer[2 * i] + buffer[2 * i + 1] * 256);
                }
                //pddata
                len = br.ReadInt32();
                pdData = new short[len];
                buffer = new byte[len * 2];
                br.Read(buffer, 0, buffer.Length);
                for (int i = 0; i < len; i++)
                {
                    pdData[i] = (short)(buffer[2 * i] + buffer[2 * i + 1] * 256);
                }
            }
            return info;
        }

        public static DataInfo Read(FileInfo file, out double[] hvData, out double[] pdData)
        {
            DataInfo info = null;
            using (BinaryReader br = new BinaryReader(file.OpenRead()))
            {
                info = ReadInfo(br);
                //hvdata
                int len = br.ReadInt32();
                hvData = new double[len];
                byte[] buffer = new byte[len * 2];
                br.Read(buffer, 0, buffer.Length);
                for (int i = 0; i < len; i++)
                {
                    hvData[i] = (short)(buffer[2 * i] + (buffer[2 * i + 1] << 8)) * Params.HV_Coeffi;
                }
                //pddata
                len = br.ReadInt32();
                pdData = new double[len];
                buffer = new byte[len * 2];
                br.Read(buffer, 0, buffer.Length);
                for (int i = 0; i < len; i++)
                {
                    pdData[i] = (short)(buffer[2 * i] + (buffer[2 * i + 1] << 8)) * Params.PD_Coeffi*Params.Range[info.RangeIndex];
                }
            }
            return info;
        }

        public static DataInfo ReadInfoOnly(FileInfo file)
        {
            DataInfo info = null;
            using (BinaryReader br = new BinaryReader(file.OpenRead()))
            {
                info = ReadInfo(br);
            }
            return info;
        }

        private static DataInfo ReadInfo(BinaryReader br)
        {
            DataInfo info = new DataInfo();
            info.RangeIndex = br.ReadInt32();
            info.Phase = br.ReadInt32();
            info.VoltageLevel = br.ReadDouble();
            info.Fre = br.ReadDouble();
            info.TestDate = DateTime.FromBinary(br.ReadInt64());
            info.Cap = br.ReadDouble();
            info.MaxPd = br.ReadDouble();
            int len = br.ReadInt32();
            info.Indexs = new int[len];
            for (int i = 0; i < len; i++)
            {
                info.Indexs[i] = br.ReadInt32();
            }
            return info;
        }

        public static DataInfo ReadRestainOnly(FileInfo file, out double[] restainPD)
        {
            DataInfo info = null;
            using (BinaryReader br = new BinaryReader(file.OpenRead()))
            {
                info = ReadInfo(br);
                //hvdata
                int len = br.ReadInt32();
                byte[] buffer = new byte[len * 2];
                br.Read(buffer, 0, buffer.Length);
                //pddata
                len = br.ReadInt32();
                //真正的局放长度
                len = info.Indexs[Params.RetainPeriod] * Params.Multi;
                restainPD = new double[len];
                buffer = new byte[len * 2];
                br.Read(buffer, 0, buffer.Length);
                for (int i = 0; i < len; i++)
                {
                    restainPD[i] = (short)(buffer[2 * i] + (buffer[2 * i + 1] << 8)) * Params.PD_Coeffi * Params.Range[info.RangeIndex];
                }
            }
            return info;
        }
    }
}
