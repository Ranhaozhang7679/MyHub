using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.ReportUI.Model
{
    public class ReportGetInput : BindableBase
    {
        /// <summary>
        /// 查询关键字
        /// </summary>
        private string _searchkey;
        public string SearchKey
        {
            get => _searchkey;
            set => SetProperty(ref _searchkey, value);
        }

        /// <summary>
        /// 查询关键字
        /// </summary>
        private DateTime _startTime;
        public DateTime StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        /// <summary>
        /// 查询关键字
        /// </summary>
        private DateTime _endTime;
        public DateTime EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }
    }
}
