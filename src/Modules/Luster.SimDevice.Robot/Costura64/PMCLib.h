#ifndef	__PMCLIB_H__
#define	__PMCLIB_H__

/***********************************************************************************
  Copyright (C) 2011-2012,Adtech Tech. Co., Ltd.
  库文件名(Library Name):			PMCLib.lib
  创建日期(Creation Name):			2011/8/1
  库功能描述(Library Function):		提供标准Modbus通讯访问的接口函数。
  修改记录(Modification Record):
  1. 2018.11.16
	创建库
	Creat Library
  2. 2018.11.19
    修正宏名称并对手系建立宏
  3. 2018.11.21
    增加机器人连接状态检测功能,相对坐标单轴点位运动功能
  4.2018.12.07
	修正连接错误标志宏的标注
	增加连接失败返回的标志
	增加自定义控制器IP的modbus连接函数 ADT_Connect2Controller_CustomAddress
  5.2018.12.19
    加入连接限定锁:
	    每一次连接失败都会执行断开连接操作;在断开连接操作后,才允许系统的连接句柄的初始化
	加入读写限定锁:
	    除了【连接/断开/连线状态检查(伪)】函数外,其他函数在执行前进行读写限定锁检测,若其
		值为真,则不执行操作,直接返回错误;读写限定锁只有在连接成功时才会置为假,在执行断开
		连接或连接失败时会置为真
  6.2019.07.10
    适配QC410一体机.
	1.修改了modbus的读写地址
	2.去除了寸动功能的函数,并加入手动速度模式设置函数,并增删相应的宏定义
	3.暂时删除了外部输入输出的读写接口
  7.2019.07.29
    加入机器人当前状态宏
	修正机器人手动运动时,连线状态检测不正确问题
***********************************************************************************/
/****************************************************************************************************
										宏定义 Macro define
****************************************************************************************************/
typedef	unsigned char	INT8U;
typedef char  			INT8S;
typedef unsigned short	INT16U;
typedef short			INT16S;
typedef unsigned int	INT32U;
typedef int				INT32S;
typedef float			FP32;
typedef double			FP64;
typedef	int				BOOL;

#define		math_EPS					0.000001

//错误类型定义 Error type define
#define	WORK_NO_ERR				0x00	//操作无错误	operation successfully
#define	WORK_COM_SEBD_FAIL		0x01	//发送数据失败
#define	WORK_COM_RECV_FAIL		0x02	//接收数据失败
#define	WORK_COM_UNCONNECT		0x03	//网络未连接
#define	WORK_PARAM_OVER			0x04	//参数超出范围
#define	WORK_READDATA_ERR		0x05	//读数据错误	read data error
#define	WORK_WRITEDATA_ERR		0x06	//写数据错误	write data error
#define	WORK_TIMEOUT_ERR		0x07	//通讯超时错误	connection timeout
#define	WORK_BUSY_ERR			0x08	//通讯繁忙错误	connection busy
#define	WORK_MOVE_STOP			0x09	//运动停止
#define	WORK_PARAM_ERROR		0x0a	//参数错误
#define	WORK_TCPNET_ERROR		0x0b	//网络通讯错误
#define	WORK_AUTHORITY_ERR		0x0c	//未获取运动权限
#define	WORK_AUTHORITYING_ERR	0x0d	//运动权限下不能操作
#define	WORK_RUNNING_ERR		0x0e	//运动状态下不能操作
#define	WORK_ANALYSIS_ERR		0x0f	//解析数据错误
#define WORK_ORDERMATCH_ERR		0x10 //接收数据与指令不匹配
/****************************************************************************************************
										保留的函数(禁止调用) reservation function(prohibited)
****************************************************************************************************/
//INT8U _stdcall ConnectToMBDataServer(const char *address);
/****************************************************************************************************
										用户应用接口 User API
****************************************************************************************************/
//---- Macro define ----//
#define	AR_PROGRAM_START	0x01
#define	AR_PROGRAM_PAUSE	0x02
#define	AR_PROGRAM_STOP		0x03
#define	AR_PROGRAM_RESET	0x04

