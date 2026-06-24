using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathNetExtend.Utils
{
    /// <summary>
    /// 位置区间判定类
    /// </summary>
    public class PosiHelper
    {

        public const double MAX_DOUBLE_ERROR = 1e-10;
        /// <summary>
        /// 判断是否接近
        /// </summary>
        /// <param name="p1">位置1</param>
        /// <param name="p2">位置2</param>
        /// <param name="offsetLimit">误差带</param>
        /// <returns></returns>
        public static bool IsClose(double p1, double p2, double offsetLimit)
        {
            return IsClose(p1 - p2, offsetLimit);
        }
        /// <summary>
        /// 判断是否接近
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="offsetLimit">误差带</param>
        /// <returns></returns>
        public static bool IsClose(double offset, double offsetLimit)
        {
            return Math.Abs(offset) <= Math.Abs(offsetLimit);
        }
        /// <summary>
        /// 判断点位是否处于目标点位区域内
        /// </summary>
        /// <param name="p">点位</param>
        /// <param name="dest">目标点</param>
        /// <param name="offset">目标区域</param>
        /// <param name="containEdge">是否包含边界</param>
        /// <param name="offsetLimit">位置误差带</param>
        /// <returns></returns>
        public static bool IsInsideOffset(double p, double dest, double offset, bool containEdge = true, double offsetLimit = 0)
        {
            offset = Math.Abs(offset);
            if (p > dest + offset || p < dest - offset) return false;
            if (!containEdge) { if (IsClose(p, dest + offset, MAX_DOUBLE_ERROR) || IsClose(p, dest - offset, MAX_DOUBLE_ERROR)) return false; }
            return true;
        }
        /// <summary>
        /// 判断点位是否处于目标点位区域外
        /// </summary>
        /// <param name="p">点位</param>
        /// <param name="dest">目标点</param>
        /// <param name="offset">目标区域</param>
        /// <param name="containEdge">是否包含边界</param>
        /// <returns></returns>
        public static bool IsOutsideOffset(double p, double dest, double offset, bool containEdge = true)
        {
            return !IsInsideOffset(p, dest, offset, !containEdge);
        }
        /// <summary>
        /// 获得点位与AB区间的关系
        /// </summary>
        /// <param name="p">点位</param>
        /// <param name="aPoint">区间A点</param>
        /// <param name="bPoint">区间B点</param>
        /// <returns></returns>
        public static PosiRelationType Relation2AB(double p, double aPoint, double bPoint)
        {
            if (IsClose(p, aPoint, MAX_DOUBLE_ERROR)) return PosiRelationType.AEqual;
            if (IsClose(p, bPoint, MAX_DOUBLE_ERROR)) return PosiRelationType.BEqual;
            bool a_b = aPoint <= bPoint;
            if (p < (a_b ? aPoint : bPoint)) return a_b ? PosiRelationType.ASide : PosiRelationType.BSide;
            if (p > (a_b ? bPoint : aPoint)) return a_b ? PosiRelationType.BSide : PosiRelationType.ASide;
            return PosiRelationType.ABInSide;
        }
        /// <summary>
        /// 判断点位是否处于AB区间内
        /// </summary>
        /// <param name="p">点位</param>
        /// <param name="aPoint">区间A点</param>
        /// <param name="bPoint">区间B点</param>
        /// <param name="containEdge">是否包含边界</param>
        /// <returns></returns>
        public static bool IsInsideAB(double p, double aPoint, double bPoint, bool containEdge = true)
        {
            var relation = Relation2AB(p, aPoint, bPoint);
            if (containEdge) return relation >= PosiRelationType.AEqual && relation <= PosiRelationType.BEqual;
            return relation > PosiRelationType.AEqual && relation < PosiRelationType.BEqual;
        }
        /// <summary>
        /// 判断点位是否处于AB区间外
        /// </summary>
        /// <param name="p">点位</param>
        /// <param name="aPoint">区间A点</param>
        /// <param name="bPoint">区间B点</param>
        /// <param name="containEdge">是否包含边界</param>
        /// <returns></returns>
        public static bool IsOutsideAB(double p, double aPoint, double bPoint, bool containEdge = true)
        {
            return !IsInsideAB(p, aPoint, bPoint, !containEdge);
        }
        /// <summary>
        /// 判断点位是否处于区间A侧
        /// </summary>
        /// <param name="p">点位</param>
        /// <param name="aPoint">区间A点</param>
        /// <param name="bPoint">区间B点</param>
        /// <param name="containEdge">是否包含边界</param>
        /// <returns></returns>
        public static bool IsASide(double p, double aPoint, double bPoint, bool containEdge = true)
        {
            var relation = Relation2AB(p, aPoint, bPoint);
            if (containEdge) return relation <= PosiRelationType.AEqual;
            return relation < PosiRelationType.AEqual;
        }
        /// <summary>
        /// 判断点位是否处于区间B侧
        /// </summary>
        /// <param name="p">点位</param>
        /// <param name="aPoint">区间A点</param>
        /// <param name="bPoint">区间B点</param>
        /// <param name="containEdge">是否包含边界</param>
        /// <returns></returns>
        public static bool IsBSide(double p, double aPoint, double bPoint, bool containEdge = true)
        {
            var relation = Relation2AB(p, aPoint, bPoint);
            if (containEdge) return relation >= PosiRelationType.BEqual;
            return relation > PosiRelationType.BEqual;
        }
        /// <summary>
        /// 判断点位是否处于AB区间内
        /// </summary>
        /// <param name="p">点位</param>
        /// <param name="aPoint">区间A点</param>
        /// <param name="bPoint">区间B点</param>
        /// <param name="offestLimit">误差带</param>
        /// <returns></returns>
        public static bool IsInsideAB(double p, double aPoint, double bPoint, double offestLimit)
        {
            double limit = Math.Abs(offestLimit);
            double max = Math.Max(aPoint, bPoint);
            double min = Math.Min(aPoint, bPoint);

            if (IsClose(p, aPoint, offestLimit)) return true;
            if (IsClose(p, bPoint, offestLimit)) return true;
            if (p >= min && p <= max) return true;
            return false;
        }
        /// <summary>
        /// 判断点位是否处于AB区间外
        /// </summary>
        /// <param name="p">点位</param>
        /// <param name="aPoint">区间A点</param>
        /// <param name="bPoint">区间B点</param>
        /// <param name="offestLimit">误差带</param>
        /// <returns></returns>
        public static bool IsOutSideAB(double p, double aPoint, double bPoint, double offestLimit)
        {
            double limit = Math.Abs(offestLimit);
            double max = Math.Max(aPoint, bPoint);
            double min = Math.Min(aPoint, bPoint);

            if (IsClose(p, aPoint, offestLimit)) return true;
            if (IsClose(p, bPoint, offestLimit)) return true;
            if (p <= min || p >= max) return true;
            return false;
        }
        /// <summary>
        /// 判断点位是否处于区间A侧
        /// </summary>
        /// <param name="p">点位</param>
        /// <param name="aPoint">区间A点</param>
        /// <param name="bPoint">区间B点</param>
        /// <param name="offestLimit">误差带</param>
        /// <returns></returns>
        public static bool IsASide(double p, double aPoint, double bPoint, double offestLimit)
        {
            if (IsClose(p, aPoint, offestLimit)) return true;
            if (IsASide(p, aPoint, bPoint)) return true;
            return false;
        }
        /// <summary>
        /// 判断点位是否处于区间B侧
        /// </summary>
        /// <param name="p">点位</param>
        /// <param name="aPoint">区间A点</param>
        /// <param name="bPoint">区间B点</param>
        /// <param name="offestLimit">误差带</param>
        /// <returns></returns>
        public static bool IsBSide(double p, double aPoint, double bPoint, double offestLimit)
        {
            if (IsClose(p, bPoint, offestLimit)) return true;
            if (IsBSide(p, aPoint, bPoint)) return true;
            return false;
        }
        /// <summary>
        /// 位置方向类型
        /// </summary>
        public enum PosiDirType
        {
            /// <summary>
            /// 由B向A
            /// </summary>
            TOA,
            /// <summary>
            /// 由A向B
            /// </summary>
            TOB,
            /// <summary>
            /// 未产生运动
            /// </summary>
            EQUAL,
            /// <summary>
            /// 参考点相等
            /// </summary>
            ERROR,
        }
        /// <summary>
        /// 获取运动方向
        /// </summary>
        /// <param name="a">参考起点</param>
        /// <param name="b">参考终点</param>
        /// <param name="cur">当前位置</param>
        /// <param name="dest">目标位置</param>
        /// <returns></returns>
        public static PosiDirType GetDir(double a, double b, double cur, double dest)
        {
            double d = b - a;
            double offset = dest - cur;
            if (d == 0) return PosiDirType.ERROR;
            if (offset == 0) return PosiDirType.EQUAL;
            if (d > 0 && offset > 0
                || d < 0 && offset < 0) return PosiDirType.TOB;
            return PosiDirType.TOA;
        }
        /// <summary>
        /// 位置关系类型
        /// </summary>
        public enum PosiRelationType : int
        {
            /// <summary>
            /// A侧
            /// </summary>
            ASide = 0,
            /// <summary>
            /// 等于A
            /// </summary>
            AEqual = 1,
            /// <summary>
            /// 界内
            /// </summary>
            ABInSide = 2,
            /// <summary>
            /// 等于B
            /// </summary>
            BEqual = 3,
            /// <summary>
            /// B侧
            /// </summary>
            BSide = 4,
        }
    }
}
