using Luster.Common.DataStruct;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Luster.Motion.EditorUI.ViewModel.Dialogs
{
    public class SetRunModeDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 模块引擎
        /// </summary>
        private IMotionEngine _engine;

        private IMotionController _mController;

        public SetRunModeDialogVM(IMotionEngine motionEngine, IMotionController mController)
        {
            _engine = motionEngine;
            _mController = mController;
            SelectVarDatas = new ObservableCollection<GlobalVar>();
            OrignVarDatas = new ObservableCollection<GlobalVar>();
        }

        /// <summary>
        /// 新增模式名称
        /// </summary>
        private string _modeName;
        public string ModeName
        {
            get { return _modeName; }
            set
            {
                SetProperty(ref _modeName, value);
            }
        }

        /// <summary>
        /// 是否启用模式
        /// </summary>
        private bool _isEnable;
        public bool IsEnable
        {
            get { return _isEnable; }
            set
            {
                SetProperty(ref _isEnable, value);
            }
        }

        /// <summary>
        /// 当前运行模式
        /// </summary>
        private RunModeModel _curRunModel = null;

        /// <summary>
        /// 左侧变量集
        /// </summary>
        private ObservableCollection<GlobalVar> _orignvarDatas;
        public ObservableCollection<GlobalVar> OrignVarDatas
        {
            get { return _orignvarDatas; }
            set { SetProperty(ref _orignvarDatas, value); }
        }

        /// <summary>
        /// 右侧变量集
        /// </summary>
        private ObservableCollection<GlobalVar> _selectVarDatas;
        public ObservableCollection<GlobalVar> SelectVarDatas
        {
            get { return _selectVarDatas; }
            set { SetProperty(ref _selectVarDatas, value); }
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

        /// <summary>
        /// 选择全局变量
        /// </summary>
        private DelegateCommand<object> _reciveDropCommand;
        public DelegateCommand<object> ReciveDropCommand => _reciveDropCommand ?? (_reciveDropCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj == null) { return; }
            var source = obj as DragEventArgs;
            if (source != null)
            {
                var node = source.Data.GetData(typeof(GlobalVar)) as GlobalVar;
                if (node != null)
                {
                    var mode = OrignVarDatas.FirstOrDefault(u => u.Key == node.Key);
                    if (mode != null)
                    {
                        SelectVarDatas.Add(mode);
                        OrignVarDatas.Remove(mode);
                    }
                }
            }
        }));

        /// <summary>
        /// 撤回选择
        /// </summary>
        private DelegateCommand<object> _recallDropCommand;
        public DelegateCommand<object> ReCallDropCommand => _recallDropCommand ?? (_recallDropCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj == null) { return; }
            var source = obj as DragEventArgs;
            if (source != null)
            {
                var node = source.Data.GetData(typeof(GlobalVar)) as GlobalVar;
                if (node != null)
                {
                    var mode = SelectVarDatas.FirstOrDefault(u => u.Key == node.Key);
                    if (mode != null)
                    {
                        OrignVarDatas.Add(mode);
                        SelectVarDatas.Remove(mode);
                    }
                }
            }
        }));

        /// <summary>
        /// 删除选择
        /// </summary>
        private DelegateCommand<object> _deleteCommand;
        public DelegateCommand<object> DeleteCommand => _deleteCommand ?? (_deleteCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj == null) { return; }

            var node = obj as GlobalVar;
            if (node != null)
            {
                var mode = SelectVarDatas.FirstOrDefault(u => u.Key == node.Key);
                if (mode != null)
                {
                    OrignVarDatas.Add(mode);
                    SelectVarDatas.Remove(mode);
                }
            }
        }));

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);

            if (SelectVarDatas != null)
            {
                var selectList = new List<GlobalModel>();
                foreach (var varData in SelectVarDatas)
                {
                    var modelvar = new GlobalModel()
                    {
                        GlobalKey = varData.Key,
                        GlobalName = varData.Name,
                        IsSelected = varData.IsChecked
                    };
                    selectList.Add(modelvar);
                }

                if (_curRunModel == null)
                {
                    _curRunModel = new RunModeModel();
                }

                _curRunModel.Mode = ModeName;
                _curRunModel.SelectGlobalVar = selectList;
                _curRunModel.IsRunMode = IsEnable;

                result.Parameters.Add("RunModeModel", _curRunModel);
            }
        }

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

            if (parameters.TryGetValue<RunModeModel>("RunModeModel", out var model))
            {
                if (model != null)
                {
                    _curRunModel = model;
                    ModeName = model.Mode;
                    IsEnable = model.IsRunMode;
                }

                BuildLeftTreeVars(model);
                BuildRightVars(model);
            }
        }

        private void BuildLeftTreeVars(RunModeModel runModel)
        {
            var g = GetGlobal();
            OrignVarDatas = new ObservableCollection<GlobalVar>();
            foreach (var item in g.Parameters)
            {
                if (item.Value.Type == typeof(bool))
                {
                    // 右侧树有树就排除掉
                    if (runModel != null && runModel.SelectGlobalVar.Any(u => u.GlobalKey == item.Key)) continue;

                    OrignVarDatas.Add(new GlobalVar()
                    {
                        IsChecked = false,
                        Key = item.Key,
                        Name = item.Value.CN
                    });
                }
   
            }
        }

        /// <summary>
        /// 构建右侧树
        /// </summary>
        /// <param name="runModel"></param>
        private void BuildRightVars(RunModeModel runModel)
        {
            SelectVarDatas = new ObservableCollection<GlobalVar>();
            if (runModel == null) return;

            foreach (var item in runModel.SelectGlobalVar)
            {
                SelectVarDatas.Add(new GlobalVar()
                {
                    Key = item.GlobalKey,
                    IsChecked = item.IsSelected,
                    Name = item.GlobalName,
                });
            }
        }
    }
}
