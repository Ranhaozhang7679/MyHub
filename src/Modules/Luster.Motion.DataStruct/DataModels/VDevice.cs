#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LDevice
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Models
* 文 件 名:       LDevice.cs
* 创建时间:       2022/6/10 9:11:22
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6f6a0e66-4ec9-43f7-9867-5d666303739f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/10 9:11:22
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// 设备参数对象
    /// </summary>
    public class VDevice : IXMLParser, IEmptyObj, System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 对应的设备ID
        /// </summary>
        public Guid DeviceID { get; set; }

        private string name;
        /// <summary>
        /// 对应的设备别名
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }

        /// <summary>
        /// 虚拟设备,用于对象缓存
        /// </summary>
        [Ignore]
        public IVirtualDevice Virtual { get; set; }

        /// <summary>
        /// 导出到XML
        /// </summary>
        /// <returns></returns>
        public virtual XElement ExportXml()
        {
            return this.ToXml();
        }

        public virtual bool IsEmpty()
        {
            return Guid.Empty == this.DeviceID;
        }

        /// <summary>
        /// 解析Xml
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }


        /// <summary>
        /// 获取设备对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="device"></param>
        /// <param name="outDevice"></param>
        /// <exception cref="FriendlyException"></exception>
        public T GetVDevice<T>(IDeviceEngine deviceEngine) where T : IVirtualDevice
        {
            // 如果对象已经获取过了，那么就不需要重复获取
            if (this.Virtual != null)
            {
                return (T)Virtual;
            }

            var vDevice = deviceEngine.GetVirtualByID(DeviceID);
            if (vDevice == null)
            {
                throw new FriendlyException($"设备:{Name} 不存在");
            }

            // 缓存已经查询对象
            Virtual = vDevice;

            return (T)vDevice;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }

    /// <summary>
    /// 多轴配置参数
    /// </summary>
    public class VAxisMDevice : VDevice
    {
        [Ignore]
        public List<AxisItem> Items { get; set; }

        /// <summary>
        /// 移动方向
        /// </summary>
        public MoveDirection MoveDirection { get; set; }

        /// <summary>
        /// 记录轴的优先级
        /// </summary>
        private Dictionary<Priority, List<AxisItem>> MovePriorities;

        /// <summary>
        /// 构造函数
        /// </summary>
        public VAxisMDevice()
        {
            Items = new List<AxisItem>();
            MovePriorities = new Dictionary<Priority, List<AxisItem>>();
        }

        public VAxisMDevice(VAxisMDevice vAxisM)
        {
            Items = new List<AxisItem>();
            foreach (var item in vAxisM.Items)
            {
                Items.Add(new AxisItem(item));
            }

            MovePriorities = vAxisM.MovePriorities;
        }

        /// <summary>
        /// 清理移动优先级
        /// </summary>
        public void ClearMovePriority()
        {
            MovePriorities?.Clear();
        }

        /// <summary>
        /// 构建优先级
        /// </summary>
        /// <returns></returns>
        public Dictionary<Priority, List<AxisItem>> GetMovePriorities()
        {
            if (MovePriorities == null || MovePriorities.Count == 0)
            {
                foreach (var item in Items)
                {
                    if (!MovePriorities.ContainsKey(item.MovePriority))
                    {
                        MovePriorities[item.MovePriority] = new List<AxisItem>();
                    }

                    // 保证轴不会存在相同
                    var list = MovePriorities[item.MovePriority];
                    if (!list.Any(u => u.AxisID == item.AxisID))
                    {
                        list.Add(item);
                    }
                }
            }

            return MovePriorities;
        }

        /// <summary>
        /// 导出到XML
        /// </summary>
        /// <returns></returns>
        public override XElement ExportXml()
        {
            var xml = this.ToXml();
            foreach (var item in Items)
            {
                xml.Add(item.ExportXml());
            }

            return xml;
        }

        /// <summary>
        /// 解析Xml
        /// </summary>
        /// <param name="xElement"></param>
        public override void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);

            // 先清除历史数据
            Items.Clear();
            foreach (var item in xElement.Elements("AxisItem"))
            {
                AxisItem data = new AxisItem();
                data.ParserXml(item);
                Items.Add(data);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Items)
            {
                sb.Append($"{item.Axis?.Name}:{item.Position} ");
            }

            if (sb.Length > 0)
            {
                return sb.Remove(sb.Length - 1, 1).ToString();
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// 单轴设备参数对象
    /// </summary>
    public class VAxisDevice : VDevice, IAxisParam
    {
        /// <summary>
        /// 速度
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// 补偿值
        /// </summary>
        public double Compensate { get; set; } = 0;

        /// <summary>
        /// 移动前位置
        /// </summary>
        [Ignore]
        public double PrevMovePos { get; set; }

        /// <summary>
        /// 点位
        /// </summary>
        public double Position { get; set; }

        /// <summary>
        /// 方向
        /// </summary>
        public MoveDirection Direction { get; set; }

        /// <summary>
        /// 运动方式
        /// </summary>
        public MoveMode MoveMode { get; set; }

        /// <summary>
        /// 加速度
        /// </summary>
        public double Acc { get; set; }

        /// <summary>
        /// 减速带
        /// </summary>
        public double Dec { get; set; }

        /// <summary>
        /// 当前的轴
        /// </summary>
        [Ignore]
        public VAxis Axis { get; set; }

        public VAxisDevice()
        {

        }

        public VAxisDevice(VAxisDevice vDevice)
        {
            Axis = vDevice.Axis;
            Speed = vDevice.Speed;
            Compensate = vDevice.Compensate;
            Direction = vDevice.Direction;
            MoveMode = vDevice.MoveMode;
            Position = vDevice.Position;
        }

        public override string ToString()
        {
            return $"{Axis?.Name}:{Position}";
        }

        public object Clone()
        {
            var clone = new VAxisDevice(this);
            return clone;
        }
    }
}