using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Robot.AGILE
{
    public class Thor3Lib
    {
        private const string DllPath = @"DianaApi.dll";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FncErrorCallback(int e, string strIpAddress);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FncWarningCallback(int e, string strIpAddress);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct SrvNetSt
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string SrvIp;
            public ushort LocHeartbeatPort;
            public ushort LocRobotStatePort;
            public ushort LocSrvPort;
            public ushort LocRealtimeSrvPort;
            public ushort LocPassThroughSrvPort;
        }


        public enum MoveTCPDirection
        {
            T_MOVE_X_UP, //沿 x 轴正向
            T_MOVE_X_DOWN, //沿 x 轴负向
            T_MOVE_Y_UP, //沿 y 轴正向
            T_MOVE_Y_DOWN, //沿 y 轴负向
            T_MOVE_Z_UP, //沿 z 轴正向
            T_MOVE_Z_DOWN //沿 z 轴负向
        }

        public enum MoveJointDirection
        {
            T_MOVE_UP = 0,
            T_MOVE_DOWN,
        }

        public enum Mode
        {
            T_MODE_INVALID = -1,
            T_MODE_POSITION = 0,
            T_MODE_JOINT_IMPEDANCE,
            T_MODE_CART_IMPEDANCE
        }

        public enum COMPLEX_PATH_TYPE
        {
            NORMAL_JOINT_PATH = 0,
            MOVEP_JOINT_PATH = 1,
            NORMAL_POSE_PATH = 2,
            MOVEP_POSE_PATH = 3,
        }

        public enum CONTROLLER_FEATURE_CODE
        {
            NONE_FEATURE = 0,
            AUTO_SWITCH_TO_IMPEDANCE_MODE_WHEN_COLLISION_DETECTED = 1,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ErrorInfo
        {
            public int errorId;
            public int errorType;
            public int errorCode;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string errorMsg;
        }

        public struct CustomStateInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public double[] dblField;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 160)]
            public sbyte[] int8Field;
        }

        // 这里是工业API特有的数据结构
        //  TrajectoryState
        public struct TrajectoryState
        {
            public int taskId;
            public int segCount;
            public int segIndex;
            public int errorCode;
            public int isControllerPaused;
            public int isSafetyDriving;
            public int isFreeDriving;
            public int isZeroSpaceFreeDriving;
            public int isRobotHoldBrake;
            public int isProgramRunningOrPause;
            public int isTeachPendantPaused;
            public int isControllerTerminated;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct WarningInfo
        {
            public int warningId;
            public int warningCode;
            public int warningType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string warningMsg;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        // 用户层机械臂反馈状态结构体
        public struct RobotStateInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public double[] jointPos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public double[] jointAngularVel;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public double[] jointCurrent;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public double[] jointTorque;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public double[] tcpPos;

            public double tcpExternalForce;

            public bool bCollision;

            public bool bTcpForceValid;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public double[] tcpForce;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public double[] jointForce;

            TrajectoryState trajState;

            ErrorInfo errorInfo;

            WarningInfo warningInfo;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FncStateCallBack(RobotStateInfo pInfo, string strIpAddress);

        public enum Coordinate_E
        {
            E_BASE_COORDINATE = 0,
            E_TOOL_COORDINATE,
            E_WORK_PIECE_COORDINATE,
            E_VIEW_COORDINATE,
        }

        public enum FreeDrivingMode
        {
            T_DISABLE_FREEDRIVING = 0,
            T_NORMAL_FREEDRIVING = 1,
            T_FORCE_FREEDRIVING = 2,
        }

        public enum CustomRobotStateAction
        {
            API_CUSTOM_ADD = 0,    // 定制
            API_CUSTOM_DEL = 1,    // 取消
            API_CUSTOM_RESET = 2,  // 重置
        }

        /// <summary>
        /// 初始化机器人的网络结构体，这会使得全部端口随机分配
        /// </summary>
        /// <param name="pInfo">网络结构体，包含 IP 地址以及所需要的全部端口号</param>
        [DllImport(DllPath)]

        public static extern void initSrvNetInfo(ref SrvNetSt pInfo);


        /// <summary>
        /// 初始化 API，完成其他功能函数使用前的初始化准备工作
        /// </summary>
        /// <param name="fnError">错误处理回调函数</param>
        /// <param name="fnState">robot state 回调函数</param>
        /// <param name="pInfo">srv_net_st 结构体指针，用于配置本地连接服务器、心跳服务和状态反馈服务的端口号信息及服务器 IP。端口号如果传 0 则由系统自动分配</param>
        [DllImport(DllPath)]

        public static extern int initSrv(FncErrorCallback fnError, FncStateCallBack fnState,  ref SrvNetSt pInfo);


        /// <summary>
        /// 设置指定 IP 地址机械臂的数据推送周期
        /// </summary>
        /// <param name="intPeriod">输入参数。推送周期，单位为 ms。</param>
        /// <param name="strIpAddress">可选参数，需要控制机械臂的 IP 地址字符串，不填仅当只连接一台机械臂时生效</param>
        [DllImport(DllPath)]

        public static extern int setPushPeriod(int intPeriod, string strIpAddress = "");


        /// <summary>
        /// 结束调用API，用于结束时释放指定 IP 地址机械臂的资源
        /// </summary>
        /// <param name="strIpAddress">可选参数，需要释放服务资源的机械臂的 IP 地址字符串，如果为空，则会释放全部已经成功 initSrv 的机械臂的资源</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern int destroySrv(string strIpAddress = "");

        /// <summary>
        /// 手动移动指定 IP 地址的机械臂工具中心点。该函数会立即返回，停止运动需要调用stop函数
        /// </summary>
        /// <param name="moveTCPDirection">表示移动方向的枚举类型</param>
        /// <param name="speed">速度，单位：m/s</param>
        /// <param name="acc">加速度，单位：m/s2</param>
        /// <param name="active_tcp">d 参数的参考坐标系（基于法兰坐标系），大小为 6 的数组（位置和旋转矢量（轴角））；为空时将参考基坐标系</param>
        /// <param name="strIpAddress"></param>
        /// <returns>可选参数，需要控制机械臂的 IP 地址字符串，不填仅当只连接一台机械臂时生效</returns>
        [DllImport(DllPath)]
        public static extern int moveTCP(MoveTCPDirection moveTCPDirection, double speed,
            double acc, ref double[] active_tcp, string strIpAddress = "");

        /// <summary>
        /// 手动控制指定 IP 地址的机械臂关节移动。该函数会立即返回，停止运动需要调用 stop 函数
        /// </summary>
        /// <param name="moveTCPDirection"></param>
        /// <param name="index">关节索引号</param>
        /// <param name="speed">速度，单位：rad/s</param>
        /// <param name="acc">加速度，单位：rad/s2</param>
        /// <param name="strIpAddress">可选参数，需要控制机械臂的 IP 地址字符串，不填仅当只连接一台机械臂时生效</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern int moveJoint(MoveTCPDirection moveTCPDirection, int index,
            double speed, double acc, string strIpAddress = "");


        /// <summary>
        /// 控制指定 IP 地址的机械臂停止当前执行的任务。将会以最大加速度停止
        /// </summary>
        /// <param name="strIpAddress">可选参数，需要控制机械臂的 IP 地址字符串，不填仅当只连接一台机械臂时生效</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern int stop(string strIpAddress = "");

        /// <summary>
        /// 获取指定 IP 地址机械臂当前工作状态
        /// </summary>
        /// <param name="strIpAddress"></param>
        /// <returns>
        //0：running（机械臂正在运动或者示教器程序正在运行）
        //1：paused（暂停正在运动的机械臂或者暂停正在运行的示教器程序）
        //2：idle（空闲状态）
        //3：free-driving（进入正常零力驱动模式或者安全零力驱动模式）
        //4：zero-space-free-driving（进入零空间零力驱动模式）
        //5：hold-brake（机械臂关闭抱闸）
        //6：error（机械臂发生错误）
        //7：handling-error（机械臂的关节超出极限位置）</returns>
        [DllImport(DllPath)]
        public static extern char getRobotState(string strIpAddress = "");

        /// <summary>
        /// 当指定 IP 地址机械臂发生碰撞或其他原因暂停后，恢复运行时使用
        /// </summary>
        /// <param name="strIpAddress">可选参数，需要控制机械臂的 IP 地址字符串，不填仅当只连接一台机械臂时生效</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern int resume(string strIpAddress = "");

        /// <summary>
        /// 打开指定 IP 地址机械臂的抱闸，启动机械臂。调用该接口后，需要调用者延时 2s 后再做其他操作
        /// </summary>
        /// <param name="strIpAddress"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern int releaseBrake(string strIpAddress = "");

        /// <summary>
        /// 关闭指定 IP 地址机械臂的抱闸，停止机械臂
        /// </summary>
        /// <param name="strIpAddress"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern int holdBrake(string strIpAddress = "");

        /// <summary>
        /// 获取指定 IP 地址机械臂各个关节角度的位置，库初始化后，后台会自动同步机器人状态信息，因此所有的监测函数都是从本地缓存取数
        /// </summary>
        /// <param name="joints">输出关节角数组首地址，数组长度为 JOINT_NUM。用于传递获取到的结果</param>
        /// <param name="strIpAddress"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern int getJointPos(double[] joints, string strIpAddress = "");

        /// <summary>
        /// 获取指定 IP 地址机械臂当前 TCP 位姿数据，TCP 位姿可被 setDefaultActiveTcp 和setDefaultActiveTcpPose 函数改变
        /// </summary>
        /// <param name="pose">输出 TCP 位姿数组首地址，数组长度为 6。用于传递获取到的结果</param>
        /// <param name="strIpAddress"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern int getTcpPos(double[] pose, string strIpAddress = "");

        /// <summary>
        /// 获取指定 IP 地址机械臂的错误码 e 的字符串描述
        /// </summary>
        /// <param name="e">错误码</param>
        /// <param name="strIpAddress"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern string formatError(int e, string strIpAddress = "");

        /// <summary>
        /// 清除指定 IP 地址机械臂的错误信息
        /// </summary>
        /// <param name="strIpAddress"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern int cleanErrorInfo(string strIpAddress = "");
    }
}
