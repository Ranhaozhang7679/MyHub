#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlarmItemModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       AlarmItemModel.cs
* 创建时间:       2022/9/9 10:20:49
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      61d67fd0-1303-44ed-aeeb-11f347103631
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/9 10:20:49
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.Virtual;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.Models
{
    public class RunningModuleItemModel : BindableBase
    {
        /// <summary>
        /// 工站名称
        /// </summary>
        private string _stationName = string.Empty;
        public string StationName
        {
            get { return _stationName; }
            set { SetProperty(ref _stationName, value); }
        }

        /// <summary>
        /// 工站ID
        /// </summary>
        private string _stationID = string.Empty;
        public string StationID
        {
            get { return _stationID; }
            set { SetProperty(ref _stationID, value); }
        }

        /// <summary>
        /// 正在运行模块名称
        /// </summary>
        private string _moduleName = string.Empty;
        public string ModuleName
        {
            get { return _moduleName; }
            set { SetProperty(ref _moduleName, value); }
        }

        /// <summary>
        /// 正在运行模块ID
        /// </summary>
        private string _moduleID = string.Empty;
        public string ModuleID
        {
            get { return _moduleID; }
            set { SetProperty(ref _moduleID, value); }
        }

        /// <summary>
        /// 正在运行模块x
        /// </summary>
        private string _moduleAlias = string.Empty;
        public string ModuleAlias
        {
            get { return _moduleAlias; }
            set { SetProperty(ref _moduleAlias, value); }
        }

        public RunningModuleItemModel(string stationName, string stationID,
                                      string moduleName = "", string moduleID = "", string moduleAlias = "")
        {
            StationName = stationName;
            StationID = stationID;
            ModuleName = moduleName;
            ModuleID = moduleID;
            ModuleAlias = moduleAlias;
        }
    }
}
