using Luster.Motion.DataStruct.DataModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class VRobotPointModel : BindableBase
    {
        private string _name;
        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }


        private double _pos;
        /// <summary>
        /// 点位位置
        /// </summary>
        public double Pos
        {
            get => _pos;
            set
            {
                SetProperty(ref _pos, value);
                _pos = Math.Round(_pos, 3);
                switch (Name)
                {
                    case "X":
                        Tag.PositionX = _pos;
                        Tag.Tag.PositionX = _pos;
                        break;
                    case "Y":
                        Tag.PositionY = _pos;
                        Tag.Tag.PositionY = _pos;
                        break;
                    case "Z":
                        Tag.PositionZ = _pos;
                        Tag.Tag.PositionZ = _pos;
                        break;
                    case "C":
                        Tag.PositionC = _pos;
                        Tag.Tag.PositionC = _pos;
                        break;
                    case "Hand":
                        Tag.PositionH = _pos;
                        Tag.Tag.PositionH = _pos;
                        break;
                }
            }
        }

        public RobotPointItem Tag { get; set; }

    }

    public class VSixRobotPointModel : BindableBase
    {
        private string _name;
        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }


        private double _pos;
        /// <summary>
        /// 点位位置
        /// </summary>
        public double Pos
        {
            get => _pos;
            set
            {
                SetProperty(ref _pos, value);
                _pos = Math.Round(_pos, 3);
                switch (Name)
                {
                    case "X":
                        Tag.PositionX = _pos;
                        Tag.Tag.PositionX = _pos;
                        break;
                    case "Y":
                        Tag.PositionY = _pos;
                        Tag.Tag.PositionY = _pos;
                        break;
                    case "Z":
                        Tag.PositionZ = _pos;
                        Tag.Tag.PositionZ = _pos;
                        break;
                    case "U":
                        Tag.PositionU = _pos;
                        Tag.Tag.PositionU = _pos;
                        break;
                    case "V":
                        Tag.PositionV = _pos;
                        Tag.Tag.PositionV = _pos;
                        break;
                    case "W":
                        Tag.PositionW = _pos;
                        Tag.Tag.PositionW = _pos;
                        break;
                }
            }
        }

        public SixRobotPointItem Tag { get; set; }

    }

}
