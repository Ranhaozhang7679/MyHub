#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ShowOperationTipDialogVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.ViewModel.Dialogs
* 文 件 名:       ShowOperationTipDialogVM.cs
* 创建时间:       2022/12/1 17:27:45
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      40ac4ac1-d1de-4545-9b0a-930bcbf28638
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/1 17:27:45
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using HandyControl.Controls;
using Luster.Common.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class ShowOperationTipDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 停止原因
        /// </summary>
        private List<DownReason> _stopReason;
        public List<DownReason> StopReasons
        {
            get => _stopReason;
            set => SetProperty(ref _stopReason, value);
        }

        /// <summary>
        /// 维修原因
        /// </summary>
        private List<DownReason> _repairReason;
        public List<DownReason> RepairReasons
        {
            get => _repairReason;
            set => SetProperty(ref _repairReason, value);
        }

        /// <summary>
        /// 其他
        /// </summary>
        private bool _isOther;
        public bool IsOther
        {
            get { return _isOther; }
            set
            {
                SetProperty(ref _isOther, value);
                if (value)
                {
                    foreach (var item in StopReasons)
                    {
                        item.IsSelected = false;
                    }

                    foreach (var item in RepairReasons)
                    {
                        item.IsSelected = false;
                    }
                }
            }
        }

        /// <summary>
        /// 备注提示
        /// </summary>
        private string _memo;
        public string Memo
        {
            get { return _memo; }
            set { SetProperty(ref _memo, value); }
        }

        private IMotionController _motionController;

        public ShowOperationTipDialogVM(IMotionController motionController)
        {
            _motionController = motionController;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            StopReasons = _motionController.SysConfig.
                 OperationTips.
                 Where(x => (x.Operation == SystemOperation.Stop || x.Operation == SystemOperation.Pause)).Select(u => new DownReason()
                 {
                     Text = u.Tip,
                     IsSelected = false
                 }).ToList();

            RepairReasons = _motionController.SysConfig.
              OperationTips.
              Where(x => x.Operation == SystemOperation.Repair).Select(u => new DownReason()
              {
                  Text = u.Tip,
                  IsSelected = false
              }).ToList();
        }

        protected override void Ok(IDialogResult result)
        {
            var stop = StopReasons.FirstOrDefault(u => u.IsSelected);
            var repair = RepairReasons.FirstOrDefault(u => u.IsSelected);

            if (stop != null)
            {
                result.Parameters.Add("Type", stop.Text);
                Memo = stop.Text;
            }
            else if (repair != null)
            {
                result.Parameters.Add("Type", repair.Text);
                Memo = stop.Text;
            }
            else
            {
                if (!IsOther)
                {
                    throw new FriendlyException("请选择停机原因");
                }

                if ((string.IsNullOrEmpty(Memo) || Memo.Length < 5))
                {
                    throw new FriendlyException("其它原因不能为空,并且字符长度必须大于5");
                }
            }

            result.Parameters.Add("Memo", Memo);
        }
    }

    public class DownReason : BindableBase
    {
        /// <summary>
        /// 备注提示
        /// </summary>
        private string _memo;
        public string Text
        {
            get { return _memo; }
            set { SetProperty(ref _memo, value); }
        }

        /// <summary>
        /// 备注提示
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
    }
}
