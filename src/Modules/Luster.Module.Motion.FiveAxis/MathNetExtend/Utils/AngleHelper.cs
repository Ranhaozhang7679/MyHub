using MathNetExtend.Model.Position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathNetExtend.Utils
{
    /// <summary>
    /// 角度转换帮助类
    /// </summary>
    public class AngleHelper
    {
        /// <summary>
        /// 将角度转换到与参考角度同一个量级（度）
        /// </summary>
        /// <param name="angle">要转换的角度</param>
        /// <param name="reference">参考角度</param>
        /// <returns>转换后的角度</returns>
        public static double SameDirAngle(double angle, double reference)
        {
            double convertAngle = angle;
            double offsetAngle = reference - angle;
            double rate = offsetAngle / 360;
            if (rate >= 0) rate += 0.5;
            else rate -= 0.5;
            convertAngle = angle + (int)rate * 360;
            return convertAngle;
        }

        /// <summary>
        /// 将角度转换到与参考角度同一个量级（弧度）
        /// </summary>
        /// <param name="rad">要转换的角度</param>
        /// <param name="reference">参考角度</param>
        /// <returns>转换后的角度</returns>
        public static double SameDirRad(double rad, double reference)
        {
            double convertAngle = rad;
            double offsetAngle = reference - rad;

            double rate = offsetAngle / (2 * Math.PI);
            if (rate >= 0) rate += 0.5;
            else rate -= 0.5;
            convertAngle = rad + (int)rate * (2 * Math.PI);
            return convertAngle;
        }
        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="rad">弧度</param>
        /// <param name="range">角度范围</param>
        /// <returns>转换后的角度</returns>
        public static double RadToAngle(double rad, AngleRange range = AngleRange.Angle_P0_P360)
        {
            //角度 = 180°×弧度÷π
            return TrimAngle(180 * rad / Math.PI, range);
        }
        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="angle">角度</param>
        /// <param name="range">角度范围</param>
        /// <returns>转换后的弧度</returns>
        public static double AngleToRad(double angle, AngleRange range = AngleRange.Angle_P0_P360)
        {
            //弧度 = 角度×π÷180°
            return TrimRad(angle * Math.PI / 180, range);
        }
        /// <summary>
        /// 将角度调整到指定范围内（弧度）
        /// </summary>
        /// <param name="rad">弧度</param>
        /// <param name="range">角度范围</param>
        /// <returns>调整后的弧度</returns>
        public static double TrimRad(double rad, AngleRange range = AngleRange.Angle_P0_P360)
        {
            // 使用 Math.Floor 替代 (int) 强转，避免 rad 为 NaN 或极大负值时溢出
            double convertAngle = rad - Math.Floor(rad / (2 * Math.PI)) * (2 * Math.PI);
            switch (range)
            {
                case AngleRange.Angle_P0_P360: break;
                case AngleRange.Angle_N180_P180:
                    convertAngle = convertAngle > Math.PI ? convertAngle - (2 * Math.PI) : convertAngle;
                    break;
            }
            return convertAngle;
        }
        /// <summary>
        /// 将角度调整到指定范围内（度）
        /// </summary>
        /// <param name="angle">角度</param>
        /// <param name="range">范围</param>
        /// <returns>调整后的角度</returns>
        public static double TrimAngle(double angle, AngleRange range = AngleRange.Angle_P0_P360)
        {
            // 使用 Math.Floor 替代 (int) 强转，避免 angle 为 NaN 或极大负值时
            // (int)(angle/360) 溢出为 int.MinValue 导致 Math.Abs 抛出 OverflowException
            double convertAngle = angle - Math.Floor(angle / 360) * 360;
            switch (range)
            {
                case AngleRange.Angle_P0_P360: break;
                case AngleRange.Angle_N180_P180:
                    convertAngle = convertAngle > 180 ? convertAngle - 360 : convertAngle;
                    break;
            }
            return convertAngle;
        }
        /// <summary>
        /// 计算旋转后的点坐标
        /// </summary>
        /// <param name="posi">初始位置</param>
        /// <param name="rotateCenter">旋转中心</param>
        /// <param name="angle">角度，单位：度</param>
        /// <returns>旋转后的位置</returns>
        public static PositionXY CalculateRoatePoint(PositionXY posi, PositionXY rotateCenter, double angle)
        {
            //第一种计算方式
            double u = AngleToRad(angle, AngleRange.Angle_P0_P360);

            double tempX = (posi.X - rotateCenter.X) * Math.Cos(u) - (posi.Y - rotateCenter.Y) * Math.Sin(u) + rotateCenter.X;
            double tempY = (posi.X - rotateCenter.X) * Math.Sin(u) + (posi.Y - rotateCenter.Y) * Math.Cos(u) + rotateCenter.Y;
            return new PositionXY(tempX, tempY);
        }

        /// <summary>
        /// 计算旋转后的点坐标
        /// </summary>
        /// <param name="posi">初始位置</param>
        /// <param name="rotateCenter">旋转中心</param>
        /// <param name="angle">角度，单位：度</param>
        /// <returns>旋转后的位置</returns>
        public static PositionXYRz CalculateRoatePoint(PositionXYRz posi, PositionXY rotateCenter, double angle)
        {
            var tmp = CalculateRoatePoint(posi as PositionXY, rotateCenter, angle);
            return new PositionXYRz()
            {
                X = tmp.X,
                Y = tmp.Y,
                RZ = posi.RZ + angle
            };
        }
        /// <summary>
        /// 两带角度点计算旋转中心
        /// </summary>
        /// <param name="point1">旋转前</param>
        /// <param name="point2">旋转后</param>
        /// <returns></returns>
        public static PositionXY CalculateRoateCenter(PositionXYRz point1, PositionXYRz point2)
        {
            return CalculateRoateCenter(point1, point2, point2.RZ - point1.RZ);
        }

        /// <summary>
        /// 两点+角度计算旋转中心
        /// </summary>
        /// <param name="point1">旋转前</param>
        /// <param name="point2">旋转后</param>
        /// <param name="angle">旋转角度</param>
        /// <returns></returns>
        public static PositionXY CalculateRoateCenter(PositionXY point1, PositionXY point2, double angle)
        {
            double rad = AngleHelper.AngleToRad(angle);
            PositionXY center = new PositionXY();
            double distance = Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
            double r = (distance / 2) / Math.Sin(rad / 2);

            double xTemp = (1 - r / distance) * point1.X + (r / distance) * point2.X;
            double yTemp = (1 - r / distance) * point1.Y + (r / distance) * point2.Y;

            center.X = Math.Cos(Math.PI / 2 - rad / 2) * (xTemp - point1.X) - Math.Sin(Math.PI / 2 - rad / 2) * (yTemp - point1.Y) + point1.X;
            center.Y = Math.Cos(Math.PI / 2 - rad / 2) * (yTemp - point1.Y) + Math.Sin(Math.PI / 2 - rad / 2) * (xTemp - point1.X) + point1.Y;
            return center;
        }
        public static double VectorToRad(PositionXY vec)
        {
            return Math.Atan2(vec.Y, vec.X);
        }
        /// <summary>
        /// 角度范围
        /// </summary>
        public enum AngleRange
        {
            /// <summary>
            /// 0~360度
            /// </summary>
            Angle_P0_P360,
            /// <summary>
            /// -180~180度
            /// </summary>
            Angle_N180_P180,
        }
    }
}
