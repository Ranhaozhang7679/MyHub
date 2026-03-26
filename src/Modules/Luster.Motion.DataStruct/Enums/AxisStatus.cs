#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AxisStatus
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct._6.Enums
* 文 件 名:       AxisStatus.cs
* 创建时间:       2022/4/21 17:28:16
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ac0e689a-7612-4840-aa54-3f35cbdfcb9e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/21 17:28:16
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    public enum HomeMode
    {
        [Description("负极限寻EZ")]
        NegativeToZ = 1,

        [Description("正极限寻EZ")]
        PositiveToZ = 2,

        [Description("负向")]
        Negative = 17,

        [Description("正向")]
        Positive = 18,


        [Description("步进正向原点")]
        PositiveMotorHome = 20,

        [Description("步进负向原点")]
        NegativeMotorHome = 22,

        [Description("正向原点")]
        PositiveHome = 26,

        [Description("负向原点")]
        NegativeHome = 27,

        [Description("负向寻EZ")]
        NegativeToEZ = 33,

        [Description("正向寻EZ")]
        PositiveToEZ = 34,

        [Description("当前原点")]
        CurrentHome = 35,

    }

    /// <summary>
    /// 轴的状态
    /// </summary>
    public enum AxisStatus
    {
        [Description("报警")]
        Alarm = 0,


        Normal = 1,

        [Description("运动")]
        Moving = 2,

        [Description("正限位")]
        Pel = 3,

        [Description("负限位")]
        Mel = 4,

        [Description("原点")]
        Org = 5,

        [Description("急停")]
        Emg = 6,

        [Description("使能")]
        SvOn = 7,

        [Description("到位")]
        OnPos = 8,

        [Description("已回零")]
        IsHome = 9,
        Error1 = 20,
        Error2 = 21,
        Error3 = 22,
        Error4 = 23,
    }

    /// <summary>
    /// 移动方式
    /// </summary>
    public enum MoveMode
    {
        [Description("绝对")]
        Abs = 0,

        [Description("相对")]
        Rel = 1,
    }

    /// <summary>
    /// 轴的运动模式
    /// </summary>
    public enum AxisMode
    {
        [Description("点位模式")]
        Move,

        [Description("皮带模式")]
        JOG,

        [Description("直线差补")]
        LineMove,

        [Description("圆弧差补")]
        CircleMove,

        [Description("回零模式")]
        Home,

        [Description("读取位置")]
        GetPosition,

        [Description("读取原点")]
        GetOrg,

        [Description("读取正限位")]
        GetPEL,

        [Description("读取负限位")]
        GetAEL
    }

    /// <summary>
    /// 皮带运动方向
    /// </summary>
    public enum MoveDirection
    {
        [Description("正向")]
        Positive,

        [Description("负向")]
        Negative,
    }

    /// <summary>
    /// JOG方式
    /// </summary>
    public enum JOGMethod
    {
        [Description("启动")]
        Start,

        [Description("停止")]
        Stop
    }

    /// <summary>
    /// 轴类型
    /// </summary>
    public enum AxisType
    {
        [Description("步进")]
        None,

        [Description("X轴")]
        X,

        [Description("Y轴")]
        Y,

        [Description("Z轴")]
        Z,

        /// <summary>
        /// 绕Z轴旋转
        /// </summary>
        [Description("U轴")]
        U,

        /// <summary>
        /// 绕Y轴旋转
        /// </summary>
        [Description("V轴")]
        V,

        /// <summary>
        /// 绕X轴旋转
        /// </summary>
        [Description("W轴")]
        W
    }


    /// <summary>
    /// 圆弧插补方向
    /// </summary>
    public enum InterpolationDir
    {
        [Description("顺时针")]
        ClockWise,

        [Description("逆时针")]
        AntiClockWise,
    }

    /// <summary>
    /// 转盘运动到位方式
    /// </summary>
    public enum MoveMethod
    {
        [Description("最近路径")]
        Nearest,

        [Description("最远路径")]
        Farthest
    }

    
    public enum AxisPML
    {
        [Description("Unknown")]
        Unknown = 0,//0x603f,0x00
        [Description("ASDAB3")]
        ASDAB3 =1,// 0x2001,0x00
        [Description("SV630N")]
        SV630N = 2,//0x200b,0x23
        [Description("LDHD")]
        LDHD = 3,//0x603f,0x00
        [Description("Leadshine")]
        Leadshine = 4,//0x603f,0x00        
    }

    public enum AxisForward
    {
        [Description("左")] Left = 0,
        [Description("右")] Right = 1,
        [Description("前")] Front = 2,
        [Description("后")] Behind = 3,
        [Description("上")] Up = 4,
        [Description("下")] Down = 5,
    }

    public struct AxisSDOCode
    {
        public short Index;
        public short SubIndex;
    }


}