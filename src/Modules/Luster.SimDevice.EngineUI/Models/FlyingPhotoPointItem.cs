using Luster.Motion.DataStruct.DataModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class FlyingPhotoPointItem : BindableBase
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
                //if (src != value && Tag != null)
                //{
                //    Tag.Name = value;
                //}
            }
        }

        private int _encoderNo;
        /// <summary>
        /// 编码器通道号
        /// </summary>
        public int EncoderNo
        {
            get { return _encoderNo; }
            set
            {
                SetProperty(ref _encoderNo, value);
            }
        }

        private int _triggerNo;
        /// <summary>
        /// 触发输出通道
        /// </summary>
        public int TriggerNo
        {
            get { return _triggerNo; }
            set
            {
                SetProperty(ref _triggerNo, value);
            }
        }


        private long _position;
        /// <summary>
        /// 点位位置
        /// </summary>
        public long Position
        {
            get { return _position; }
            set
            {
                SetProperty(ref _position, value);
            }
        }

        /// <summary>
        /// 飞拍点位对象
        /// </summary>
        public FlyingPhotoPosition Tag { get; set; }

    }
}
