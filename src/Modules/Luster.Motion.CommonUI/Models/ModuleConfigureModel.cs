#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleConfigureModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       ModuleConfigureModel.cs
* 创建时间:       2022/11/3 13:48:22
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      6c6e0eb7-de50-4d45-a804-6f1f9cb0394e
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/3 13:48:22
* 修 改 人:		  L05590
************************************************************************************/
#endregion
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Luster.Motion.CommonUI.Models
{
    public class ModuleConfigureModel : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }


        /// <summary>
        /// DockKeyName
        /// </summary>
        private string _dockname = "";
        public string DockName
        {
            get { return _dockname; }
            set { SetProperty(ref _dockname, value); }
        }


        /// <summary>
        /// 控件名称
        /// </summary>
        private string _controlName = "";
        public string ControlName
        {
            get { return _controlName; }
            set { SetProperty(ref _controlName, value); }
        }


        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        ///// <summary>
        ///// 名称
        ///// </summary>
        //private string _region = "";
        //public string Region
        //{
        //    get { return _region; }
        //    set { SetProperty(ref _region, value); }
        //}

        //private static ObservableCollection<ModuleConfigureModel> _moduleConfigures;
        //public static ObservableCollection<ModuleConfigureModel> ModuleConfigures
        //{
        //    get
        //    {
        //        if (_moduleConfigures == null || _moduleConfigures.Count == 0)
        //        {

        //            _moduleConfigures = new ObservableCollection<ModuleConfigureModel>()
        //            {
        //                 //new ModuleConfigureModel() { Name = "日志",IsSelected=true,ControlName="LogContent"},
        //                 new ModuleConfigureModel() { Name =Luster.Motion.Assests.Langs.LangProvider.GetLang("ReportForm"),IsSelected=false,ControlName="ChartConfigureContent"},
        //                 new ModuleConfigureModel() { Name = Luster.Motion.Assests.Langs.LangProvider.GetLang("GlobalVar"),IsSelected=false,ControlName="GlobalVarContent"},
        //                 new ModuleConfigureModel() { Name = Luster.Motion.Assests.Langs.LangProvider.GetLang("WorkFlow"),IsSelected=false,ControlName="WorkFlowContent"},
        //                 new ModuleConfigureModel(){Name =  Luster.Motion.Assests.Langs.LangProvider.GetLang("DeviceMonitor"),IsSelected=false,ControlName="HeartMonitorContent"}
        //            };
        //        }
        //        else if (_moduleConfigures.Count == 0)
        //        {
        //            _moduleConfigures = new ObservableCollection<ModuleConfigureModel>()
        //            {
        //                 //new ModuleConfigureModel() { Name = "日志",IsSelected=true,ControlName="LogContent"},
        //                 new ModuleConfigureModel() { Name =Luster.Motion.Assests.Langs.LangProvider.GetLang("ReportForm"),IsSelected=false,ControlName="ChartConfigureContent"},
        //                 new ModuleConfigureModel() { Name = Luster.Motion.Assests.Langs.LangProvider.GetLang("GlobalVar"),IsSelected=false,ControlName="GlobalVarContent"},
        //                 new ModuleConfigureModel() { Name = Luster.Motion.Assests.Langs.LangProvider.GetLang("WorkFlow"),IsSelected=false,ControlName="WorkFlowContent"},
        //                 new ModuleConfigureModel() { Name = Luster.Motion.Assests.Langs.LangProvider.GetLang("DeviceMonitor"), IsSelected=false,ControlName="HeartMonitorContent"}
        //            };
        //        }
        //        return _moduleConfigures;
        //    }
        //}
    }
}