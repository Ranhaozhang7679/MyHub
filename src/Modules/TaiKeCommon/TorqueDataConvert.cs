using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaiKeCommon
{
    public class TorqueDataConvert
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>         //原始字节数据
        /// <param name="torq_double"></param>  //扭力数组
        /// <param name="Angle_double"></param> //角度数组
        /// <param name="count"></param>        //原始字节数据个数
        /// <param name="titleAxisY"></param>   //Y轴坐标名称
        /// <param name="indexSSL"></param>     //着座点位置
        /// <param name="optID"></param>        //锁付编号
        /// <returns></returns>
        public bool DataConvert_TCR(byte[] data, ref double[] torq_double, ref double[] Angle_double, int TK_dataLen, ref string titleAxisY, ref int indexSSL, ref int optID)   //阅读模式数据解析
        {
            //int TK_dataLen = (count - 46) / 4;        //扭力采集点的个数
            int[] torq_data = new int[TK_dataLen];
            int[] Angle_data = new int[TK_dataLen];
            double[] torq_double_1 = new double[TK_dataLen];
            double[] Angle_double_1 = new double[TK_dataLen];
            byte[] HeaderBlock = new byte[16];    //头块
            byte[] FooterBlock = new byte[20];    //尾块

            if ((data[0] == 0xAA) && (data[1] == 0xB1))
            {
                for (int i = 0; i < 16; i++)
                    HeaderBlock[i] = data[i + 9];              //头块数据
                for (int i = 0; i < FooterBlock.Length; i++)
                {
                    FooterBlock[i] = data[i + 4 * TK_dataLen + 25];           //尾块数据
                }

                int y = data[10] >> 4;     //字节右移4位取高位
                int multiple = Convert.ToInt32(Math.Pow(10, y));//数值倍率，控制小数点
                string unitAxisY = "kgf.cm";             //Y轴单位
                y = data[10] & 15;         //和00001111按位与取低位
                switch (y)                  //获取扭力单位
                {
                    case 0:
                        unitAxisY = "Unknown";
                        break;
                    case 1:
                        unitAxisY = "gf.cm";
                        break;
                    case 2:
                        unitAxisY = "kgf.cm";
                        break;
                    case 3:
                        unitAxisY = "kgf.m";
                        break;
                    case 4:
                        unitAxisY = "mN.m";
                        break;
                    case 5:
                        unitAxisY = "cN.m";
                        break;
                    case 6:
                        unitAxisY = "N.m";
                        break;
                    case 7:
                        unitAxisY = "ozf.in";
                        break;
                    case 8:
                        unitAxisY = "lbf.in";
                        break;
                    case 9:
                        unitAxisY = "lbf.ft";
                        break;
                    default:
                        break;
                }
                titleAxisY = "Torque / " + unitAxisY;       //更改Y轴标题

                int AngC1 = data[21] * 256 + data[22];    //角度计算系数1
                int AngC2 = data[23];                     //角度计算系数2
                int CA_start = 0;                         //找出扭力大于0时的位置
                double AngleNumber = 0;                   //计算角度的临时值
                string ScrewStatusStart = "";             //找到不为0的扭力标志
                for (int i = 0; i < TK_dataLen; i++)
                {
                    torq_data[i] = data[2 * i + 25] * 256 + data[2 * i + 26];

                    if (torq_data[i] > 32767)    //按负数处理
                    {
                        torq_data[i] = torq_data[i] - 65536;
                    }
                    torq_double_1[i] = (double)(torq_data[i]) / multiple;


                    Angle_data[i] = data[2 * i + 2 * TK_dataLen + 25] * 256 + data[2 * i + 2 * TK_dataLen + 26];
                    if (Angle_data[i] > 32767)    //按负数处理
                    {
                        Angle_data[i] = Angle_data[i] - 65536;
                    }
                    AngleNumber = AngleNumber + (double)(Angle_data[i]);
                    Angle_double_1[i] = AngleNumber * AngC1 / Math.Pow(10, AngC2);

                    if (torq_double_1[i] != 0 && ScrewStatusStart == "")   //去掉初段为0的部分
                    {
                        CA_start = i;
                        ScrewStatusStart = "Start";
                    }
                }

                indexSSL = data[11] * 256 + data[12];  //SSL位置
                if (indexSSL != 0)
                    indexSSL = indexSSL - CA_start + 1;
                if (indexSSL < 0)
                    indexSSL = 0;

                optID = data[15] * 256 + data[16];                   //操作编号
                int length = TK_dataLen - CA_start + 1;
                torq_double = new double[length];                     //去掉初段为0的部分后重新整理
                Angle_double = new double[length];
                for (int i = 1; i < length; i++)
                {
                    torq_double[i] = torq_double_1[i - 1 + CA_start];
                    Angle_double[i] = Angle_double_1[i - 1 + CA_start];
                    Angle_double[i] = Math.Round(Angle_double[i], 2);  //角度保留2位小数
                }
            }
            return true;
        }
    }
}
