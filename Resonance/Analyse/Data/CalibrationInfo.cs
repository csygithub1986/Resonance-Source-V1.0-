using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Resonance
{
    /// <summary>
    /// 标定数据
    /// </summary>
    public class CalibrationInfo
    {
        /// <summary>
        /// 放电量 pc
        /// </summary>
        public double Discharge;

        /// <summary>
        /// 幅值 mV
        /// </summary>
        public double Amplitude;

        /// <summary>
        /// 波速 m/us
        /// </summary>
        public double Velocity;

        /// <summary>
        /// //衰减系数 Exp(-αt)，按us算
        /// </summary>
        public double Attenuation;

        /// <summary>
        /// 放电量，电压比值
        /// </summary>
        public double PcPerMv
        {
            get
            {
                if (Amplitude != 0)
                {
                    return Discharge / Amplitude;
                }
                return 0;
            }
        }

        /// <summary>
        /// 用于显示标定细节的截选数据
        /// </summary>
        public short[] ShowData;

        /// <summary>
        /// 入射波索引
        /// </summary>
        public int Index1;

        /// <summary>
        /// 反射波索引
        /// </summary>
        public int Index2;

        /// <summary>
        /// 全部标定数据
        /// </summary>
        public short[] AllData;

        /// <summary>
        /// 量程
        /// </summary>
        public int RangeIndex;

        /// <summary>
        /// 从文件读出标定数据
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static CalibrationInfo ReadFile(FileInfo fileInfo)
        {
            CalibrationInfo cd = new CalibrationInfo();
            using (BinaryReader bw = new BinaryReader(fileInfo.OpenRead()))
            {
                cd.Discharge = bw.ReadDouble();
                cd.Amplitude = bw.ReadDouble();
                cd.Velocity = bw.ReadDouble();
                cd.Attenuation = bw.ReadDouble();

                cd.RangeIndex = bw.ReadInt32();//量程

                int showLen = bw.ReadInt32();
                byte[] temp = bw.ReadBytes(showLen * 2);
                cd.ShowData = new short[showLen];
                for (int i = 0; i < showLen; i++)
                {
                    cd.ShowData[i] = BitConverter.ToInt16(temp, i * 2);
                }

                cd.Index1 = bw.ReadInt32();
                cd.Index2 = bw.ReadInt32();

                int allLen = bw.ReadInt32();
                temp = bw.ReadBytes(allLen * 2);
                cd.AllData = new short[allLen];
                for (int i = 0; i < allLen; i++)
                {
                    cd.AllData[i] = BitConverter.ToInt16(temp, i * 2);
                }
            }
            return cd;
        }

        /// <summary>
        /// 将标定数据写入文件
        /// </summary>
        /// <param name="cData"></param>
        /// <param name="fileInfo"></param>
        public static void WriteFile(CalibrationInfo cData, FileInfo fileInfo)
        {
            using (BinaryWriter bw = new BinaryWriter(fileInfo.Create()))
            {
                bw.Write(cData.Discharge);
                bw.Write(cData.Amplitude);
                bw.Write(cData.Velocity);
                bw.Write(cData.Attenuation);
                bw.Write(cData.RangeIndex);
                //showdata
                bw.Write(cData.ShowData.Length);
                byte[] temp = new byte[cData.ShowData.Length * 2];
                for (int i = 0; i < cData.ShowData.Length; i++)
                {
                    byte[] bs = BitConverter.GetBytes(cData.ShowData[i]);
                    temp[2 * i] = bs[0];
                    temp[2 * i + 1] = bs[1];
                }
                bw.Write(temp);

                bw.Write(cData.Index1);
                bw.Write(cData.Index2);
                //alldata
                bw.Write(cData.AllData.Length);
                temp = new byte[cData.AllData.Length * 2];
                for (int i = 0; i < cData.AllData.Length; i++)
                {
                    byte[] bs = BitConverter.GetBytes(cData.AllData[i]);
                    temp[2 * i] = bs[0];
                    temp[2 * i + 1] = bs[1];
                }
                bw.Write(temp);

            }
        }
    }
}
