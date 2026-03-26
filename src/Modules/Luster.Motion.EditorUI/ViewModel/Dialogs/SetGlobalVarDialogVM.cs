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

namespace Luster.Motion.EditorUI.ViewModel.Dialogs
{
    public class SetGlobalVarDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 模块引擎
        /// </summary>
        private IMotionEngine _engine;

        private IMotionController _mController;

        public SetGlobalVarDialogVM(IMotionEngine motionEngine,IMotionController mController)
        {
            _engine = motionEngine;
            _mController = mController;
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
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            var SelectGlobalVars = VarDatas.Where(u => u.Visible == true).ToList();
            result.Parameters.Add("GlobalVars", SelectGlobalVars);
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

        }

        /// <summary>
        /// 工站是否显示
        /// </summary>
        private DelegateCommand<GlobalVar> _checkCommand;
        public DelegateCommand<GlobalVar> CheckCommand => _checkCommand ?? (_checkCommand = new DelegateCommand<GlobalVar>((module) =>
        {
            if (module != null)
            {
            }
        }));
    }
}
