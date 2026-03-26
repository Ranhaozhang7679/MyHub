#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DialogExtension
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Extensions
* 文 件 名:       DialogExtension.cs
* 创建时间:       2022/6/9 15:02:35
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      fe4c5fe3-23d1-43da-a578-dc7ba056ffdb
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/9 15:02:35
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.Extensions
{
    public static class DialogExtension
    {
        private static IDialogWindow _showWindow;

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowAxisMConfig(this IDialogService service, ParameterAttribute pAttr, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);

            DialogParameters param = new DialogParameters();
            param.Add("Title", pAttr.CN);
            param.Add("Parameter", pAttr);
            param.Add("VAxisM", pAttr.Value);

            _showWindow = service.Show("AxisMDialog", param, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowAxisConfig(this IDialogService service, ParameterAttribute pAttr, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);

            DialogParameters param = new DialogParameters();
            param.Add("Title", pAttr.CN);
            param.Add("Parameter", pAttr);
            param.Add("VAxis", pAttr.Value);

            _showWindow = service.Show("AxisConfigDialog", param, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowAxisPosDialog(this IDialogService service, ParameterAttribute pAttr, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);

            DialogParameters param = new DialogParameters();
            param.Add("Title", pAttr.CN);
            param.Add("Parameter", pAttr);
            param.Add("VAxisPos", pAttr.Value);

            _showWindow = service.Show("AxisPosDialog", param, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowStationDialog(this IDialogService service, List<IMotionModule> modules, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);

            DialogParameters param = new DialogParameters();
            param.Add("Title", "Station");
            param.Add("Stations", modules);

            _showWindow = service.Show("StationDialog", param, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowSwitchConfig(this IDialogService service, int selectNum, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);

            DialogParameters param = new DialogParameters();
            param.Add("Title", "Switch");
            param.Add("SelectNum", selectNum);

            _showWindow = service.Show("SwitchDialog", param, callback);
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowArrayConfig(this IDialogService service, ParameterAttribute pAttr, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);

            DialogParameters param = new DialogParameters();
            param.Add("Title", "Array");
            param.Add("Parameter", pAttr);

            _showWindow = service.Show("ArrayDialog", param, callback);
        }

        /// <summary>
        /// 创建项目
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowCreateProject(this IDialogService service, string title, string name, string slnpath, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", title);
            param.Add("ProjectName", name);
            param.Add("ProjectPath", slnpath);
            param.Add("Recipe", name);
            _showWindow = service.Show("CreateProjectDialog", param, callback);
        }

        /// <summary>
        /// 配置Chart输出曲线
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowChartVASet(this IDialogService service, List<ChartDataModel> list, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "设置参数");
            param.Add("ChartList", list);
            _showWindow = service.Show("SetOutPutChartDialog", param, callback);
        }

        public static void ShowModuleSet(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "模块配置");
            _showWindow = service.Show("ModuleConfigureDialog", param, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowGloblaVarSet(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "设置全局变量");
            _showWindow = service.Show("SetGlobalVarDialog", param, callback);
        }

        /// <summary>
        /// 设置运动模式全局变量
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowModeGloblaVarSet(this IDialogService service, List<GlobalModel> list, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "设置全局变量");
            param.Add("SelectModeList", list);
            _showWindow = service.Show("SetModeGlobalVarDialog", param, callback);
        }

        /// <summary>
        /// 设置运行模式
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowSetRunMode(this IDialogService service, RunModeModel Model, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "运行模式");
            param.Add("RunModeModel", Model);
            _showWindow = service.Show("SetRunModeDialog", param, callback);
        }

        /// <summary>
        /// 选择模块
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowSelectModule(this IDialogService service, IMotionModule m, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "选择模块弹窗");
            param.Add("Module", m);
            _showWindow = service.Show("ModuleDialog", param, callback);
        }     
    }
}