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
    public class TeachFlyingPhotoPointDialogVM : DialogVM
    {
        protected TeachFlyingPhotoPointDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
        }

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

        private long _position;
        /// <summary>
        /// 点位位置
        /// </summary>
        [Required]
        public long Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        private int _encoderNo;
        /// <summary>
        /// 编码器通道号
        /// </summary>
        [Required]
        public int EncoderNo
        {
            get { return _encoderNo; }
            set { SetProperty(ref _encoderNo, value); }
        }

        private int _triggerNo;
        /// <summary>
        /// 触发输出通道
        /// </summary>
        [Required]
        public int TriggerNo
        {
            get { return _triggerNo; }
            set { SetProperty(ref _triggerNo, value); }
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (parameters.TryGetValue<long>("EncoderValue", out var value))
            {
                Position = value;
            }
        }

        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add(nameof(Name), Name);
            result.Parameters.Add(nameof(Position), Position);
            result.Parameters.Add(nameof(EncoderNo), EncoderNo);
            result.Parameters.Add(nameof(TriggerNo), TriggerNo);
        }

    }
}
