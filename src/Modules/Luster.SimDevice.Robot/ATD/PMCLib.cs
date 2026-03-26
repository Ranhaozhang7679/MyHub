using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Luster.SimDevice.Robot.ATD.PMCLib;

namespace Luster.SimDevice.Robot.ATD
{
    public class PMCLib
    {
        private const string DllPath = @"PMCLib.dll";
        public PMCLib()
        {

        }

        /// <summary>
        /// 错误类型定义
        /// </summary>
        enum ErrCode
        {
            WORK_NO_ERR = 0,            //0x00操作无错误	operation successfully
            WORK_COM_SEBD_FAIL = 1,	    //0x01发送数据失败
            WORK_COM_RECV_FAIL = 2,     //0x02接收数据失败
            WORK_COM_UNCONNECT = 3,		//0x03网络未连接
            WORK_PARAM_OVER = 4,		//0x04参数超出范围
            WORK_READDATA_ERR = 5,		//0x05读数据错误	read data error
            WORK_WRITEDATA_ERR = 6,		//0x06写数据错误	write data error
            WORK_TIMEOUT_ERR = 7,		//0x07通讯超时错误	connection timeout
            WORK_BUSY_ERR = 8,			//0x08通讯繁忙错误	connection busy
            WORK_MOVE_STOP = 9,			//0x09运动停止
            WORK_PARAM_ERROR = 10,		//0x0a参数错误
            WORK_TCPNET_ERROR = 11,		//0x0b网络通讯错误
            WORK_AUTHORITY_ERR = 12,	//0x0c未获取运动权限
            WORK_AUTHORITYING_ERR = 13,	//0x0d运动权限下不能操作
            WORK_RUNNING_ERR = 14,		//0x0e运动状态下不能操作
            WORK_ANALYSIS_ERR = 15,     //0x0f解析数据错误
            WORK_ORDERMATCH_ERR = 16	//0x10接收数据与指令不匹配
        }


        public struct ADT_point
        {
            public float x;
            public float y;
            public float z;
            public float c;
            public float handSide;
        }

        public struct ADT_pointAngle
        {
            public float J1;
            public float J2;
            public float J3;
            public float J4;
            public float handSide;
        }

        public struct ADT_alertStatus
        {
            public short Joint1; //J1 轴的伺服报警号
            public short Joint2; //J2 轴的伺服报警号
            public short Joint3; //J3 轴的伺服报警号
            public short Joint4; //J4 轴的伺服报警号
            public short Motion; //DSP 报警号
            public short Flags; //所有类型的报警
            public short Reserve1;
            public short Reserve2;
        }

        public struct ADT_MArchPara
        {
            public int A; //共享点位表点位序号（0-499）
            public float B;//LimitZ-related 上抬上限位高度（绝对位置）
            public float C;//StartZ-related 上升高度（相对起始点）
            public float D;//EndZ-related 下降高度（相对终点）
            public int CP;//5
            public int Acc;
            public int Dec;
            public int Spd;
            public int Jerk;
            public float ZsAcc;//10
            public float ZsDec;
            public float ZsSpd;
            public float ZeAcc;
            public float ZeDec;
            public float ZeSpd;//15
            public int ArchType; //运动类型（0：拱形；1：门型）
        }

        public struct ADT_MovPPara
        {
            public int A; //共享点位表点位序号（0-499；四轴联动）或 轴号（1-4；单轴运动）
            public float B;//单轴运动相对运动量
            public int CP;//平滑过渡百分比         
            public int Acc;//加速度百分比
            public int Dec;//减速度百分比
            public int Spd;//速度百分比
            public int Jerk; //加加速度百分比
            public int PType;//运动类型（0：四轴联动；1：单轴运动）
        }

        public struct ADT_MovLPara
        {
            public int A; //共享点位表点位序号（0-499；四轴联动）或 轴号（1-4；单轴运动）
            public float B;//单轴运动相对运动量
            public int CP;//5
            public int Acc;
            public int Dec;
            public int Spd;
            public int Jerk;
            public int AccC;
            public int DecC;
            public int SpdC;
            public int JerkC;
            public int PType;//运动类型（0：四轴联动；1：单轴运动）
        }

