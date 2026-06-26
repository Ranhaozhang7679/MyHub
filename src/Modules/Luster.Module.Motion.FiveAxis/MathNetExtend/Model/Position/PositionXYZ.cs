using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.FiveAxis.Position
{
    /// <summary>
    /// 包括XYZ的坐标类型
    /// </summary>
    [Serializable]
    public class PositionXYZ : PositionXY
    {
        public PositionXYZ()
        {
            this.Z = 0;
        }
        public PositionXYZ(double x, double y, double z) : base(x, y)
        {
            this.Z = z;
        }

        public PositionXYZ(PositionXYZ other)
        {
            this.CopyFrom(other);
        }

        public override void CopyFrom(PositionBase obj)
        {
            if (obj is PositionXYZ)
            {
                base.CopyFrom(obj);
                PositionXYZ other = obj as PositionXYZ;
                this.Z = other.Z;
            }
        }

        public double Z { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1}", base.ToString(), Z);
        }
        public override string ToString(string fmt)
        {
            return string.Format("{0},{1}", base.ToString(fmt), Z.ToString(fmt));
        }
        public override PositionBase Clone()
        {
            return new PositionXYZ(this);
        }
        /// <summary>
        /// 从位置字符串中获取位置
        /// </summary>
        /// <param name="str">位置字符串</param>
        /// <returns>位置</returns>
        public static new PositionXYZ ParseString(string str)
        {
            string[] details = str.Split(new string[] { "," }, StringSplitOptions.None);
            if (details.Length != 3) return null;
            return new PositionXYZ()
            {
                X = Convert.ToDouble(details[0]),
                Y = Convert.ToDouble(details[1]),
                Z = Convert.ToDouble(details[2]),
            };
        }
        /// <summary>
        /// 从位置列表中解析得到位置
        /// </summary>
        /// <param name="lis"></param>
        /// <returns></returns>
        public static new PositionXYZ FromLis(IEnumerable<double> lis)
        {
            double[] vs = lis.ToArray();
            if (vs.Length != 3) return null;
            return new PositionXYZ()
            {
                X = Convert.ToDouble(vs[0]),
                Y = Convert.ToDouble(vs[1]),
                Z = Convert.ToDouble(vs[2]),
            };
        }
        /// <summary>
        /// 获取当前位置类型
        /// </summary>
        /// <returns></returns>
        protected override PositionCodeType GetCurrentPosiCode()
        {
            PositionCodeType total = base.GetCurrentPosiCode();
            total |= PositionCodeType.Z;
            return total;
        }
        /// <summary>
        /// 根据坐标轴类型获取位置值
        /// </summary>
        /// <param name="coType">坐标轴</param>
        /// <param name="posi">位置值</param>
        /// <returns></returns>
        public override bool GetPosition(PositionCodeType coType, out double posi)
        {
            if (base.GetPosition(coType, out posi)) return true;
            if (!CheckCodeInLimit(coType)) return false;
            if (coType == PositionCodeType.Z)
            {
                posi = Z;
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
            if (coType == PositionCodeType.Z)
            {
                Z = posi;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 根据坐标轴类型获取位置列表
        /// </summary>
        /// <param name="coType">坐标轴类型</param>
        /// <param name="posiLis">位置列表</param>
        /// <returns></returns>
        public override bool GetPosition(PositionCodeType coType, out List<double> posiLis)
        {
            if (!base.GetPosition(coType, out posiLis)) return false;
            if (!CheckCodeInLimit(coType)) return false;
            if ((coType & PositionCodeType.Z) > 0) posiLis.Add(Z);
            return true;
        }
        /// <summary>
        /// 获取位置列表
        /// </summary>
        /// <param name="posiLis">位置列表</param>
        /// <returns></returns>
        public override bool GetPosition(out List<double> posiLis)
        {
            if (!base.GetPosition(out posiLis)) return false;
            posiLis.Add(this.Z);
            return true;
        }
    }
}
