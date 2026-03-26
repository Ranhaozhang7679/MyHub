using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaiKeCommon
{
    public class TaiKeData : ICloneable
    {
        public string[] arrResultTK = new string[44];   //电批1过程数据

        public byte[] originalData;    //截取原始数据
        public double[] torq_double = new double[200];    //电批1已算好的扭力
        public double[] Angle_double = new double[200];   //电批1已算好的角度
        public double Max_Torque;   //电批1最大扭力
        public double Max_Angle;    //电批1最大角度  
        public string titleAxisY = "Torque / kgf.cm";//Y轴标题
        public int indexSSL = 0;    //SSL位置

        public string EC = "";  //错误代码
        public string T1 = "";  //第一阶段(螺丝旋入)的时间
        public string T2 = "";  //第二阶段(扭矩爬坡)的时间
        public string T3 = "";  //第三阶段(保持扭矩)的时间
        public string Tt = "";  //总时间
        public string A1 = "";  //第一阶段(螺丝旋入)的角度
        public string A2 = "";  //第二阶段(扭矩爬坡)自的角度
        public string A3 = "";  //第三阶段(保持扭矩)的角度
        public string SSL = "";  //第一阶段和第二阶段分界点
        public string MaxAngle = "";  //总角度
        public string MaxTorque = "";  //最大扭矩值
        public string SeatingTorque = "";  //着座扭矩
        public string ClampTorque = "";  //夹紧扭矩
        public string PrevailingTorque = "";  //摩擦力扭矩
        public string MC = "";  //所选的程序


        public object Clone()
        {
            var clone = new TaiKeData();

            clone.arrResultTK = (string[])arrResultTK.Clone();
            clone.originalData = (byte[])originalData.Clone();
            clone.torq_double = (double[])torq_double.Clone();
            clone.Angle_double = (double[])Angle_double.Clone();
            clone.Max_Torque = Max_Torque;
            clone.Max_Angle = Max_Angle;
            clone.titleAxisY = titleAxisY;
            clone.indexSSL = indexSSL;

            clone.EC = EC;
            clone.T1 = T1;
            clone.T2 = T2;
            clone.T3 = T3;
            clone.Tt = Tt;
            clone.A1 = A1;
            clone.A2 = A2;
            clone.A3 = A3;
            clone.SSL = SSL;
            clone.MaxAngle = MaxAngle;
            clone.MaxTorque = MaxTorque;
            clone.SeatingTorque = SeatingTorque;
            clone.ClampTorque = ClampTorque;
            clone.PrevailingTorque = PrevailingTorque;
            clone.MC = MC;

            return clone;
        }
    }
}
