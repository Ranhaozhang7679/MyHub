using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class ClassDialogVM : MotionDialogVM
    {
        public ClassDialogVM()
        {
            StartTime=DateTime.Now.AddHours(-8);
            EndTime=DateTime.Now;
        }

        /// <summary>
        /// 班别
        /// </summary>
        private string _className;
        [Required]
        public string ClassName
        {
            get { return _className; }
            set { SetProperty(ref _className, value); }
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime _startTime;
        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        private DateTime _endTime;
        public DateTime EndTime
        {
            get { return _endTime; }
            set { SetProperty(ref _endTime, value); }
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add("ClassName", ClassName);
            result.Parameters.Add("StartTime", StartTime);
            result.Parameters.Add("EndTime", EndTime);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var tvalue))
            {
                Title = tvalue;
                if (Title == Luster.Motion.Assests.Langs.LangProvider.GetLang("EditClass"))
                {
                    if (parameters.TryGetValue<string>("ClassName", out var value))
                    {
                        ClassName = value;
                    }
                    else
                    {
                        ClassName = string.Empty;
                    }
                    if (parameters.TryGetValue<DateTime>("StartTime", out var startTimevalue))
                    {
                        StartTime = startTimevalue;
                    }
                    else
                    {
                        StartTime = DateTime.Now;
                    }
                    if (parameters.TryGetValue<DateTime>("EndTime", out var endtimevalue))
                    {
                        EndTime = endtimevalue;
                    }
                    else
                    {
                        EndTime = DateTime.Now;
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
