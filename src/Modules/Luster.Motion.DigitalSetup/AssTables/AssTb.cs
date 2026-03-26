using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using Prism.Mvvm;

namespace Luster.Motion.DigitalSetup.AssTables
{
    public class AssTb : BindableBase
    {
        private long _项序 ;
        private string _项次;
        private string _标准;
        private string _实测;
        private string _状态; 
        private DateTime _完成时间;

        /// <summary>
        /// 项序
        /// </summary>
        [Column(IsPrimary = true)]
        public long 项序 
        {
            get { return _项序; }
            set { SetProperty(ref _项序, value); }
        }
        /// <summary>
        /// 项次
        /// </summary>
        public string 项次
        {
            get { return _项次; }
            set { SetProperty(ref _项次, value); }
        }

        /// <summary>
        /// 标准
        /// </summary>
        public string 标准
        {
            get { return _标准; }
            set { SetProperty(ref _标准, value); }
        }

        /// <summary>
        /// 实测
        /// </summary>
        public string 实测
        {
            get { return _实测; }
            set { SetProperty(ref _实测, value); }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public string 状态
        {
            get { return _状态; }
            set { SetProperty(ref _状态, value); }
        }

        /// <summary>
        /// 完成时间
        /// </summary>
        [Column(IsNullable = true)]
        public DateTime 完成时间
        {
            get { return _完成时间; }
            set { SetProperty(ref _完成时间, value); }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public AssTb()
        {
            完成时间 = DateTime.Now;
        }
    }
}
