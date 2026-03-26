using Luster.Common.Assets;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Model;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class ControlParaVM : ModuleDialogVM
    {

        /// <summary>
        /// 模块名列表
        /// </summary>
        private ObservableCollection<ModuleModel> moduleList;
        public ObservableCollection<ModuleModel> ModuleList
        {
            get { return moduleList; }
            set { SetProperty(ref moduleList, value); }
        }


        /// <summary>
        /// 参数列表
        /// </summary>
        private ObservableCollection<ControlParaModel> controlParaList;
        public ObservableCollection<ControlParaModel> ControlParaList
        {
            get { return controlParaList; }
            set { SetProperty(ref controlParaList, value); }
        }

        /// <summary>
        /// 模块名
        /// </summary>
        private List<string> modules;

        /// <summary>
        /// 选中模组
        /// </summary>
        private List<string> selectedModules;


        /// <summary>
        /// 对话框
        /// </summary>
        protected IDialogService dialogService;



        private bool _isAllowUpdate = true;
        public bool IsAllowUpdate
        {
            get { return _isAllowUpdate; }
            set { SetProperty(ref _isAllowUpdate, value); }
        }

        protected ControlParaVM(ISimDeviceEngineUI _engine) : base(_engine)
        {

            ModuleList = new ObservableCollection<ModuleModel>();
            ControlParaList=new ObservableCollection<ControlParaModel>();
            modules = deviceEngine.GetModules();
            selectedModules = new List<string>();    
            BuildModuleList();
            UpdateSelectModules();
            BuildParaList(selectedModules);
            dialogService = _engine.Dialog;
        }

        /// <summary>
        /// 构建模块名列表
        /// </summary>
        private void BuildModuleList()
        {
           foreach(var module in modules)
            {
                ModuleModel moduleModel = new ModuleModel();
                moduleModel.IsSelected = true;
                moduleModel.ModuleName = module;
                ModuleList.Add(moduleModel);
                moduleModel.SelectedChanged += ModuleModel_SelectedChanged;
            }
        }

        /// <summary>
        /// 选中事件
        /// </summary>
        /// <param name="selected"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ModuleModel_SelectedChanged(bool selected)
        {
            UpdateSelectModules();
            BuildParaList(selectedModules);

        }

        /// <summary>
        /// 构建控制参数列表
        /// </summary>
        private void BuildParaList(List<string> modules)
        {
            controlParaList?.Clear();
            foreach (var xItem in deviceEngine.LoadControlPara())
            {
                ControlParaModel paraModel = new ControlParaModel();

                string primName="" ;
                xItem.Parent.GetAttribute("Alias",alias=>primName=alias.ToString());
                if(!string.IsNullOrEmpty(primName))
                {
                    xItem.SetAttributeValue("Prim", primName);
                }
                paraModel.ParserXml(xItem);
                if(modules.Contains(paraModel.Module))
                {                            
                    ControlParaList.Add(paraModel);
                    paraModel.ValueChanged -= ParaModel_ValueChanged;
                    paraModel.ValueChanged += ParaModel_ValueChanged;
                }
            }

        }




        /// <summary>
        /// 按钮切换
        /// </summary>
        private DelegateCommand<object> _selectAllCommand;
        public DelegateCommand<object> SelectAllCommand => _selectAllCommand ?? (_selectAllCommand = new DelegateCommand<object>((dgControl) =>
        {
           
            CheckBox checkBox=(CheckBox)dgControl;

            foreach (var item in ModuleList)
            {
                item.IsSelected =  (bool)checkBox.IsChecked;
            }
        }));


        /// <summary>
        /// 更新选中的模块名
        /// </summary>
        private void UpdateSelectModules()
        {
            selectedModules?.Clear();
            foreach (var module in ModuleList)
            {
                if (module.IsSelected)
                    selectedModules.Add(module.ModuleName);
            }
        }


        /// <summary>
        /// 控制参数值变化
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ParaModel_ValueChanged(Guid guid, string name, string value)
        {
            //更新控制参数
            deviceEngine.UpdateControlPara(guid, name, value);  
        }





        public void ConfirmUpdate()
        {

            //dialogService.ShowText("密码","2", r =>
            //{
            //    if (r.Result == ButtonResult.OK)
            //    {
            //        if (r.Parameters.TryGetValue("Text", out string password))
            //        {
            //            if (password == "Luster@1996")
            //                IsAllowUpdate = true;
            //            else
            //                IsAllowUpdate = false;
            //        }
            //    }             
            //}         
            //);


            dialogService.ShowConfirm($"确认更改当前数据?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    IsAllowUpdate = true;
                }
                else
                {
                    IsAllowUpdate = false;
                }
            });
        }

    }

}
  