        public struct ADT_MovJPara
        {
            public float[] Joint;  //J1-J2-J3-J4轴关节坐标
            public int Axis; //轴号（1-4；单轴运动）
            public float Rel;//单轴运动相对运动量  
            public int CP;
            public int Acc;
            public int Dec;
            public int Spd; //10
            public int Jerk;
            public int PType;//运动类型（0：四轴联动；1：单轴运动）
        }

        public struct ADT_MArcPara
        {
            public int A; //目标点，共享点位表点位序号（0-499）
            public int B; //中间点，共享点位表点位序号（0-499）
            public float Angle;//圆弧角度度数
            public float ci;//圆心坐标x
            public float cj;//圆心坐标y
            public int dir;//圆弧方向（0：顺圆；1：逆圆）
            public int CP;
            public int Acc;
            public int Dec;
            public int Spd;
            public int Jerk;
            public int AccC;
            public int DecC;
            public int SpdC;
            public int JerkC;
            public int isCircle;//是否整圆（0：非整圆；1：整圆）
            public int PType; //运动方式（0：3点圆弧；1：终点+圆心+方向）
        }


        /// <summary>
        /// 连接控制器，默认地址（“192.168.0.123”）
        /// </summary>
        /// <returns>0:连接正常 1:建立连接失败</returns>
        [DllImport(DllPath)]
        public static extern short ADT_Connect2Controller();

        /// <summary>
        /// 按指定地址连接控制器
        /// </summary>
        /// <param name="address">IP地址</param>
        /// <returns>0:连接正常 1:建立连接失败</returns>
        [DllImport(DllPath)]
        public static extern short ADT_Connect2Controller_CustomAddress(string address);

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_Disconnect2Controller();


        /// <summary>
        /// 读取SRAM地址数据（整型）
        /// </summary>
        /// <param name="number">0-127</param>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_ReadUserRegisterStatic(short number, int datBuf);

        /// <summary>
        /// 读取SRAM地址数据（浮点）
        /// </summary>
        /// <param name="number">0-127</param>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_ReadUserRegisterStatic_F(short number, float datBuf);

        /// <summary>
        /// 写入SRAM地址数据（整型）
        /// </summary>
        /// <param name="number">0-127</param>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_WriteUserRegisterStatic(short number, int inputDat);

        /// <summary>
        /// 写入SRAM地址数据（浮点）
        /// </summary>
        /// <param name="number">0-127</param>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_WriteUserRegisterStatic_F(short number, int inputDat);

        /// <summary>
        /// 读取DRAMM地址数据（整型）
        /// </summary>
        /// <param name="number">0-127</param>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_ReadUserRegisterDynamic(short number, int datBuf);

        /// <summary>
        /// 读取DRAMM地址数据（浮点）
        /// </summary>
        /// <param name="number">0-127</param>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_ReadUserRegisterDynamic_F(short number, int datBuf);

        /// <summary>
        /// 写入DRAMM地址数据（整型）
        /// </summary>
        /// <param name="number">0-127</param>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_WriteUserRegisterDynamic(short number, int inputDat);

        /// <summary>
        /// 写入DRAMM地址数据（浮点）
        /// </summary>
        /// <param name="number">0-127</param>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_WriteUserRegisterDynamic_F(short number, int inputDat);

        /// <summary>
        /// 按16位读取输入信号的状态
        /// </summary>
        /// <param name="number">0-3</param>
        /// <param name="datBuf"></param>
        /// <param name="realTime">True读实时；False读缓存</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_ReadInputSignal(short number, out short datBuf, bool realTime = true);

        /// <summary>
        /// 按16位读取输出信号的状态
        /// </summary>
        /// <param name="number">0-3</param>
        /// <param name="datBuf"></param>
        /// <param name="realTime">True读实时；False读缓存</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_ReadOutputSignal(short number, out short datBuf, bool realTime = true);

        /// <summary>
        /// 按16位写入输出信号状态
        /// </summary>
        /// <param name="number"></param>
        /// <param name="inputDat"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_WriteOutputSignal(short number, short inputDat);


