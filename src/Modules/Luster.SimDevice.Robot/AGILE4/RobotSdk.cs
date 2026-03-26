using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace Luster.SimDevice.Robot.AGILE4
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RobotPos
    {
        public double x;        //机器人TCP点相对当前用户坐标系坐标的x分量，单位毫米
        public double y;        //机器人TCP点相对当前用户坐标系坐标的y分量，单位毫米
        public double z;        //机器人TCP点相对当前用户坐标系坐标的z分量，单位毫米
        public double a;        //机器人工具姿态在当前用户坐标系中欧拉角表达的a分量，单位度
        public double b;        //机器人工具姿态在当前用户坐标系中欧拉角表达的b分量，单位度
        public double c;        //机器人工具姿态在当前用户坐标系中欧拉角表达的c分量，单位度
        public int cfg;         /*机器人轴配置。由于机器人可能存在几种不同的方式使TCP到达同一个位姿，
                                 所以这里需要通过cfg参数来指定其中一种方式，从而唯一确定一组机器人轴位置*/
        public int turn;        //配合 cfg 用于确定反解的轴位置
        public double ej1;      //附加轴1的位置，单位度
        public double ej2;      //附加轴2的位置，单位度
        public double ej3;      //附加轴3的位置，单位度
        public double ej4;      //附加轴4的位置，单位度
        public double ej5;      //附加轴5的位置，单位度
        public double ej6;      //附加轴6的位置，单位度
        public RobotPos(double _x = 1e10, double _y = 1e10, double _z = 1e10,
            double _a = 1e10, double _b = 1e10, double _c = 1e10,
            int _cfg = 0, int _turn = 0,
            double _ej1 = 1e10, double _ej2 = 1e10, double _ej3 = 1e10,
            double _ej4 = 1e10, double _ej5 = 1e10, double _ej6 = 1e10)
        {
            x = _x;
            y = _y;
            z = _z;
            a = _a;
            b = _b;
            c = _c;
            cfg = _cfg;
            turn = _turn;
            ej1 = _ej1;
            ej2 = _ej2;
            ej3 = _ej3;
            ej4 = _ej4;
            ej5 = _ej5;
            ej6 = _ej6;
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RobotPos2
    {
        public double x;     //机器人TCP点相对当前用户坐标系坐标的x分量， 单位毫米
        public double y;     //机器人TCP点相对当前用户坐标系坐标的y分量， 单位毫米
        public double z;     //机器人TCP点相对当前用户坐标系坐标的z分量， 单位毫米
        public double a;     //机器人工具姿态在当前用户坐标系中欧拉角表达的a分量，单位度
        public double b;     //机器人工具姿态在当前用户坐标系中欧拉角表达的b分量，单位度
        public double c;     //机器人工具姿态在当前用户坐标系中欧拉角表达的c分量，单位度
        public int cfgx;     /* 机器人轴配置。 可用于区分记录直角坐标时机器人相对于3种奇异点的状态，
                                并作为机器人在运动学逆解时的取值依据。 cfgx参数,包含CFG0-CFG8,共 9
                                种配置。相信见下文cfgx参数说明*/
        public int cfg1;     //机器人笛卡尔坐标系位置1轴关节多圈值
        public int cfg4;     //机器人笛卡尔坐标系位置4轴关节多圈值
        public int cfg6;     //机器人笛卡尔坐标系位置6轴关节多圈值
        public double ej1;   //附加轴1的位置，单位度
        public double ej2;   //附加轴2的位置，单位度
        public double ej3;   //附加轴3的位置，单位度
        public double ej4;   //附加轴4的位置，单位度
        public double ej5;   //附加轴5的位置，单位度
        public double ej6;   //附加轴6的位置，单位度
        public RobotPos2(double _x = 1e10, double _y = 1e10, double _z = 1e10,
            double _a = 1e10, double _b = 1e10, double _c = 1e10,
            int _cfgx = 0, int _cfg1 = 0, int _cfg4 = 0, int _cfg6 = 0,
            double _ej1 = 1e10, double _ej2 = 1e10, double _ej3 = 1e10,
            double _ej4 = 1e10, double _ej5 = 1e10, double _ej6 = 1e10)
        {
            x = _x;
            y = _y;
            z = _z;
            a = _a;
            b = _b;
            c = _c;
            cfgx = _cfgx;
            cfg1 = _cfg1;
            cfg4 = _cfg4;
            cfg6 = _cfg6;
            ej1 = _ej1;
            ej2 = _ej2;
            ej3 = _ej3;
            ej4 = _ej4;
            ej5 = _ej5;
            ej6 = _ej6;
        }

    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RobotJoint
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public double[] j;  //6个关节轴和6个附加轴的角度值
        public RobotJoint(double j1 = 1e10, double j2 = 1e10, double j3 = 1e10, double j4 = 1e10,
            double j5 = 1e10, double j6 = 1e10, double ej1 = 1e10, double ej2 = 1e10,
            double ej3 = 1e10, double ej4 = 1e10, double ej5 = 1e10, double ej6 = 1e10)
        {
            j = new double[12];
            j[0] = j1;
            j[1] = j2;
            j[2] = j3;
            j[3] = j4;
            j[4] = j5;
            j[5] = j6;
            j[6] = ej1;
            j[7] = ej2;
            j[8] = ej3;
            j[9] = ej4;
            j[10] = ej5;
            j[11] = ej6;
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RobotTool
    {
        public double x;  //工具坐标系 x,y,z 分量
        public double y;
        public double z;
        public double a; //工具坐标系 a,b,c 分量
        public double b;
        public double c;
        public bool stationary; //是否为固定工具，非固定工具安装于机器人法兰，固定工具相对世界坐标系不动，默认为非固定工具
        public string mu_name;  //参考机械单元名称，默认为参考世界坐标系
        public RobotTool(double _x = 0, double _y = 0, double _z = 0,
            double _a = 0, double _b = 0, double _c = 0,
            bool _stationary = false, string _mu_name = "WORLD")
        {
            x = _x;
            y = _y;
            z = _z;
            a = _a;
            b = _b;
            c = _c;
            stationary = _stationary;
            mu_name = _mu_name;
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RobotWorkpiece
    {
        public double x; //工件坐标系 x,y,z 分量
        public double y;
        public double z;
        public double a; //工件坐标系 a,b,c 分量
        public double b;
        public double c;
        public bool robhold; //是否为机器人抓取工件，机器人抓取工件安装于机器人法兰，非机器人抓取工件相对世界坐标系不动， 默认为非机器人抓取工件
        public string mu_name; //参考机械单元名称， 默认为参考世界坐标系
        public RobotWorkpiece(double _x = 0, double _y = 0, double _z = 0,
            double _a = 0, double _b = 0, double _c = 0,
            bool _robhold = false, string _mu_name = "WORLD")
        {
            x = _x;
            y = _y;
            z = _z;
            a = _a;
            b = _b;
            c = _c;
            robhold = _robhold;
            mu_name = _mu_name;
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct strToolVar
    {
        public string name;
        public int id;
        public int classID;
        public string initValue;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct strRefVar
    {
        public string name;
        public int id;
        public int classID;
        public string initValue;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct xplRuntimeMemoMoveData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public double[] qj;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public double[] qt;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public double[] qp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public double[] eauxQj;
        public int cfgx;
        public int toolVarID;
        public int refsysVarID;
        public int speedVarID;
        public int zoneVarID;
        public int cfg1;
        public int cfg4;
        public int cfg6;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct xplRuntimeStatusInfo
    {
        // Environment data
        public int sysLogCounter;	// XPL environment log record counter from its creation
        public int envMagic;		// environment magic number (environment variables definitions changed) @@@ if changed reload definitions
        public int execPouId;		// execution pou id
        public int execStepId;		// execution step id !!! we use pouStepId
        public int enaCmd;			// enabled command mask
        public int envSts;			// environment status (RUN, PAUSE, LOADED)
        public int movePouId;		// movement pou id
        public int moveStepId;		// movement step id
        public int usrLogCounter;	// User log record counter (messages)
        public int isModified;		// actual program and/or a module is modified and need to be saved (1=Program modified)
        public int toolVarID;
        public int refsysVarID;

        // POU data (ID pou <> 0) !!! Not used data (obsolete) !!!
        public int pouSts;		    // POU status (RUN, PAUSE, LOADED)
        public int pouStepId;		// POU execution step ID (it can be a CALL, so the true execution step is the step of the subroutine)
    };

    public enum ErrorCode : int
    {
        ERROR_OK = 0,                                   // 操作成功
        ERROR_CONNECT_FAILED = 1,                       // 与机器人连接失败
        ERROR_NO_CONNECTION = 2,                        // 尚未与机器人连接
        ERROR_CONNECTION_BROKEN = 3,                    // 与机器人连接中断
        ERROR_ACCESS_REJECTED = 4,                      // 访问拒绝，机器人设置为不允许 API 访问
        ERROR_CUR_MODE_NOT_SUPPORT = 5,                 // 当前模式不支持该操作
        ERROR_SERVO_ON_FAILED = 6,                      // 上电失败
        ERROR_POWER_OFF_FAILED = 7,                     // 下电失败
        ERROR_SET_MODE_FAILED = 8,                      // 设置控制模式失败
        ERROR_SET_SPEED_RATIO_FAILED = 9,               // 设置速度倍率失败，请检查设置的倍率值是否有效
        ERROR_SWITCH_CHANNEL_FAILED = 10,               // 切换通道失败，请检查设置的通道号是否有效
        ERROR_START_PROGRAM_FAILED = 11,                // 启动程序失败，只有在没有告警、已成功加载程序且系统状态处于暂停或停止的情况下才可以启动程序
        ERROR_RESET_PROGRAM_FAILED = 12,                // 复位程序失败，只有系统状态处于暂停或停止时才可以复位程序
        ERROR_LOAD_PROGRAM_FAILED = 13,                 // 加载程序失败，请获取告警列表查看具体失败信息
        ERROR_PARA_INVALID = 14,                        // 输入参数范围有误
        ERROR_STILL_RUNNING = 15,                       // 系统仍处于运行中，无法执行运动指令
        ERROR_SET_PARA_FAILED = 16,                     // 设置变量失败
        ERROR_GET_PARA_FAILED = 17,                     // 查询变量失败
        ERROR_MOVE_FAILED = 18,                         // 控制机器人移动失败
        ERROR_ENABLE_API_CONTROL_FAILED = 19,           // 设置外部 API 控制失败
        ERROR_THIRD_PARTY = 10000,                      // 第三方库抛出的异常码起始值
        ERROR_THIRD_OPERATION_FAILED = 10001,           // 操作失败
        ERROR_THIRD_GET_CLIENT_FAILED = 10002,          // 获取客户端失败
        ERROR_THIRD_GET_MON_FAILED = 10003,             // 创建monitor失败
        ERROR_THIRD_START_MON_FAILED = 10004,           // 启动monitor失败
        ERROR_THIRD_FLASH_FAILED = 10005,               // 获取文件操作接口失败
        ERROR_THIRD_SAVE_FILE_FAILED = 10006,           // 从控制器下载文件失败
        ERROR_THIRD_LOAD_FILE_FAILED = 10007,           // 往控制器上传XPL文件失败
        ERROR_THIRD_FILE_EXIST = 10008,                 // XPL文件存在
        ERROR_THIRD_FILE_NOTEXIST = 10009,              // XPL文件不存在
        ERROR_THIRD_FILE_ISEXIST_FAILED = 10010,        // XPL文件查询失败
        ERROR_THIRD_GET_ROBOTTYPE_FAILED = 10011,       // 获取机型名失败
        ERROR_THIRD_GET_MOVEDATA_FAILED = 10012,        // 获取运动数据失败
        ERROR_THIRD_NOT_ATTACHED = 10013,               // 环境未获取
        ERROR_THIRD_SET_WRITEMODE_FAILED = 10014,       // 写权限设置失败
        ERROR_THIRD_GET_MONITORDATA_FAILED = 10015,     // 获取monitor中数据失败
        ERROR_THIRD_GET_STSINFO_FAILED = 10016,         // 获取状态信息失败
        ERROR_THIRD_CREATE_FOLDER_FAILED = 10017,       // 创建文件夹失败
        ERROR_THIRD_DELETE_FOLDER_FAILED = 10018,       // 删除文件夹失败
        ERROR_THIRD_PATH_INVALID = 10019,               // 文件夹路径无效
        ERROR_THIRD_DELETE_SYSTEM_FOLDER = 10020,       // 禁止删除系统文件夹
        ERROR_THIRD_SAVE_GVAR_FILE_FAILED = 10021       // 保存文件失败
    };

    // 机器人运行模式
    public enum RobotMode
    {
        ROBOT_MODE_MANUAL = 1,  // 手动低速模式
        ROBOT_MODE_AUTO,        // 自动模式
        ROBOT_MODE_MANUFAST     // 手动高速模式
    };

    public enum VarNameType
    {
        VAR_BOOL = 0,
        VAR_INT = 1,
        VAR_DOUBLE = 2,
        VAR_STRING = 3,
        VAR_TOOL = 4,
        VAR_WOBJ = 5,
        VAR_POINTJ = 6,
        VAR_POINTC = 7
    };

    // 程序状态
    public enum ProgramState
    {
        PROG_STATE_NOT_LOAD = 0,  // 程序未加载
        PROG_STATE_RUNNING,		  // 程序运行中
        PROG_STATE_PAUSE,		  // 程序暂停
        PROG_STATE_STOP			  // 程序停止
    };

    // ROBOX 运行模式
    public enum RoboxKeyMode
    {
        ROBOX_MODE_MANUAL = 0,  // 手动低速模式
        ROBOX_MODE_MANUFAST,    // 手动高速模式
        ROBOX_MODE_AUTO         // 自动模式
    };

    public enum RoboxProgramState
    {
        RPL_INIT = 1,   // 程序未加载
        RPL_RUN,	    // 程序运行中
        RPL_PAUSE,	    // 程序暂停
        RPL_UNUSED4,    // 未使用
        RPL_STOPPED,    // 程序停止
        RPL_ENDED,      // 程序终止成功
        RPL_ERROR       // 程序终止错误
    };

    public enum ROBOXPCMoveType
    {
        PC_MOVEJ = 1,   // 关节运动
        PC_MOVEL,       // 直线运动
        PC_MOVEC,       // 圆弧运动
        PC_MOVECA       // 圆弧运动（指定角度）
    };

    public enum MoveExecStatus
    {
        MV_NOT_SET = 0,   //未设置
        MV_NOT_PROCESSED, //未处理
        MV_WAITING_START, //等待启动
        MV_ACCELERATION,  //加速中
        MV_AT_SPEED,      //匀速运动
        MV_DECELERATION,  //减速中
        MV_AT_END         //运动结束
    };

    public enum GlobalVarType
    {
        GVAR_TYPE = 1,
        GPOINTJ_TYPE,
        GPOINTC_TYPE,
        GTOOL_TYPE,
        GWOBJ_TYPE,
        ALL
    };

    public enum GvarType2
    {
        GBOOL = 0x00000001,
        GINT = 0x00000002,
        GDOUBLE = 0x00000004,
        GSTRING = 0x00000008,
        GTF = 0x00000010,
        GUF = 0x00000020,
        GPJ = 0x00000040,
        GPC = 0x00000080
    };

    public enum CfgxType
    {
        CFG0 = 0,
        CFG1 = 1,
        CFG2 = 2,
        CFG3 = 3,
        CFG4 = 4,
        CFG5 = 5,
        CFG6 = 6,
        CFG7 = 7,
        CFG8 = 8
    };

    public enum Count : uint
    {
        DOUT_COUNT = 176,
        DIN_COUNT = 176,
        VAR_BOOL_COUNT = 100,
        VAR_INT_COUNT = 100,
        VAR_DOUBLE_COUNT = 100,
        VAR_STRING_COUNT = 20,
        VAR_ROBOT_JOINT_COUNT = 10000,
        VAR_ROBOT_POS_COUNT = 10000,
        VAR_ROBOT_TOOL_COUNT = 200,
        VAR_ROBOT_WOBJ_COUNT = 200,

        GET_VAR_INTERVAL = 10,   // 毫秒
        GET_VAR_COUNT = 500  // 次数
    }

    public class RobotSdkNet
    {
        /// <summary>
        /// 初始化并连接机器人
        /// </summary>
        /// <param name="robot_addr"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int ConnectRobot(string robot_addr);

        /// <summary>
        /// 断开机器人连接
        /// </summary>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void DisconnectRobot();

        /// <summary>
        /// 使能或禁止外部 API 控制
        /// </summary>
        /// <param name="enable">True:使能,启动API;False:取消使能</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int EnableApiControl(bool enable);

        /// <summary>
        /// 设置当前机器人控制模式
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetControlMode(RobotMode mode);

        /// <summary>
        /// 机器人上电
        /// </summary>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int PowerOn();

        /// <summary>
        /// 机器人下电
        /// </summary>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int PowerOff();

        /// <summary>
        /// 清除机器人警告
        /// </summary>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int ClearAlarm();

        /// <summary>
        /// 控制各轴运动到某角度
        /// </summary>
        /// <param name="robot_joint">各轴目标角度</param>
        /// <param name="speed_percent">运动速度百分比，相对于各轴最大速度</param>
        /// <param name="speed_exj">附加轴移动速度，如果没有附加轴，则此参数可以缺省</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int Move2Joint(RobotJoint robot_joint, double speed_percent, double speed_exj = 0);

        /// <summary>
        /// 检查是否可以通过 PTP移动到某些位姿
        /// </summary>
        /// <param name="target_pos">目标位姿</param>
        /// <param name="len"></param>
        /// <param name="tool_index">工具坐标系序号，取值范围 0-199</param>
        /// <param name="wobj_index">工件坐标系序号，取值范围 0-199</param>
        /// <param name="valid">是否可达</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int CheckPtpPosValid(RobotPos[] target_pos, uint len, uint tool_index, uint wobj_index, ref bool[] valid);

        /// <summary>
        /// 检查是否可以通过PTP移动到某些位姿
        /// </summary>
        /// <param name="target_pos">目标位姿</param>
        /// <param name="len"></param>
        /// <param name="tool_index">工具坐标系序号，取值范围 0-199</param>
        /// <param name="wobj_index">工件坐标系序号，取值范围 0-199</param>
        /// <param name="valid">是否可达</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int CheckPtpPosValid2(RobotPos2[] target_pos, uint len, uint tool_index, uint wobj_index, ref bool[] valid);

        /// <summary>
        /// 控制机器人移动到某个位姿
        /// </summary>
        /// <param name="robot_pos">目标位姿</param>
        /// <param name="tool_index">工具坐标系序号，取值范围 0-199</param>
        /// <param name="wobj_index">工件坐标系序号，取值范围 0-199</param>
        /// <param name="speed_percent">运动速度百分比，相当于各轴最大速度</param>
        /// <param name="speed_exj">附加轴移动速度，如果没有附加轴，则此参数可以缺省</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int Move2Pos(RobotPos robot_pos, uint tool_index, uint wobj_index, double speed_percent, double speed_exj = 0);

        /// <summary>
        /// 控制机器人移动到某个位姿
        /// </summary>
        /// <param name="robot_pos">目标位姿</param>
        /// <param name="tool_index">工具坐标系序号，取值范围 0-199</param>
        /// <param name="wobj_index">工件坐标系序号，取值范围 0-199</param>
        /// <param name="speed_percent">运动速度百分比，相当于各轴最大速度</param>
        /// <param name="speed_exj">附加轴移动速度，如果没有附加轴，则此参数可以缺省</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int Move2Pos2(RobotPos2 robot_pos, uint tool_index, uint wobj_index, double speed_percent, double speed_exj = 0);

        /// <summary>
        /// 控制机器人以直线移动到某个位姿
        /// </summary>
        /// <param name="robot_pos"></param>
        /// <param name="tool_index">工具坐标系序号，取值范围 0-199</param>
        /// <param name="wobj_index">工件坐标系序号，取值范围 0-199</param>
        /// <param name="speed_tcp">TCP 点移动速度</param>
        /// <param name="speed_ori">工具坐标系姿态转动速度</param>
        /// <param name="speed_exj">附加轴移动速度，如果没有附加轴，则此参数可以缺省</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int Line2Pos(RobotPos robot_pos, uint tool_index, uint wobj_index, double speed_tcp, double speed_ori, double speed_exj = 0);

        /// <summary>
        /// 控制机器人以直线移动到某个位姿
        /// </summary>
        /// <param name="robot_pos"></param>
        /// <param name="tool_index"></param>
        /// <param name="wobj_index"></param>
        /// <param name="speed_tcp"></param>
        /// <param name="speed_ori"></param>
        /// <param name="speed_exj"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int Line2Pos2(RobotPos2 robot_pos, uint tool_index, uint wobj_index, double speed_tcp, double speed_ori, double speed_exj = 0);

        /// <summary>
        /// 控制机器人停止运动
        /// </summary>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int StopMove();

        /// <summary>
        /// 获取某一路的数字量输入值
        /// </summary>
        /// <param name="index">DI序号，取值范围 0-175</param>
        /// <param name="value">DI值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetDigitalIn(uint index, ref uint value);

        /// <summary>
        /// 获取某一路的数字量输出值
        /// </summary>
        /// <param name="index">DOut序号，取值范围 0-175</param>
        /// <param name="value">DOut值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetDigitalOut(uint index, ref uint value);

        /// <summary>
        /// 设置某一路的数字量输出值
        /// </summary>
        /// <param name="index">DOut序号，取值范围 0-175</param>
        /// <param name="value">DOut值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetDigitalOut(uint index, uint value);

        /// <summary>
        /// 设置速度倍率
        /// </summary>
        /// <param name="ratio">倍率值，取值范围 1-100</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetSpeedRatio(uint ratio);

        /// <summary>
        /// 设置布尔型变量值
        /// </summary>
        /// <param name="index">变量序号，取值范围 0-99</param>
        /// <param name="value">变量值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetBoolVariable(uint index, bool value);

        /// <summary>
        /// 设置布尔型变量值
        /// </summary>
        /// <param name="name">变量别名</param>
        /// <param name="value">变量值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetBoolVariable2(string name, bool value);

        /// <summary>
        /// 设置整型变量值
        /// </summary>
        /// <param name="index">变量序号，取值范围 0-99</param>
        /// <param name="value">变量值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetIntVariable(uint index, int value);

        /// <summary>
        /// 设置整型变量值
        /// </summary>
        /// <param name="name">变量别名</param>
        /// <param name="value">变量值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetIntVariable2(string name, int value);

        /// <summary>
        /// 设置浮点型变量值
        /// </summary>
        /// <param name="index">变量序号，取值范围 0-99</param>
        /// <param name="value">变量值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetDoubleVariable(uint index, double value);

        /// <summary>
        /// 设置浮点型变量值
        /// </summary>
        /// <param name="name">变量别名</param>
        /// <param name="value">变量值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetDoubleVariable2(string name, double value);

        /// <summary>
        /// 设置字符串型变量值
        /// </summary>
        /// <param name="index">变量序号，取值范围 0-19</param>
        /// <param name="value">变量值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetStringVariable(uint index, string value);

        /// <summary>
        /// 设置字符串型变量值
        /// </summary>
        /// <param name="name">变量别名</param>
        /// <param name="value">变量值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetStringVariable2(string name, string value);

        /// <summary>
        /// 设置工具坐标系值
        /// </summary>
        /// <param name="tool_name">工具坐标系别名</param>
        /// <param name="Coordinate">工具坐标系坐标值（double 类型数组）</param>
        /// <param name="stationary">是否为固定工具，详见 3.3.4</param>
        /// <param name="mu_name">参考机械单元名称，详见 3.3.4</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetToolCoordinate(string tool_name, double[] Coordinate, bool stationary, string mu_name);

        /// <summary>
        /// 设置工具坐标系值
        /// </summary>
        /// <param name="tool_index">工具坐标系序号，取值范围 0-199</param>
        /// <param name="Coordinate"></param>
        /// <param name="stationary"></param>
        /// <param name="mu_name"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetToolCoordinate2(uint tool_index, double[] Coordinate, bool stationary, string mu_name);

        /// <summary>
        /// 设置工件坐标系值
        /// </summary>
        /// <param name="workpiece_name">工件坐标系别名</param>
        /// <param name="Coordinate">工件坐标系坐标值（double 类型数组）</param>
        /// <param name="robhold">是否为机器人抓取工件，详见 3.3.5</param>
        /// <param name="mu_name">参考机械单元名称，详见 3.3.5</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetWorkpieceCoordinate(string workpiece_name, double[] Coordinate, bool robhold, string mu_name);

        /// <summary>
        /// 设置工件坐标系值
        /// </summary>
        /// <param name="workpiece_index">工件坐标系序号，取值范围 0-199</param>
        /// <param name="Coordinate"></param>
        /// <param name="robhold"></param>
        /// <param name="mu_name"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetWorkpieceCoordinate2(uint workpiece_index, double[] Coordinate, bool robhold, string mu_name);

        /// <summary>
        /// 设置关节型变量值
        /// </summary>
        /// <param name="name">变量序号， 取值范围 0-9999</param>
        /// <param name="value"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetJointVariable(string name, RobotJoint value);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetJointVariable2(uint index, RobotJoint value);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetPoseVariable(string name, RobotPos value, uint tool_index = 0, uint wobj_index = 0);

        /// <summary>
        /// 设置位姿型变量值
        /// </summary>
        /// <param name="index">变量序号，取值范围 0-9999</param>
        /// <param name="value">变量值</param>
        /// <param name="tool_index">此位姿示教时的工具坐标系序号，默认 0</param>
        /// <param name="wobj_index">此位姿示教时的工件坐标系序号，默认 0</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetPoseVariable2(uint index, RobotPos value, uint tool_index = 0, uint wobj_index = 0);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetPoseVariable3(string name, RobotPos2 value, uint tool_index = 0, uint wobj_index = 0);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetPoseVariable4(uint index, RobotPos2 value, uint tool_index = 0, uint wobj_index = 0);

        /// <summary>
        /// 设置变量别名
        /// </summary>
        /// <param name="type">变量序号，取值范围 0-9999</param>
        /// <param name="index">变量序号</param>
        /// <param name="name">变量别名</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SetVariableName(VarNameType type, uint index, string name);

        /// <summary>
        /// 保存设置的变量别名到文件
        /// </summary>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SaveVariableName();

        /// <summary>
        /// 查询当前是否已经和机器人连接
        /// </summary>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static bool IsConnected();

        /// <summary>
        /// 查询使能或禁止外部API控制
        /// </summary>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static bool IsApiControl();

        /// <summary>
        /// 查询当前是否已经上电
        /// </summary>
        /// <param name="is_poweron"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int IsPowerOn(ref bool is_poweron);

        /// <summary>
        /// 查询当前机器人控制模式
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetControlMode(ref RobotMode mode);

        /// <summary>
        /// 获取当前机器人运行状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetProgramState(ref ProgramState state);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetSpeedRatio(ref uint ratio);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetLoadedProg(ref string file_path);

        /// <summary>
        /// 查询当前告警状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetAlarmState(ref bool state);

        /// <summary>
        /// 查询当前告警列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetAlarmList(ref uint[] list);

        /// <summary>
        /// 查询当前机器人位姿
        /// </summary>
        /// <param name="robot_pos"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetPos(ref RobotPos robot_pos);

        /// <summary>
        /// 查询当前机器人位姿
        /// </summary>
        /// <param name="robot_pos"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetPos2(ref RobotPos2 robot_pos);

        /// <summary>
        /// 查询当前机器人各轴角度
        /// </summary>
        /// <param name="robot_joint"></param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetJoint(ref RobotJoint robot_joint);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetBoolVariable(uint index, ref bool value);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetBoolVariable2(string name, ref bool value);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetIntVariable(uint index, ref int value);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetIntVariable2(string name, ref int value);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetDoubleVariable(uint index, ref double value);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetDoubleVariable2(string name, ref double value);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetStringVariable(uint index, ref string value);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetStringVariable2(string name, ref string value);

        /// <summary>
        /// 查询工具坐标系值
        /// </summary>
        /// <param name="tool_name">工具坐标系别名</param>
        /// <param name="tool">工具坐标系值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetToolCoordinate(string tool_name, ref RobotTool tool);

        /// <summary>
        /// 查询工具坐标系值
        /// </summary>
        /// <param name="tool_index">工具坐标系序号，取值范围 0-199</param>
        /// <param name="tool">工具坐标系值</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetToolCoordinate2(uint tool_index, ref RobotTool tool);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetWorkpieceCoordinate(string workpiece_name, ref RobotWorkpiece workpiece);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetWorkpieceCoordinate2(uint workpiece_index, ref RobotWorkpiece workpiece);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetJointVariable(string name, ref RobotJoint value);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetJointVariable2(uint index, ref RobotJoint value);

        /// <summary>
        /// 查询位姿型变量值
        /// </summary>
        /// <param name="name">变量别名</param>
        /// <param name="value">变量值</param>
        /// <param name="tool_index">此位姿示教时的工具坐标系序号</param>
        /// <param name="wobj_index">此位姿示教时的工件坐标系序号</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetPoseVariable(string name, ref RobotPos value, ref uint tool_index, ref uint wobj_index);

        /// <summary>
        /// 查询位姿型变量值
        /// </summary>
        /// <param name="index">变量序号，取值范围 0-9999</param>
        /// <param name="value">变量值</param>
        /// <param name="tool_index">此位姿示教时的工具坐标系序号</param>
        /// <param name="wobj_index">此位姿示教时的工件坐标系序号</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetPoseVariable2(uint index, ref RobotPos value, ref uint tool_index, ref uint wobj_index);
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetPoseVariable3(string name, ref RobotPos2 value, ref uint tool_index, ref uint wobj_index);

        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetPoseVariable4(uint index, ref RobotPos2 value, ref uint tool_index, ref uint wobj_index);

        /// <summary>
        /// 查询运动状态
        /// </summary>
        /// <param name="status">运动状态，请参考 MoveExecStatus</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetMoveStatus(ref int status);

        /// <summary>
        /// 机器人位姿、关节角度逆解接口
        /// </summary>
        /// <param name="pos">机器人位姿</param>
        /// <param name="tool_index">工具坐标系序号，取值范围 0-199</param>
        /// <param name="wobj_index">工件坐标系序号，取值范围 0-199</param>
        /// <param name="joint">通过逆解转换得到的各轴角度</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int IkSolver(RobotPos pos, uint tool_index, uint wobj_index, ref RobotJoint joint);

        /// <summary>
        /// 机器人位姿、关节角度逆解接口
        /// </summary>
        /// <param name="pos">机器人位姿</param>
        /// <param name="tool_index">工具坐标系序号，取值范围 0-199</param>
        /// <param name="wobj_index">工件坐标系序号，取值范围 0-199</param>
        /// <param name="joint">通过逆解转换得到的各轴角度</param>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int IkSolver2(RobotPos2 pos, uint tool_index, uint wobj_index, ref RobotJoint joint);

        /// <summary>
        /// 启动程序
        /// </summary>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int StartProgram();

        /// <summary>
        /// 暂停程序
        /// </summary>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int PauseProgram();

        /// <summary>
        /// 复位程序
        /// </summary>
        /// <returns></returns>
        [DllImport("RobotSdk.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int ResetProgram();
    }
}