#define ROBOT_STATUS_FREE	0x00
#define ROBOT_STATUS_PUASE	0x01
#define ROBOT_STATUS_RUN	0x02
#define ROBOT_STATUS_MANUAl 0x03

#define CONTROL_MODE_MANUAL	0x00
#define CONTROL_MODE_AUTO	0x01

#define	MODE_DISABLE		0x00
#define	MODE_ENABLE			0x01
#define	MODE_DRAG			0x02
#define	MODE_ENABLE_ALL		0x01

#define	STOP_MOVE			0x00
#define	MOVE_X_POSITIVE		0x01
#define	MOVE_X_NAGETIVE		0x02
#define	MOVE_Y_POSITIVE		0x03
#define	MOVE_Y_NAGETIVE		0x04
#define	MOVE_Z_POSITIVE		0x05
#define	MOVE_Z_NAGETIVE		0x06
#define	MOVE_C_POSITIVE		0x07
#define	MOVE_C_NAGETIVE		0x08

#define	MOVE_J1_POSITIVE	0x01
#define	MOVE_J1_NAGETIVE	0x02
#define	MOVE_J2_POSITIVE	0x03
#define	MOVE_J2_NAGETIVE	0x04
#define	MOVE_J3_POSITIVE	0x05
#define	MOVE_J3_NAGETIVE	0x06
#define	MOVE_J4_POSITIVE	0x07
#define	MOVE_J4_NAGETIVE	0x08

#define	TRACK_Z_HOLD		0x00
#define	TRACK_Z_UP			0x01

#define HAND_LEFT			0x00
#define HAND_RIGHT			0x01

#define CONNECT_STATUS_FALSE 0x00
#define CONNECT_STATUS_TRUE  0x01

#define INCHING_SPEED1		0x00
#define INCHING_SPEED2		0x01
#define INCHING_SPEEDC		0x02
#define INCHING_CONTINUE	0x03

//---- struct define/
typedef struct _ADT_point_
{
	float x;
	float y;
	float z;
	float c;
	float handSide;
	//HAND_RIGHT: not 0
	//HAND_LEFT: 0
}ADT_point;

typedef struct _ADT_pointAngle_
{
	float J1;
	float J2;
	float J3;
	float J4;
	float handSide;
	//HAND_RIGHT: not 0
	//HAND_LEFT: 0
}ADT_pointAngle;

typedef struct _ADT_alertStatus_
{
	short Joint1;
	short Joint2;
	short Joint3;
	short Joint4;
	short Motion;
	short Flags;
	short Reserve1;
	short Reserve2;
}ADT_alertStatus;

typedef struct _ADT_MArchPara_
{
    int A;
    float B;//LimitZ-related
    float C;//StartZ-related
    float D;//EndZ-related
    int CP;//5
    int Acc;
    int Dec;
    int Spd;
    int Jerk;
    float ZsAcc;//10
    float ZsDec;
    float ZsSpd;
    float ZeAcc;
    float ZeDec;
    float ZeSpd;//15
	int ArchType;
//    int isMove;//17
}ADT_MArchPara;

typedef struct _ADT_MArchLPara_
{
    int A;
    float B;//LimitZ-related
    float C;//StartZ-related
    float D;//EndZ-related
    int CP;//5
    int Acc;
    int Dec;
    int Spd;
    int Jerk;
	int AccC;
    int DecC;
    int SpdC;
    int JerkC;
    float ZsAcc;//10
    float ZsDec;
    float ZsSpd;
    float ZeAcc;
    float ZeDec;
    float ZeSpd;//15
	int ArchLType;
//    int isMove;//17
}ADT_MArchLPara;

typedef struct _ADT_MArcPara_
{
    int A;
    int B;
    float Angle;//StartZ-related
    float ci;//EndZ-related
    float cj;//5
	int dir;
	int CP;
    int Acc;
    int Dec;
    int Spd;
    int Jerk;
	int AccC;
    int DecC;
    int SpdC;
    int JerkC;
	int isCircle;
	int PType;
//    int isMove;//18
}ADT_MArcPara;

