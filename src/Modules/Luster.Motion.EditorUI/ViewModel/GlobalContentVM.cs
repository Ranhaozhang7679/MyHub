#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GlobalContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       GlobalContentVM.cs
* 创建时间:       2022/7/4 18:02:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      776e3f37-2a55-4987-8992-1c768682bc7a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/4 18:02:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion
using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.EditorUI.Models;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.ViewModel
{
    public class GlobalContentVM : MotionPageVM
    {
        private IDialogService _dialog;

        private FlowBus eventBus;

        public GlobalContentVM(FlowBus _eventBus, ICommonBus commonBus, IDialogService dialogService) : base(commonBus)
        {
            eventBus = _eventBus;
            _dialog = dialogService;
        }

        /// <summary>
        /// 构建变量
        /// </summary>
        private void BuildGlobals(string key = "")
        {
            VarDatas = new ObservableCollection<GlobalVar>();
            RefModules = new List<IMotionModule>();
            var gModule = GetGlobal();
            foreach (var item in gModule.Parameters)
            {
                item.Value.IsSensitive = true;
                item.Value.IsBindUI = true;
                if (!item.Value.Visible) continue;
                if (string.IsNullOrEmpty(key))
                {
                    VarDatas.Add(new GlobalVar(item.Value));
                }
                else
                {
                    if (item.Value.Alias.ToLower().Contains(key.ToLower()))
                    {
                        VarDatas.Add(new GlobalVar(item.Value));
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
            return eventBus.GetModule(id);
        }

        /// <summary>
        /// 模块移除
        /// </summary>
        private DelegateCommand _addCommand;
        public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(() =>
        {
            var module = GetGlobal();
            _dialog.ShowAddInParam(module, null, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    commonBus.IsNeedSave = true;
                }

                BuildGlobals();
                commonBus.EventBus.GetEvent<GlobalVarEvent>().Publish();
            });
        }));


        /// <summary>
        /// 模块移除
        /// </summary>
        private DelegateCommand _refreshCommand;
        public DelegateCommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new DelegateCommand(() =>
        {
            BuildGlobals();
            commonBus.EventBus.GetEvent<GlobalVarEvent>().Publish();

        }));

        /// <summary>
        /// 模块移除
        /// </summary>
        private DelegateCommand<object> _searchCommand;
        public DelegateCommand<object> SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand<object>((obj) =>
        {
            string key = obj?.ToString();
            BuildGlobals(key);
        }));


        /// <summary>
        /// 模块移除
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is GlobalVar gVar)
            {
                var refModules = eventBus.GetRefGlobalModules(gVar.Parameter);

                if (refModules.Count > 0)
                {
                    throw new FriendlyException($"全局变量被模块:{string.Join(",", refModules.Select(u => u.Alias))} 引用，无法删除!");
                }

                _dialog.ShowConfirm($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("ConfirmDeleteVar")}:{gVar.Name}?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        VarDatas.Remove(gVar);
                        GetGlobal().Parameters.Remove(gVar.Key);
                        commonBus.IsNeedSave = true;
                        commonBus.EventBus.GetEvent<GlobalVarEvent>().Publish();
                    }
                });
            }
        }));

        /// <summary>
        /// 模块移除
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is GlobalVar gVar)
            {
                _dialog.ShowAddInParam(gVar.Parameter.Owner, gVar.Parameter, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        commonBus.IsNeedSave = true;
                    }

                    BuildGlobals();
                    commonBus.EventBus.GetEvent<GlobalVarEvent>().Publish();
                });
            }
        }));

        /// <summary>
        /// 引用Title
        /// </summary>
        private string _refTitle;
        public string RefTitle
        {
            get { return _refTitle; }
            set { SetProperty(ref _refTitle, value); }
        }

        /// <summary>
        /// 重新加载
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            BuildGlobals();
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
        /// 模块移除
        /// </summary>
        private DelegateCommand<GlobalVar> _selectedCommand;
        public DelegateCommand<GlobalVar> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<GlobalVar>((gV) =>
        {
            if (gV != null)
            {
                RefModules = eventBus.GetRefGlobalModules(gV.Parameter);
                RefTitle = $"{gV.Parameter.CN} {Luster.Motion.Assests.Langs.LangProvider.GetLang("Reference(DoubleClick)")}";
            }
        }));

        /// <summary>
        /// 变量列表
        /// </summary>
        private List<IMotionModule> _refModules;
        public List<IMotionModule> RefModules
        {
            get { return _refModules; }
            set { SetProperty(ref _refModules, value); }
        }

        /// <summary>
        /// 模块移除
        /// </summary>
        private DelegateCommand<IMotionModule> _refSelectedCommand;
        public DelegateCommand<IMotionModule> RefSelectedCommand => _refSelectedCommand ?? (_refSelectedCommand = new DelegateCommand<IMotionModule>((m) =>
        {
            if (m != null)
            {
                // 切换到模块所在层级
                eventBus.OnLoaded(m.Parent, m);
                eventBus.OnSelected(m);
            }
        }));

        /// <summary>
        /// 变量导出
        /// </summary>
        private DelegateCommand _exportCommand;
        public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(() =>
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "全局变量|*.xlsx";
            if (saveDialog.ShowDialog() == true)
            {
                string file = saveDialog.FileName;
                List<GlobalVarData> data = new List<GlobalVarData>();
                foreach (var item in VarDatas)
                {
                    data.Add(new GlobalVarData()
                    {
                        ParamName = item.Name,
                        DataType = item.TypeName,
                        DefaultV = item.DefaultV?.ToString(),
                        Value = item.Value?.ToString(),
                        IsHomeDefault = item.IsHomeDefault,
                        IsMemoric=item.IsMemoric,
                    }) ;
                }

                var excel = new Luster.Common.Tools.ExcelTool(file);
                excel.Save(data);
            }
        }));

        /// <summary>
        /// 变量导入
        /// </summary>
        private DelegateCommand _importCommand;
        public DelegateCommand ImportCommand => _importCommand ?? (_importCommand = new DelegateCommand(() =>
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "全局变量|*.xlsx";
            if (openDialog.ShowDialog() == true)
            {
                string file = openDialog.FileName;

                if (!File.Exists(file))
                {
                    throw new FriendlyException($"文件:{file}不存在!");
                }
                var excel = new ExcelTool(file, false);
                var gDatas = excel.GetListBySheet<GlobalVarData>();
                foreach (var item in gDatas)
                {
                    if (string.IsNullOrEmpty(item.ParamName))
                    {
                        continue;
                    }

                    var existParam = VarDatas.FirstOrDefault(u => u.Name == item.ParamName);
                    if (existParam != null)
                    {
                        try
                        {
                            // 更新参数值
                            existParam.Parameter.Value = Convert.ChangeType(item.Value, existParam.Parameter.Type);
                            existParam.Parameter.DefaultV = Convert.ChangeType(item.DefaultV, existParam.Parameter.Type);
                            existParam.Parameter.IsMemoric=item.IsMemoric;
                            existParam.Parameter.IsHomeDefault = item.IsHomeDefault;
                        }
                        catch (Exception ex)
                        {
                            commonBus.OnLog(LogType.Error, $"参数:{item.ParamName} 值解析错误:{ex.Message}");
                            continue;
                        }
                    }
                    else
                    {
                        Type pType = ParameterAttribute.GetTypeByDataType(item.DataType);
                        if (pType == null) continue;

                        var gModule = GetGlobal();

                        // 新建全局变量
                        var p = ParameterAttribute.CreateByType($"Extend_{item.ParamName}", pType, gModule, item.ParamName);
                        try
                        {
                            // 更新参数值
                            p.Value = Convert.ChangeType(item.Value, pType);
                            p.DefaultV = Convert.ChangeType(item.DefaultV, pType);
                            p.IsMemoric = item.IsMemoric;
                            p.IsHomeDefault= item.IsHomeDefault;
                        }
                        catch (Exception ex)
                        {
                            commonBus.OnLog(LogType.Error, $"参数:{item.ParamName} 值解析错误:{ex.Message}");
                            continue;
                        }

                        gModule.Parameters.Add(p.Name, p);
                    }

                    BuildGlobals();
                    commonBus.EventBus.GetEvent<GlobalVarEvent>().Publish();
                }
            }
        }));


        private DelegateCommand _exportToTxtCommand;

        public DelegateCommand ExportToTxtCommand => _exportToTxtCommand ?? (_exportToTxtCommand = new DelegateCommand(() =>
        {
            var saveFile = new SaveFileDialog();
            var result = saveFile.ShowDialog().Value;
            saveFile.Filter = "Txt|*.txt";
            if (result)
            {
                System.IO.TextWriter writer = new System.IO.StreamWriter(saveFile.FileName);
                StringBuilder sb = new StringBuilder();
                foreach(var item in VarDatas) 
                {
                    sb.AppendLine(item.Name+","+item.TypeName+","+item.Value);
                }
                writer.WriteLine(sb.ToString());
                Thread.Sleep(1);
                writer.Flush();
                Thread.Sleep(1);
                writer.Close();
            }
        }));
    }

    /// <summary>
    /// 用于构建导入
    /// </summary>
    class GlobalVarData
    {
        [DisplayName("参数名称")]
        public string ParamName { get; set; }

        [DisplayName("参数值")]
        public string Value { get; set; }

        [DisplayName("默认值")]
        public string DefaultV { get; set; }

        [DisplayName("参数类型")]
        public string DataType { get; set; }

        [DisplayName("回零恢复默认值")]
        public bool IsHomeDefault { get; set; }

        [DisplayName("参数记忆")]
        public bool IsMemoric {  get; set; }
    }
}