        /// <summary>
        /// 按位读取输入信号的状态
        /// </summary>
        /// <param name="number">0-35</param>
        /// <param name="datBuf"></param>
        /// <param name="realTime"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_ReadInputSignalBit(short number, out short datBuf, bool realTime = true);

        /// <summary>
        /// 按位读取输出信号的状态
        /// </summary>
        /// <param name="number">0-26</param>
        /// <param name="datBuf"></param>
        /// <param name="realTime">True读实时；False读缓存</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_ReadOutputSignalBit(short number, out short datBuf, bool realTime = true);

        /// <summary>
        /// 按位写入输出信号状态
        /// </summary>
        /// <param name="number">0-26</param>
        /// <param name="inputDat"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_WriteOutputSignalBit(short number, short inputDat);

        /// <summary>
        /// 设置机器人运行状态
        /// </summary>
        /// <param name="fiag">1:启动AR程序；2:暂停AR程序；3:停止AR程序；4:复位机器人（只能清除报警）</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_ProgramControl(short fiag);

        /// <summary>
        /// 获取机器人当前末端笛卡尔位置
        /// </summary>
        /// <param name="datBuf"></param>
        /// <param name="realTime">True:实时；False:缓存</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetCurrentPosition(ref ADT_point datBuf, bool realTime = true);

        /// <summary>
        /// 获取机器人当前关节位置
        /// </summary>
        /// <param name="datPointAngle"></param>
        /// <param name="realTime">True:实时；False:缓存</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetCurrentPositionAngle(ref ADT_pointAngle datPointAngle, bool realTime = true);

        /// <summary>
        /// 获取机器人当前速率大小
        /// </summary>
        /// <param name="datBuf">0-100</param>
        /// <param name="realTime"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetCurrentVelScale(ref short datBuf, bool realTime = true);

        /// <summary>
        /// 设置机器人当前速率大小
        /// </summary>
        /// <param name="intputDat">0-100整数</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_SetCurrentVelScale(short intputDat);

        /// <summary>
        /// 获取当前机器人的状态
        /// </summary>
        /// <param name="datBuf">0:停止(空闲) 1:暂停 2:运行 3:手动正在运行中</param>
        /// <param name="realTime"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetRobotRunStatus(ref short datBuf, bool realTime = true);

        /// <summary>
        /// 获取当前机器人的系统模式值
        /// </summary>
        /// <param name="datBuf">0:手动模式 1:自动模式</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetSystemMode(ref short datBuf);

        /// <summary>
        /// 将机器人当前笛卡尔坐标点记录到共享点位表中
        /// </summary>
        /// <param name="pointNumber"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_RecordPoint2Table(short pointNumber);

        /// <summary>
        /// 获取报警信息
        /// </summary>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetAlertInfo(ref ADT_alertStatus datBuf);

        /// <summary>
        /// 读取使能状态信息
        /// </summary>
        /// <param name="datBuf">0:断使能模式 1:上使能模式 2:拖拽模式 3:模式切换中</param>
        /// <param name="realTime"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetEnableStatus(ref short datBuf, bool realTime = true);

        /// <summary>
        /// 设置使能
        /// </summary>
        /// <param name="inputDat">0:断使能 1:上使能 2:拖拽模式</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_SetEnableStatus(short inputDat);

        /// <summary>
        /// 外部运动：点动或连续
        /// </summary>
        /// <param name="mode">0:单步运动，单步移动量为0.1；1:单步运动，单步移动量为1；2:单步运动，单步移动量为最大单步移动量；3：连续运动</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_ManualMode(short mode);

        /// <summary>
        /// 手动笛卡尔运动
        /// </summary>
        /// <param name="Axis">0:停止运动
        ///1:X轴正方向运动
        ///2:X轴负方向运动
        ///3:Y轴正方向运动
        ///4:Y轴负方向运动
        ///5:Z轴正方向运动
        ///6:Z轴负方向运动
        ///7:C轴正方向运动
        ///8:C轴负方向运动</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_EulerMoveManual(short Axis);

