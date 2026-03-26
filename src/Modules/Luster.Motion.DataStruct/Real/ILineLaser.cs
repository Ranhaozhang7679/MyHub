#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ILineLaser
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.LineLasers
* 文 件 名:       ILineLaser.cs
* 创建时间:       2022/4/11 14:43:46
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ba86733e-e4e0-440c-b1e1-f21461e93bb2
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/11 14:43:46
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Real
{
    /// <summary>
    /// 线激光
    /// </summary>
    public interface ILineLaser
    {
        /// <summary>
        /// 序列号
        /// </summary>
        string SN { get; set; }

        /// <summary>
        /// 当前配置ID
        /// </summary>
        string CurJobID { get; set; }

        /// <summary>
        /// 帧率
        /// </summary>
        int Frame { get; set; }

        /// <summary>
        /// 激光线亮度
        /// </summary>
        int LineBrightness { get; set; }

        /// <summary>
        /// 增益
        /// </summary>
        float Gain { get; set; }

        /// <summary>
        /// 曝光时间
        /// </summary>
        double Exposure { get; set; }

        /// <summary>
        /// 采集行数
        /// </summary>
        int RowNum { get; set; }

        /// <summary>
        /// 点云行间隔距离
        /// </summary>
        double RowDis { get; set; }

        /// <summary>
        /// 一条点云线的点数
        /// </summary>
        int ColNum { get; set; }

        /// <summary>
        /// 一条点云线的点间距
        /// </summary>
        double ColDis { get; set; }

        /// <summary>
        /// 触发开关（软触发、硬件触发）
        /// </summary>
        LaserStartTrigger StartTriggerMode { get; set; }

        /// <summary>
        /// 数据触发模式（时间、硬件IO、编码器）
        /// </summary>
        LaserDataTrigger DataTriggerMode { get; set; }

        /// <summary>
        /// 开始触发IO信号
        /// </summary>
        VIO StartIO { get; set; }

        /// <summary>
        /// 自动搜索所有的Laser
        /// </summary>
        /// <param name="ips">ip端口</param>
        void LaserListRead(out List<IDevice> ips);

        /// <summary>
        /// 激光开启
        /// </summary>
        void LaserStart();

        /// <summary>
        /// 激光停止
        /// </summary>
        void LaserStop();

        /// <summary>
        /// 启用软触发
        /// </summary>
        /// <param name="id"></param>
        void SoftTrigger();

        /// <summary>
        /// 加载参数配置
        /// </summary>
        /// <param name="configPath">参数配置文件路径</param>
        void LoadConfig(string configPath);

        /// <summary>
        /// 设置激光参数
        /// </summary>
        void LaserSetPamas(LaserPamas pamas, object val);

        /// <summary>
        /// 设置非常用参数
        /// </summary>
        /// <param name="keyValues"></param>
        void LaserSetSpPama(string key,object value);

        /// <summary>
        /// 获取激光参数
        /// </summary>
        object LaserGetPamas(string pamaName = "");

        /// <summary>
        /// 激光完成回调
        /// </summary>
        event Action<LineLaser> ScanFinishEvent;

        /// <summary>
        /// 获取JOBs
        /// </summary>
        /// <returns></returns>
        List<string> GetJobs();

        /// <summary>
        /// 设置当前JOB
        /// </summary>
        /// <param name="jobName"></param>
        void SetJob(string jobName);
    }
}