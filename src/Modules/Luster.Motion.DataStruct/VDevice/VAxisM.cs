#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VThreeAxis
* 机器名称:       DESKTOP-QQITNMU
* 命名空间:       Luster.Motion.DataStruct
* 文 件 名:       VThreeAxis.cs
* 创建时间:       2022/6/8 16:34:10
* 作    者:       Administrator
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       xingjinxu@lusterinc.com 
* 唯一标识：      e0052bec-5cf2-4c44-8bd2-c96b86384604
* 登录用户:       xuxingjin
* 所 属 域:       DESKTOP-QQITNMU
* 创建年份:       2022
* 修改时间:		  2022/6/8 16:34:10
* 修 改 人:		  Administrator
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.FXVirtual;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
//using Luster.Motion.DataStruct.Real;

namespace Luster.Motion.DataStruct.DataModels
{

    /// <summary>
    /// 多轴的每个单轴的参数对象
    /// </summary>
    public class AxisItem : IAxisParam
    {
        /// <summary>
        /// 轴对象
        /// </summary>
        [Ignore]
        public VAxis Axis { get; set; }

        /// <summary>
        /// 轴ID
        /// </summary>
        public Guid AxisID { get; set; }

        /// <summary>
        /// 运动的优先级
        /// </summary>
        public Priority MovePriority { get; set; }

        /// <summary>
        /// 速度
        /// </summary>
        public double Speed { get; set; }

        [Ignore]
        public double PrevMovePos { get; set; }

        /// <summary>
        /// 点位
        /// </summary>
        public double Position { get; set; }

        /// <summary>
        /// 补偿值
        /// </summary>
        public double Compensate { get; set; } = 0;

        /// <summary>
        /// 运动方式
        /// </summary>
        public MoveMode MoveMode { get; set; } = MoveMode.Abs;

        public AxisItem()
        {
        }

        public AxisItem(AxisItem item)
        {
            Axis = item.Axis;
            AxisID = item.AxisID;
            Position = item.Position;
            MovePriority = item.MovePriority;
            Speed = item.Speed;
            Compensate = item.Compensate;
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            var xElemet = new XElement(nameof(AxisItem));
            xElemet.SetAttributeValue("AxisID", AxisID);
            xElemet.SetAttributeValue("MovePriority", MovePriority);
            xElemet.SetAttributeValue("Speed", Speed);
            xElemet.SetAttributeValue("Position", Position);

            return xElemet;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("AxisID", (axisID) =>
            {
                AxisID = Guid.Parse(axisID);
            });

            xElement.GetAttribute("MovePriority", (priority) =>
            {
                MovePriority = priority.ToEnum<Priority>();
            });

            xElement.GetAttribute("Speed", (speed) =>
            {
                Speed = int.Parse(speed);
            });

            xElement.GetAttribute("Position", (pos) =>
            {
                Position = double.Parse(pos);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Position}";
        }

        public object Clone()
        {
            return new AxisItem(this);
        }
    }



    /// <summary>
    /// 多轴场景
    /// </summary>
    public class VAxisM : VirtualDeviceBase, IStop
    {
        /// <summary>
        /// 排序
        /// </summary>
        public override int Sort => 3;

        /// <summary>
        /// 轴Item
        /// </summary>
        private const string AxisItem = "AxisItem";

        /// <summary>
        /// 多轴集合
        /// </summary>
        [Ignore]
        public List<AxisItem> Axises { get; set; }

        /// <summary>
        /// 轴数量
        /// </summary>
        public int Number => Axises.Count;

        /// <summary>
        /// 回零优先级
        /// </summary>
        private Dictionary<Priority, List<AxisItem>> HomePriorities;

        /// <summary>
        /// 构造函数
        /// </summary>
        public VAxisM()
        {
            // 最大8个
            Axises = new List<AxisItem>(8);
            HomePriorities = new Dictionary<Priority, List<AxisItem>>();
        }


