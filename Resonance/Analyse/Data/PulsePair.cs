using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Resonance
{
    /// <summary>
    ///脉冲匹配对
    /// </summary>
    [Serializable]
    public class PulsePair
    {
        //定位结果字段

        /// <summary>
        /// 距离
        /// </summary>
        public double Distance;

        /// <summary>
        /// 峰值
        /// </summary>
        public double Amplitude;

        /// <summary>
        /// 相位
        /// </summary>
        public double Phase;

        /// <summary>
        /// 入射峰时间
        /// </summary>
        public int EnterIndex;

        /// <summary>
        /// 反射峰时间
        /// </summary>
        public int ReflectIndex;

        /// <summary>
        /// 反射峰集合
        /// </summary>
        public List<int> RTList = new List<int>();


        //public double T;//归一化脉宽，越窄脉冲越陡峭
        //public double F;//归一化带宽，越宽说明脉冲越原始


        //不写入文件的属性
        /// <summary>
        /// 隶属的文件
        /// </summary>
        public string BelongTo;

        /// <summary>
        /// 放电量
        /// </summary>
        public double Q;

        #region Override
        /// <summary>
        /// 如果入射时间相同，则判定为相同
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            var o = obj as PulsePair;
            if (o.EnterIndex == EnterIndex && o.ReflectIndex == ReflectIndex)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (int)(EnterIndex + ReflectIndex);
        }
        #endregion

        #region 文件存取
        /// <summary>
        /// 从文件读取脉冲匹配信息
        /// </summary>
        /// <param name="fileInfo">文件</param>
        /// <returns>脉冲匹配信息</returns>
        public static Dictionary<string, List<PulsePair>> ReadFile(FileInfo fileInfo)
        {
            Dictionary<string, List<PulsePair>> mapResult = new Dictionary<string, List<PulsePair>>();
            using (BinaryReader br = new BinaryReader(fileInfo.OpenRead()))
            {
                List<PulsePair> mapList = null;
                PulsePair mapItem = null;
                ASCIIEncoding asc = new ASCIIEncoding();
                string name = "";
                while (true)
                {
                    int head = br.ReadInt32();
                    if (head == (int)Head.Name)
                    {
                        int len = br.ReadInt32();
                        byte[] buffer = new byte[len];
                        br.Read(buffer, 0, len);
                        name = asc.GetString(buffer);
                        mapList = new List<PulsePair>();
                        mapResult.Add(name, mapList);
                    }
                    else if (head == (int)Head.Data)
                    {
                        mapItem = new PulsePair();
                        mapItem.BelongTo = name;
                        mapItem.Distance = br.ReadDouble();
                        mapItem.Amplitude = br.ReadDouble();
                        mapItem.Phase = br.ReadDouble();
                        mapItem.EnterIndex = br.ReadInt32();
                        mapItem.ReflectIndex = br.ReadInt32();
                        int count = br.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            mapItem.RTList.Add(br.ReadInt32());
                        }
                        mapList.Add(mapItem);
                    }
                    else if (head == (int)Head.End)
                    {
                        break;
                    }
                }
                return mapResult;
            }
        }

        /// <summary>
        /// 将脉冲匹配信息写入文件
        /// </summary>
        /// <param name="fileInfo">文件</param>
        public static void WriteMapFile(FileInfo fileInfo)
        {
            using (BinaryWriter bw = new BinaryWriter(fileInfo.Create()))//始终覆盖源文件
            {
                foreach (KeyValuePair<string, List<PulsePair>> mapItem in AnalyseState.Instance.AllMapResults)
                {
                    bw.Write((int)Head.Name);  //文件名头
                    //写入fileName
                    ASCIIEncoding asc = new ASCIIEncoding();
                    byte[] b = asc.GetBytes(mapItem.Key);
                    bw.Write(b.Length);
                    bw.Write(b);
                    //写入thresh
                    if (mapItem.Value != null)
                    {
                        foreach (PulsePair listItem in mapItem.Value)
                        {
                            bw.Write((int)Head.Data);//数据头
                            //写入距离、幅值、相位、 入射时间、反射时间
                            bw.Write(listItem.Distance);
                            bw.Write(listItem.Amplitude);
                            bw.Write(listItem.Phase);
                            bw.Write(listItem.EnterIndex);
                            bw.Write(listItem.ReflectIndex);
                            // (反射峰集合)
                            bw.Write(listItem.RTList.Count);
                            foreach (var bpeak in listItem.RTList)
                            {
                                bw.Write(bpeak);
                            }
                        }
                    }
                }
                bw.Write((int)Head.End);//结束标志 
                bw.Close();
            }
        }

        /// <summary>
        /// （弃用）
        /// </summary>
        /// <param name="fileInfo"></param>
        public static void WriteMapFile2(FileInfo fileInfo)
        {
            using (BinaryWriter bw = new BinaryWriter(fileInfo.Create()))//始终覆盖源文件
            {
                foreach (KeyValuePair<string, List<PulsePair>> mapItem in AnalyseState.Instance.AllMapResults)
                {
                    bw.Write((int)Head.Name);  //文件名头
                    //写入fileName
                    ASCIIEncoding asc = new ASCIIEncoding();
                    byte[] b = asc.GetBytes(mapItem.Key);
                    bw.Write(b.Length);
                    bw.Write(b);


                    Random ran=new Random();
                    if (mapItem.Value != null)
                    {
                        foreach (PulsePair listItem in mapItem.Value)
                        {
                            //if (mapItem.Key.Substring(0, 1) == "A")
                            //{
                            //    listItem.Distance=ran.NextDouble()
                            //}

                            bw.Write((int)Head.Data);//数据头
                            //写入距离、幅值、相位、 入射时间、反射时间
                            bw.Write(listItem.Distance);
                            bw.Write(listItem.Amplitude);
                            bw.Write(listItem.Phase);
                            bw.Write(listItem.EnterIndex);
                            bw.Write(listItem.ReflectIndex);
                            // (反射峰集合)
                            bw.Write(listItem.RTList.Count);
                            foreach (var bpeak in listItem.RTList)
                            {
                                bw.Write(bpeak);
                            }
                        }
                    }
                }
                bw.Write((int)Head.End);//结束标志 
                bw.Close();
            }
        }


        #endregion

        /// <summary>
        /// 数据保存协议中的枚举
        /// </summary>
        enum Head
        {
            Name, Data, End = 2
        }
    }

}
