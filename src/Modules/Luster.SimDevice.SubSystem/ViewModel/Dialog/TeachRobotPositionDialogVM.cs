using Luster.Common.DataStruct.DataModels;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class TeachRobotPositionDialogVM : DialogVM
    {

        private string _name;
        /// <summary>
        /// 点位名称
        /// </summary>
        [Required]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private double _point_X;
        /// <summary>
        /// 点位位置X
        /// </summary>
        [Required]
        public double Point_X
        {
            get { return _point_X; }
            set { SetProperty(ref _point_X, value); }
        }

        private double _point_Y;
        /// <summary>
        /// 点位位置Y
        /// </summary>
        [Required]
        public double Point_Y
        {
            get { return _point_Y; }
            set { SetProperty(ref _point_Y, value); }
        }


        private double _point_Z;
        /// <summary>
        /// 点位位置Z
        /// </summary>
        [Required]
        public double Point_Z
        {
            get { return _point_Z; }
            set { SetProperty(ref _point_Z, value); }
        }

        private double _point_U;
        /// <summary>
        /// 点位位置U
        /// </summary>
        [Required]
        public double Point_U
        {
            get { return _point_U; }
            set { SetProperty(ref _point_U, value); }
        }

        private double _point_V;
        /// <summary>
        /// 点位位置V
        /// </summary>
        [Required]
        public double Point_V
        {
            get { return _point_V; }
            set { SetProperty(ref _point_V, value); }
        }

        private double _point_W;
        /// <summary>
        /// 点位位置V
        /// </summary>
        [Required]
        public double Point_W
        {
            get { return _point_W; }
            set { SetProperty(ref _point_W, value); }
        }


        private double _point_H;
        /// <summary>
        /// 点位位置H
        /// </summary>
        [Required]
        public double Point_H
        {
            get { return _point_H; }
            set { SetProperty(ref _point_H, value); }
        }


        private List<KeyValue> _point;
        /// <summary>
        /// 点位位置
        /// </summary>
        //[Required]
        public List<KeyValue> Point
        {
            get { return _point; }
            set { SetProperty(ref _point, value); }
        }

        /// <summary>
        /// 机器人是否为六轴机器人
        /// </summary>
        private bool _isSixRobot;
        public bool IsSixRobot
        {
            get { return _isSixRobot; }
            set
            {
                SetProperty(ref _isSixRobot, value);
            }
        }



        protected TeachRobotPositionDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (parameters.TryGetValue<List<KeyValue>>("Point", out var point) &&
                parameters.TryGetValue<bool>("IsSixRobot", out var isSixRobot))
            {
                Point = point;
                IsSixRobot = isSixRobot;
                foreach (var item in point)
                {
                    if(IsSixRobot)
                    {
                        if (item.Key == "X") Point_X = Convert.ToDouble(item.Value);
                        if (item.Key == "Y") Point_Y = Convert.ToDouble(item.Value);
                        if (item.Key == "Z") Point_Z = Convert.ToDouble(item.Value);
                        if (item.Key == "U") Point_U = Convert.ToDouble(item.Value);
                        if (item.Key == "V") Point_V = Convert.ToDouble(item.Value);
                        if (item.Key == "W") Point_W = Convert.ToDouble(item.Value);
                    }
                    else
                    {
                        if (item.Key == "X") Point_X = Convert.ToDouble(item.Value);
                        if (item.Key == "Y") Point_Y = Convert.ToDouble(item.Value);
                        if (item.Key == "Z") Point_Z = Convert.ToDouble(item.Value);
                        if (item.Key == "U") Point_U = Convert.ToDouble(item.Value);
                        if (item.Key == "Hand") Point_H = Convert.ToDouble(item.Value);
                    }
                }
            }
        }

        protected override void Ok(IDialogResult result)
        {
            result.Parameters.Add(nameof(Name), Name);
            foreach (var item in Point)
            {
                if (item.Key == "X") item.Value = Point_X;
                if (item.Key == "Y") item.Value = Point_Y;
                if (item.Key == "Z") item.Value = Point_Z;
                if (item.Key == "U") item.Value = Point_U;
                if(!IsSixRobot)
                {
                    if (item.Key == "Hand") item.Value = Point_H;
                }
                else
                {
                    if (item.Key == "V") item.Value = Point_V;
                    if (item.Key == "W") item.Value = Point_W;
                }
                
            }
            result.Parameters.Add(nameof(Point), Point);
        }
    }
}
