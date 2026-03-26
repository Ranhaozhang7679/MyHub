#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlarmDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.ViewModel.Dialogs
* 文 件 名:       AlarmDialogVM.cs
* 创建时间:       2022/7/14 13:39:40
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      05460fef-6080-4427-b164-203f27165bf2
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/14 13:39:40
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Logic;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class HomeConfirmDialogVM : MotionDialogVM
    {
        private IDeviceEngine _deviceEngine;
        private IMotionEngine _mEngine;

        /// <summary>
        /// 回零提示
        /// </summary>
        private SolidColorBrush _fontBrush = Brushes.Red;
        public SolidColorBrush FontBrush
        {
            get { return _fontBrush; }
            set { SetProperty(ref _fontBrush, value); }
        }

        /// <summary>
        /// 回零提示
        /// </summary>
        private string _homeTip;
        public string HomeTip
        {
            get { return _homeTip; }
            set { SetProperty(ref _homeTip, value); }
        }

        /// <summary>
        /// 是否支持复位
        /// </summary>
        private bool _canReset = false;
        public bool CanReset
        {
            get { return _canReset; }
            set { SetProperty(ref _canReset, value); }
        }


        public HomeConfirmDialogVM(IDeviceEngine deviceEngine, IMotionEngine mEngine) : base()
        {
            _deviceEngine = deviceEngine;
            _mEngine = mEngine;
        }

        /// <summary>
        /// 弹窗打开
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            HomeTip = $"{_deviceEngine.DeviceMode.GetDescription()}";

            FontBrush = _deviceEngine.DeviceMode == DeviceMode.Virtual ? Brushes.Red : Brushes.Green;

            // 判断机台是否可以进行复位
            bool isAllHome = false;
            try
            {
                isAllHome = _deviceEngine.IsHome(out var msg);
            }
            catch (Exception)
            {
                // 因为弹窗检查异常提示，导致程序无法运行
            }

            // 判断是否有复位工站
            var isReset = _mEngine.Any(u => typeof(IResetStation).IsAssignableFrom(u.TaskFunction.GetType()));

            // 硬件回零了，软件有复位工站
            CanReset = isAllHome && isReset;
        }

        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add("HomeType", HomeType.Home);
        }

        protected override void Cancel(IDialogResult result)
        {
            base.Cancel(result);
            result.Parameters.Add("HomeType", HomeType.Cancel);
        }

        protected override void No(IDialogResult result)
        {
            base.No(result);
            result.Parameters.Add("HomeType", HomeType.Reset);
        }
    }
}