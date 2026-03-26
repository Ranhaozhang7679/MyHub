#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IMotionCard
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.MotionCards
* 文 件 名:       IMotionCard.cs
* 创建时间:       2022/4/22 18:22:01
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7bfe77a1-81ee-4718-afad-59d0fa56ca23
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/22 18:22:01
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Real
{
    /// <summary>
    /// 通用轴卡接口
    /// </summary>
    public interface IMotionCard
    {
        #region 数字IO获取
        /// <summary>
        /// 获取数字信号
        /// </summary>
        /// <param name="index">轴编号</param>
        /// <returns>数字信号值</returns>
        bool GetDigitalIn(int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="axisNo">轴编号</param>
        /// <returns>数字信号值</returns>
        bool GetDigitalOut(int index);

        /// <summary>
        /// 设置数字输出信号
        /// </summary>
        /// <param name="axisNo">轴编号</param>
        /// <param name="digitalOut">数字信号值</param>
        void SetDigitalOut(int index, bool digitalOut);
        #endregion

        #region 模拟IO获取
        /// <summary>
        /// 获取模拟量输入
        /// </summary>
        /// <param name="axisNo">轴编号</param>
        /// <returns>模拟信号值</returns>
        double GetAnalogIn(int index);

        /// <summary>
        /// 获取模拟量输出
        /// </summary>
        /// <param name="axisNo">轴编号</param>
        /// <returns>模拟信号值</returns>
        double GetAnalogOut(int index);

        /// <summary>
        /// 设置模拟量输出信号
        /// </summary>
        /// <param name="index">轴编号</param>
        /// <param name="digitalOut">模拟信号值</param>
        void SetAnalogOut(int index, double analogVal);
        #endregion

        /// <summary>
        /// 扫描轴
        /// </summary>
        /// <param name="axisNum"></param>
        /// <returns></returns>
        void ScanAxis(out uint axisNum);

        /// <summary>
        /// 扫描IO
        /// </summary>
        /// <param name="digitalIn"></param>
        /// <param name="digitalOut"></param>
        /// <returns></returns>
        void ScanDigitalIO(out uint digitalIn, out ushort digitalOut);

        /// <summary>
        /// 扫描模拟信号
        /// </summary>
        /// <param name="anglogIn"></param>
        /// <param name="anglogOut"></param>
        /// <returns></returns>
        void ScanAnglog(out ushort anglogIn, out ushort anglogOut);

        #region 轴卡操作
        /// <summary>
        /// 确认轴卡是否运动完成
        /// </summary>
        /// <param name="precision">脉冲偏差</param>
        /// <returns>是否成功</returns>
        bool CheckMotionDone(int precision, int axisNo = 0,double targetPulse=0);

        /// <summary>
        /// 获取轴当前位置
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="perPulse">脉冲当量</param>
        /// <returns></returns>
        double GetCurrentPos(int axisNo, double perPulse);

        /// <summary>
        /// 设置轴当前位置
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="perPulse">脉冲当量</param>
        /// <param name="position">轴的位置，单位mm或角度</param>
        void SetCurrentPos(int axisNo, double perPulse, double position);

        /// <summary>
        /// 回零
        /// </summary>
        /// <returns>是否成功</returns>
        void Home(int axisNo, HomeMode homeMode, double high, double low, double perPlus, double homeAcc, double Offset, AxisPML axisPML);

        /// <summary>
        /// 取消回零
        /// </summary>
        /// <returns>是否成功</returns>
        void HomeCancel(int axisNo);

        /// <summary>
        /// 检测回零是否完成
        /// </summary>
        /// <returns></returns>
        bool CheckHomeDone(int axisNo = 0);

        /// <summary>
        /// 皮带模式，就是持续运动
        /// </summary>
        /// <returns>是否成功</returns>
        void Jog(double vel, double acc, double dec, double perPlus, double slineTime, int axisNo, AxisPML axisPML);

        /// <summary>
        /// 绝对移动
        /// </summary>
        /// <returns>是否成功</returns>
        void Move(double pos, double vel, double acc, double dec, double perPlus, double slineTime, bool isAbsMove, int axisNo, AxisPML axisPML);

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="axisNo">轴编号</param>
        /// <param name="isAll">是否停止所有</param>
        /// <returns>是否成功</returns>
        void Stop(int axisNo, bool isAll = false);

        /// <summary>
        /// 直线插补
        /// </summary>
        /// <param name="axisId">轴号list</param>
        /// <param name="pos">位置list</param>
        /// <param name="perPlusArr">脉冲比list</param>
        /// <param name="vel">速度list</param>
        /// <param name="acc">加速度list</param>
        void MoveLine(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc);


        /// <summary>
        /// 圆弧插补
        /// </summary>
        /// <param name="axisId">轴号list</param>
        /// <param name="pos">位置list</param>
        /// <param name="perPlusArr">脉冲比list</param>
        /// <param name="vel">速度list</param>
        /// <param name="acc">加速度list</param>
        /// <param name="radius">半径</param>
        /// <param name="dir">方向</param>
        void MoveCircle(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir);


        /// <summary>
        /// 获取轴的状态信息
        /// </summary>
        /// <param name="axisNo">轴编号</param>
        /// <returns>状态信息</returns>
        Dictionary<AxisStatus, bool> GetAxisStatus(int axisNo,bool IsThrowException = true);

        /// <summary>
        /// 对轴使能
        /// </summary>
        /// <param name="axisNo"></param>
        /// <param name="isOn"></param>
        /// <returns></returns>
        void ServOn(int axisNo, bool isOn);

        /// <summary>
        /// 状态重置
        /// </summary>
        /// <param name="axisNo">轴卡编号</param>
        void ResetState(int axisNo);
        #endregion

        #region 控制卡清错
        /// <summary>
        /// 清理急停信号
        /// </summary>
        void ClearEmg();
        #endregion

        #region SDO读写

        void SDORead(short slave, short index, short subindex, short data_size, out int value, short count);

        void SDOWrite(short slave, short index, short subindex, int data, short data_size);

        #endregion

        #region PDO读写
        void PDORead(short axis, short index, short subindex, short data_size, ref int value, short count);

        void PDOWrite(short axis, short index, short subindex, int data, short data_size);

        #endregion

        #region 单轴连续运动
        /// <summary>
        /// 单轴连续运动
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="acc">连续运动的加速度</param>
        /// <param name="dec">连续运动的减速度</param>
        /// <param name="perPulse">脉冲当量</param>
        /// <param name="pos">连续运动的点位集合</param>
        /// <param name="vel">连续运动的点位对应的速度集合</param>

        void AxisContinuousMove(int axisNo, double acc, double dec,double perPulse, List<double> pos, List<double> vel);


        #endregion

    }
}