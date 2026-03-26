//DianaApi.cs
//@author:neo
//@date:2022/12/14

namespace Luster.SimDevice.Robot.AGILE
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    public delegate void ErrorCallBack(int err);//错误处理回调函数
    public delegate void RobotstateCallBack(RobotStateInfo robotStateInfo);//用户层机械臂反馈

    public struct TrajectoryState
    {
        public int taskId;
        public int segCount;
        public int segIndex;
        public int errorCode;
        public int isPaused;
        public int isFreeDriving;
        public int isZeroSpaceFreeDriving;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ErrorInfo
    {
        public int errorId;
        public int errorType;
        public int errorCode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] errorMsg;
    }

    // 用户层机械臂反馈状态结构体.
    [StructLayout(LayoutKind.Sequential)]
    public struct RobotStateInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public Double[] jointPos;//关节角度数组
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public Double[] jointAngularVel;//关节角速度数组
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public Double[] jointCurrent;//关节角电流当前值数组
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public Double[] jointTorque;//关节角扭矩数组
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public Double[] tcpPos;//TCP 位姿向量
        public double tcpExternalForce;//TCP 外部力
        public bool isCollisioned;//碰撞标志
        public bool isTcpForceValid;//TCP 外部力是否有效标志
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public double[] tcpForce;//TCP 六维力数组
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public double[] jointForce;//轴空间力数组
        public TrajectoryState trajState;
        public ErrorInfo errorInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CustomStateInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public double[] dblField;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 160)]
        public int[] int8Field;
    }

    /// 结构体指针，用于配置本地连接服务器、
    /// 心跳服务和状态反馈服务的端口号信息及服务器 
    /// IP。端口号如果传 0 则由系统自动分配.
    public struct SrvNetSt
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string srvIp;
        public short locHeartbeatPort;
        public short locRobotStatePort;
        public short locSrvPort;
        public short locRealtimeSrvPort;
        public short locPassThroughSrvPort;
    }

    public static class DianaApi
    {
        private const string DllPath = @"DianaApi.dll";

        public static double[] jointPos = new double[7];//关节角度数组
        public static double[] jointAngularVel = new double[7];//关节角速度数组
        public static double[] jointCurrent = new double[7];//关节角电流当前值数组
        public static double[] jointTorque = new double[7];//关节角扭矩数组
        public static double[] tcpPos = new double[6];//TCP 位姿向量
        public static double tcpExternalForce;//TCP 外部力
        public static bool isCollisioned;//碰撞标志
        public static bool isTcpForceValid;//TCP 外部力是否有效标志
        public static double[] tcpForce = new double[6];//TCP 六维力数组
        public static double[] jointForce = new double[7];//轴空间力数组


        public static int taskId;
        public static int segCount;
        public static int segIndex;
        public static int errorCode;
        public static int isPaused;
        public static int isFreeDriving;
        public static int isZeroSpaceFreeDriving;
        public static int errorId;
        public static int errorType;
        public static int errorCode1;
        public static char[] errorMsg = new char[64];
        public static int error;

        public static void errorControl()
        {
            error = DianaApi.setLastError(0);
        }

        public static void logRobotState(RobotStateInfo robotStateInfo)
        {

            for (int i = 0; i < 7; i++)
            {
                jointPos[i] = robotStateInfo.jointPos[i] * 180 / Math.PI;
                jointAngularVel[i] = robotStateInfo.jointAngularVel[i];
                jointCurrent[i] = robotStateInfo.jointCurrent[i];
                jointTorque[i] = robotStateInfo.jointTorque[i];
                jointForce[i] = robotStateInfo.jointForce[i];
            }
            for (int i = 0; i < 6; i++)
            {
                tcpForce[i] = robotStateInfo.tcpForce[i];
            }

            tcpPos[0] = robotStateInfo.tcpPos[0] * 1000.0;
            tcpPos[1] = robotStateInfo.tcpPos[1] * 1000.0;
            tcpPos[2] = robotStateInfo.tcpPos[2] * 1000.0;
            //tcpPos[3] = robotStateInfo.tcpPos[3] * 180 / Math.PI;
            //tcpPos[4] = robotStateInfo.tcpPos[4] * 180 / Math.PI;
            //tcpPos[5] = robotStateInfo.tcpPos[5] * 180 / Math.PI;

            double[] arr = new double[3] { robotStateInfo.tcpPos[3], robotStateInfo.tcpPos[4], robotStateInfo.tcpPos[5] };

            //rpy2Axis(arr);

            axis2RPY(arr);

            tcpPos[3] = arr[0] * 180 / Math.PI;
            tcpPos[4] = arr[1] * 180 / Math.PI;
            tcpPos[5] = arr[2] * 180 / Math.PI;

            for (int i = 0; i < 64; i++)
            {
                errorMsg[i] = robotStateInfo.errorInfo.errorMsg[i];
            }
            tcpExternalForce = robotStateInfo.tcpExternalForce;//TCP 外部力
            isCollisioned = robotStateInfo.isCollisioned;//碰撞标志
            taskId = robotStateInfo.trajState.taskId;
            segCount = robotStateInfo.trajState.segCount;
            segIndex = robotStateInfo.trajState.segIndex;
            errorCode = robotStateInfo.trajState.errorCode;
            isPaused = robotStateInfo.trajState.isPaused;
            isFreeDriving = robotStateInfo.trajState.isFreeDriving;
            isZeroSpaceFreeDriving = robotStateInfo.trajState.isZeroSpaceFreeDriving;

            errorId = robotStateInfo.errorInfo.errorId;
            errorType = robotStateInfo.errorInfo.errorType;
            errorCode1 = robotStateInfo.errorInfo.errorCode;
        }

        /// 初始化网络结构体，这会使得全部端口随机分配.
        /// 网络结构体，包含 IP 地址以及所需要的全部端口号
        [DllImport(DllPath)]
        public static extern void initSrvNetInfo(ref SrvNetSt pInfo);

        /// <summary>
        /// 1、初始化 API.
        /// </summary>
        /// <param name="errFun">错误处理回调函数.</param>
        /// <param name="robotstateFun">用户层机械臂反馈.</param>
        /// <param name="pInfo">结构体指针，用于配置本地连接服务器、心跳服务和状态反馈服务的端口号信息及服务器 IP。端口号如果传 0 则由系统自动分配.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern Int32 initSrv(ErrorCallBack errFun, RobotstateCallBack robotstateFun, SrvNetSt pInfo);

        /// <summary>
        /// 2、结束调用 API.
        /// </summary>
        /// <param name="ipAddress">.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int destroySrv(string ipAddress = "");

        /// <summary>
        /// 设置指定 IP 地址机械臂的数据推送周期.
        /// </summary>
        /// <param name="intPeriod">输入参数。推送周期，单位为 ms。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setPushPeriod(int intPeriod, string ipAddress = "");

        /// <summary>
        /// 移动方向的枚举类型
        /// </summary>
        public enum TcpDirectionE
        {
            T_MOVE_X_UP = 0, //x 轴正向
            T_MOVE_X_DOWN,   //x 轴负向
            T_MOVE_Y_UP,     //y 轴正向
            T_MOVE_Y_DOWN,   //y 轴负向
            T_MOVE_Z_UP,     //z 轴正向
            T_MOVE_Z_DOWN    //z 轴负向
        }

        /// <summary>
        /// 坐标系类型
        /// </summary>
        public enum CoordinateE
        {
            E_BASE_COORDINATE = 0,    //基坐标系
            E_TOOL_COORDINATE,        //工具坐标系
            E_WORK_PIECE_COORDINATE,  //工件坐标系
            E_VIEW_COORDINATE         //视角坐标系
        }

        /// <summary>
        /// 8、手动移动末端中心点。该函数会立即返回，停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="d">表示移动方向的枚举类型。.</param>
        /// <param name="v">速度.</param>
        /// <param name="a">加速度.</param>
        /// <param name="activeTcp">工具坐标系对应的位姿向量，大小为 6 的数组，为空时，将使用默认的工具坐标系 default_tcp。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int moveTCP(TcpDirectionE d, double v, double a, double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 9、绕末端中心点变换位姿。该函数会立即返回，停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="d">表示移动方向的枚举类型.</param>
        /// <param name="v">速度.</param>
        /// <param name="a">加速度.</param>
        /// <param name="activeTcp">工具坐标系对应的位姿向量，大小为 6 的数组，为空时，将使用默认的工具坐标系 default_tcp.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int rotationTCP(TcpDirectionE d, double v, double a, double[] activeTcp, string ipAddress = "");

        public enum JointDirectionE
        {
            T_MOVE_UP = 0,
            T_MOVE_DOWN,
        }

        /// <summary>
        /// 10、手动控制关节移动。该函数会立即返回，停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="d">表示关节移动方向的枚举类型.</param>
        /// <param name="i">关节索引.</param>
        /// <param name="v">速度.</param>
        /// <param name="a">加速度.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int moveJoint(JointDirectionE d, int i, double v, double a, string ipAddress = "");

        /// <summary>
        /// 11、以七个关节角度为终点的 moveJ。该函数会立即返回，停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="joints">终点关节角度数组首地址，数组长度为 7.</param>
        /// <param name="v">速度.</param>
        /// <param name="a">加速度.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int moveJToTarget(double[] joints, double v, double a, string ipAddress = "");

        /// <summary>
        /// 12、以工具中心点位姿为终点的 moveJ。该函数会立即返回，停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="pose">终点位姿数组首地址，数组长度为 6。保存 TCP 坐标（x, y, z）和轴角（rx, ry, rz）组合的矢量数据.</param>
        /// <param name="v">速度.</param>
        /// <param name="a">加速度.</param>
        /// <param name="activeTcp">当前工具坐标系名称.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int moveJToPose(double[] pose, double v, double a, double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 13、以七个关节角度为终点的 moveL。该函数会立即返回，停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="joints">终点关节角度数组首地址，数组长度为 7.</param>
        /// <param name="v">速度.</param>
        /// <param name="a">加速度.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int moveLToTarget(double[] joints, double v, double a, string ipAddress = "");

        /// <summary>
        /// 14、以工具中心点位姿为终点的 moveL。该函数会立即返回，停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="pose">终点位姿数组首地址，数组长度为 6，保存 TCP 坐标（x, y, z）和轴角（rx, ry, rz） 组合数据。.</param>
        /// <param name="v">速度.</param>
        /// <param name="a">加速度.</param>
        /// <param name="activeTcp">目标工具坐标系相对末端位姿，double 数组的首地址，数组长度为 6.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int moveLToPose(double[] pose, double v, double a, double[] activeTcp, int zv_shaper_order,int  zv_shaper_frequency,int zv_shaper_damping_ratio,  string ipAddress = "");

        /// <summary>
        /// 17、速度模式，关节空间运动。时间 t 为可选项，时间 t 是可选项，如果提供了 t 值，机器人将在 t 时间后减速。如果没有提供时间 t 值，机器人将在达到目标速度时减速。该函数调用后立即返回。停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="speed">关节角速度数组首地址，数组长度为 7。单位：rad/s.</param>
        /// <param name="a">加速度.</param>
        /// <param name="t">时间.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int speedJ(double[] speed, double a, double t, string ipAddress = "");

        /// <summary>
        /// 18、速度模式，笛卡尔空间下直线运动。时间 t 为可选项，时间 t 是可选项，如果提供了 t 值，机器人将在 t 时间后减速。如果没有提供时间 t 值，机器人将在达到目标速度时减速。该函数调用后立即返回。停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="speed">工具空间速度，数组长度为 6,其中前 3 个单位为 m/s，后 3 个单位为 rad/s。 .</param>
        /// <param name="a">加速度，单位：m/s2.</param>
        /// <param name="t">时间.</param>
        /// <param name="activeTcp">目标工具坐标系相对末端位姿，double 数组的首地址，数组长度为 6.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int speedL(double[] speed, double[] a, double t, double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 19、实现正常模式与零力驱动模式之间的切换.
        /// </summary>
        /// <param name="mode">是否进入零力驱动模式,0 表明退出零力驱动， 1 为进入正常零力驱动模式</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int freeDriving(int mode, string ipAddress = "");

        /// <summary>
        /// 20.停止当前执行的任务。将会以最大加速度停止。对应于 UR 的 stopL，stopJ 指令.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int stop(string ipAddress = "");

        /// <summary>
        /// 21、正解函数，通过关节角度计算出正解 TCP 位姿.
        /// </summary>
        /// <param name="joints">传入关节角度数组首地址，数组长度为 7。单位：rad.</param>
        /// <param name="pose">输出位姿数组首地址，数组长度为 6。用于传递转化后的结果，数据为包含 active_tcp 坐标（x, y, z）和旋转矢量（轴角坐标）组合.</param>
        /// <param name="activeTcp">.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int forward(double[] joints, double[] pose, double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 22、逆解函数，通过 TCP 位姿计算出最佳逆解关节角度.
        /// </summary>
        /// <param name="pose">输入位姿数组首地址，数据为包含 active_tcp 坐标（x, y, z）和旋转矢量（轴角坐标）组合.</param>
        /// <param name="joints">输出关节角度数组首地址，用于传递转换的结果.</param>
        /// <param name="activeTcp">.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int inverse(double[] pose, double[] joints, double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 23、获取关节角度位置，库初始化后，后台会自动同步机器人状态信息，因此所有的监测函数都是从本地缓存取数.
        /// </summary>
        /// <param name="joints">输出关节角数组首地址，数组长度为 7。用于传递获取到的结果.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getJointPos(double[] joints, string ipAddress = "");

        /// <summary>
        /// 24、获取当前各关节角速度。.
        /// </summary>
        /// <param name="vels">输出关节速度数组首地址，数组长度为 7。用于传递获取到的结果.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getJointAngularVel(double[] vels, string ipAddress = "");

        /// <summary>
        /// 25、获取当前关节电流。.
        /// </summary>
        /// <param name="joints">输出关节电流数组首地址，数组长度为 7。用于传递转获取到的结果。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getJointCurrent(double[] joints, string ipAddress = "");

        /// <summary>
        /// 26、获取各关节真实扭矩数据，即减去零偏的数据.
        /// </summary>
        /// <param name="torques">输出关节阻抗数组首地址，数组长度为 7。用于传递获取到的结果.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getJointTorque(double[] torques, string ipAddress = "");

        /// <summary>
        /// 27、获取当前 TCP 位姿数据，末端可被 setDefaultActiveTcp 函数改变。.
        /// </summary>
        /// <param name="pose">：输出关节 TCP 位姿数组首地址，数组长度为 6。用于传递获取到的结果.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getTcpPos(double[] pose, string ipAddress = "");

        /// <summary>
        /// 28、获取末端实际感受到的力大小，末端可被 setDefaultActiveTcp 函数改变。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回力的大小.</returns>
        [DllImport("DianaApi.dll")]
        public static extern double getTcpExternalForce(string ipAddress = "");

        /// <summary>
        /// 29、打开抱闸，启动机械臂。调用该接口后，需要调用者延时 2s 后再做其他操作.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int releaseBrake(string ipAddress = "");

        /// <summary>
        /// 30、关闭抱闸，停止机械臂。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int holdBrake(string ipAddress = "");

        public enum ModeE
        {
            T_MODE_INVALID = -1,    //无意义
            T_MODE_POSITION = 0,    //位置模式
            T_MODE_JOINT_IMPEDANCE, //关节空间阻抗模式
            T_MODE_CART_IMPEDANCE,  //笛卡尔空间阻抗模式
        }

        /// <summary>
        /// 31、控制模式切换。.
        /// </summary>
        /// <param name="m">.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int changeControlMode(ModeE m, string ipAddress = "");

        /// <summary>
        /// 3、获取当前库的版本号。.
        /// </summary>
        /// <returns>当前版本号,高 8 位为主版本号，低 8 位为次版本号。.</returns>
        [DllImport("DianaApi.dll")]
        public static extern ushort getLibraryVersion();

        /// <summary>
        /// 4、获取错误码 e 的字符串描述，该错误码在初始化指定的回调函数中会作为形参传入，也
        /// 可以在函数调用失败后查询得到。对于错误码为-2001的硬件错误，会延时回馈，一般建
        /// 议对此类错误延时 100 毫秒后调用 formatError 函数获取具体硬件错误提示信息，否则将
        /// 提示“refresh later...”而看不到具体内容。.
        /// </summary>
        /// <param name="e">错误码.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>错误描述信息.</returns>
        [DllImport("DianaApi.dll")]
        public static extern string formatError(int e, string ipAddress = "");

        /// <summary>
        /// 5、返回最近发生的错误码。该错误码会一直保存，确保可以查询得到，直至库卸载，因
        /// 此，当库函数调用失败后，如果想知道具体的错误原因，应该调用该函数获取错误码。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>0：没有错误。 其它值：具体错误码。.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getLastError(string ipAddress = "");

        /// <summary>
        /// 6、重置错误码。将系统中记录的错误码重置为 e，通常用于清除错误。.
        /// </summary>
        /// <param name="e">错误码.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>错误码.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setLastError(int e, string ipAddress = "");

        /// <summary>
        /// 7、设置默认的工具坐标系。在没有调用该函数时，默认工具中心点为法兰盘中心，调用该
        /// 函数后，默认的工具坐标系将被改变。该函数将会改变 moveTCP，rotationTCP，
        /// moveJToPos，moveLToPose，speedJ，speedL，forward，inverse，getTcpPos，
        /// getTcpExternalForce 的默认行为。.
        /// </summary>
        /// <param name="defaultTcp">输入参数，TCP 相对于末端法兰盘的 4*4 齐次变换矩阵的首地址，数组长度 为 16.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setDefaultActiveTcp(double[] defaultTcp, string ipAddress = "");

        /// <summary>
        /// 获取与指定 IP 地址机械臂间的链路状态。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getLinkState(string ipAddress = "");

        /// <summary>
        /// 33、获取 TCP 末端六维力，末端可被 setDefaultActiveTcp 函数改变。.
        /// </summary>
        /// <param name="forces">工具中心点处六维力数组的首地址，数组长度为 6。用于传递获取到的结果.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>0： 获取到六维力且六维力有效。-1： 获取不到六维力矩，或六维力数值无效（发生奇异）.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getTcpForce(double[] forces, string ipAddress = "");

        /// <summary>
        /// 34、获取轴空间七个关节所受力。.
        /// </summary>
        /// <param name="forces">七关节轴力矩数组的首地址，数组长度为 7。用于传递获取到的结果.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>0：链路正常。-1：链路断开。.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getJointForce(double[] forces, string ipAddress = "");

        /// <summary>
        /// 35、从轴空间判断获取机器人是否发生碰撞。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>true：机器人发生碰撞。false：机器人未发生碰撞。.</returns>
        [DllImport("DianaApi.dll")]
        public static extern bool isCollision(string ipAddress = "");

        /// <summary>
        /// 36、根据输入的关节角以及末端位置数组计算 DH 参数。.
        /// </summary>
        /// <param name="tcpMeas">输入参数。TCP 位置数据数组的首地址，数组长度为 3 * nrSets。每组数据为[x, y, z]，共 nrSets 组。单位：米.</param>
        /// <param name="jntPosMeas">输入参数。关节角位置数组的首地址，, 数组长度为 7 * nrSets，每组数据为各关节角位置信息[q1~q7]，共 nrSets 组。单位：弧度。.</param>
        /// <param name="nrSets">输入参数。测量样本数量，最少 32 组，至少保证大于或等于需要辨识的参数.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int initDHCali(double[] tcpMeas, double[] jntPosMeas, uint nrSets, string ipAddress = "");

        /// <summary>
        /// 37、获取 DH 参数计算结果。.
        /// </summary>
        /// <param name="rDH">输出参数。机器人各关节 DH 参数数组的首地址，数组长度为 28。每七个数为一组，共四组数据[a，alpha，d，theta]。单位：rad、m.</param>
        /// <param name="wRT">输出参数。机器人基坐标系相对于世界坐标系下的位姿数组的首地址，数组长度为 6。位姿数据[x, y, z, Rx，Ry，Rz]。单位：rad、m.</param>
        /// <param name="tRT">输出参数。靶球在法兰坐标系下的位置描述数组的首地址，数组长度为 3。数组为靶球位置坐标[x, y, z]。单位：m.</param>
        /// <param name="confid">输出参数。绝对定位精度参考值数组的首地址，数组长度为 2。其中，第一个值为标定前绝对定位精度，第二个值为标定后绝对定位精度。单位：m.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>0：获取成功。1：获取结果精度可能够较低。-1：获取失败，发生异常。.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getDHCaliResult(out double[] rDH, out double[] wRT, out double[] tRT, out double[] confid, string ipAddress = "");

        /// <summary>
        /// 38、设置机器人当前 DH 参数。特别注意，错误的参数设置，可能引起机器人损坏，需谨慎设置！.
        /// </summary>
        /// <param name="a">输入参数。各关节的 a 参数数组的首地址，数组长度为 7.</param>
        /// <param name="alpha">输入参数。各关节的 alpha 参数数组的首地址，数组长度为 7.</param>
        /// <param name="d">输入参数。各关节的 d 参数数组的首地址，数组长度为 7.</param>
        /// <param name="theta">输入参数。各关节的 theta 参数数组的首地址，数组长度为 7.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setDH(double[] a, double[] alpha, double[] d, double[] theta, string ipAddress = "");

        /// <summary>
        /// 39、初始化世界坐标系到机器人坐标系的平移和旋转位姿。用于 DH 参数标定前设置，若用
        ///  户不能提供此参数，DH 参数标定功能依旧可以使用。如果调用此函数则使用用户自定
        ///  义的位姿。特别注意，此功能每次移动机器人与激光跟踪仪都需要重新计算，使用错误
        ///  的参数可能引起 DH 参数计算不准确或标定异常。.
        /// </summary>
        /// <param name="rtw2b">输入参数。世界坐标系到机器人坐标系的平移和旋转位姿数组的首地址，数组长度为 6。单位：米和弧度.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setWrd2BasRT(double[] rtw2b, string ipAddress = "");

        /// <summary>
        /// 40、初始化法兰坐标系到工具坐标系的平移位置。用于 DH 参数标定前设置，若用户不能提
        ///  供此参数，DH 参数标定功能依旧可以使用。如果调用此函数则使用用户自定义的位
        ///   姿。特别注意，此功能每次移动机器人与激光跟踪仪都需要重新计算，使用错误的参数
        ///   可能引起 DH 参数计算不准确或标定异常。.
        /// </summary>
        /// <param name="rtf2t">输入参数。初始化法兰坐标系到工具坐标系的平移位置数组的首地址，数组长度 为 3，位置信息数据[x, y, z]。单位：米.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setFla2TcpRT(double[] rtf2t, string ipAddress = "");

        /// <summary>
        /// 41、获取机器人当前工作状态。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>0：运行中。 1：暂停。 2：闲置。 3：自由驱动。 4：零空间自由驱动。.</returns>
        [DllImport("DianaApi.dll")]
        public static extern char getRobotState(string ipAddress = "");

        /// <summary>
        /// 42、当机器人发生碰撞或其他原因暂停后，恢复运行时使用。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int resume(string ipAddress = "");

        /// <summary>
        /// 43、设置关节空间碰撞检测的力矩阈值。.
        /// </summary>
        /// <param name="collision">输入参数。七关节轴力矩阈值数组的首地址，数组长度为 7.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setJointCollision(double[] collision, string ipAddress = "");

        /// <summary>
        /// 44、设置笛卡尔空间碰撞检测的力矩阈值。.
        /// </summary>
        /// <param name="collision">输入参数。六维力数组的首地址，数组长度为 6。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setCartCollision(double[] collision, string ipAddress = "");

        /// <summary>
        /// 45、进入力控模式。.
        /// </summary>
        /// <param name="frameType">参考坐标系类型。0：基坐标系；1：工具坐标系；2：自定义坐标系（暂 不支持）。.</param>
        /// <param name="frameMatrix">自定义坐标系矩阵（暂不支持），使用时传单位矩阵即可(4x4矩阵),数组长度为 16。.</param>
        /// <param name="forceDirection">表达力的方向的数组首地址，数组长度为 3。.</param>
        /// <param name="forceValue">力大小。单位：N。推荐取值范围（1N~100N）。.</param>
        /// <param name="maxApproachVelocity">最 大 接 近 速 度 。 单 位 ：m/s。 推 荐 取 值 范 围（0.01m/s ~0.5m/s）。.</param>
        /// <param name="maxAllowTcpOffset">允许的最大偏移。单位：m。推荐取值范围（0.01m~1m）。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int enterForceMode(int frameType, double[] frameMatrix, double[] forceDirection, double forceValue, double maxApproachVelocity, double maxAllowTcpOffset, string ipAddress = "");

        /// <summary>
        /// 46、退出力控模式,并设置退出后 robot 的工作模式。.
        /// </summary>
        /// <param name="exitMode">控制模式。0：代表位置模式； 1：代表关节空间阻抗模式；2：代表笛卡尔空间阻抗模式。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int leaveForceMode(int exitMode, string ipAddress = "");

        /// <summary>
        /// 47、设置默认工具坐标系。在没有调用该函数时，默认工具中心点为法兰盘中心，调用该函
        /// 数 后 ， 默认 的 工 具坐标 系 将 被改 变 。 该函数 将 会 改变 moveTCP，rotationTCP，
        /// moveJToPos，moveLToPose，speedJ，speedL，forward，inverse，getTcpPos，
        /// getTcpExternalForce 的默认行为。.
        /// </summary>
        /// <param name="arrPose">输入参数。TCP 相对于末端法兰盘的位姿向量的首地址，数组长度为 6。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setDefaultActiveTcpPose(double[] arrPose, string ipAddress = "");

        /// <summary>
        /// 48、设置笛卡尔空间碰撞检测 TCP 的合力矩阈值。.
        /// </summary>
        /// <param name="force">合力值.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setResultantCollision(double force, string ipAddress = "");

        /// <summary>
        /// 49、设置 Robot 各关节阻抗参数，包含钢度 Stiffiness 和阻尼 Damping 的数据。.
        /// </summary>
        /// <param name="arrStiff">表示各关节钢度 Stiffiness 的数组的首地址，数组长度为 7。.</param>
        /// <param name="arrDamp">表示各关节阻尼 Damping 的数组的首地址，数组长度为 7。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setJointImpedance(double[] arrStiff, double[] arrDamp, string ipAddress = "");

        /// <summary>
        /// 50、获取 Robot 各关节阻抗参数，包含钢度 Stiffiness 和阻尼 Damping 的数据。.
        /// </summary>
        /// <param name="arrStiff">表示各关节钢度 Stiffiness 的数组的首地址，数组长度为 7，用于接收获取到的值.</param>
        /// <param name="arrDamp">表示各关节阻尼 Damping 的数组的首地址，数组长度为 7，用于接收获取到的值.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getJointImpedance(out double[] arrStiff, out double[] arrDamp, string ipAddress = "");

        /// <summary>
        /// 51、设置笛卡尔空间阻抗参数。.
        /// </summary>
        /// <param name="arrStiff">表示笛卡尔空间，各维度钢度 Stiffiness 的数组的首地址，数组长度为 6。.</param>
        /// <param name="arrDamp">表示笛卡尔空间，各维度阻尼 Damping 的数组的首地址，数组长度为 6。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setCartImpedance(double[] arrStiff, double[] arrDamp, string ipAddress = "");

        /// <summary>
        /// 52、获取笛卡尔空间各维度阻抗参数，包含钢度 Stiffiness 和阻尼 Damping 的数据。.
        /// </summary>
        /// <param name="arrStiff">表示笛卡尔空间，各维度钢度 Stiffiness 的数组的首地址，数组长度为 6，用于接收获取到的值。.</param>
        /// <param name="arrDamp">表示笛卡尔空间，各维度阻尼 Damping 的数组的首地址，数组长度为 6，用于接收获取到的值。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getCartImpedance(out double[] arrStiff, out double[] arrDamp, string ipAddress = "");

        /// <summary>
        /// 53、进入或退出零空间自由驱动模式。.
        /// </summary>
        /// <param name="enable">输入参数。True 为进入零空间自由驱动模式；False 为退出零空间自由驱动模式.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int zeroSpaceFreeDriving(bool enable, string ipAddress = "");

        /// <summary>
        /// 54、创建一个路段。.
        /// </summary>
        /// <param name="type">输入参数。1 :表示 moveJ, 2：表示 moveL。.</param>
        /// <param name="idPath">输出参数。用于保存新创建 Path 的 ID。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int createPath(int type, ref uint idPath, string ipAddress = "");

        /// <summary>
        /// 55、向已创建的路段添加 MoveL 路点。.
        /// </summary>
        /// <param name="idPath">输入参数。要添加路点的路径 ID。.</param>
        /// <param name="joints">输入参数。要添加的路点，即该路点的各关节角度数组的首地址，数组长度为7。单位：rad。.</param>
        /// <param name="vel">MoveL移动到目标路点的速度。单位：m/s。.</param>
        /// <param name="acc">MoveL移动到目标路点的加速度。单位：m/s2。.</param>
        /// <param name="blendRadius">交融半径。单位：m。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int addMoveL(uint idPath, double[] joints, double vel, double acc, double blendRadius, string ipAddress = "");

        /// <summary>
        /// 56、向已创建的路段添加 MoveJ 路点。.
        /// </summary>
        /// <param name="idPath">输入参数。要添加路点的路径 ID。.</param>
        /// <param name="joints">输入参数。要添加的路点，即该路点的各关节角度数组的首地址，数组长度为7。单位：rad。.</param>
        /// <param name="velPercent">moveJ 移动到目标路点的速度百分比。（拟改为 rad/s）.</param>
        /// <param name="accPercent">moveJ 移动到目标路点的加速度百分比。（拟改为 rad/s2）.</param>
        /// <param name="blendradiusPercent">交融半径百分比。每个关节位移的一半为交融半径最大值。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int addMoveJ(uint idPath, double[] joints, double velPercent, double accPercent, double blendradiusPercent, string ipAddress = "");

        /// <summary>
        /// 57、启动运行设置好的路段。.
        /// </summary>
        /// <param name="idPath">输入参数。要运行的路径 ID。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int runPath(uint idPath, string ipAddress = "");

        /// <summary>
        /// 58、销毁路段。.
        /// </summary>
        /// <param name="idPath">输入参数。要销毁的路径 ID。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int destroyPath(uint idPath, string ipAddress = "");

        /// <summary>
        /// 59、欧拉角转轴角。.
        /// </summary>
        /// <param name="arr">输入参数。欧拉角数组的首地址，数组长度为 3。.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int rpy2Axis(double[] arr);

        /// <summary>
        /// 60、轴角转欧拉角。.
        /// </summary>
        /// <param name="arr">输入参数。轴角数组的首地址，数组长度为 3。.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int axis2RPY(double[] arr);

        /// <summary>
        /// 61、齐次变换矩阵转位姿。.
        /// </summary>
        /// <param name="matrix">齐次变换矩阵数组的首地址，数组长度为 16。.</param>
        /// <param name="pose">位姿数组的首地址，数组长度为 6。.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int homogeneous2Pose(double[] matrix, out double[] pose);

        /// <summary>
        /// 62、位姿转齐次变换矩阵。.
        /// </summary>
        /// <param name="pose">位姿数组的首地址，数组长度为 6。.</param>
        /// <param name="matrix">齐次变换矩阵数组的首地址，数组长度为 16。.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int pose2Homogeneous(double[] pose, out double[] matrix);

        /// <summary>
        /// 在指定 IP 地址机械臂上，打开或关闭实时扭矩的接受。.
        /// </summary>
        /// <param name="enable">输入参数，是否开启实时扭矩的接收。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int enableTorqueReceiver(bool enable, string ipAddress = "");

        /// <summary>
        /// 对指定 IP 地址机械臂，用户发送实时扭矩.
        /// </summary>
        /// <param name="torque">输入参数，用户传入的扭矩值，大小为 7 的数组。.</param>
        /// <param name="t">持续时间，单位 ms.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int sendTorqueRt(double[] torque, double t, string ipAddress = "");

        /// <summary>
        /// 开启指定 IP 地址机械臂碰撞检测.
        /// </summary>
        /// <param name="enable">输入参数，是否开启碰撞检测模式。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int enableCollisionDetection(bool enable, string ipAddress = "");

        /// <summary>
        /// 设置指定 IP 地址机械臂的负载信息.
        /// </summary>
        /// <param name="payload">负载信息，第 1 位为质量，2~4 位为质心，5~10 位为张量，大小为 10 的数组.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setActiveTcpPayload(double[] payload, string ipAddress = "");

        /// <summary>
        /// 63、关节空间内，伺服到指定关节角位置。 Servo 函数用于在线控制机器人，lookahead 时间
        /// 和 gain 能够调整轨迹是否平滑或尖锐。 注意：太高的 gain 或太短的 lookahead 时间可能
        /// 会导致不稳定。由于该函数主要用于以较短位移为目标点的多次频繁调用，建议在实时
        /// 系统环境下使用。.
        /// </summary>
        /// <param name="joints">目标关节角位置数组的首地址，数组长度为 7。单位：rad。.</param>
        /// <param name="time">运动时间。单位：s。.</param>
        /// <param name="lookAheadTime">时间（S），范围（0.03-0.2）用这个参数使轨迹更平滑。.</param>
        /// <param name="gain">目标位置的比例放大器，范围（100,2000）。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int servoJ(double[] joints, double time, double lookAheadTime, double gain, string ipAddress = "");

        /// <summary>
        /// 64、笛卡尔空间内，伺服到指定位姿。由于该函数主要用于以较短位移为目标点的多次频繁调用，建议在实时系统环境下使用。.
        /// </summary>
        /// <param name="pose">目标位姿数组的首地址，数组长度为 6。前三个元素单位：m；后三个元素单位：rad.</param>
        /// <param name="time">运动时间。单位：s。.</param>
        /// <param name="lookAheadTime">时间（S），范围（0.03-0.2）用这个参数使轨迹更平滑。.</param>
        /// <param name="gain">目标位置的比例放大器，范围（100,2000）.</param>
        /// <param name="scale">平滑比例系数。范围（0.0~1.0）。.</param>
        /// <param name="activeTcp">目标工具坐标系相对末端位姿，double 数组的首地址，数组长度为 6.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int servoL(double[] pose, double time, double lookAheadTime, double gain, double scale, double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 67、关节空间内，伺服到指定关节角位置优化版。 Servo 函数用于在线控制机器人，
        ///   lookahead 时间和 gain 能够调整轨迹是否平滑或尖锐。 注意：太高的 gain 或太短的
        /// lookahead 时间可能会导致不稳定。由于该函数主要用于以较短位移为目标点的多次频繁
        ///   调用，建议在实时系统环境下使用。.
        /// </summary>
        /// <param name="joints">目标关节角位置数组的首地址，数组长度为 7。单位：rad。.</param>
        /// <param name="time">运动时间。单位：s。.</param>
        /// <param name="lookAheadTime">：时间（S），范围（0.03-0.2）用这个参数使轨迹更平滑。.</param>
        /// <param name="gain">目标位置的比例放大器，范围（100,2000）。.</param>
        /// <param name="realiable">bool 型变量，值为 true 需要 socket 反馈通信状态，行为等同 servoJ；值为 false  则无需反馈直接返回。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int servoJ_ex(double[] joints, double time, double lookAheadTime, double gain, bool realiable, string ipAddress = "");

        /// <summary>
        /// 68、笛卡尔空间内，伺服到指定位姿优化版。由于该函数主要用于以较短位移为目标点的多次频繁调用，建议在实时系统环境下使用。.
        /// </summary>
        /// <param name="pose">目标位姿数组的首地址，数组长度为 6。前三个元素单位：m；后三个元素单位： rad。.</param>
        /// <param name="time">运动时间。单位：s。.</param>
        /// <param name="lookAheadTime">时间（S），范围（0.03-0.2）用这个参数使轨迹更平滑。.</param>
        /// <param name="gain">目标位置的比例放大器，范围（100,2000）。.</param>
        /// <param name="scale">平滑比例系数。范围（0.0~1.0）.</param>
        /// <param name="realiable">bool 型变量，值为 true 需要 socket 反馈通信状态，行为等同 servoL；值为 false 则无需反馈直接返回。.</param>
        /// <param name="active_tcp">目标工具坐标系相对末端位姿，double 数组的首地址，数组长度为 6.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int servoL_ex(double[] pose, double time, double lookAheadTime, double gain, double scale, bool realiable, double[] active_tcp, string ipAddress = "");

        /// <summary>
        /// 65、速度模式优化版，关节空间运动。时间 t 为可选项，时间 t 是可选项，如果提供了 t 值，
        ///   机器人将在 t 时间后减速。如果没有提供时间 t 值，机器人将在达到目标速度时减速。该
        ///   函数调用后立即返回。停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="speed">关节角速度数组首地址，数组长度为 7。单位：rad/s.</param>
        /// <param name="a">加速度，单位：rad/s2。.</param>
        /// <param name="t">时间，单位：s。.</param>
        /// <param name="realiable">bool 型变量，值为 true 需要 socket 反馈通信状态，行为等同 speedJ；值为 false 则无需反馈直接返回。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int speedJ_ex(double[] speed, double a, double t, bool realiable, string ipAddress = "");

        /// <summary>
        /// 66、速度模式优化版，笛卡尔空间下直线运动。时间 t 为可选项，时间 t 是可选项，如果提供
        ///  了 t 值，机器人将在 t 时间后减速。如果没有提供时间 t 值，机器人将在达到目标速度时
        ///  减速。该函数调用后立即返回。停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="speed">工具空间速度，数组长度为 6,其中前 3 个单位为 m/s，后 3 个单位为 rad/s。.</param>
        /// <param name="a">加速度，单位：m/s2。.</param>
        /// <param name="t">时间，单位：s。.</param>
        /// <param name="realiable">bool型变量，值为 true 需要 socket反馈通信状态，行为等同 speedL；值为 false则无需反馈直接返回。.</param>
        /// <param name="activeTcp">目标工具坐标系相对末端位姿，double 数组的首地址，数组长度为 6.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int speedL_ex(double[] speed, double a, double t, bool realiable, double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 69、导出日志文件到 u 盘。控制箱中的系统日志文件（主要包含 ControllerLog.txt 和
        ///  DianaServerLog.txt）会自动复制到 u 盘。需要注意的是目前控制箱仅支持 FAT32 格式 u
        ///盘，调用 dumpToUDisk 函数前需先插好 u 盘，如果系统日志拷贝失败将不会提示。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int dumpToUDisk(string ipAddress = "");

        /// <summary>
        /// 针对指定 IP 地址机器人，逆解函数，给定一个参考关节角，算出欧式距离最近的逆解。现支持在不同工具坐标系下求逆解，由传入的 active_tcp 决定。.
        /// </summary>
        /// <param name="refJoints">参考的关节角，大小为 7 的数组。.</param>
        /// <param name="pose">输入位姿数组首地址，数据为包含 active_tcp 坐标（x, y, z）和旋转矢量（轴角坐 标）组合。.</param>
        /// <param name="joints">输出关节角度数组首地址，用于传递转换的结果。.</param>
        /// <param name="activeTcp">工具坐标系对应的位姿向量，大小为 6 的数组，为空时，将使用默认的工具坐标系 default_tcp.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int inverse_ext(double[] refJoints, double[] pose, out double[] joints, double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 获取指定 IP 地址机械臂当前低速侧关节角.
        /// </summary>
        /// <param name="joints">输出参数。低速侧关节角,大小为 7 的数组。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getJointLinkPos(out double[] joints, string ipAddress = "");

        enum ComplexPathType
        {

            NORMAL_JOINT_PATH = 0,
            MOVEP_JOINT_PATH = 1,
            NORMAL_POSE_PATH = 2,
            MOVEP_POSE_PATH = 3,
        };

        /// <summary>
        /// 104、创建一个复杂路段。.
        /// </summary>
        /// <param name="complexPathType">The complex_path_type<see cref="int"/>.</param>
        /// <param name="complexPathId">输出参数。用于保存新创建 complexPath 的 ID。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int createComplexPath(int complexPathType, ref uint complexPathId, string ipAddress = "");

        /// <summary>
        /// 105、向已创建的路段添加 MoveL 路点。.
        /// </summary>
        /// <param name="complexPathId">输入参数。要添加路点的路径 ID。.</param>
        /// <param name="targetJoints">输入参数。要添加的路点，即该路点的各关节角度数组的首地址，数组长 度为 7。单位：rad。.</param>
        /// <param name="vel">移动到目标路点的速度。单位：m/s.</param>
        /// <param name="acc">移动到目标路点的加速度。单位：m/s2。.</param>
        /// <param name="blendRadius">交融半径。单位：m。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int addMoveLByTarget(uint complexPathId, double[] targetJoints, double vel, double acc, double blendRadius, string ipAddress = "");

        /// <summary>
        /// 106、向已创建的路段添加 MoveL 路点。.
        /// </summary>
        /// <param name="complexPathId">输入参数。要添加路点的路径 ID。.</param>
        /// <param name="targetPose">输入参数。路径点位姿数组首地址，数组长度为 6，保存 TCP 坐标（x, y,  z）和轴角（rx, ry, rz）组合数据。.</param>
        /// <param name="vel">moveL 移动到目标路点的速度。单位：m/s。.</param>
        /// <param name="acc">moveL 移动到目标路点的加速度。单位：m/s2。.</param>
        /// <param name="blendRadius">交融半径。单位：m。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int addMoveLByPose(/*IN*/ uint complexPathId, /*IN[6]*/ double[] targetPose, /*IN*/ double vel,   /*IN*/ double acc, /*IN*/ double blendRadius, string ipAddress = "");

        /// <summary>
        /// 107、向已创建的路段添加 MoveJ 路点。.
        /// </summary>
        /// <param name="complexPathId">输入参数。要添加路点的路径 ID。.</param>
        /// <param name="targetJoints">输入参数。要添加的路点，即该路点的各关节角度数组的首地址，数组长度为 7。单位：rad。.</param>
        /// <param name="velPercent">moveJ 移动到目标路点的速度百分比，相对于系统软限位中设定最大速度的百分比.</param>
        /// <param name="accPercent">moveJ 移动到目标路点的加速度百分比，相对于系统软限位中设定最大加速度的百分比。.</param>
        /// <param name="blendRadiusPercent">交融半径百分比，相对于系统软限位中设定最大交融半径的百分比.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int addMoveJByTarget(/*IN*/ uint complexPathId, /*IN[7]*/ double[] targetJoints,/*IN*/ double velPercent, /*IN*/ double accPercent, /*IN*/ double blendRadiusPercent, string ipAddress = "");

        /// <summary>
        /// 108、向已创建的路段添加 MoveJ 路点。.
        /// </summary>
        /// <param name="complexPathId">输入参数。要添加路点的路径 ID。.</param>
        /// <param name="targetPose">输入参数。要添加的路点，路径点位姿数组首地址，数组长度为 6，保存TCP 坐标（x, y, z）和轴角（rx, ry, rz）组合数据。.</param>
        /// <param name="velPercent">moveJ 移动到目标路点的速度百分比。相对于系统软限位中设定最大速度的百分比。.</param>
        /// <param name="accPercent">moveJ 移动到目标路点的加速度百分比。相对于系统软限位中设定最大加速度的百分比。.</param>
        /// <param name="blendRadiusPercent">交融半径百分比。相对于系统软限位中设定最大交融半径的百分比。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int addMoveJByPose(/*IN*/ uint complexPathId, /*IN[6]*/ double[] targetPose,  /*IN*/ double velPercent, /*IN*/ double accPercent, /*IN*/ double blendRadiusPercent, string ipAddress = "");

        /// <summary>
        /// 109、向已创建的路段添加 MoveC 路点。.
        /// </summary>
        /// <param name="complexPathId">输入参数。要添加路点的路径 ID。.</param>
        /// <param name="passJoints">输入参数。要添加的 moveC 中间路点，即该路点的各关节角度数组的首地址，数组长度为 7。单位：rad。.</param>
        /// <param name="targetJoints">输入参数。要添加的 moveC 目标路点，即该路点的各关节角度数组的首地址，数组长度为 7。单位：rad。.</param>
        /// <param name="vel">moveC 移动到目标路点的速度。单位：m/s。.</param>
        /// <param name="acc">moveC 移动到目标路点的加速度。单位：m/s2。.</param>
        /// <param name="blendRadius">交融半径。单位：m。.</param>
        /// <param name="ignoreRotation">是否忽略姿态变化.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int addMoveCByTarget(/*IN*/ uint complexPathId, /*IN[7]*/ double[] passJoints,  /*IN[7]*/ double[] targetJoints, /*IN*/ double vel, /*IN*/ double acc, /*IN*/ double blendRadius, /*IN*/ bool ignoreRotation, string ipAddress = "");

        /// <summary>
        /// 110、向已创建的路段添加 MoveC 路点。.
        /// </summary>
        /// <param name="complexPathId">输入参数。要添加路点的路径 ID.</param>
        /// <param name="passPose">输入参数。要添加的 moveC中间路点，即路径点位姿数组首地址，数组长度为 6，保存 TCP 坐标（x, y, z）和轴角（rx, ry, rz）组合数据.</param>
        /// <param name="targetPose">输入参数。要添加的 moveC 目标路点，即该路径点位姿数组首地址，数组长度为 6，保存 TCP 坐标（x, y, z）和轴角（rx, ry, rz）组合数据。.</param>
        /// <param name="vel">moveC 移动到目标路点的速度。单位：m/s。.</param>
        /// <param name="acc">moveC 移动到目标路点的加速度。单位：m/s2。.</param>
        /// <param name="blendRadius">交融半径。单位：m。.</param>
        /// <param name="ignoreRotation">是否忽略姿态变化.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int addMoveCByPose(/*IN*/ uint complexPathId, /*IN[6]*/ double[] passPose,   /*IN[6]*/ double[] targetPose, /*IN*/ double vel, /*IN*/ double acc,     /*IN*/ double blendRadius, /*IN*/ bool ignoreRotation, string ipAddress = "");

        /// <summary>
        /// 111、启动运行设置好的路段。.
        /// </summary>
        /// <param name="complexPathId">输入参数。要运行的路径 ID。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int runComplexPath(/*IN*/ uint complexPathId, string ipAddress = "");

        /// <summary>
        /// 112、销毁路段。.
        /// </summary>
        /// <param name="complexPathId">输入参数。要销毁的路径 ID。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int destroyComplexPath(/*IN*/ uint complexPathId, string ipAddress = "");

        /// <summary>
        /// 70、将控制器当前参数数据写入配置文件，用于重启机器人时初始化设置各参数，包括碰撞检测阈值、阻抗参数、DH 参数等所用可通过 API 设置的参数数据。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int saveEnvironment(string ipAddress = "");

        /// <summary>
        /// 导出指定 IP 地址机械臂的日志文件到 u 盘。控制箱中的系统日志文件（主要包含
        /// ControllerLog.txt 和 DianaServerLog.txt）会自动复制到 u 盘。需要注意的是目前控制箱仅
        /// 支持 FAT32 格式 u 盘，调用 dumpToUDiskEx 函数前需先插好 u 盘，如果系统日志拷贝失
        /// 败将不会提示。.
        /// </summary>
        /// <param name="timeoutSecond">单位为秒，设置超时时间，一般需要大于 3 秒,-1 表示设置超时时间无穷大。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int dumpToUDiskEx(/*IN*/ double timeoutSecond, /*IN*/string ipAddress = "");

        /// <summary>
        /// 使指定 IP 地址机械臂进入力控模式，相较于 enterForceMode 增加不同坐标系的支持。.
        /// </summary>
        /// <param name="forceDirection">表达力的方向的数组首地址，数组长度为 3。.</param>
        /// <param name="forceValue">力大小。单位：N。推荐取值范围（1N~100N）。.</param>
        /// <param name="maxApproachVelocity">最大接近速度。单位：m/s。推荐取值范围（0.01m/s~0.5m/s）。.</param>
        /// <param name="maxAllowTcpOffset">允许的最大偏移。单位：m。推荐取值范围（0.01m~1m）。.</param>
        /// <param name="activeTcp">可选参数，坐标系对应的位姿向量，大小为 6 的数组，不传则默认为基坐标系.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int enterForceMode_ex(/*IN[3]*/ double[] forceDirection, /*[IN]*/ double forceValue, /*[IN]*/ double maxApproachVelocity, /*[IN]*/ double maxAllowTcpOffset,   /*IN[6]*/ double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 71、读取一个数字输入的值。.
        /// </summary>
        /// <param name="groupName">数字输入的分组，例如，’board’,’plc’；.</param>
        /// <param name="ioName">数字输入的信号名，例如，’di0’；.</param>
        /// <param name="value">读取返回的值。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int readDI(/*IN*/ string groupName, /*IN*/ string ioName, ref int value, string ipAddress = "");

        /// <summary>
        /// 72、读取一个模拟输入的值和模式。.
        /// </summary>
        /// <param name="groupName">模拟输入的分组，例如，’board’,’plc’；.</param>
        /// <param name="ioName">模拟输入的信号名，例如，’ai0’；.</param>
        /// <param name="mode">当前模拟输入模式；.</param>
        /// <param name="value">读取返回的值。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int readAI(/*IN*/string groupName, /*IN*/ string ioName, /*OUT*/out int mode, /*OUT*/out double value, string ipAddress = "");

        /// <summary>
        /// 73、设置模拟输入的模式。.
        /// </summary>
        /// <param name="groupName">模拟输入的分组，例如，’board’,’plc’；.</param>
        /// <param name="ioName">模拟输入的信号名，例如，’ai0’；.</param>
        /// <param name="mode"> 模拟输入模式，1 代表电流，2 代表电压。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setAIMode(/*IN*/ string groupName, /*IN*/ string ioName, /*IN*/ int mode, string ipAddress = "");

        /// <summary>
        /// 74.设置一个数字输出的值。 可用于改变模拟信号的模式，将信号名替换为“aimode”或“aomode”。.
        /// </summary>
        /// <param name="groupName">数字输出的分组，例如，’board’,’plc’；.</param>
        /// <param name="ioName">数字输出的信号名，例如，’do0’；.</param>
        /// <param name="value">设置的值。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int writeDO(/*IN*/ string groupName, /*IN*/ string ioName, /*IN*/ int value, string ipAddress = "");

        /// <summary>
        /// 75、设置一个模拟输出的值和模式。.
        /// </summary>
        /// <param name="groupName">模拟输出的分组，例如，’board’,’plc’；.</param>
        /// <param name="ioName">模拟输出的信号名，例如，’ao0’；.</param>
        /// <param name="mode">当前模拟输出模式；.</param>
        /// <param name="value">设置输出的值。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int writeAO(/*IN*/ string groupName, /*IN*/ string ioName, /*IN*/ int mode, /*IN*/ double value, string ipAddress = "");

        /// <summary>
        /// 76、读取总线电流。.
        /// </summary>
        /// <param name="current">总线电流。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int readBusCurrent(/*OUT*/out double current, string ipAddress = "");

        /// <summary>
        /// 77、读取总线电压。.
        /// </summary>
        /// <param name="voltage">总线电压。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int readBusVoltage(/*OUT*/out double voltage, string ipAddress = "");

        /// <summary>
        /// 78、获取 DH 参数.
        /// </summary>
        /// <param name="aDH">连杆长度的数组，长度为 7.</param>
        /// <param name="alphaDH">连杆转角的数组，长度为 7.</param>
        /// <param name="dDH">连杆偏距的数组，长度为 7.</param>
        /// <param name="thetaDH">连杆的关节角的数组，长度为 7.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getDH(/*OUT[7]*/out double[] aDH, /*OUT[7]*/out double[] alphaDH, /*OUT[7]*/out double[] dDH,   /*OUT[7]*/out double[] thetaDH, string ipAddress = "");

        /// <summary>
        /// 79、获取传感器反馈的扭矩值，未减去零偏。.
        /// </summary>
        /// <param name="torques">反馈扭矩的数组，长度为 7.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getOriginalJointTorque(/*OUT[7]*/out double[] torques, string ipAddress = "");

        /// <summary>
        /// 80、获取雅各比矩阵。.
        /// </summary>
        /// <param name="matrixJacobi">雅各比矩阵的数组，长度为 42.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getJacobiMatrix(/*OUT[42]*/out double[] matrixJacobi, string ipAddress = "");

        /// <summary>
        /// 81、重置用户自定义 DH 参数。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int resetDH(string ipAddress = "");

        /// <summary>
        /// 82、运行程序。.
        /// </summary>
        /// <param name="programName">程序的名字。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int runProgram(string programName, string ipAddress = "");

        /// <summary>
        /// 83、停止指定程序。.
        /// </summary>
        /// <param name="programName">程序的名字。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int stopProgram(string programName, string ipAddress = "");

        /// <summary>
        /// 84、获取全局变量的值。.
        /// </summary>
        /// <param name="variableName">全局变量的名字.</param>
        /// <param name="value">获取的全局变量的值.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getVariableValue(string variableName, out double value, string ipAddress = "");

        /// <summary>
        /// 85、设置全局变量的值。.
        /// </summary>
        /// <param name="variableName">全局变量的名字.</param>
        /// <param name="value">设置的全局变量的值.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setVariableValue(string variableName, double value, string ipAddress = "");

        /// <summary>
        /// 86、判断指定程序是否在运行。.
        /// </summary>
        /// <param name="variableName">程序名称.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>0：运行中。-1：没有运行中。.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int isTaskRunning(string variableName, string ipAddress = "");

        /// <summary>
        /// 87、暂停所有程序。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int pauseProgram(string ipAddress = "");

        /// <summary>
        /// 88、恢复运行已经暂停的程序。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int resumeProgram(string ipAddress = "");

        /// <summary>
        /// 89、停止所有程序。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int stopAllProgram(string ipAddress = "");

        /// <summary>
        /// 90、是否有程序在运行。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int isAnyTaskRunning(string ipAddress = "");

        /// <summary>
        /// 91、清除错误信息。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int cleanErrorInfo(string ipAddress = "");

        /// <summary>
        /// 92、设置碰撞检测类型.
        /// </summary>
        /// <param name="level">碰撞等级，int 类型.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setCollisionLevel(int level, string ipAddress = "");

        /// <summary>
        /// 93、映射 int8 型变量。.
        /// </summary>
        /// <param name="variantName">变量名称.</param>
        /// <param name="index">映射变量序号.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int mappingInt8Variant(string variantName, int index, string ipAddress = "");

        /// <summary>
        /// 94、映射 double 型变量。.
        /// </summary>
        /// <param name="variantName">变量名称.</param>
        /// <param name="index">映射变量序号.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int mappingDoubleVariant(string variantName, int index, string ipAddress = "");

        /// <summary>
        /// 95、映射数字信号 IO。.
        /// </summary>
        /// <param name="groupName">IO 组名.</param>
        /// <param name="name">IO 名字.</param>
        /// <param name="index">映射序号.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int mappingInt8IO(string groupName, string name, int index, string ipAddress = "");

        /// <summary>
        /// 96、映射模拟信号 IO。.
        /// </summary>
        /// <param name="groupName">IO 组名.</param>
        /// <param name="name">IO 名称.</param>
        /// <param name="index">映射序号.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int mappingDoubleIO(string groupName, string name, int index, string ipAddress = "");

        /// <summary>
        /// 97、设置地址映射。.
        /// </summary>
        /// <param name="address">double 型数据的映射地址，double 数组.</param>
        /// <param name="itemCount">double 型数据的映射数量，int 型，最大支持 40 个.</param>
        /// <param name="address">int8 型数据的映射地址，int8 数组.</param>
        /// <param name="itemCount"> int8 型数据的映射数量，int 型，最大支持 160 个.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setMappingAddress(double[] address, int itemCount, System.UInt16[] address8i, int itemCount8i, string ipAddress = "");

        /// <summary>
        /// 98、访问数据时加锁。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int lockMappingAddress(string ipAddress = "");

        /// <summary>
        /// 99、解锁数据锁。.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int unlockMappingAddress(string ipAddress = "");

        /// <summary>
        /// 在指定 IP 地址机械臂上，获取其机械臂的关节数目.
        /// </summary>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>非负数：关节数目。-1：失败。.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getJointCount(string ipAddress = "");

        /// <summary>
        /// 101、获取路点变量信息。.
        /// </summary>
        /// <param name="waypointName">路点变量名称 。.</param>
        /// <param name="tcpPos">位姿信息。.</param>
        /// <param name="joints">关节角信息。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getWayPoint(string waypointName, double[] tcpPos, double[] joints, string strToolName, string strWorkpieceName,string ipAddress = "");

        /// <summary>
        /// 100、修改路点变量。.
        /// </summary>
        /// <param name="waypointName">路点变量名称 。.</param>
        /// <param name="tcpPos">位姿信息。.</param>
        /// <param name="joints">关节角信息。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setWayPoint(string waypointName, double[] tcpPos, double[] joints, string strToolName, string strWorkpieceName, string ipAddress = "");

        /// <summary>
        /// 102、新增路点变量。.
        /// </summary>
        /// <param name="waypointName">路点变量名称 。.</param>
        /// <param name="tcpPos">位姿信息。.</param>
        /// <param name="joints">关节角信息。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int addWayPoint(string waypointName, double[] tcpPos, double[] joints,string strToolName,string strWorkpieceName, string ipAddress = "");

        /// <summary>
        /// 103、删除路点变量。.
        /// </summary>
        /// <param name="waypointName">路点变量名称。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int deleteWayPoint(string waypointName, string ipAddress = "");

        /// <summary>
        /// 获取指定 IP 地址机械臂的默认工具坐标系。.
        /// </summary>
        /// <param name="defaultTcp">输出参数，Tcp 相对于末端法兰盘的 4*4 齐次变换矩阵的首地址，数组长度为 16。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getDefaultActiveTcp(/*OUT[16]*/out double[] defaultTcp, string ipAddress = "");

        /// <summary>
        /// 获取指定 IP 地址机械臂默认的工具坐标系的位姿。.
        /// </summary>
        /// <param name="arrPose">输出参数。TCP 相对于末端法兰盘的位姿向量的首地址，数组长度为 6.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getDefaultActiveTcpPose(/*OUT[6]*/ double[] arrPose, string ipAddress = "");

        /// <summary>
        /// 获取指定 IP 地址机械臂的负载信息.
        /// </summary>
        /// <param name="payload">负载信息，第 1 位为质量，2~4 位为质心，5~10 位为张量，大小为 10 的数组.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getActiveTcpPayload(/*OUT[10]*/out double[] payload, string ipAddress = "");

        /// <summary>
        /// 控制指定 IP 地址机械臂进入或退出零空间手动移动模式。.
        /// </summary>
        /// <param name="direction">输入参数，控制零空间手动运动的方向，1 表示向前，-1 表示向后运动.</param>
        /// <param name="jointsVel">输入参数，各关节运动的速度，大小为 7 的数组，单位 rad/s.</param>
        /// <param name="jointAcc">输入参数，各关节运动的加速度，大小为 7 的数组，单位 rad/s.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int zeroSpaceManualMove(/*IN*/ int direction, /*IN[7 or 6]*/ double[] jointsVel,  /*IN[7 or 6]*/ double[] jointAcc, string ipAddress = "");

        /// <summary>
        /// 手动移动指定 IP 地址的机械臂末端中心点。该函数会立即返回，停止运动需要调用 stop函数。.
        /// </summary>
        /// <param name="c">坐标系类型.</param>
        /// <param name="d">表示移动方向的枚举类型.</param>
        /// <param name="v">速度，单位：m/s。.</param>
        /// <param name="a">加速度，单位：m/s2.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int moveTcp_ex(/*IN*/ CoordinateE c, /*IN*/ TcpDirectionE d, /*IN*/ double v, /*IN*/ double a, string ipAddress = "");

        /// <summary>
        /// 使指定的 IP 地址的机械臂绕末端中心点变换位姿。该函数会立即返回，停止运动需要调用 stop 函数。.
        /// </summary>
        /// <param name="c">坐标系类型.</param>
        /// <param name="d">表示移动方向的枚举类型.</param>
        /// <param name="v">速度，单位：rad/s。.</param>
        /// <param name="a">加速度，单位：rad/s2。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int rotationTCP_ex(/*IN*/ CoordinateE c, /*IN*/ TcpDirectionE d, /*IN*/ double v, /*IN*/ double a, string ipAddress = "");

        /// <summary>
        /// 针对指定 IP 地址机械臂，设置其附加力矩的滤波截止频率.
        /// </summary>
        /// <param name="freq">输入参数，附加力矩的滤波截止频率，需要提供一个正值。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setExternalAppendTorCutoffFreq(/*IN*/ double freq, string ipAddress = "");

        /// <summary>
        /// 把指定坐标系下的位姿转换到其他坐标系下.
        /// </summary>
        /// <param name="srcPose">输入参数，源坐标系下的位姿，大小为 6 的数组.</param>
        /// <param name="srcMatrixPose">输入参数，源坐标系对应的位姿向量 ，大小为 6 的数组.</param>
        /// <param name="dstMartixPose">输入参数，目标坐标系对应的位姿向量 ，大小为 6 的数组.</param>
        /// <param name="dstPose">输出参数，转换到目标坐标系下的位姿，大小为 6 的数组.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int poseTransform(/*IN[6]*/ double[] srcPose, /*IN[6]*/ double[] srcMatrixPose, /*IN[6]*/ double[] dstMartixPose,    /*OUT[6]*/out double[] dstPose);

        /// <summary>
        /// 针对指定 IP 地址机械臂，使能其末端零力驱动按钮.
        /// </summary>
        /// <param name="enable">输入参数，使能末端零力驱动按钮.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setEndKeyEnableState(bool enable, string ipAddress = "");

        /// <summary>
        /// 针对指定 IP 地址机械臂，在力控模式下，实时改变力指令的大小与方向.
        /// </summary>
        /// <param name="forceDirection">输入参数，力的方向，大小为三的数组，分别表示下 x,y,z 方向的分量.</param>
        /// <param name="forceValue">输入参数，力的大小，需要是一个大于 0 的数.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int updateForce(/*IN[3]*/ double[] forceDirection, /*[IN]*/ double forceValue, string ipAddress = "");

        /// <summary>
        /// 针对指定 IP 地址机械臂，在力控模式下，实时改变力指令的大小与方向，相比于updateForce 增加了对工具坐标系支持.
        /// </summary>
        /// <param name="forceDirection">输入参数，力的方向，大小为三的数组,分别表示在 xyz 各方向上的分量.</param>
        /// <param name="forceValue">输入参数，力的大小，单位 N.</param>
        /// <param name="activeTcp">可选参数，坐标系对应的位姿向量，大小为 6 的数组，不传则默认为基坐标系.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int updateForce_ex(/*IN[3]*/ double[] forceDirection, /*[IN]*/ double forceValue,    /*IN[6]*/ double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 针对指定 IP 地址的机械臂，开启或关闭透传功能，即不经过控制器规划，使机械臂直接运动到目的点位.
        /// </summary>
        /// <param name="enable">输入参数，使能透传功能.</param>
        /// <param name="stepCnt">输入参数，当经过指定数目的点后机械臂才开始移动.</param>
        /// <param name="memCnt">输入参数，暂存点位的缓存区的大小，需要大于等于 uintStepCount，透传功能才生效.</param>
        /// <param name="getDataPeriod">输入参数，设置控制器从缓存区取点的间隔，单位 ms.</param>
        /// <param name="sampleRate">输入参数，滤波频率，当该值小于 0 或大于 1000 则不开启滤波.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int enablePassThrough(/*IN*/ bool enable, /*IN*/ System.UInt32 stepCnt, /*IN*/ System.UInt32 memCnt,    /*IN*/ uint getDataPeriod, /*IN*/ int sampleRate, string ipAddress = "");

        /// <summary>
        /// 针对指定 IP 地址的机械臂，实时发送透传需要的关节角度.
        /// </summary>
        /// <param name="joints">输入参数，想要机械臂运动到的关节角度.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int sendPassThroughJoints_rt(/*IN[7]*/ double[] joints, string ipAddress = "");

        /// <summary>
        /// 在指定 IP 地址机械臂上，基于工具坐标系，给定一个参考关节角，约束单轴求逆解，注 意，工业臂只能锁定七轴。现支持在不同工具坐标系下求逆解，由传入的 active_tcp 决定。.
        /// </summary>
        /// <param name="pose">输入位姿数组首地址，数据为包含 active_tcp 坐标（x, y, z）和旋转矢量（轴角坐 标）组合。当 active_tcp 不为空时，pose 描述工具中心点在基座标系下的位姿，否则active_tcp 描述法兰中心点在基座标系下的位姿。.</param>
        /// <param name="lockJointIndex">输入参数，被约束的关节号。.</param>
        /// <param name="lockJointPosition">输入参数，被约束关节的角度，单位为弧度。.</param>
        /// <param name="refJoints">参考的关节角，大小为 7 的数组.</param>
        /// <param name="activeTcp">工具坐标系对应的位姿向量，大小为 6 的数组。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>非负数：生成逆解对应的 ID-1：失败。返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int inverseClosedFull(/*IN[6]*/  double[] pose, /*IN*/ int lockJointIndex,   /*IN*/ double lockJointPosition, /*IN[7]*/ double[] refJoints,  /*IN[6]*/ double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 在指定 IP 地址的机械臂上，根据 ID 获取约束单轴求逆解结果的组数.
        /// </summary>
        /// <param name="id">输入参数，所求逆解对应的 ID。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>非负数：ID 对应逆解的组数。-1：失败。.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getInverseClosedResultSize(/*IN*/  int id, string ipAddress = "");

        /// <summary>
        /// 在指定 IP 地址机械臂上，根据 ID 按索引获取对应关节角。.
        /// </summary>
        /// <param name="id">输入参数，所求逆解对应的 ID。.</param>
        /// <param name="index">输入参数，对应的多组逆解中的编号。.</param>
        /// <param name="joints">输出参数，需要求的多组逆解中编号对应的逆解值.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getInverseClosedJoints(/*IN*/  int id, /*IN*/ int index, /*OUT*/out double[] joints, string ipAddress = "");

        /// <summary>
        /// 在指定 IP 地址机械臂上，根据 ID 删除约束单轴求逆解的结果数据集。.
        /// </summary>
        /// <param name="id">输入参数，逆解对应的 ID。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int destoryInverseClosedItems(/*IN*/int id, string ipAddress = "");

        /// <summary>
        /// 针对指定 IP 地址的机械臂，获取其安装信息的重力矢量。.
        /// </summary>
        /// <param name="grav">重力矢量，大小为 3 的数组。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getGravInfo(/*IN[3]*/ double[] grav, string ipAddress = "");

        /// <summary>
        /// 针对指定 IP 地址的机械臂，设置其重力矢量。.
        /// </summary>
        /// <param name="grav">重力矢量，大小为 3 的数组。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setGravInfo(/*IN[3]*/ double[] grav, string ipAddress = "");

        /// <summary>
        /// 针对指定 IP 地址的机械臂，获取其安装信息的轴角。.
        /// </summary>
        /// <param name="grav_axis">安装时的轴角，单位为 rad。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getGravAxis(/*IN[3]*/ double[] grav_axis, string ipAddress = "");

        /// <summary>
        /// 针对指定 IP 地址的机械臂，设置其安装信息的轴角。.
        /// </summary>
        /// <param name="gravAxis">安装轴角，单位 rad。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setGravAxis(/*IN[3]*/ double[] gravAxis, string ipAddress = "");

        /// <summary>
        /// 速度模式优化版，使指定 IP 地址机械臂笛卡尔空间下直线运动。时间 t 为可选项，时间 t
        /// 是可选项，如果提供了 t 值，机器人将在 t 时间后减速。如果没有提供时间 t 值，机器人
        ///     将在达到目标速度时减速。该函数调用后立即返回。停止运动需要调用 stop 函数。现支
        ///    持在不同工具坐标系下移动，由传入的 active_tcp 决定。.
        /// </summary>
        /// <param name="speed">工具空间速度，数组长度为 6,其中前 3 个单位为 m/s，后 3 个单位为 rad/s。 .</param>
        /// <param name="a">加速度，数组长度为 2，第 1 个单位：m/s2，第 2 个单位为 rad/s2。.</param>
        /// <param name="t">时间，单位：s.</param>
        /// <param name="activeTcp">当前工具坐标系对应的位姿向量，大小为 6 的数组，为空时，将使用默认的 工具坐标系 default_tcp。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int speedLOnTcp(/*IN[6]*/ double[] speed, /*IN[2]*/ double[] a, /*option*/ double t,     /*IN[6]*/ double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 在指定 IP 地址机械臂上，获取工具坐标系的 Tcp 外力值。.
        /// </summary>
        /// <param name="forces">输出参数，Tcp 外力，大小为 6。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int getTcpForceInToolCoordinate(/*OUT[6]*/out double[] forces, string ipAddress = "");

        /// <summary>
        /// 在指定 IP 地址机械臂上，求解末端法兰中心点坐标系相对于基坐标系的雅各比矩阵。.
        /// </summary>
        /// <param name="jacobiMatrix">输出参数，雅各比矩阵，大小为 6*7 的矩阵.</param>
        /// <param name="jointPosition">输入参数，用于计算雅各比矩阵的关节角，大小为 7 的数组。.</param>
        /// <param name="jointCount">输入参数，关节数量.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int calculateJacobi(/*OUT[6*JOINT_COUNT]*/out double[] jacobiMatrix, /*IN[7]*/ double[] jointPosition,  /*IN*/ int jointCount, string ipAddress = "");

        /// <summary>
        /// 在指定 IP 地址机械臂上，求解工具中心点坐标系相对于基坐标系的雅各比矩阵，现在支持在不同的工具坐标系下求解，由传入的 active_tcp 决定。.
        /// </summary>
        /// <param name="jacobiMatrix">输出参数，雅各比矩阵，大小为 6*7 的矩阵。.</param>
        /// <param name="jointPosition">输入参数，用于计算雅各比矩阵的关节角，大小为 7 的数组。.</param>
        /// <param name="jointCount">输入参数，关节数量。.</param>
        /// <param name="activeTcp">当前工具坐标系对应的位姿向量，大小为 6 的数组，为空时，将使用默认的 工具坐标系 default_tcp。.</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int calculateJacobiTF(/*OUT[6*JOINT_COUNT]*/out double[] jacobiMatrix,     /*IN[7]*/ double[] jointPosition,  /*IN*/ int jointCount, /*IN[6]*/ double[] activeTcp, string ipAddress = "");

        /// <summary>
        /// 根据路点名称走位.
        /// </summary>
        /// <param name="jointsName">路点名.</param>
        /// <param name="v">速度.</param>
        /// <param name="a">加速度.</param>
        /// <param name="ipAddress">.</param>
        public static void moveLToTarget(string jointsName, double v, double a, string ipAddress = "")
        {
            double[] dblTcppos = new double[6];
            double[] dblJoints = new double[7];
            int sef = DianaApi.getWayPoint(jointsName, dblTcppos, dblJoints, ipAddress,"","");
            sef = DianaApi.moveLToTarget(dblJoints, v, a);
        }

        /// <summary>
        /// 根据路点名补偿走位.
        /// </summary>
        /// <param name="jointsName">路点名.</param>
        /// <param name="v">速度.</param>
        /// <param name="a">加速度.</param>
        /// <param name="x">x补偿mm.</param>
        /// <param name="y">y补偿mm.</param>
        /// <param name="z">z补偿mm.</param>
        /// <param name="r">角度补偿（度）.</param>
        /// <param name="ipAddress">.</param>
        public static void moveLoffset(string jointsName, double v, double a, double x, double y, double z, double r, string ipAddress = "")
        {
            double[] dblTcppos = new double[6];
            double[] dblJoints = new double[7];
            double[] activeTcp = new double[6];
            int sef = DianaApi.getWayPoint(jointsName, dblTcppos, dblJoints,"","", ipAddress);

            sef = forward(dblJoints, dblTcppos, activeTcp, ipAddress);

            dblTcppos[0] = dblTcppos[0] + x / 1000.0;
            dblTcppos[1] = dblTcppos[1] + y / 1000.0;
            dblTcppos[2] = dblTcppos[2] + z / 1000.0;
            dblTcppos[5] = dblTcppos[5] + r * Math.PI / 180.0;

            sef = inverse(dblTcppos, dblJoints, activeTcp, ipAddress);

            sef = DianaApi.moveLToTarget(dblJoints, v, a);
        }

        /// <summary>
        /// 等待机器人停止.
        /// </summary>
        /// <param name="ipAddress">.</param>
        public static void waitStop(string ipAddress = "")
        {
            Thread.Sleep(150);
            int sef = DianaApi.getRobotState(ipAddress);
            while (sef != 2)
            {
                Thread.Sleep(5);
                sef = DianaApi.getRobotState(ipAddress);
            }
        }

        /// <summary>
        /// 判断输入状态.
        /// </summary>
        /// <param name="groupName">数字输入名.</param>
        /// <param name="ipAddress">.</param>
        public static void waitDI(string groupName, string ipAddress = "")
        {
            int in_state = 0;
            int sef = DianaApi.readDI("board", groupName, ref in_state);
            while (in_state != 1)
            {
                Thread.Sleep(10);
                sef = DianaApi.readDI("board", groupName, ref in_state);
            }
        }

        /// <summary>
        /// 194.设置速度百分比
        /// </summary>
        /// <param name="value">整形，百分比的值</param>
        /// <param name="ipAddress">机械手IP.</param>
        /// <returns>返回值：0：成功。-1：失败.</returns>
        [DllImport("DianaApi.dll")]
        public static extern int setVelocityPercentValue(int value, string ipAddress = "");

    }
}
