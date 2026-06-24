using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.FiveAxis.Position
{
    /// <summary>
    /// 包括XY的位置类型
    /// </summary>
    [Serializable]
    public class PositionXY : PositionBase
    {
        public PositionXY() : this(0, 0)
        {
        }
        public PositionXY(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public PositionXY(PositionXY other)
        {
            this.CopyFrom(other);
        }

        public override void CopyFrom(PositionBase obj)
        {
            if (obj is PositionXY)
            {
                base.CopyFrom(obj);
                PositionXY other = obj as PositionXY;
                this.X = other.X;
                this.Y = other.Y;
            }
        }
        public double X { get; set; }
        public double Y { get; set; }
        /// <summary>
        /// 从位置字符串中解析得到位置
        /// </summary>
        /// <param name="str">位置字符串</param>
        /// <returns>位置</returns>
        public static PositionXY ParseString(string str)
        {
            string[] details = str.Split(new string[] { "," }, StringSplitOptions.None);
            if (details.Length != 2) return null;
            return new PositionXY()
            {
                X = Convert.ToDouble(details[0]),
                Y = Convert.ToDouble(details[1]),
            };
        }
        /// <summary>
        /// 从位置列表中解析得到位置
        /// </summary>
        /// <param name="lis"></param>
        /// <returns></returns>
        public static PositionXY FromLis(IEnumerable<double> lis)
        {
            double[] vs = lis.ToArray();
            if (vs.Length != 2) return null;
            return new PositionXY()
            {
                X = Convert.ToDouble(vs[0]),
                Y = Convert.ToDouble(vs[1]),
            };
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", X, Y);
        }
        public override string ToString(string fmt)
        {
            return string.Format("{0},{1}", X.ToString(fmt), Y.ToString(fmt));
        }
        public override PositionBase Clone()
        {
            return new PositionXY(this);
        }

        /// <summary>
        /// 获取当前位置类型
        /// </summary>
        /// <returns></returns>
        protected override PositionCodeType GetCurrentPosiCode()
        {
            PositionCodeType total = base.GetCurrentPosiCode();
            total |= PositionCodeType.X | PositionCodeType.Y;
            return total;
        }
        /// <summary>
        /// 根据坐标轴类型获取坐标值
        /// </summary>
        /// <param name="coType">坐标轴类型</param>
        /// <param name="posi">坐标值</param>
        /// <returns></returns>
        public override bool GetPosition(PositionCodeType coType, out double posi)
        {
            if (base.GetPosition(coType, out posi)) return true;
            if (!CheckCodeInLimit(coType)) return false;
            if (coType == PositionCodeType.X)
            {
                posi = X;
                return true;
            }
            else if (coType == PositionCodeType.Y)
            {
                posi = Y;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 根据坐标轴类型设置坐标值
        /// </summary>
        /// <param name="coType">坐标轴类型</param>
        /// <param name="posi">坐标值</param>
        /// <returns></returns>
        public override bool SetPosition(PositionCodeType coType, double posi)
        {
            if (base.SetPosition(coType, posi)) return true;
            if (!CheckCodeInLimit(coType)) return false;
            if (coType == PositionCodeType.X)
            {
                X = posi;
                return true;
            }
            else if (coType == PositionCodeType.Y)
            {
                Y = posi;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 根据坐标轴类型获取坐标列表
        /// </summary>
        /// <param name="coType">坐标轴类型</param>
        /// <param name="posiLis">位置列表</param>
        /// <returns></returns>
        public override bool GetPosition(PositionCodeType coType, out List<double> posiLis)
        {
            if (!base.GetPosition(coType, out posiLis)) return false;
            if ((coType & PositionCodeType.X) > 0) posiLis.Add(X);
            if ((coType & PositionCodeType.Y) > 0) posiLis.Add(Y);
            return true;
        }

        /// <summary>
        /// 获取一系列坐标轴的位置,排列顺序为默认顺序
        /// </summary>
        /// <param name="posiLis">坐标轴位置列表</param>
        /// <returns></returns>
        public override bool GetPosition(out List<double> posiLis)
        {
            if (!base.GetPosition(out posiLis)) return false;
            posiLis.Add(this.X);
            posiLis.Add(this.Y);
            return true;
        }
    }
}
