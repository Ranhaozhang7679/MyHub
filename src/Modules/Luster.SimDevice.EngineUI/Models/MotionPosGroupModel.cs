using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class MotionPosGroupModel : BindableBase
    {
        public List<AxisPosition> Tag { get; set; }

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
                //if (Tag != null)
                //{
                //    Tag.Name = value;

                //    // 更新位置
                //    foreach (var item in Tag)
                //    {
                //        item.Name = $"{item.Axis.AxisType}_{value}";
                //    }
                //}
            }
        }

#if False //为了通用六个轴显示为ABCDEF，客服要求仍用XYUZVW
        /// <summary>
        /// 默认值
        /// </summary>
        private double _A = double.NaN;
        public double A
        {
            get => _A; set
            {
                var src = _A;
                SetProperty(ref _A, value);
                //if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                //{
                //    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.X, value);
                //}
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _B = double.NaN;
        public double B
        {
            get => _B; set
            {
                var src = _B;
                SetProperty(ref _B, value);
                //if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                //{
                //    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.Y, value);
                //}
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _C = double.NaN;
        public double C
        {
            get => _C; set
            {
                var src = _C;
                SetProperty(ref _C, value);
                //if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                //{
                //    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.Z, value);
                //}
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _D = double.NaN;
        public double D
        {
            get => _D; set
            {
                var src = _D;
                SetProperty(ref _D, value);
                //if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                //{
                //    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.U, value);
                //}
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _E = double.NaN;
        public double E
        {
            get => _E; set
            {
                var src = _E;
                SetProperty(ref _E, value);
                //if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                //{
                //    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.V, value);
                //}
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _F = double.NaN;
        public double F
        {
            get => _F; set
            {
                var src = _F;
                SetProperty(ref _F, value);
                //if (Tag != null && src.ToString() != double.NaN.ToString() && src != value)
                //{
                //    Tag.DeviceEngine.UpdatePosGroup(Tag, AxisType.W, value);
                //}
            }
        }

        /// 构造函数
        /// </summary>
        /// <param name="vIO"></param>
       public MotionPosGroupModel(Guid guid, string name, List<AxisPosition> pGroup)
        {
            Key = guid;
            Name = name;
            Tag = pGroup;
            for (int i = 0; i < pGroup.Count(); i++)
            {
                switch (i)
                {
                    case 0:
                        A = (pGroup[i].Axis != null) ? pGroup[i].Position : double.NaN;
                        break;
                    case 1:
                        B = (pGroup[i].Axis != null) ? pGroup[i].Position : double.NaN;
                        break;
                    case 2:
                        C = (pGroup[i].Axis != null) ? pGroup[i].Position : double.NaN;
                        break;
                    case 3:
                        D = (pGroup[i].Axis != null) ? pGroup[i].Position : double.NaN;
                        break;
                    case 4:
                        E = (pGroup[i].Axis != null) ? pGroup[i].Position : double.NaN;
                        break;
                    case 5:
                        F = (pGroup[i].Axis != null) ? pGroup[i].Position : double.NaN;
                        break;
                }
            }

            Axises = Tag.Select(u => u.Axis).ToList();
        }

#else
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
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _V = double.NaN;
        public double V
        {
            get => _V; set
            {
                var src = _V;
                SetProperty(ref _V, value);
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _W = double.NaN;
        public double W
        {
            get => _W; set
            {
                var src = _W;
                SetProperty(ref _W, value);
            }
        }


        /// 构造函数
        /// </summary>
        /// <param name="vIO"></param>
        public MotionPosGroupModel(Guid guid, string name, List<AxisPosition> pGroup)
        {
            Key = guid;
            Name = name;
            Tag = pGroup;
            X = pGroup.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == AxisType.X)?.Position ?? double.NaN;
            Y = pGroup.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == AxisType.Y)?.Position ?? double.NaN;
            Z = pGroup.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == AxisType.Z)?.Position ?? double.NaN;
            U = pGroup.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == AxisType.U)?.Position ?? double.NaN;
            V = pGroup.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == AxisType.V)?.Position ?? double.NaN;
            W = pGroup.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == AxisType.W)?.Position ?? double.NaN;

            Axises = Tag.Select(u => u.Axis).ToList();
        }

#endif


        public AxisPosition GetTeachPos(int axisNo)
        {
            return Tag.FirstOrDefault(u => u.Axis.AxisNo == axisNo);
        }


        public void UpdatePos(VAxis vAxis)
        {
            var prop = this.GetType().GetProperty(vAxis.AxisType.ToString());
            if (prop != null)
            {
                prop.SetValue(this, vAxis.GetCurrentPos(), null);
            }
        }

        public void UpdatePriority(int axisNo, Priority mPriority)
        {
            var pos = GetTeachPos(axisNo);
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

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals(obj as MotionPosGroupModel);
        }

        public bool Equals(MotionPosGroupModel obj)
        {
            if (obj == null)
            {
                return false;
            }

            List<AxisPosition> aList = obj.Tag;


            return (this.Tag.Count() == obj.Tag.Count()
                    && this.Tag.Select(s => s.Key.ToString()).ToList().OrderBy(s => s)
                        .SequenceEqual(obj.Tag.Select(s => s.Key.ToString()).ToList().OrderBy(s => s)));
        }

        public static bool operator == (MotionPosGroupModel a, MotionPosGroupModel b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return (a.Tag.Count() == b.Tag.Count()
                    && a.Tag.Select(s => s.Key.ToString()).ToList().OrderBy(s => s)
                        .SequenceEqual(b.Tag.Select(s => s.Key.ToString()).ToList().OrderBy(s => s)));
        }

        public static bool operator != (MotionPosGroupModel a, MotionPosGroupModel b)
        {
            return !(a == b);
        }
    }
}
