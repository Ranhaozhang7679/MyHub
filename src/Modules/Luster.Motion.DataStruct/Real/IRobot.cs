using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Real
{
    public interface IRobot
    {
        /// <summary>
        /// 轴数
        /// </summary>
        int AxisCount { get; set; }

        /// <summary>
        /// 连接控制器
        /// </summary>
        void ConnectController();

        /// <summary>
        /// 断开控制器
        /// </summary>
        void DisConnectController();

        /// <summary>
        /// 设置使能状态
        /// </summary>
        /// <param name="mode"></param>
        void ServOn(short mode);

        /// <summary>
        /// 清除报警
        /// </summary>
        void ClearAlarm();

        /// <summary>
        /// 设置运动权限
        /// </summary>
        /// <param name="onoff"></param>
        void SetMoveAccept(bool onoff);


        /// <summary>
        /// 设置机器人手动单步/连续运动
        /// </summary>
        /// <param name="mode"></param>
        void SetManualMode(short mode);

        /// <summary>
        /// 手动运动
        /// </summary>
        void ManualMove(short Axis, bool IsJoint);


        /// <summary>
        /// 获取机器人当前坐标
        /// </summary>
        /// <param name="IsJoint"></param>
        void GetCurrentPosion(out List<double> posions, bool IsJoint);

        /// <summary>
        /// 点到点方式运行到笛卡尔坐标系绝对位置的指令
        /// </summary>
        void MovP(int index, bool isAbsolute = true, double distance = 0);

        /// <summary>
        /// 点到点方式控制机器人进行拱形移动的指令
        /// </summary>
        void Jump(int index, bool isArch = true);

        /// <summary>
        /// 设置（修改）机器人当前速率
        /// </summary>
        /// <param name="inputDat"></param>
        void SetVelScale(short inputDat);

        /// <summary>
        /// 获取机器人当前速率
        /// </summary>
        short GetVelScale();

        /// <summary>
        /// 获取机器人的状态信息
        /// </summary>
        /// <returns></returns>
        Dictionary<RobotStatus, bool> GetRobotStatus();

        /// <summary>
        /// 读点位
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        List<double> ReadPoint(int index);

        /// <summary>
        /// 写点位
        /// </summary>
        /// <param name="index"></param>
        /// <param name="point"></param>
        void WritePoint(int index, List<double> point);

        /// <summary>
        /// 写当前点位
        /// </summary>
        /// <param name="index"></param>
        void WriteCurPoint(int index);

        void Stop();

        void WriteCurSixPoint(string pointName);

        void SixMoveL(string pointName,double[] pose);

    }
}
