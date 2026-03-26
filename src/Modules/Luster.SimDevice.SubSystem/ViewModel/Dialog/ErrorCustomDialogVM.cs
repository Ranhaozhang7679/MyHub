using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
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
    public class ErrorCustomDialogVM : DialogVM
    {
        /// <summary>
        /// 报警代码
        /// </summary>
        private string _alarmCode;
        [Required]
        public string AlarmCode
        {
            get => _alarmCode;
            set => SetProperty(ref _alarmCode, value);
        }

        /// <summary>
        /// 报警内容-中文
        /// </summary>
        private string _alarmContent;
        [Required]
        public string AlarmContent
        {
            get => _alarmContent;
            set => SetProperty(ref _alarmContent, value);
        }

        /// <summary>
        /// 报警英文
        /// </summary>
        private string _alarmEnglish;
        [Required]
        public string AlarmEnglish
        {
            get => _alarmEnglish;
            set => SetProperty(ref _alarmEnglish, value);
        }

        protected ErrorCustomDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            
        }

        protected override void Ok(IDialogResult result)
        {
            result.Parameters.Add("AlarmCode", AlarmCode);
            result.Parameters.Add("AlarmContent", AlarmContent);
            result.Parameters.Add("AlarmEnglish", AlarmEnglish);
        }
    }
}
