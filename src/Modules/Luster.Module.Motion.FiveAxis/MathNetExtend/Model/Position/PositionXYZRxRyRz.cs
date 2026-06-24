using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.FiveAxis.Position
{
    /// <summary>
    /// 包括XYZRxRyRz的坐标类型
    /// </summary>
    [Serializable]
    public class PositionXYZRxRyRz : PositionXYZ
    {
        public PositionXYZRxRyRz()
        {
            this.RX = 0;
            this.RY = 0;
            this.RZ = 0;
        }

        public PositionXYZRxRyRz(PositionXYZRxRyRz other)
        {
            this.CopyFrom(other);
        }

        public override void CopyFrom(PositionBase obj)
        {
            if (obj is PositionXYZRxRyRz)
            {
                base.CopyFrom(obj);
                PositionXYZRxRyRz other = obj as PositionXYZRxRyRz;
                this.RX = other.RX;
                this.RY = other.RY;
                this.RZ = other.RZ;
            }
        }
        public double RX { get; set; }
        public double RY { get; set; }
        public double RZ { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", base.ToString(), RX, RY, RZ);
        }
        public override string ToString(string fmt)
        {
            return string.Format("{0},{1},{2},{3}", base.ToString(fmt), RX.ToString(fmt), RY.ToString(fmt), RZ.ToString(fmt));
        }
        public override PositionBase Clone()
        {
            return new PositionXYZRxRyRz(this);
        }
        /// <summary>
        /// 根据位置字符串解析
        /// </summary>
        /// <param name="str">位置字符串</param>
        /// <returns>位置</returns>
        public static new PositionXYZRxRyRz ParseString(string str)
        {
            string[] details = str.Split(new string[] { "," }, StringSplitOptions.None);
            if (details.Length != 6) return null;
            return new PositionXYZRxRyRz()
            {
                X = Convert.ToDouble(details[0]),
                Y = Convert.ToDouble(details[1]),
                Z = Convert.ToDouble(details[2]),
                RX = Convert.ToDouble(details[3]),
                RY = Convert.ToDouble(details[4]),
                RZ = Convert.ToDouble(details[5]),
            };
        }
        /// <summary>
        /// 从位置列表中解析得到位置
        /// </summary>
        /// <param name="lis"></param>
        /// <returns></returns>
        public static new PositionXYZRxRyRz FromLis(IEnumerable<double> lis)
        {
            double[] vs = lis.ToArray();
            if (vs.Length != 6) return null;
            return new PositionXYZRxRyRz()
            {
                X = Convert.ToDouble(vs[0]),
                Y = Convert.ToDouble(vs[1]),
                Z = Convert.ToDouble(vs[2]),
                RX = Convert.ToDouble(vs[3]),
                RY = Convert.ToDouble(vs[4]),
                RZ = Convert.ToDouble(vs[5]),
            };
        }
        /// <summary>
        /// 获取当前位置类型
        /// </summary>
        /// <returns></returns>
        protected override PositionCodeType GetCurrentPosiCode()
        {
            PositionCodeType total = base.GetCurrentPosiCode();
            total |= PositionCodeType.RX | PositionCodeType.RY | PositionCodeType.RZ;
            return total;
        }
        /// <summary>
        /// 根据坐标轴类型获取坐标位置
        /// </summary>
        /// <param name="coType">坐标轴类型</param>
        /// <param name="posi">位置值</param>
        /// <returns></returns>
        public override bool GetPosition(PositionCodeType coType, out double posi)
        {
            if (base.GetPosition(coType, out posi)) return true;
            if (!CheckCodeInLimit(coType)) return false;
            if (coType == PositionCodeType.RX)
            {
                posi = RX;
                return true;
            }
            else if (coType == PositionCodeType.RY)
            {
                posi = RY;
                return true;
            }
            else if (coType == PositionCodeType.RZ)
            {
                posi = RZ;
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
            if (coType == PositionCodeType.RX)
            {
                RX = posi;
                return true;
            }
            else if (coType == PositionCodeType.RY)
            {
                RY = posi;
                return true;
            }
            else if (coType == PositionCodeType.RZ)
            {
                RZ = posi;
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
            if ((coType & PositionCodeType.RX) > 0) posiLis.Add(RX);
            if ((coType & PositionCodeType.RY) > 0) posiLis.Add(RY);
            if ((coType & PositionCodeType.RZ) > 0) posiLis.Add(RZ);
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
            posiLis.Add(this.RX);
            posiLis.Add(this.RY);
            posiLis.Add(this.RZ);
            return true;
        }
    }
}