typedef struct _ADT_MovPPara_
{
    int A;
    float B;
    int CP;
    int Acc;
    int Dec;//5
    int Spd;
    int Jerk;
	int PType;
	int I;
	int Stop;
//	int isMove;//9
}ADT_MovPPara;

typedef struct _ADT_MovLPara_
{
    int A;
    float B;
    int CP;
    int Acc;
    int Dec;//5
    int Spd;
    int Jerk;
	int AccC;
    int DecC;
    int SpdC;//10
    int JerkC;
	int PType;
//	int isMove;
}ADT_MovLPara;

typedef struct _ADT_MovJPara_
{
    float Joint[4];
	int Axis;//5
    float Rel;
    int CP;
    int Acc;
    int Dec;
    int Spd;//10
    int Jerk;
	int PType;
//	int isMove;
}ADT_MovJPara;
//---- Function define ----//

//---- connect
unsigned short _stdcall ADT_Connect2Controller(void);
unsigned short _stdcall ADT_Connect2Controller_CustomAddress(const char *address);
unsigned short _stdcall ADT_Disconnect2Controller(void);

//---- read User register
//Static
unsigned short _stdcall ADT_ReadUserRegisterStatic(unsigned short number, int* datBuf);
unsigned short _stdcall ADT_ReadUserRegisterStatic_F(unsigned short number, float* datBuf);
unsigned short _stdcall ADT_WriteUserRegisterStatic(unsigned short number, int inputDat);
unsigned short _stdcall ADT_WriteUserRegisterStatic_F(unsigned short number, float inputDat);
//Dynamic
unsigned short _stdcall ADT_ReadUserRegisterDynamic(unsigned short number, int* datBuf);
unsigned short _stdcall ADT_ReadUserRegisterDynamic_F(unsigned short number, float* datBuf);
unsigned short _stdcall ADT_WriteUserRegisterDynamic(unsigned short number, int inputDat);
unsigned short _stdcall ADT_WriteUserRegisterDynamic_F(unsigned short number, float inputDat);

//---- Input/Output Signal
unsigned short _stdcall ADT_ReadInputSignal(unsigned short number, unsigned short* datBuf, bool realTime = true);
unsigned short _stdcall ADT_ReadOutputSignal(unsigned short number, unsigned short* datBuf, bool realTime = true);
unsigned short _stdcall ADT_WriteOutputSignal(unsigned short number, unsigned short inputDat);

unsigned short _stdcall ADT_ReadInputSignalBit(unsigned short number, unsigned short* datBuf, bool realTime = true);
unsigned short _stdcall ADT_ReadOutputSignalBit(unsigned short number, unsigned short* datBuf, bool realTime = true);
unsigned short _stdcall ADT_WriteOutputSignalBit(unsigned short number, unsigned short inputDat);

//---- Program Control
unsigned short _stdcall ADT_ProgramControl(short flag);

//---- Coordinate Infomation
unsigned short _stdcall ADT_GetCurrentPosition(ADT_point* datBuf,bool realTime=true);
unsigned short _stdcall ADT_GetCurrentPositionAngle(ADT_pointAngle* datBuf,bool realTime=true);
unsigned short _stdcall ADT_GetCurrentVelScale(short* datBuf,bool realTime=true);
unsigned short _stdcall ADT_SetCurrentVelScale(short inputDat);

//---- machine status
unsigned short _stdcall ADT_GetRobotRunStatus(short* datBuf,bool realTime=true);
unsigned short _stdcall ADT_GetSystemMode(short* datBuf);

//---- Record Point To Table
unsigned short _stdcall ADT_RecordPoint2Table(short pointNumber);

//---- get alert infomation
unsigned short _stdcall ADT_GetAlertInfo(ADT_alertStatus* datBuf);


//---- Enable Status
unsigned short _stdcall ADT_GetEnableStatus(short* datBuf, bool realTime = true);
unsigned short _stdcall ADT_SetEnableStatus(short inputDat);

//---- External Manual Motion
unsigned short _stdcall ADT_ManualMode(short mode);
unsigned short _stdcall ADT_EulerMoveManual(short Axis);
unsigned short _stdcall ADT_JointMoveManual(short Joint);

