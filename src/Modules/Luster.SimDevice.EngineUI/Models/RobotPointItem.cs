using Luster.Motion.DataStruct.DataModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class RobotPointItem : BindableBase
    {
        private string _name;
        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                var src = _name;
                SetProperty(ref _name, value);
                if (src != value && Tag != null)
                {
                    Tag.Name = value;
                }
            }
        }

        private double _positionX;
        /// <summary>
        /// 点位位置X
        /// </summary>
        public double PositionX
        {
            get { return _positionX; }
            set
            {
                var src = _positionX;
                SetProperty(ref _positionX, value);
                //if (src != value && Tag != null)
                //{
                //    Tag.Axis.Engine.UpdatePostion(Tag, value);
                //}
            }
        }

        private double _positionY;
        /// <summary>
        /// 点位位置Y
        /// </summary>
        public double PositionY
        {
            get { return _positionY; }
            set
            {
                var src = _positionY;
                SetProperty(ref _positionY, value);
            }
        }

        private double _positionZ;
        /// <summary>
        /// 点位位置Z
        /// </summary>
        public double PositionZ
        {
            get { return _positionZ; }
            set
            {
                var src = _positionZ;
                SetProperty(ref _positionZ, value);
            }
        }

        private double _positionC;
        /// <summary>
        /// 点位位置Z
        /// </summary>
        public double PositionC
        {
            get { return _positionC; }
            set
            {
                var src = _positionC;
                SetProperty(ref _positionC, value);
            }
        }

        private double _positionH;
        public double PositionH
        {
            get { return _positionH; }
            set
            {
                var src = _positionH;
                SetProperty(ref _positionH, value);
            }
        }


        /// <summary>
        /// 机器人点位对象
        /// </summary>
        public RobotPosition Tag { get; set; }
    }

    public class SixRobotPointItem : BindableBase
    {
        private string _name;
        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                var src = _name;
                SetProperty(ref _name, value);
                if (src != value && Tag != null)
                {
                    Tag.Name = value;
                }
            }
        }

        private double _positionX;
        /// <summary>
        /// 点位位置X
        /// </summary>
        public double PositionX
        {
            get { return _positionX; }
            set
            {
                var src = _positionX;
                SetProperty(ref _positionX, value);
                //if (src != value && Tag != null)
                //{
                //    Tag.Axis.Engine.UpdatePostion(Tag, value);
                //}
            }
        }

        private double _positionY;
        /// <summary>
        /// 点位位置Y
        /// </summary>
        public double PositionY
        {
            get { return _positionY; }
            set
            {
                var src = _positionY;
                SetProperty(ref _positionY, value);
            }
        }

        private double _positionZ;
        /// <summary>
        /// 点位位置Z
        /// </summary>
        public double PositionZ
        {
            get { return _positionZ; }
            set
            {
                var src = _positionZ;
                SetProperty(ref _positionZ, value);
            }
        }

        private double _positionU;
        /// <summary>
        /// 点位位置U
        /// </summary>
        public double PositionU
        {
            get { return _positionU; }
            set
            {
                var src = _positionU;
                SetProperty(ref _positionU, value);
            }
        }

        private double _positionV;

        /// <summary>
        /// 点位位置V
        /// </summary>
        public double PositionV
        {
            get { return _positionV; }
            set
            {
                var src = _positionV;
                SetProperty(ref _positionV, value);
            }
        }

        private double _positionW;

        /// <summary>
        /// 点位位置V
        /// </summary>
        public double PositionW
        {
            get { return _positionW; }
            set
            {
                var src = _positionW;
                SetProperty(ref _positionW, value);
            }
        }

        /// <summary>
        /// 机器人点位对象
        /// </summary>
        public SixRobotPosition Tag { get; set; }
    }
}
