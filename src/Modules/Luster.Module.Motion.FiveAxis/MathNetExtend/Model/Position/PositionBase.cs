using RpcLibrary.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathNetExtend.Model.Position
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PositionBase : FieldToPropertyTypeDescriptor
    {
        public PositionBase()
        {
        }
        public PositionBase(PositionBase other)
        {
            this.CopyFrom(other);
        }
        public virtual void CopyFrom(PositionBase obj)
        {
        }
        /// <summary>
        /// 无效的坐标位置
        /// </summary>
        public static double INVALID = -999;

        /// <summary>
        /// 获取某个坐标轴的位置
        /// </summary>
        /// <param name="coType">坐标轴类型</param>
        /// <param name="posi">位置</param>
        /// <returns></returns>
        public virtual bool GetPosition(PositionCodeType coType, out double posi)
        {
            posi = 0;
            if (!CheckCodeInLimit(coType)) return false;
            return false;
        }
        /// <summary>
        /// 设置某个坐标轴的位置
        /// </summary>
        /// <param name="coType">坐标轴类型</param>
        /// <param name="posi">位置</param>
        /// <returns></returns>
        public virtual bool SetPosition(PositionCodeType coType, double posi)
        {
            if (!CheckCodeInLimit(coType)) return false;
            return false;
        }
        /// <summary>
        /// 获取一系列坐标轴的位置,排列顺序为坐标轴类型顺序
        /// </summary>
        /// <param name="coType">坐标轴类型，多个坐标轴按位或</param>
        /// <param name="posiLis">坐标轴位置列表</param>
        /// <returns></returns>
        public virtual bool GetPosition(PositionCodeType coType, out List<double> posiLis)
        {
            posiLis = new List<double>();
            if (!CheckCodeInLimit(coType)) return false;
            return true;
        }
        /// <summary>
        /// 获取一系列坐标轴的位置,排列顺序为默认顺序
        /// </summary>
        /// <param name="posiLis">坐标轴位置列表</param>
        /// <returns></returns>
        public virtual bool GetPosition(out List<double> posiLis)
        {
            posiLis = new List<double>();
            return true;
        }
        /// <summary>
        /// 获取一系列坐标轴的位置,排列顺序为默认顺序
        /// </summary>
        /// <returns>坐标轴位置列表</returns>
        public List<double> ToPosiLis()
        {
            List<double> posilis = new List<double>();
            GetPosition(out posilis);
            return posilis;
        }

        /// <summary>
        /// 判断坐标轴类型是否超限
        /// </summary>
        /// <param name="para">输入坐标轴类型</param>
        /// <returns></returns>
        public bool CheckCodeInLimit(PositionCodeType para)
        {
            return CheckCodeInLimit(para, GetCurrentPosiCode());
        }
        /// <summary>
        /// 获取当前位置类型
        /// </summary>
        /// <returns></returns>
        protected virtual PositionCodeType GetCurrentPosiCode()
        {
            return PositionCodeType.Unknown;
        }
        /// <summary>
        /// 判断坐标轴类型是否超限
        /// </summary>
        /// <param name="para">输入坐标轴类型</param>
        /// <param name="total">全部类型</param>
        /// <returns></returns>
        protected bool CheckCodeInLimit(PositionCodeType para, PositionCodeType total)
        {
            if (para == PositionCodeType.Unknown) return false;
            if ((para | total) > total) return false;
            return true;
        }

        public virtual string ToString(string fmt)
        {
            return string.Empty;
        }

        public virtual PositionBase Clone()
        {
            return new PositionBase(this);
        }

        /// <summary>
        /// 坐标计算
        /// </summary>
        /// <param name="p">操作参数</param>
        /// <param name="func">计算方法</param>
        /// <returns>计算结果</returns>
        public PositionBase Calculate(PositionBase p, Func<double, double, double> func)
        {
            PositionBase posi = this.Clone();
            foreach (PositionCodeType type in Enum.GetValues(typeof(PositionCodeType)))
            {
                double currentPosi = 0;
                if (!posi.GetPosition(type, out currentPosi)) continue;
                double tempPosi = 0;
                if (!p.GetPosition(type, out tempPosi)) continue;
                if (!posi.SetPosition(type, func(currentPosi, tempPosi))) return null;
            }
            return posi;
        }
        /// <summary>
        /// 坐标计算
        /// </summary>
        /// <param name="p">操作参数</param>
        /// <param name="func">计算方法</param>
        /// <returns>计算结果</returns>
        public T Calculate<T>(PositionBase p, Func<double, double, double> func) where T : PositionBase
        {
            PositionBase posi = this.Clone();
            foreach (PositionCodeType type in Enum.GetValues(typeof(PositionCodeType)))
            {
                double currentPosi = 0;
                if (!posi.GetPosition(type, out currentPosi)) continue;
                double tempPosi = 0;
                if (!p.GetPosition(type, out tempPosi)) continue;
                if (!posi.SetPosition(type, func(currentPosi, tempPosi))) return null;
            }
            return posi as T;
        }
        /// <summary>
        /// 坐标计算
        /// </summary>
        /// <param name="para">操作参数</param>
        /// <param name="func">计算方法</param>
        /// <returns>计算结果</returns>
        public PositionBase Calculate(double para, Func<double, double, double> func)
        {
            PositionBase posi = this.Clone();
            foreach (PositionCodeType type in Enum.GetValues(typeof(PositionCodeType)))
            {
                double currentPosi = 0;
                if (!posi.GetPosition(type, out currentPosi)) continue;
                if (!posi.SetPosition(type, func(currentPosi, para))) return null;
            }
            return posi;
        }
        /// <summary>
        /// 坐标计算
        /// </summary>
        /// <param name="para">操作参数</param>
        /// <param name="func">计算方法</param>
        /// <returns>计算结果</returns>
        public T Calculate<T>(double para, Func<double, double, double> func) where T : PositionBase
        {
            PositionBase posi = this.Clone();
            foreach (PositionCodeType type in Enum.GetValues(typeof(PositionCodeType)))
            {
                double currentPosi = 0;
                if (!posi.GetPosition(type, out currentPosi)) continue;
                if (!posi.SetPosition(type, func(currentPosi, para))) return null;
            }
            return posi as T;
        }
        /// <summary>
        /// 重载操作符+,两个位置类型相加，结果类型与第一个参数类型相同
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static PositionBase operator +(PositionBase p1, PositionBase p2)
        {
            return p1.Calculate(p2, (a, b) => a + b);
        }
        /// <summary>
        /// 重载操作符-，两个位置类型相减，结果类型与第一个参数类型相同
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static PositionBase operator -(PositionBase p1, PositionBase p2)
        {
            return p1.Calculate(p2, (a, b) => a - b);
        }
        /// <summary>
        /// 重载操作符*，两个位置类型相乘，结果类型与第一个参数类型相同
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static PositionBase operator *(PositionBase p1, PositionBase p2)
        {
            return p1.Calculate(p2, (a, b) => a * b);
        }
        /// <summary>
        /// 重载操作符*，两个位置类型相乘，结果类型与第一个参数类型相同
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static PositionBase operator /(PositionBase p1, PositionBase p2)
        {
            return p1.Calculate(p2, (a, b) => a / b);
        }
        /// <summary>
        /// 重载操作符+，位置类型加上一个数值类型，结果类型与第一个参数类型相同
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static PositionBase operator +(PositionBase p1, double offset)
        {
            return p1.Calculate(offset, (a, b) => a + b);
        }
        /// <summary>
        /// 重载操作符-，位置类型减去一个数值类型，结果类型与第一个参数类型相同
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static PositionBase operator -(PositionBase p1, double offset)
        {
            return p1.Calculate(offset, (a, b) => a - b);
        }
        /// <summary>
        /// 重载操作符*，位置类型乘上一个数值类型，结果类型与第一个参数类型相同
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static PositionBase operator *(PositionBase p1, double rate)
        {
            return p1.Calculate(rate, (a, b) => a * b);
        }
        /// <summary>
        /// 重载操作符/，位置类型除以一个数值类型，结果类型与第一个参数类型相同
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static PositionBase operator /(PositionBase p1, double rate)
        {
            return p1.Calculate(rate, (a, b) => a / b);
        }
    }
    /// <summary>
    /// 坐标轴类型
    /// </summary>
    public enum PositionCodeType : uint
    {
        Unknown = 0,
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        RX = 1 << 3,
        RY = 1 << 4,
        RZ = 1 << 5,
        Extend = 1 << 6,
    }
}
