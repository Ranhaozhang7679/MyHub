#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VPointPos
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.DataStruct.DataModels
* 文 件 名:       VPointPos.cs
* 创建时间:       2022/12/14 18:47:07
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a4e080b2-971d-48dc-9979-52a5a3ccfbc3
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/14 18:47:07
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.FXVirtual;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// 轴点位对象
    /// </summary>
    public class AxisPosition : IXMLParser
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// 对应的轴
        /// </summary>
        [Ignore]
        public VAxis Axis { get; set; }

        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 点位位置
        /// </summary>
        public double Position { get; set; }

        /// <summary>
        /// 对应的轴编号
        /// </summary>
        [Ignore]
        public int AxisNo { get; set; }

        /// <summary>
        /// 运动优先级
        /// </summary>
        public Priority MovePriority { get; set; } = Priority.Low;

        #region 导入和导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xPos = new XElement(Name);
            xPos.SetAttributeValue("Key", Key);
            xPos.SetAttributeValue("Priority", MovePriority);
            xPos.SetValue(Position);

            if (Axis != null)
            {
                xPos.SetAttributeValue("AxisNo", Axis.AxisNo);
            }

            return xPos;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            Name = xElement.Name.ToString();

            if (double.TryParse(xElement.Value, out var pos))
            {
                Position = pos;
            }

            xElement.GetAttribute(nameof(Key), (key) =>
            {
                Key = Guid.Parse(key);
            });

            xElement.GetAttribute("Priority", (priority) =>
            {
                if (Enum.TryParse<Priority>(priority, out var r))
                {
                    MovePriority = r;
                }
            });

            // 隶属轴编号
            xElement.GetAttribute(nameof(AxisNo), (axisNo) =>
            {
                if (int.TryParse(axisNo, out var n))
                {
                    AxisNo = n;
                }
            });
        }
        #endregion
    }

    /// <summary>
    /// 点位参数对象
    /// </summary>
    public class AxisPosItem : IXMLParser, IAxisParam
    {
        /// <summary>
        /// 对应的PosKey
        /// </summary>
        public Guid PosKey { get; set; }

        /// <summary>
        /// 点位
        /// </summary>
        [Ignore]
        public AxisPosition AxisPostion { get; set; }

        [Ignore]
        public VAxis Axis { get => AxisPostion?.Axis; set { } }

        [Ignore]
        public double Position { get => AxisPostion == null ? 0 : AxisPostion.Position; set { } }

        /// <summary>
        /// 加速度
        /// </summary>
        public double Acc { get; set; }

        /// <summary>
        /// 减速度
        /// </summary>
        public double Dec { get; set; }

        /// <summary>
        /// 运动速度
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// 运动优先级
        /// </summary>
        public Priority MovePriority { get; set; } = Priority.Low;

        /// <summary>
        /// 补偿值
        /// </summary>
        [Ignore]
        public double Compensate { get; set; } = 0;

        [Ignore]
        public MoveMode MoveMode { get; set; } = MoveMode.Abs;

        /// <summary>
        /// 行程
        /// </summary>
        [Ignore]
        public double Distance { get; set; }

        /// <summary>
        /// 运行前需要更新对应的Position
        /// </summary>
        /// <param name="deviceEngine">设备引擎</param>
        public void UpdatePosition(IDeviceEngine deviceEngine)
        {
            if (AxisPostion == null)
            {
                AxisPostion = deviceEngine.GetAxisPosition(PosKey);
            }
        }

        #region 导入和导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            var xml = this.ToXml();
            return xml;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }

        public object Clone()
        {
            return new AxisPosItem()
            {
                PosKey = this.PosKey,
                AxisPostion = this.AxisPostion,
                Acc = this.Acc,
                Dec = this.Dec,
                Speed = this.Speed,
                MovePriority = this.MovePriority,
                Compensate = this.Compensate,
                MoveMode = this.MoveMode
            };
        }
        #endregion
    }

    /// <summary>
    /// 轴点位对象
    /// </summary>
    public class VAxisPos : List<AxisPosItem>, IName, IXMLParser, ICloneable
    {
        /// <summary>
        /// 设备对象
        /// </summary>
        public IDeviceEngine _deviceEngine;

        /// <summary>
        /// 当前名称
        /// </summary>
        public string Name { get => ToString(); set { } }

        /// <summary>
        /// 缓存对象
        /// </summary>
        private Dictionary<Priority, List<AxisPosItem>> movePriorites = null;

        /// <summary>
        /// 点位运动
        /// </summary>
        /// <param name="deviceEngine"></param>
        public void MovePostion(IDeviceEngine deviceEngine, bool IsFixedSpd = false)
        {
            isStopCommand = false;

            if (movePriorites == null)
            {
                movePriorites = new Dictionary<Priority, List<AxisPosItem>>();
                foreach (var item in this)
                {
                    item.UpdatePosition(deviceEngine);
                    if (!movePriorites.ContainsKey(item.MovePriority))
                    {
                        movePriorites[item.MovePriority] = new List<AxisPosItem>();
                    }

                    movePriorites[item.MovePriority].Add(item);
                }
            }

            // 开始运动
            if (movePriorites.ContainsKey(Priority.High))
            {
                MoveWaitDone(movePriorites[Priority.High], IsFixedSpd);
            }

            if (movePriorites.ContainsKey(Priority.Middle))
            {
                MoveWaitDone(movePriorites[Priority.Middle], IsFixedSpd);
            }

            if (movePriorites.ContainsKey(Priority.Low))
            {
                MoveWaitDone(movePriorites[Priority.Low], IsFixedSpd);
            }
        }

        /// <summary>
        /// 判断是否有发生停止命令,防止由于优先级问题，XYU走完，但是Z还没有发送质量，没有停止
        /// </summary>
        private bool isStopCommand = false;

        /// <summary>
        /// 等待完成
        /// </summary>
        /// <param name="priority"></param>
        private void MoveWaitDone(List<AxisPosItem> items, bool IsFixedSpd = false)
        {
            // 如果停止了，就不在发送指令了
            if (isStopCommand) return;

            foreach (var item in items)
            {
                double curPos = item.Axis.GetCurrentPos();

                // 示教位置及补偿值
                double pos = item.AxisPostion.Position + item.Compensate;

                // 配置行程
                item.Distance = Math.Abs(pos - curPos);

                item.Axis.MoveAbs(pos, item.Speed, -1, -1, IsFixedSpd);
                item.Axis.TargetPulse = item.Axis.PerPluse * pos;
            }

            // 等待完成
            items.ForEach(u => u.Axis.CheckMotionDone(u, 100, null, u.Axis.TargetPulse));
        }

        /// <summary>
        /// 轴暂停
        /// </summary>
        public void Stop(IDeviceEngine deviceEngine, bool isCrd = false)
        {
            isStopCommand = true;

            if (!isCrd)
            {
                // 正常运动停止
                foreach (var item in this)
                {
                    item.UpdatePosition(deviceEngine);
                    item.Axis.Stop();
                }
            }
            else
            {
                // 插补运动停止
                isCrdStop = true;
                var motionCard = GetMotionCard();

                // 对插补进行停止
                motionCard.Stop(-1);
                isCrdStop = false;
            }
        }

        /// <summary>
        /// 多轴直线插补运动
        /// </summary>
        /// <param name="mDevice"></param>
        public void MoveLineAbs(IDeviceEngine deviceEngine)
        {
            if (Count < 2 || Count > 4)
            {
                throw new Exception("插补运动的轴数量请设置在2~4之间");
            }

            List<int> AxisId = new List<int>();
            List<double> pos = new List<double>();
            List<double> perPlus = new List<double>();
            List<double> vel = new List<double>();
            List<double> acc = new List<double>();


            foreach (var item in this)
            {
                item.UpdatePosition(deviceEngine);

                AxisId.Add(item.Axis.AxisNo);
                pos.Add(item.Position + item.Compensate);
                perPlus.Add(item.Axis.PerPluse);
                vel.Add(item.Speed);
                acc.Add(item.Acc);
            }

            IMotionCard motionCard = GetMotionCard();
            motionCard.MoveLine(AxisId, pos, perPlus, vel, acc);

            // 等待插补
            WaitCrdDone(motionCard);

            // 插补运行完成，需要主动停止下插补模式
            Stop(deviceEngine, true);
        }

        /// <summary>
        /// 控制卡
        /// </summary>
        /// <returns></returns>
        private IMotionCard GetMotionCard()
        {
            IMotionCard motionCard = null;
            foreach (var item in this)
            {
                if (motionCard == null)
                {
                    motionCard = item.Axis.GetDevice() as IMotionCard;
                    break;
                }
            }
            return new FXVirtualMotionCardAspect(motionCard, _deviceEngine);
        }

        /// <summary>
        /// 是否停止
        /// </summary>
        private bool isCrdStop = false;

        /// <summary>
        /// 等待插补是否完成
        /// </summary>
        /// <param name="motionCard"></param>
        private void WaitCrdDone(IMotionCard motionCard)
        {
            // 等待
            while (true)
            {
                //对于多轴的检测，不会检测具体位置
                if (motionCard.CheckMotionDone(45, -1))
                {
                    break;
                }

                if (isCrdStop)
                {
                    break;
                }

                // 延时
                Thread.Sleep(5);
                //ThreadStaticAttribute.
            }
        }

        /// <summary>
        /// 多轴圆弧插补运动
        /// </summary>
        /// <param name="mDevice"></param>
        public void MoveCircleAbs(IDeviceEngine deviceEngine, double radius, short dir)
        {
            if (Count < 2 || Count >= 4)
            {
                throw new Exception("插补运动的轴数量请设置在2~4之间");
            }

            List<int> AxisId = new List<int>();
            List<double> pos = new List<double>();
            List<double> perPlus = new List<double>();
            List<double> vel = new List<double>();
            List<double> acc = new List<double>();

            IMotionCard motionCard = null;
            foreach (var item in this)
            {
                item.UpdatePosition(deviceEngine);

                AxisId.Add(item.Axis.AxisNo);
                pos.Add(item.Position + item.Compensate);
                perPlus.Add(item.Axis.PerPluse);
                vel.Add(item.Speed);
                acc.Add(item.Acc);

                if (motionCard == null)
                {
                    motionCard = item.Axis.GetDevice() as IMotionCard;
                }
                motionCard = new FXVirtualMotionCardAspect(motionCard, deviceEngine);
            }

            motionCard.MoveCircle(AxisId, pos, perPlus, vel, acc, radius, dir);
        }

        #region 多点位回零

        /// <summary>
        /// 回零优先级
        /// </summary>
        private Dictionary<Priority, List<AxisPosItem>> HomePriorities;

        /// <summary>
        /// 归零
        /// </summary>
        public void Home(IDeviceEngine deviceEngine)
        {
            isStopCommand = false;
            HomePriorities = new Dictionary<Priority, List<AxisPosItem>>();
            foreach (var item in this)
            {
                item.UpdatePosition(deviceEngine);
                if (!HomePriorities.ContainsKey(item.MovePriority))
                {
                    HomePriorities[item.MovePriority] = new List<AxisPosItem>();
                }

                HomePriorities[item.MovePriority].Add(item);
            }

            if (HomePriorities.ContainsKey(Priority.High))
            {
                MoveHomeDone(deviceEngine, Priority.High);
            }

            if (HomePriorities.ContainsKey(Priority.Middle))
            {
                MoveHomeDone(deviceEngine, Priority.Middle);
            }

            if (HomePriorities.ContainsKey(Priority.Low))
            {
                MoveHomeDone(deviceEngine, Priority.Low);
            }
        }

        /// <summary>
        /// 等待完成
        /// </summary>
        /// <param name="priority"></param>
        private void MoveHomeDone(IDeviceEngine deviceEngine, Priority priority)
        {
            if (isStopCommand) return;

            var list = HomePriorities[priority];
            foreach (var item in list)
            {
                item.UpdatePosition(deviceEngine);
                item.Axis.Home();
            }

            list.ForEach(u => u.Axis.CheckHomeDone());
        }

        /// <summary>
        /// 更新轴对象
        /// </summary>
        /// <param name="deviceEngine"></param>
        public void UpdateAxis(IDeviceEngine deviceEngine)
        {
            _deviceEngine = deviceEngine;
            foreach (var item in this)
            {
                item.UpdatePosition(deviceEngine);
            }
        }
        #endregion

        #region 导入和导出
        public XElement ExportXml()
        {
            XElement xPosArr = new XElement("Postions");

            foreach (var item in this)
            {
                xPosArr.Add(item.ExportXml());
            }

            return xPosArr;
        }

        public void ParserXml(XElement xElement)
        {
            this.Clear();
            if (xElement != null)
            {
                foreach (var xItem in xElement.Elements())
                {
                    AxisPosItem item = new AxisPosItem();
                    item.ParserXml(xItem);

                    this.Add(item);
                }
            }

            if (_deviceEngine != null)
            {
                UpdateAxis(_deviceEngine);
            }
        }
        #endregion

        public override string ToString()
        {
            if (this.Count == 1)
            {
                return this[0].AxisPostion?.Name;
            }
            else if (this.Count > 1)
            {
                return string.Join("_", this.Select(u => u.AxisPostion?.Name));
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 对象克隆
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            VAxisPos newVPos = new VAxisPos();
            newVPos.ParserXml(this.ExportXml());
            newVPos.UpdateAxis(_deviceEngine);

            return newVPos;
        }
    }
}