        /// <summary>
        /// 归零
        /// </summary>
        public override void Home()
        {
            HomePriorities.Clear();
            foreach (var item in Axises)
            {
                var axis = item.Axis;
                if (!HomePriorities.ContainsKey(axis.HomePriority))
                {
                    HomePriorities[axis.HomePriority] = new List<AxisItem>();
                }

                HomePriorities[axis.HomePriority].Add(item);
            }

            if (HomePriorities.ContainsKey(Priority.High))
            {
                MoveHomeDone(Priority.High);
            }

            if (HomePriorities.ContainsKey(Priority.Middle))
            {
                MoveHomeDone(Priority.Middle);
            }

            if (HomePriorities.ContainsKey(Priority.Low))
            {
                MoveHomeDone(Priority.Low);
            }

            VStatus = VStatus.Idle;
        }

        /// <summary>
        /// 等待完成
        /// </summary>
        /// <param name="priority"></param>
        private void MoveHomeDone(Priority priority)
        {
            var list = HomePriorities[priority];
            foreach (var item in list)
            {
                UpdateAxis(item);
                item.Axis.Home();
            }

            list.ForEach(u => u.Axis.CheckHomeDone());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void UpdateAxis(AxisItem item)
        {
            // 更新当前轴
            if (item.Axis == null)
            {
                item.Axis = Axises.FirstOrDefault(u => u.AxisID == item.AxisID).Axis;
            }
        }

        /// <summary>
        /// 开始移动
        /// </summary>
        public void Move(VAxisMDevice mDevice)
        {
            axisMode = AxisMode.Move;
            VStatus = VStatus.Running;
            var movePriorites = mDevice.GetMovePriorities();

            if (movePriorites.ContainsKey(Priority.High))
            {
                MoveWaitDone(movePriorites[Priority.High]);
            }

            if (movePriorites.ContainsKey(Priority.Middle))
            {
                MoveWaitDone(movePriorites[Priority.Middle]);
            }

            if (movePriorites.ContainsKey(Priority.Low))
            {
                MoveWaitDone(movePriorites[Priority.Low]);
            }

            // 如果是运行中，就状态更新完成，如果是被暂停，那么状态不更新
            if (VStatus == VStatus.Running)
            {
                // 运行完成，将配置更新为null
                VStatus = VStatus.Idle;
            }

            // 等待恢复
            WaitRecovery();
        }

        /// <summary>
        /// 多轴直线插补运动
        /// </summary>
        /// <param name="mDevice"></param>
        public void MoveLineAbs(VAxisMDevice mDevice)
        {
            // 如果被暂停了，则不执行该动作，因为多轴有优先级问题
            if (VStatus == VStatus.Pause) return;

            axisMode = AxisMode.LineMove;

            if (mDevice.Items.Count < 2 || mDevice.Items.Count > 4)
            {
                throw new Exception("插补运动的轴数量请设置在2~4之间");
            }

            List<int> AxisId = new List<int>();
            List<double> pos = new List<double>();
            List<double> perPlus = new List<double>();
            List<double> vel = new List<double>();
            List<double> acc = new List<double>();

            foreach (var item in mDevice.Items)
            {
                UpdateAxis(item);

                AxisId.Add(item.Axis.AxisNo);
                pos.Add(item.Position + item.Compensate);
                perPlus.Add(item.Axis.PerPluse);
                vel.Add(item.Speed);
                acc.Add(item.Axis.Acc);
            }

            IMotionCard motionCard = GetDevice() as IMotionCard;
            motionCard = new FXVirtualMotionCardAspect(motionCard, Engine);
            motionCard.MoveLine(AxisId, pos, perPlus, vel, acc);
            // mDevice.Items.ForEach(u => u.Axis.CheckMotionDone());

            // 如果是运行中，就状态更新完成，如果是被暂停，那么状态不更新
            if (VStatus == VStatus.Running)
            {
                // 运行完成，将配置更新为null
                VStatus = VStatus.Idle;
            }

            // 暂停等待
            WaitRecovery("Move 等待");
        }

        /// <summary>
        /// 多轴圆弧插补运动
        /// </summary>
        /// <param name="mDevice"></param>
        public void MoveCircleAbs(VAxisMDevice mDevice, double radius, int dir)
        {
            axisMode = AxisMode.CircleMove;

            if (mDevice.Items.Count < 2 || mDevice.Items.Count >= 4)
            {
                throw new Exception("插补运动的轴数量请设置在2~4之间");
            }

            List<int> AxisId = new List<int>();
            List<double> pos = new List<double>();
            List<double> perPlus = new List<double>();
            List<double> vel = new List<double>();
            List<double> acc = new List<double>();


            foreach (var item in mDevice.Items)
            {
                UpdateAxis(item);

                AxisId.Add(item.Axis.AxisNo);
                pos.Add(item.Position + item.Compensate);
                perPlus.Add(item.Axis.PerPluse);
                vel.Add(item.Speed);
                acc.Add(item.Axis.Acc);
            }

            IMotionCard motionCard = GetDevice() as IMotionCard;
            motionCard = new FXVirtualMotionCardAspect(motionCard,Engine);
            motionCard.MoveCircle(AxisId, pos, perPlus, vel, acc, radius, (short)dir);
            mDevice.Items.ForEach(u => u.Axis.CheckMotionDone());
            // 如果是运行中，就状态更新完成，如果是被暂停，那么状态不更新
            if (VStatus == VStatus.Running)
            {
                // 运行完成，将配置更新为null
                VStatus = VStatus.Idle;
            }

            // 暂停等待
            WaitRecovery("Move 等待");
        }

        /// <summary>
        /// 等待完成
        /// </summary>
        /// <param name="priority"></param>
        private void MoveWaitDone(List<AxisItem> items)
        {
            // 如果被暂停了，则不执行该动作，因为多轴有优先级问题
            if (VStatus == VStatus.Pause) return;

            foreach (var item in items)
            {
                // 更新当前轴
                UpdateAxis(item);

                // 记录运行前的位置信息
                item.PrevMovePos = item.Axis.GetCurrentPos();

                // 示教位置及补偿值
                double pos = item.Position + item.Compensate;

                item.Axis.MoveAbs(pos, item.Speed);
            }

            // 验证是否完成
            items.ForEach(u => u.Axis.CheckMotionDone(u, 45, null));
        }

        /// <summary>
        /// 获取所有引用对象
        /// </summary>
        /// <returns></returns>
        public override List<IVirtualDevice> GetRefObjs()
        {
            List<IVirtualDevice> refObjs = new List<IVirtualDevice>();
            if (Axises.Count == 0) return refObjs;

            foreach (var item in Axises)
            {
                refObjs.Add(item.Axis);
            }

            return refObjs;
        }

        /// <summary>
        /// JOG
        /// </summary>
        /// <param name="speed">正向速度</param>
        public void Jog(VAxisMDevice mDevice)
        {
            axisMode = AxisMode.JOG;
            VStatus = VStatus.Running;
            foreach (var item in mDevice.Items)
            {
                UpdateAxis(item);
                item.Axis.Jog(mDevice.MoveDirection == MoveDirection.Positive, item.Axis.JogSpeed);
            }
        }

        /// <summary>
        /// 轴停止
        /// </summary>
        public override void Stop()
        {
            foreach (var item in Axises)
            {
                item.Axis.Stop();
            }

            // 如果处于运行状态，那么停止并清理配置
            if (VStatus == VStatus.Running || VStatus == VStatus.Stop)
            {
                VStatus = VStatus.Idle;
            }
        }

        public override IDevice GetDevice()
        {
            if (Axises.Count < 2)
            {
                throw new DeviceException("多轴未配置轴");
            }

            return Axises[0].Axis.GetDevice();
        }
        #region 导入导出实现
        public override XElement ExportXml()
        {
            var xml = base.ExportXml();
            foreach (var item in Axises)
            {
                xml.Add(item.ExportXml());
            }

            return xml;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement"></param>
        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);

            foreach (var item in xElement.Elements(AxisItem))
            {
                AxisItem data = new AxisItem();
                data.ParserXml(item);
                data.Axis = Engine.GetVirtualByID(data.AxisID) as VAxis;
                Axises.Add(data);
            }
        }

        #endregion

        #region 暂停和恢复
        /// <summary>
        /// 记录最后的运动配置
        /// </summary>
        private AxisMode axisMode = AxisMode.Move;
        #endregion
    }
}