#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DialogServiceExntesion
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.Extension
* 文 件 名:       DialogServiceExntesion.cs
* 创建时间:       2022/1/12 16:01:42
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      21081f8d-1dc3-46ed-89c7-5024cc64cb60
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/12 16:01:42
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Interfaces;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.Assets.Langs;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Module;

namespace Luster.Common.Assets
{
    public static class DialogServiceExntesion
    {
        /// <summary>
        /// 记录当前已经打开的Window,用于防止多次弹窗
        /// </summary>
        private static IDialogWindow _showWindow = null;
        private static IDialogWindow _alarmWindow = null;

        /// <summary>
        /// 打开窗体时默认关闭已经存在的窗口，防止同时弹出多个窗口
        /// </summary>
        /// <param name="action"></param>
        public static void CloseExistShowWindow(this IDialogService service, IDialogWindow closeWin = null)
        {
            if (closeWin != null)
            {
                closeWin.Close();
            }
        }


        #region 消息提醒

        public static void ShowErrorTip(this IDialogService service, string message, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang("Error"));
            param.Add("MessageType", "Error");
            param.Add("Message", message);
            param.Add("MsgIcon", "\xea96");

            service.ShowDialog("MessageDialog", param, callback);
        }

        public static void ShowInfoTip(this IDialogService service, string message, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang("Info"));
            param.Add("MessageType", "Info");
            param.Add("Message", message);
            param.Add("MsgIcon", "\xe681");

            service.ShowDialog("MessageDialog", param, callback);
        }

        public static void ShowSuccessTip(this IDialogService service, string message, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang("Success"));
            param.Add("MessageType", "Success");
            param.Add("Message", message);
            param.Add("MsgIcon", "\xea97");

            service.ShowDialog("MessageDialog", param, callback);
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="service"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        public static void ShowConfirm(this IDialogService service, string message, Action<IDialogResult> callback = null, bool bIsBlock = true)
        {
            service.CloseExistShowWindow();

            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang("Confirm"));
            param.Add("MessageType", "Confirm");
            param.Add("Message", message);
            param.Add("MsgIcon", "\xe65d");
            if (bIsBlock)
            {
            service.ShowDialog("MessageDialog", param, callback);
        }
            else
            {
                service.Show("MessageDialog", param, callback);
            }
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="service"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        public static void ShowConfirmWithoutNo(this IDialogService service, string message, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang("Confirm"));
            param.Add("MessageType", "ConfirmWithoutNo");
            param.Add("Message", message);
            param.Add("MsgIcon", "\xe65d");

            service.ShowDialog("MessageDialog", param, callback);
        }

        #endregion

        /// <summary>
        /// 文本Dialog
        /// </summary>
        /// <param name="service"></param>
        /// <param name="title"></param>
        /// <param name="callback"></param>
        public static void ShowText(this IDialogService service, string title, string text, Action<IDialogResult> callback = null)
        {
            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang(title));
            param.Add("Text", text);

            // 弹出一个Dialog，用于用户输入对应的模块名称
            service.ShowDialog("TextDialog", param, callback);
        }


        public static void ShowInfoInput(this IDialogService service, string title, string defaultText = "", Action<IDialogResult> callback = null)
        {
            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang(title));
            param.Add("Text", defaultText);
            // 弹出一个Dialog，用于用户输入对应的模块名称
            service.ShowDialog("InfoInputDialog", param, callback);
        }

        /// <summary>
        /// 弹出表达式对话框
        /// </summary>
        /// <param name="service"></param>
        /// <param name="condition"></param>
        /// <param name="variables"></param>
        /// <param name="callback"></param>
        public static void ShowCondition(this IDialogService service, ParameterAttribute pAttr, List<LNode> variables, string iconStyle = "IconSmall", Action<IDialogResult> callback = null)
        {
            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang("SwitchDialog"));
            param.Add("Parameter", pAttr);
            param.Add("Variables", variables);
            param.Add("IconStyle", iconStyle);

            // 弹出一个Dialog，用于用户输入对应的模块名称
            service.ShowDialog("SwitchDialog", param, callback);
        }

        /// <summary>
        /// 弹出表达式对话框
        /// </summary>
        /// <param name="service"></param>
        /// <param name="condition"></param>
        /// <param name="variables"></param>
        /// <param name="callback"></param>
        public static void ShowStringEx(this IDialogService service, ParameterAttribute pAttr, List<LNode> variables, string iconStyle = "IconSmall", Action<IDialogResult> callback = null)
        {
            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang("StringExDialog"));
            param.Add("Parameter", pAttr);
            param.Add("Variables", variables);
            param.Add("IconStyle", iconStyle);
            // 弹出一个Dialog，用于用户输入对应的模块名称
            service.ShowDialog("StringExDialog", param, callback);
        }

        /// <summary>
        /// 弹出表达式对话框
        /// </summary>
        /// <param name="service"></param>
        /// <param name="condition"></param>
        /// <param name="variables"></param>
        /// <param name="callback"></param>
        public static void ShowStringMatch(this IDialogService service, ParameterAttribute pAttr, Action<IDialogResult> callback = null)
        {
            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang("StringMatchDialog"));
            param.Add("Parameter", pAttr);
            // 弹出一个Dialog，用于用户输入对应的模块名称
            service.ShowDialog("StringMatchDialog", param, callback);
        }

        /// <summary>
        /// 弹出表达式对话框
        /// </summary>
        /// <param name="service"></param>
        /// <param name="pAttr"></param>
        /// <param name="variables"></param>
        /// <param name="isBool">是否是Bool量</param>
        /// <param name="callback"></param>
        public static void ShowExpression(this IDialogService service, ParameterAttribute pAttr, List<LNode> variables, string iconStyle = "IconSmall", bool isBool = true, Action<IDialogResult> callback = null)
        {
            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang("ExpressDialog"));
            param.Add("Parameter", pAttr);
            param.Add("Variables", variables);
            param.Add("isBool", isBool);
            param.Add("IconStyle", iconStyle);
            // 弹出一个Dialog，用于用户输入对应的模块名称
            service.ShowDialog("ExpDialog", param, callback);
        }

        /// <summary>
        /// 弹出表达式对话框
        /// </summary>
        /// <param name="service"></param>
        /// <param name="condition"></param>
        /// <param name="variables"></param>
        /// <param name="callback"></param>
        public static void ShowCalculator(this IDialogService service, string exp, Func<double> getCall, Action<IDialogResult> callback = null)
        {
            DialogParameters param = new DialogParameters();
            param.Add("Title", LangProvider.GetLang("Calculator"));
            param.Add("Express", exp);
            param.Add("Callback", getCall);

            // 弹出一个Dialog，用于用户输入对应的模块名称
            service.ShowDialog("CalcDialog", param, callback);
        }


        /// <summary>
        /// 弹出表达式对话框
        /// </summary>
        /// <param name="service"></param>
        /// <param name="condition"></param>
        /// <param name="variables"></param>
        /// <param name="callback"></param>
        public static void ShowAddInParam(this IDialogService service, IModule module, ParameterAttribute pAttr = null, Action<IDialogResult> callback = null)
        {
            DialogParameters param = new DialogParameters();
            param.Add("Title", "AddInParam");
            param.Add("Module", module);
            param.Add("Parameter", pAttr);

            // 弹出一个Dialog，用于用户输入对应的模块名称
            service.ShowDialog("AddInParamDialog", param, callback);
        }
    }
}