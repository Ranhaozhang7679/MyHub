using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class PlcAlarmDialogVM : MotionDialogVM
    {
        public PlcAlarmDialogVM()
        {
        }

        /// <summary>
        /// 报警值
        /// </summary>
        private int _address;
        [Required]
        public int Address
        {
            get { return _address; }
            set { SetProperty(ref _address, value); }
        }

        /// <summary>
        /// 报警内容
        /// </summary>
        private string _desc;
        [Required]
        public string Desc
        {
            get { return _desc; }
            set { SetProperty(ref _desc, value); }
        }

        /// <summary>
        /// 报警代码
        /// </summary>
        private string _alarmcode;
        [Required]
        public string AlarmCode
        {
            get { return _alarmcode; }
            set { SetProperty(ref _alarmcode, value); }
        }


        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add("Address", Address);
            result.Parameters.Add("Desc", Desc);
            result.Parameters.Add("AlarmCode", AlarmCode);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var tvalue))
            {
                Title = tvalue;
                if (Title == $"{Luster.Motion.Assests.Langs.LangProvider.GetLang("EditPlcAlarm")}！")
                {
                    if (parameters.TryGetValue<int>("Address", out var value))
                    {
                        Address = value;
                    }

                    if (parameters.TryGetValue<string>("Desc", out var desc))
                    {
                        Desc = desc;
                    }
                    else
                    {
                        Desc = string.Empty;
                    }
                    if (parameters.TryGetValue<string>("AlarmCode", out var alarmcode))
                    {
                        AlarmCode = alarmcode;
                    }
                    else
                    {
                        AlarmCode = string.Empty;
                    }
                }
            }
            else
            {
                Title = string.Empty;
            }

        }
    }
}
