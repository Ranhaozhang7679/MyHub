#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LaserScan
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Camera.Functions
* 文 件 名:       LaserScan.cs
* 创建时间:       2022/5/30 9:33:09
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      944eadb2-d972-4078-8bef-3efebda7ce76
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/30 9:33:09
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Camera.Functions
{
    public enum IOMethod
    {
        [Description("高电频")]
        High,

        [Description("低电频")]
        Low,
    }

    /// <summary>
    /// 线扫激光
    /// </summary>
    public class LaserScan : MotionFunction
    {
        [NotEmpty]
        [Parameter("激光设备", 0, CN = "激光设备", EditorType = typeof(VLineLaser))]
        public VDevice LineDevice { get; set; }

        [Parameter("激光设备", 1, CN = "激光轴")]
        public VAxisDevice MoveAxis { get; set; }

        [Parameter("触发模式", 2, CN = "触发模式")]
        public LaserStartTrigger Trigger { get; set; }

        [DependOn("Trigger", LaserStartTrigger.Hardware)]
        [Parameter("用于触发激光扫描开始", 3, CN = "开始DO", EditorType = typeof(VIO))]
        public VDevice StartDO { get; set; }

        /// <summary>
        /// 当前配置任务
        /// </summary>
        [NotEmpty]
        [Parameter("激光JOB", 4, CN = "激光JOB")]
        public string CurJob { get; set; }

        [Parameter("行间距", 5, CN = "行间距", DefaultV = 0.01)]
        public double RowDis { get; set; }

        [Parameter("Z向有效距离范围", 6, CN = "Z向范围")]
        public LRange ZValRange { get; set; }

        [Parameter("是否自动保存点云", 7, CN = "保存点云", DefaultV = false)]
        public bool IsSaveData { get; set; }

        [DependOn("IsSaveData", true)]
        [Parameter("点云存储地址", 8, CN = "点云地址")]
        public string SavePath { get; set; }

        /// <summary>
        /// 点云数据
        /// </summary>
        [Parameter("点云对象", 11, CN = "扫描点云", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public Luster.ThreeD.Algorithm.VCloud OutCloud { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LaserScan()
        {
            this.Icon = "\xe68b";
            this.Tips = "线激光";
            ZValRange = new LRange(-100, 100);
        }

        /// <summary>
        /// 设备运行
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {

            GetVDevice<VLineLaser>(LineDevice, out var vLaser);

            //离线模式运行
            if (DeviceMode.Virtual == LineDevice.Virtual.Mode)
            {
                errMsg = string.Empty;
                if (Path.GetExtension(vLaser.LineLaserPath.Path) == ".ply")
                {
                    OutCloud = Luster.ThreeD.Algorithm.VCloud.ReadPly(vLaser.LineLaserPath.Path, true, out errMsg);
                }
                else if (Path.GetExtension(vLaser.LineLaserPath.Path) == ".txt")
                {
                    OutCloud = Luster.ThreeD.Algorithm.VCloud.ReadTxt(vLaser.LineLaserPath.Path, true, out errMsg);
                }
                else
                {
                    errMsg = "加载离线点云数据类型不正确或者数据文件为空！";
                }
                return string.IsNullOrEmpty(errMsg);
            }
            vLaser.ScanFinishEvent += PCloudCallback;
            //硬件开始触发
            GetVDevice<VIO>(StartDO, out var startDo);
            GetVDevice<VAxis>(MoveAxis, out var lineAxis);

            // 每次运行进行更新
            UpdateParam(vLaser);

            // 开始运动
            if (lineAxis != null)
            {
                lineAxis.MoveAbs(MoveAxis);
            }
            //开始触发
            if (Trigger == LaserStartTrigger.SoftWare)
            {
                vLaser.SoftTrigger();
            }
            else if (Trigger == LaserStartTrigger.Hardware)
            {

                startDo?.SetDigital(true);
            }
            isEnd = false;
            lineAxis?.CheckMotionDone();

            //关闭开始触发信号
            startDo?.SetDigital(false);

            // 等待线激光处理完成
            WaitFunc(() => isEnd, "等待激光扫描完成");
            vLaser.ScanFinishEvent -= PCloudCallback;
            return base.DoExcute(out errMsg);
        }

        bool isEnd = false;

        private void PCloudCallback(LineLaser laser)
        {
            laser.ValidMin = ZValRange.Min;
            laser.ValidMax = ZValRange.Max;
            OutCloud = Luster.ThreeD.Algorithm.VCloud.ParseFromLaser(laser);
            if (SavePath != null && Directory.Exists(SavePath) && IsSaveData)
            {
                string nowTime = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                OutCloud.Save(SavePath + nowTime + ".ply");
            }
            isEnd = true;
        }

        /// <summary>
        /// 参数更新
        /// </summary>
        private bool isNeedUpdate = true;
        private void UpdateParam(VLineLaser laser)
        {
            laser.SetJob(CurJob);
            if (!isNeedUpdate)
                return;
            laser.SetParams(LaserPamas.RowDis, RowDis);
            isNeedUpdate = false;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="parameter">参数名</param>
        /// <param name="newV">对应的值</param>
        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            //数据存储
            if (parameter.Name == nameof(SavePath))
            {
                if (Directory.Exists(newV.ToString()))
                {
                    parameter.Value = newV.ToString();
                }

            }
            //触发模式修改
            if (parameter.Name == nameof(Trigger))
            {
                GetVDevice<VLineLaser>(LineDevice, out var vLaser);
                vLaser?.SetParams(LaserPamas.StartTriggerMode, Trigger);
            }
            //配置Job文件修改
            if (parameter.Name == nameof(LineDevice))
            {
                GetVDevice<VLineLaser>(newV as VDevice, out var vLiner);
                if (vLiner != null)
                {
                    OnDataSource(nameof(CurJob), vLiner.GetJobs().ToArray());
                }
            }
            else
            {
                // 任意参数变更，都需要再次执行线激光参数更新
                isNeedUpdate = true;
            }
        }
    }
}