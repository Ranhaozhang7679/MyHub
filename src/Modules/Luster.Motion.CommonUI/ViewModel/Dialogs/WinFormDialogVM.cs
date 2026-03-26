#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WinFormDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.ViewModel.Dialogs
* 文 件 名:       WinFormDialogVM.cs
* 创建时间:       2022/9/21 19:53:04
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      db7495ab-49fb-44b4-b61e-a8b14f409911
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/21 19:53:04
* 修 改 人:		  L05123
************************************************************************************/
#endregion

//using FinSensor;
using Luster.Common.DataStruct;
using Luster.Common.Tools;
using Luster.TaskFlow.Motion;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class WinFormDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 模块引擎
        /// </summary>

        public WinFormDialogVM()
        {
        }

        /// <summary>
        /// 工站集
        /// </summary>
        //private ObservableCollection<IMotionModule> _stations;
        //public ObservableCollection<IMotionModule> Stations
        //{
        //    get { return _stations; }
        //    set { SetProperty(ref _stations, value); }
        //}

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            //result.Parameters.Add("Stations", Stations);
        }

        //private System.Windows.Forms.Form winForm = null;

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var tvalue))
            {
                Title = tvalue;
            }
            else
            {
                Title = string.Empty;
            }

            //try
            //{
            //    if (parameters.TryGetValue<string>("FormAssembly", out var frmAssembly))
            //    {
            //        string folder = Path.Combine(Luster.Common.Tools.ReflectionTool.GetAssemblyPath(), "FormTools");
            //        if (!Directory.Exists(folder))
            //        {
            //            Directory.CreateDirectory(folder);
            //        }

            //        if (!frmAssembly.Contains(".dll"))
            //        {
            //            frmAssembly = $"{frmAssembly}.dll";
            //        }

            //        // 动态加载程序集
            //        var path = Path.Combine(folder, frmAssembly);

            //        if (File.Exists(path))
            //        {
            //            var forms = Common.Tools.ReflectionTool.GetByAssembly<System.Windows.Forms.Form>(path);
            //            if (forms != null && forms.Count > 0)
            //            {
            //                winForm = forms[0];
            //            }
            //        }
            //        else
            //        {
            //            throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("AssemblyNotFound")}:{path}");
            //        }
            //    }

            //    if (parameters.TryGetValue<string>("EventName", out var eventName))
            //    {
            //        AddHandler(winForm, eventName);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //LogTool.Error("指纹登录异常", ex);
            //}
        }


        public void AddHandler<T>(T instance, string eventName)
        {
            var eventInfo = instance.GetType().GetEvent(eventName);
            var methodInfo = GetType().GetMethod(nameof(FormEvent));
            var @delegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);
            eventInfo.AddEventHandler(instance, @delegate);
        }


        /// <summary>
        /// 窗体集成Host
        /// </summary>
        //WindowsFormsHost winHost = null;

        /// <summary>
        /// 回调事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="isOk"></param>
        public void FormEvent(object sender, bool isOk)
        {
            if (isOk)
            {
                CloseDialog("ok");
            }
            else
            {
                CloseDialog("cancel");
            }
        }
    }
}