        /// <summary>
        /// 手动关节角运动
        /// </summary>
        /// <param name="Joint">0：停止运动
        ///1:J1轴正方向运动
        ///2:J1轴负方向运动
        ///3:J2轴正方向运动
        ///4:J2轴负方向运动
        ///5:J3轴正方向运动
        ///6:J3轴负方向运动
        ///7:J4轴正方向运动
        ///8:J4轴负方向运动</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_JointMoveManual(short Joint);

        /// <summary>
        /// 点位跟踪运动
        /// </summary>
        /// <param name="pointNumber">0-499</param>
        /// <param name="Z_status">true:上抬Z轴，false:不上抬</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_TrackMotion(int pointNumber, bool Z_status);

        /// <summary>
        /// 获取共享点位列表中单个点位的信息
        /// </summary>
        /// <param name="pointNumber"></param>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetPointFormTable(int pointNumber, ref ADT_point datBuf);

        /// <summary>
        /// 把点位写入到点位列表中
        /// </summary>
        /// <param name="pointNumber"></param>
        /// <param name="pointDat"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_SetPoint2Table(int pointNumber, ADT_point pointDat);

        /// <summary>
        /// 获取当前运动缓存
        /// </summary>
        /// <param name="datBuf">1：运动缓存有空闲，可以执行运动指令;0：运动缓存已满，不可以执行运动指令</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoCheckFifoBuffer(ref short datBuf);

        /// <summary>
        /// 停止当前指令运动
        /// </summary>
        /// <param name="inputDat">0</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMoveStop(short inputDat);

        /// <summary>
        /// 运动指令 MArchP
        /// </summary>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMoveArch(ADT_MArchPara datBuf);

        /// <summary>
        /// 获取错误号
        /// </summary>
        /// <param name="datBuf">
        /// 0：正常
        ///1：速度比列超出范围
        ///2：加速度比列超出范围
        ///3：减速度比列超出范围
        ///4：加加速度比列超出范围
        ///5：CP值超出范围
        ///6：目标点Z轴位置超过B值上限
        ///8：设置速度失败
        ///9：拱形运动失败
        ///10：发送运动指令失败
        ///11：拱形指令类型错误
        ///28：未获取运动权限</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMoveArchError(ref short datBuf);

        /// <summary>
        /// 运动指令 MovP
        /// </summary>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMoveP(ADT_MovPPara datBuf);

        /// <summary>
        /// 获取MovP通过电流触发停止的状态
        /// </summary>
        /// <param name="datBuf">0：正常结束；1：电流打到阈值触发运动结束</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetMovePStopState(ref short datBuf);

        /// <summary>
        /// 获取错误号
        /// </summary>
        /// <param name="datBuf">0：正常
        ///1：速度比列超出范围
        ///2：加速度比列超出范围
        ///3：减速度比列超出范围
        ///4：加加速度比列超出范围
        ///5：CP值超出范围
        ///7：运动缓存已满
        ///8：设置速度失败
        ///10：发送运动指令失败
        ///12：单轴运动轴号错误
        ///13：单轴运动失败
        ///14：四轴联动运动失败
        ///28：未获取运动权限</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMovePError(ref short datBuf);

        /// <summary>
        /// 运动指令 MovL
        /// </summary>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMoveL(ADT_MovLPara datBuf);

        /// <summary>
        /// 获取错误号
        /// </summary>
        /// <param name="datBuf">0：正常
        ///1：速度比列超出范围
        ///2：加速度比列超出范围
        ///3：减速度比列超出范围
        ///4：加加速度比列超出范围
        ///5：CP值超出范围
        ///7：运动缓存已满
        ///8：设置速度失败
        ///10：发送运动指令失败
        ///12：单轴运动轴号错误
        ///15：姿态速度比列超出范围
        ///16：姿态加速度比列超出范围
        ///17：姿态减速度比列超出范围
        ///18：姿态加加速度比列超出范围
        ///19：单轴运动失败
        ///20：四轴联动运动失败
        ///28：未获取运动权限</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMoveLError(ref short datBuf);

        /// <summary>
        /// 运动指令 MovJ
        /// </summary>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMoveJ(ADT_MovJPara datBuf);

