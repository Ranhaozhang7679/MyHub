using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Models
{
    public class ClassModel : BindableBase
    {

        /// <summary>
        /// ID
        /// </summary>
        private long _id;
        public long ID
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        /// <summary>
        /// 班别
        /// </summary>
        private string _classType;
        public string ClassType
        {
            get { return _classType; }
            set { SetProperty(ref _classType, value); }
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
            set
            {
                SetProperty(ref _endTime, value);
            }
        }

        /// <summary>
        /// 是否首班
        /// </summary>
        private bool _firstClass;
        public bool FirstClass
        {
            get { return _firstClass; }
            set
            {
                SetProperty(ref _firstClass, value);
            }
        }
    }
}
