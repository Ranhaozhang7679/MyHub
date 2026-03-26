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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Luster.Motion.EditorUI.ViewModel.Dialogs
{
    public class SetModeGlobalVarDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 模块引擎
        /// </summary>
        private IMotionEngine _engine;

        private IMotionController _mController;

        public SetModeGlobalVarDialogVM(IMotionEngine motionEngine,IMotionController mController)
        {
            _engine = motionEngine;
            _mController = mController;
            SelectVarDatas = new ObservableCollection<GlobalVar>();
            BuildGlobals();
        }

        /// <summary>
        /// 变量集
        /// </summary>
        private ObservableCollection<GlobalVar> _varDatas;
        public ObservableCollection<GlobalVar> VarDatas
        {
            get { return _varDatas; }
            set { SetProperty(ref _varDatas, value); }
        }

        /// <summary>
        /// 选中变量集
        /// </summary>
        private ObservableCollection<GlobalVar> _selectVarDatas;
        public ObservableCollection<GlobalVar> SelectVarDatas
        {
            get { return _selectVarDatas; }
            set { SetProperty(ref _selectVarDatas, value); }
        }


        /// <summary>
        /// 构建变量
        /// </summary>
        private void BuildGlobals(string key = "")
        {
            var listGlobalvar = _mController.SysConfig.ListGlobalVar;
            VarDatas = new ObservableCollection<GlobalVar>();
            var gModule = GetGlobal();
            foreach (var item in gModule.Parameters)
            {
                if (item.Value.Type.Name == "Boolean")
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
                            var model = new GlobalVar(item.Value);
                            VarDatas.Add(model);
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
                    model.Visible = true;
                }
            }
            VarDatas.Add(model);
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
                    var mode = VarDatas.FirstOrDefault(u => u.Key == node.Key);
                    if (mode != null)
                    {
                        SelectVarDatas.Add(mode);
                        VarDatas.Remove(mode);
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
                        VarDatas.Add(mode);
                        SelectVarDatas.Remove(mode);
                    }
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
            result.Parameters.Add("GlobalVars", SelectVarDatas.ToList());
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
            if (parameters.TryGetValue<List<GlobalModel>>("SelectModeList", out List<GlobalModel> selectlist))
            {
                if (selectlist != null)
                {
                    foreach (var item in selectlist)
                    {
                        var model = VarDatas.FirstOrDefault(m=>m.Key==item.GlobalKey);
                        if (model != null)
                        {
                            SelectVarDatas.Add(model);
                            VarDatas.Remove(model);
                        }
                    }
                }

            }

        }
    }
}
