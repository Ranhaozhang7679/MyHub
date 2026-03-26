using Luster.Common.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class PosGroupModel : BindableBase
    {
        public Guid Key { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name; set
            {
                SetProperty(ref _name, value);
                if (Tag != null)
                {
                    Tag.Name = value;

                    // 更新位置
                    foreach (var item in Tag)
                    {
                        item.Name = $"{item.Axis.AxisType}_{value}";
                    }
                }
            }
        }

        private string _moduel;
        public string Module
        {
            get => _moduel; set
            {
                SetProperty(ref _moduel, value);
                if (Tag != null)
                {
                    Tag.Module = value;
                }
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _X = double.NaN;
        public double X
        {
            get => _X; set
            {
                var src = _X;
                SetProperty(ref _X, value);
                if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                {
                    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.X, value);
                    UpdateAxisPosition(AxisType.X, value);
                }
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _Y = double.NaN;
        public double Y
        {
            get => _Y; set
            {
                var src = _Y;
                SetProperty(ref _Y, value);
                if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                {
                    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.Y, value);
                    UpdateAxisPosition(AxisType.Y, value);
                }
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _Z = double.NaN;
        public double Z
        {
            get => _Z; set
            {
                var src = _Z;
                SetProperty(ref _Z, value);
                if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                {
                    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.Z, value);
                    UpdateAxisPosition(AxisType.Z, value);
                }
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _U = double.NaN;
        public double U
        {
            get => _U; set
            {
                var src = _U;
                SetProperty(ref _U, value);
                if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                {
                    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.U, value);
                    UpdateAxisPosition(AxisType.U, value);
                }
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _Z2 = double.NaN;
        public double Z2
        {
            get => _Z2; set
            {
                var src = _Z2;
                SetProperty(ref _Z2, value);
                if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                {
                    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.Z2, value);
                    UpdateAxisPosition(AxisType.Z2, value);
                }
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _U2 = double.NaN;
        public double U2
        {
            get => _U2; set
            {
                var src = _U2;
                SetProperty(ref _U2, value);
                if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                {
                    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.U2, value);
                    UpdateAxisPosition(AxisType.U2, value);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public VAxisPosGroup Tag { get; set; }

        /// 构造函数
        /// </summary>
        /// <param name="vIO"></param>
        public PosGroupModel(VAxisPosGroup pGroup)
        {
            Key = pGroup.Key;
            Name = pGroup.Name;
            Module = pGroup.Module;
            Tag = pGroup;
            X = pGroup.X;
            Y = pGroup.Y;
            Z = pGroup.Z;
            U = pGroup.U;
            Z2 = pGroup.Z2;
            U2 = pGroup.U2;

            Axises = Tag.Select(u => u.Axis).ToList();
        }


        public AxisPosition GetTeachPos(AxisType axisType)
        {
            return Tag.FirstOrDefault(u => u.Axis.AxisType == axisType);
        }


        public void UpdatePos(VAxis vAxis)
        {
            var prop = this.GetType().GetProperty(vAxis.AxisType.ToString());
            if (prop != null)
            {
                prop.SetValue(this, vAxis.GetCurrentPos(), null);
            }
        }

        public void UpdatePriority(AxisType axisType, Priority mPriority)
        {
            var pos = GetTeachPos(axisType);
            if (pos != null)
            {
                pos.MovePriority = mPriority;
            }
        }

        public void RemovePos(VAxis vAxis)
        {
            var prop = this.GetType().GetProperty(vAxis.AxisType.ToString());
            if (prop != null)
            {
                prop.SetValue(this, double.NaN, null);
            }

            Tag.RemoveAll(u => u.AxisNo == vAxis.AxisNo);

            Axises.Remove(vAxis);
        }
        /// <summary>
        /// 拥有的轴
        /// </summary>
        public List<VAxis> Axises { get; set; }

        private void UpdateAxisPosition(AxisType type, double position)
        {
            var pos = Tag.FirstOrDefault(u => u.Axis.AxisType == type);
            if (pos != null)
            {
                var newName = $"{type}_{Name}";
                var axisPos = pos.Axis.Positions.FirstOrDefault(u => u.Name == newName);
                Tag.DeviceEngine.UpdatePostion(axisPos, position);
            }
        }
    }
}