        /// <summary>
        /// 获取MoveJ错误号
        /// </summary>
        /// <param name="datBuf">0：正常
        ///1：速度比列超出范围
        ///2：加速度比列超出范围
        ///3：减速度比列超出范围
        ///4：加加速度比列超出范围
        ///5：CP值超出范围
        ///7：运动缓存已满
        ///8：设置速度失败
        ///10：发送运动指令失败
        ///12：单轴运动轴号错误
        ///21：J1轴运动目标位置超过限位
        ///22：J2单轴运动目标位置超过限位
        ///23：J3单轴运动目标位置超过限位
        ///24：J4单轴运动目标位置超过限位
        ///25：单轴运动失败
        ///26：四轴联动运动失败
        ///27：单轴运动目标位置超过限位
        ///28：未获取运动权限
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMoveJError(ref short datBuf);

        /// <summary>
        /// 运动指令 Marc
        /// </summary>
        /// <param name="datBuf"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMoveMarc(ADT_MArcPara datBuf);

        /// <summary>
        /// 获取Marc错误号
        /// </summary>
        /// <param name="datBuf">0：正常
        ///1：速度比列超出范围
        ///2：加速度比列超出范围
        ///3：减速度比列超出范围
        ///4：加加速度比列超出范围
        ///5：CP值超出范围
        ///7：运动缓存已满
        ///8：设置速度失败
        ///10：发送运动指令失败
        ///15：姿态速度比列超出范围
        ///16：姿态加速度比列超出范围
        ///17：姿态减速度比列超出范围
        ///18：姿态加加速度比列超出范围
        ///28：未获取运动权限
        ///29：获取当前笛卡尔位置失败
        ///30：获取当前位置手系失败
        ///33：运动失败</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMarcError(ref short datBuf);

        /// <summary>
        /// 等待motion运动状态
        /// </summary>
        /// <param name="datBuf">0：运动状态结束；1：运动状态未结束或报错</param>
        /// <param name="oneFlag">false(直到状态为0才退出函数)；true(读一次即退出函数)</param>
        /// <param name="realTime"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoWaitPos(ref short datBuf, bool oneFlag = false, bool realTime = true);

        /// <summary>
        /// 等待伺服运动状态
        /// </summary>
        /// <param name="datBuf">0：运动状态结束；1：运动状态未结束或报错</param>
        /// <param name="oneFlag">false(直到状态为0才退出函数)；true(读一次即退出函数)</param>
        /// <param name="realTime"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoWaitRealPos(ref short datBuf, bool oneFlag = false, bool realTime = true);

        /// <summary>
        /// 获取运动权限
        /// </summary>
        /// <param name="datBuf">0：成功 ；1：失败</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMAccept(ref short datBuf);

        /// <summary>
        /// 释放运动权限
        /// </summary>
        /// <param name="datBuf">0：成功 ；1：失败</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_AutoMRelease(ref short datBuf);

        /// <summary>
        /// 获取modbus连接状态
        /// </summary>
        /// <returns>false：断开状态 true：连接状态</returns>
        [DllImport(DllPath)]
        public static extern bool ADT_GetConnectStatus();

        /// <summary>
        /// 获取当前机器人的用户坐标系数据
        /// </summary>
        /// <param name="pos">pos[0]:X （6位数组pos[6])
        ///pos[1]:Y
        ///pos[2]:Z
        ///pos[5]:C</param>
        /// <param name="realTime"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetCurrentUserCoord(float[] pos, bool realTime = true);

        /// <summary>
        /// 获取当前机器人的工具坐标系数据
        /// </summary>
        /// <param name="pos">pos[0]:X （6位数组pos[6])
        ///pos[1]:Y
        ///pos[2]:Z
        ///pos[5]:C</param>
        /// <param name="realTime"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetCurrentToolCoord(float[] pos, bool realTime = true);

        /// <summary>
        /// 获取指定用户坐标系数据：index(0-19)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetUserCoord(int index, float[] pos);

        /// <summary>
        /// 获取指定工具坐标系数据：index(0-19)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetToolCoord(int index, float[] pos);

        /// <summary>
        /// 设置指定用户坐标系数据：index(0-19)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_SetUserCoord(int index, float[] pos);

