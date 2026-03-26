#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VLineLaser
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.LineLasers
* 文 件 名:       VLineLaser.cs
* 创建时间:       2022/4/11 15:20:37
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      cd159125-943a-4ba9-b66f-eb9417a47aa3
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/11 15:20:37
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct
{
    public class VLineLaser : VirtualDeviceBase
    {
        public override int Sort => 10;

        /// <summary>
        /// 线扫完成
        /// </summary>
        public Action<LineLaser> ScanFinishEvent;

        /// <summary>
        /// 线激光
        /// </summary>
        ILineLaser _lineLaser = null;

        /// <summary>
        /// 激光序列号
        /// </summary>
        public string LaserSN { get; set; }

        /// <summary>
        /// 线激光点云数据
        /// </summary>
        public LPath LineLaserPath { get; set; }

        /// <summary>
        /// 激光配置
        /// </summary>
        /// <param name="hardDevice"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool SetDevice(IDevice hardDevice, out string errMsg)
        {
            base.SetDevice(hardDevice, out errMsg);

            _lineLaser = hardDevice as ILineLaser;
            if (_lineLaser == null)
            {
                errMsg = "设备不是激光对象";
                return false;
            }

            // 硬件中Add时候进行的初始化
            ProcessAction(() =>
            {
                // 激光初始化
                if (!hardDevice.HasInit())
                {
                    hardDevice.InitApi();

                    // 打开设备
                    hardDevice.Open();

                    // 启用
                    hardDevice.SetEnabled(true);
                }
            });

            // 注册完成事件
            _lineLaser.ScanFinishEvent -= LineLaser_ScanFinishEvent;
            _lineLaser.ScanFinishEvent += LineLaser_ScanFinishEvent;

            return string.IsNullOrEmpty(errMsg);
        }

        /// <summary>
        /// 激光扫描完成
        /// </summary>
        /// <param name="obj"></param>
        private void LineLaser_ScanFinishEvent(LineLaser obj)
        {
            ScanFinishEvent?.Invoke(obj);
        }

        /// <summary>
        /// 配置JOB
        /// </summary>
        /// <param name="jobName"></param>
        public void SetJob(string jobName)
        {
            _lineLaser.SetJob(jobName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetJobs()
        {
            return _lineLaser.GetJobs();
        }

        public void SoftTrigger()
        {
            _lineLaser.SoftTrigger();
        }

        public void SetParams(LaserPamas pName, object newV)
        {
            _lineLaser.LaserSetPamas(pName, newV);
        }

        public object LaserGetPamas(string name)
        {
           return   _lineLaser.LaserGetPamas(name);
        }
    }
}