//---- External Tracking Motion
unsigned short _stdcall ADT_TrackMotion(int pointNumber, bool Z_status);

//unsigned short _stdcall ADT_MovArchPoint(int pointNumber);
//---- Point Table
unsigned short _stdcall ADT_GetPointFormTable(int pointNumber, ADT_point* datBuf);
unsigned short _stdcall ADT_SetPoint2Table(int pointNumber, ADT_point pointDat);

//---- Auto move
unsigned short _stdcall ADT_AutoCheckFifoBuffer(short *datBuf);
unsigned short _stdcall ADT_AutoMoveStop(short inputDat);
unsigned short _stdcall ADT_AutoMoveArch(ADT_MArchPara datBuf);
unsigned short _stdcall ADT_AutoMoveArchError(short *datBuf);//1
unsigned short _stdcall ADT_AutoMoveP(ADT_MovPPara datBuf);
unsigned short _stdcall ADT_GetMovePStopState(short *datBuf);
unsigned short _stdcall ADT_AutoMovePError(short *datBuf);//1
unsigned short _stdcall ADT_AutoMoveL(ADT_MovLPara datBuf);
unsigned short _stdcall ADT_AutoMoveLError(short *datBuf);//1
unsigned short _stdcall ADT_AutoMoveJ(ADT_MovJPara datBuf);
unsigned short _stdcall ADT_AutoMoveJError(short *datBuf);//1
unsigned short _stdcall ADT_AutoMoveMarc(ADT_MArcPara datBuf);
unsigned short _stdcall ADT_AutoMarcError(short *datBuf);
unsigned short _stdcall ADT_AutoWaitPos(short *datBuf, bool oneFlag = false,bool realTime = true);
unsigned short _stdcall ADT_AutoWaitRealPos(short *datBuf, bool oneFlag = false, bool realTime = true);
unsigned short _stdcall ADT_AutoMAccept(short *datBuf);
unsigned short _stdcall ADT_AutoMRelease(short *datBuf);
unsigned short _stdcall ADT_AutoMoveArchL(ADT_MArchLPara datBuf);
unsigned short _stdcall ADT_AutoMoveArchLError(short *datBuf);//1
//---- connect status
bool _stdcall ADT_GetConnectStatus();

unsigned short _stdcall ADT_GetCurrentUserCoord(float pos[6],bool realTime=true);
unsigned short _stdcall ADT_GetCurrentToolCoord(float pos[6],bool realTime=true);
unsigned short _stdcall ADT_GetUserCoord(int index,float pos[6]);
unsigned short _stdcall ADT_GetToolCoord(int index,float pos[6]);
unsigned short _stdcall ADT_SetUserCoord(int index,float pos[6]);
unsigned short _stdcall ADT_SetToolCoord(int index,float pos[6]);
unsigned short _stdcall ADT_GetCurrentCoord(int *user,int *tool,bool realTime=true);
unsigned short _stdcall ADT_SetCurrentCoord(int user,int tool);
unsigned short _stdcall ADT_GetSetCoordError(short *datBuf);

unsigned short _stdcall ADT_GetScramState(int *scram,int *alarmNo, bool realTime = false);
unsigned short _stdcall ADT_GetControlState(int *state);
unsigned short _stdcall ADT_GetRobotRunState(int *state);

unsigned short _stdcall ADT_ClearJ3J4Encoder(short *datBuf);
unsigned short _stdcall ADT_EncoderInit(int portNo, int cdrType, int ratio, int revDir);
unsigned short _stdcall ADT_SetDynParam(int CnvNo, int CnvType, int portNo, double pulseEq, int circleDir);
unsigned short _stdcall ADT_SetDynCatch(int type);
unsigned short _stdcall ADT_SetCatch(int cnvno, int pulse);
unsigned short _stdcall ADT_GetCatchState(short *datBuf);
unsigned short _stdcall ADT_SetSynOver();
unsigned short _stdcall ADT_GetSynOverState(short *datBuf);

#endif