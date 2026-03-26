#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       KeyValueModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       KeyValueModel.cs
* 创建时间:       2022/9/2 17:33:10
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      c27c6622-7773-4594-a872-ab2e429d0971
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/2 17:33:10
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Models
{
    public class KeyValueModel : BindableBase
    {
        /// <summary>
        /// 班别
        /// </summary>
        private string _key;
        public string Key
        {
            get { return _key; }
            set { SetProperty(ref _key, value); }
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        private object _val;
        public object Value
        {
            get { return _val; }
            set { SetProperty(ref _val, value); }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        private string _desc;
        public string Desc
        {
            get { return _desc; }
            set
            {
                SetProperty(ref _desc, value);
            }
        }
    }

    public class StationLog : BindableBase
    {
        private bool _useLog;
        public bool UseLog
        {
            get
            {
                return _useLog;
            }
            set
            {
                SetProperty(ref _useLog, value);
                if (Tag != null)
                {
                    Tag.UseLog = value;
                }
            }
        }

        public string StationName { get; set; }

        public IStation Tag { get; set; }

        public IMotionModule Module { get; set; }

        public StationLog(IMotionModule module)
        {
            Module = module;
            Tag = module.TaskFunction as IStation;
            StationName = Tag.Station;
            UseLog = Tag.UseLog;
        }
    }
}