        /// <summary>
        /// 设置指定工具坐标系数据：index(0-19)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_SetToolCoord(int index, float[] pos);

        /// <summary>
        /// 获取当前机器人的坐标系序号
        /// </summary>
        /// <param name="user">用户坐标系</param>
        /// <param name="tool">工具坐标系</param>
        /// <param name="realTime"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetCurrentCoord(ref int user, ref int tool, bool realTime = true);

        /// <summary>
        /// 设置当前机器人的坐标系序号
        /// </summary>
        /// <param name="user">用户坐标系</param>
        /// <param name="tool">工具坐标系</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_SetCurrentCoord(int user, int tool);

        /// <summary>
        /// 获取设置当前机器人的坐标系序号是否成功
        /// </summary>
        /// <param name="datBuf">0：成功
        ///1：运动中设置失败
        ///2：同步位置失败</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetSetCoordError(ref short datBuf);

        /// <summary>
        /// 获取当前机器人报警状态(读缓存)
        /// </summary>
        /// <param name="scram">0：正常 非零：报警</param>
        /// <param name="alarmNo">具体报警号</param>
        /// <param name="realTime"></param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetScramState(ref int scram, ref int alarmNo, bool realTime = false);

        /// <summary>
        /// 获取当前是否获取控制器运动权限
        /// </summary>
        /// <param name="state">0：未获取 1：获取</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetControlState(ref int state);

        /// <summary>
        /// 获取当前机器人是否处于运动状态
        /// </summary>
        /// <param name="state">0：空闲状态 1：运动状态</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetRobotRunState(ref int state);

        /// <summary>
        /// 使能状态下清J3、J4轴编码器值
        /// </summary>
        /// <param name="datBuf">0：成功 非0：失败</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_ClearJ3J4Encoder(ref short datBuf);

        /// <summary>
        /// 初始化编码器
        /// </summary>
        /// <param name="portNo">M5/M6（4或者5）</param>
        /// <param name="cdrType">1：INCREMENTAL
        ///2：SIN_COS
        ///3：INCREMENTAL_INC_15LINE
        ///4：SSI
        ///5：NIKON_17_BITS_SINGLE
        ///6：Panasonic_Inc_20
        ///7：Panasonic_Abs_17_SINGLE
        ///8：Panasonic_Abs_17
        ///10：Tamagawa_Abs_17
        ///12：Tamagawa_Inc_17
        ///13：Tamagawa_Abs_17_SINGLE</param>
        /// <param name="ratio">分辨率</param>
        /// <param name="revDir">方向（正向0；反向1）</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_EncoderInit(int portNo, int cdrType, int ratio, int revDir);

        /// <summary>
        /// 设置传送带参数
        /// </summary>
        /// <param name="CnvNo">传送带编号（0，1）</param>
        /// <param name="CnvType">直线0；圆弧1</param>
        /// <param name="portNo">M5/M6（4或者5）</param>
        /// <param name="pulseEq">脉冲当量</param>
        /// <param name="circleDir">方向（正向0；反向1；顺圆0，逆圆1）</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_SetDynParam(int CnvNo, int CnvType, int portNo, double pulseEq, int circleDir);

        /// <summary>
        /// 启动传送带任务
        /// </summary>
        /// <param name="type">1</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_SetDynCatch(int type);

        /// <summary>
        /// 开始跟随运动
        /// </summary>
        /// <param name="cnvno">传送带编号（0，1）</param>
        /// <param name="pulse">编码器值</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_SetCatch(int cnvno, int pulse);

        /// <summary>
        /// 获取当前的跟随状态
        /// </summary>
        /// <param name="datBuf">0：未跟随
        ///1：准备跟随
        ///2：跟随中
        ///3：跟随结束</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetCatchState(ref short datBuf);

        /// <summary>
        /// 设置结束跟随
        /// </summary>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_SetSynOver();

        /// <summary>
        /// 获取跟随结束状态
        /// </summary>
        /// <param name="datBuf">0：跟随结束 非0：跟随未结束</param>
        /// <returns></returns>
        [DllImport(DllPath)]
        public static extern short ADT_GetSynOverState(ref short datBuf);

    }
}
