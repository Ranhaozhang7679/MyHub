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
using HandyControl.Tools.Extension;
using Luster.Common.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.TaskFlow.Engine;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Luster.Motion.Integration.Web;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Common.Tools;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{


    public class HiveDialogVM : MotionDialogVM
    {
        private HiveAPI hiveAPI;
        private VisionAPI visionAPI;
        private HiveAPI.ERepiarType repiarType = HiveAPI.ERepiarType.Unplaned;
        public event Action<Brush> HiveRepairEndEvent;

        /// <summary>
        /// 手动维修开始
        /// </summary>
        private DelegateCommand _startCommand;
        public DelegateCommand StartCommand => _startCommand ?? (_startCommand = new DelegateCommand(() =>
        {
            this.CloseDialog("start");
        }));


        /// <summary>
        /// 按钮选中,停机原因
        /// </summary>
        private DelegateCommand<object> _checkCommand;
        public DelegateCommand<object> CheckCommand => _checkCommand ?? (_checkCommand = new DelegateCommand<object>((o) =>
        {
            if (o != null)
            {
                if ((String)o == "设备故障")
                {
                    repiarType = HiveAPI.ERepiarType.Unplaned;
                }
                else if ((String)o == "预防性维护")
                {
                    repiarType = HiveAPI.ERepiarType.Planed;
                }
            }
        }));


        protected override void CloseDialog(string parameter)
        {
            base.CloseDialog(parameter);
            if (parameter == "start")
            {
                _dialogService.ShowHiveDialog("设备保养", r =>
                {
                });
            }
        }



        /// <summary>
        /// 维修原因
        /// </summary>
        private List<HiveDownReason> _repairReason;
        public List<HiveDownReason> RepairReasons
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
        private IDialogService _dialogService;

        private ICommonBus _commonbus;

        public HiveDialogVM(IMotionController motionController, HiveAPI _hiveApi, IDialogService dialogService, VisionAPI _visionAPI, ICommonBus commonBus)
        {
            _motionController = motionController;
            hiveAPI = _hiveApi;
            _dialogService = dialogService;
            visionAPI = _visionAPI;
            _commonbus = commonBus;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            RepairReasons = _motionController.SysConfig.
              OperationTips.
              Where(x => x.Operation == SystemOperation.Repair).Select(u => new HiveDownReason()
              {
                  Text = u.Tip,
                  IsSelected = false
              }).ToList();
        }

        protected override void Ok(IDialogResult result)
        {
            //var repair = RepairReasons.FirstOrDefault(u => u.IsSelected);
            //if(repair != null) 
            // { 
            //    Memo = repair.Text;
            // }
            //else
            //{
            //    Memo = "";
            //}
            //if (repair != null)
            //{
            result.Parameters.Add("Type", Memo);
            visionAPI.MachineCall(Memo);

            //主页面变回原来颜色
            _commonbus.EventBus.GetEvent<HiveRepairEvent>().Publish(false);

            //HiveRepairEndEvent?.Invoke(Brushes.Gray);

            //蜂鸣取消
            LogTool.Warn("蜂鸣器关闭");

            _motionController.SetBuzzer(false, 1000, 10000);




            //Memo = repair.Text;
            //}
            //else
            //{
            //    if (!IsOther)
            //    {
            //        throw new FriendlyException("请选择停机原因");
            //    }

            //    if ((string.IsNullOrEmpty(Memo) || Memo.Length < 5))
            //    {
            //        throw new FriendlyException("其它原因不能为空,并且字符长度必须大于5");
            //    }
            //}

            //result.Parameters.Add("Memo", Memo);
        }
    }

    public class HiveDownReason : BindableBase
    {
        /// <summary>
        /// 备注提示
        /// </summary>
        private string _text;
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
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
