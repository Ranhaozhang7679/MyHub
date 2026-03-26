#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GlobalVarContentVM
* 机器名称:       L05590
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       GlobalVarContentVM.cs
* 创建时间:       2022/11/7 9:57:43
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      a12c5d31-548c-4086-9b0b-bbf5a1d945a8
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/7 9:57:43
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class GlobalVarContentVM : MotionVM,IDockContent
    {
        /// <summary>
        /// 对话框服务
        /// </summary>
        private IDialogService _dialogService;

        private ICommonBus _commonBus;

        private IMotionController _motionController;

        private IMotionEngine _engine;

        public GlobalVarContentVM(ICommonBus commonBus, IMotionController motionController, IMotionController mController, IMotionEngine motionEngine, IDialogService dialogService) : base(commonBus)
        {
            _dialogService = dialogService;
            _commonBus = commonBus;
            _motionController = motionController;
            _engine = motionEngine;
            BuildGlobals();
        }

        public GlobalVarContentVM() { }

        /// <summary>
        /// 启动状态按钮不可点击
        /// </summary>
        private bool _btnEnable = true;
        public bool BtnEnable
        {
            get { return _btnEnable; }
            set { SetProperty(ref _btnEnable, value); }
        }

        /// <summary>
        /// 对应的Key
        /// </summary>
        public string Name => "GlobalVar";

        /// <summary>
        /// 对应的区域
        /// </summary>
        public string RegionName { get; set; } = "GlobalVarContent";


        protected override void RegisterEvent(IEventAggregator bus)
        {
            bus.GetEvent<GlobalVarEvent>().Subscribe(() =>
            {
                BuildGlobals();
            });
            bus.GetEvent<RecipeOpenEvent>().Subscribe((model) =>
            {
                BuildGlobals();
            });
            bus.GetEvent<RunModeChangeEvent>().Subscribe((model) =>
            {
                BuildGlobals(model.SelectGlobalVar);
            });

            bus.GetEvent<SysOperationBtnEvent>().Subscribe((btnobj) =>
            {
                if (btnobj == DataStruct.Enums.SystemOperation.Start || btnobj == DataStruct.Enums.SystemOperation.Recovery)
                {
                    BtnEnable = false;
                }
                else
                {
                    BtnEnable = true;
                }
            });
        }
        /// <summary>
        /// 变量列表
        /// </summary>
        private ObservableCollection<GlobalVar> _varDatas;
        public ObservableCollection<GlobalVar> VarDatas
        {
            get { return _varDatas; }
            set { SetProperty(ref _varDatas, value); }
        }


        /// <summary>
        /// 构建变量
        /// </summary>
        private void BuildGlobals(List<GlobalModel> listGlobalvar=null,string key = "")
        {
            if (listGlobalvar == null)
            {
                listGlobalvar = _motionController.SysConfig.ListGlobalVar;
            }
           
            VarDatas = new ObservableCollection<GlobalVar>();
            var gModule = GetGlobal();
            foreach (var item in gModule.Parameters)
            {
                if (item.Value.Type == typeof(bool))
                {
                    if (!item.Value.Visible) continue;
                    if (string.IsNullOrEmpty(key))
                    {
                        ProcessGlobalVar(listGlobalvar, item);
                    }
                    else
                    {
                        if (item.Value.Alias.ToLower().Contains(key.ToLower()))
                        {
                            ProcessGlobalVar(listGlobalvar, item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IMotionModule GetGlobal()
        {
            var id = Luster.TaskFlow.Motion.Logic.GlobalModule.GlobalID;
            return _engine.Get(id);
        }

        private void ProcessGlobalVar(List<GlobalModel> listGlobalvar, KeyValuePair<string, ParameterAttribute> item)
        {
            var model = new GlobalVar(item.Value);
            if (listGlobalvar != null && listGlobalvar.Count > 0)
            {
                var selectmodel = listGlobalvar.FirstOrDefault(u => u.GlobalKey == model.Key);
                if (selectmodel != null)
                {
                    VarDatas.Add(model);
                }
            }
        }


        /// <summary>
        /// 拼装ListGlobalVar
        /// </summary>
        private void ProcessConfigList()
        {
            _motionController.SysConfig.ListGlobalVar.Clear();
            foreach (var item in VarDatas)
            {
                var model = new GlobalModel()
                {
                    GlobalKey = item.Key,
                    GlobalName = item.Name,
                    IsSelected = true
                };
                _motionController.SysConfig.ListGlobalVar.Add(model);
            }
        }

        /// <summary>
        /// 全局变量选择
        /// </summary>
        private DelegateCommand<GlobalVar> _checkCommand;
        public DelegateCommand<GlobalVar> CheckCommand => _checkCommand ?? (_checkCommand = new DelegateCommand<GlobalVar>((o) =>
        {
            //var model= _motionController.SysConfig.ListGlobalVar.FirstOrDefault(u=>u.GlobalKey==o.Key);
            // model.IsSelected = o.IsChecked;
            _commonBus.IsNeedSave = true;

        }));

        /// <summary>
        /// 全局变量选择
        /// </summary>
        private DelegateCommand _setCommand;
        public DelegateCommand SetCommand => _setCommand ?? (_setCommand = new DelegateCommand(() =>
        {
            _dialogService.ShowGloblaVarSet((r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue("GlobalVars", out List<GlobalVar> listGloblavar))
                    {
                        if (listGloblavar != null)
                        {
                            VarDatas.Clear();
                            listGloblavar.ForEach(u => VarDatas.Add(u));
                            ProcessConfigList();
                        }
                    }
                }
            });
        }));

        /// <summary>
        /// 执行刷新变量操作
        /// </summary>
        private DelegateCommand _refreshCommand;

        public DelegateCommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new DelegateCommand(() =>
        {
            BuildGlobals();
        }));
    }


}