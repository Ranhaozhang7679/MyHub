#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       ModuleContentVM.cs
* 创建时间:       2022/6/8 9:17:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      61ea2e7d-1ecd-478a-aa25-6d2ffc337a79
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/8 9:17:05
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using System.IO;

namespace Luster.Motion.EditorUI.ViewModel
{
    /// <summary>
    /// 模块信息类
    /// </summary>
    public class ModuleInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Function { get; set; }
        public System.Xml.Linq.XElement XmlElement { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Type}.{Function})";
        }
    }

    public class ModuleContentVM : BaseFlowVM
    {
        private IDialogService _dialogService;

        private LNode rootNode;

        private IMotionController _mController;
        public ModuleContentVM(FlowBus _eventBus,
            IModuleFactory factory,
            ICommonBus cBus,
            IDialogService dialogService, IMotionController motionController) : base(_eventBus, cBus)
        {
            rootNode = factory.GetModuleNode(AppConfig.System);
            _dialogService = dialogService;
            BuildModule(rootNode);
            ChangeNode(_eventBus.GetCurrent());
            // 主动判断工具箱是否需要置灰
            _mController = motionController;
            CanUse = (_mController.MotionEngine.EngineStatus != EngineStatus.Running);
            // 设置最后一个分类的标志
            if (Modules != null && Modules.Count > 0)
            {
                Modules.Last().IsLastCategory = true;
            }
        }


        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

            bus.GetEvent<ModuleLoadedEvent>().Subscribe(m =>
            {
                ChangeNode(m);
            });

            // 配方打开
            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                ChangeNode(eventBus.GetCurrent());
                CanUse = true;//在运行过程中切换登录，会触发配方事件，导致可以在运行时新增基元，在CommonBus的OnActiveRecipe中增加判断
            });

            bus.GetEvent<OperationEvent>().Subscribe(op =>
            {
                //CanUse = op.Dst != EngineStatus.Running;
                CanUse = (op.Dst != EngineStatus.Running);
            });

            bus.GetEvent<ModuleCompositionEvent>().Subscribe(r =>
            {
                BuildModule(rootNode);
            });
        }

        /// <summary>
        /// 是否可以使用
        /// </summary>
        private bool _canUse = true;
        public bool CanUse
        {
            get { return _canUse; }
            set { SetProperty(ref _canUse, value); }
        }
        /// <summary>
        /// 导入XML命令
        /// </summary>
        private DelegateCommand _importXmlCommand;
        public DelegateCommand ImportXmlCommand => _importXmlCommand ?? (_importXmlCommand = new DelegateCommand(ExecuteImportXml));

        /// <summary>
        /// 导出XML命令
        /// </summary>
        private DelegateCommand _exportXmlCommand;
        public DelegateCommand ExportXmlCommand => _exportXmlCommand ?? (_exportXmlCommand = new DelegateCommand(ExecuteExportXml));

        private void ChangeNode(IMotionModule m)
        {
            var isDataFlow = m.Name == "Root";
            foreach (var item in Modules)
            {
                if (isDataFlow)
                {
                    if (item.Text == "Stations")
                    {
                        item.IsExpand = true;
                        ExpandedCommand.Execute(item);
                        item.IsEnabled = true;
                    }
                    else
                    {
                        item.IsEnabled = false;
                    }
                }
                else
                {
                    if (item.Text == "Device")
                    {
                        item.IsExpand = true;
                        ExpandedCommand.Execute(item);
                        item.IsEnabled = true;
                    }
                    else if (item.Text == "Stations")
                    {
                        item.IsEnabled = false;
                    }
                    else
                    {
                        item.IsEnabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// 解析成模块
        /// </summary>
        private void BuildModule(LNode node)
        {
            Modules = new ObservableCollection<ModuleNode>();

            var parentNode = new ModuleNode(node);

            // 将LNode 转换为ModuleNode
            foreach (var item in node.Children)
            {
                var newNode = new ModuleNode(item);
                newNode.Parent = parentNode;
                foreach (var subItem in item.Children)
                {
                    var subModule = new ModuleNode(subItem);
                    subModule.Parent = newNode;
                    newNode.Childrens.Add(subModule);
                }

                parentNode.Childrens.Add(newNode);
                Modules.Add(newNode);
            }

            // 自定义组
            ModuleNode compNode = new ModuleNode()
            {
                Text = "CustomModule",
                IsExpanded = true,
                Childrens = new ObservableCollection<ModuleNode>()
            };

            // 构建动态组
            foreach (var comp in eventBus.GetCompositions())
            {
                var subModule = new ModuleNode(comp);
                subModule.Parent = compNode;
                compNode.Childrens.Add(subModule);
            }

            Modules.Add(compNode);
            // 设置最后一个分类的标志
            if (Modules != null && Modules.Count > 0)
            {
                // 重置所有分类的IsLastCategory
                foreach (var module in Modules)
                {
                    module.IsLastCategory = false;
                }
                // 设置最后一个分类为true
                Modules.Last().IsLastCategory = true;
            }
        }
        /// <summary>
        /// 执行导入XML
        /// </summary>
        private void ExecuteImportXml()
        {
            // 实现XML导入逻辑
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML文件 (*.xml)|*.xml|所有文件 (*.*)|*.*",
                Title = "导入XML模块"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string xmlFilePath = openFileDialog.FileName;
                // 解析XML并生成模块的逻辑
                ParseXmlAndGenerateModule(xmlFilePath);
            }
        }

        private void ParseXmlAndGenerateModule(string xmlFilePath)
        {
            try
            {
                // 读取XML文件并解析模块信息
                var xImport = System.Xml.Linq.XElement.Load(xmlFilePath);
                var modulesInfo = new List<ModuleInfo>();

                // 解析XML中的所有模块信息
                foreach (var xModule in xImport.Elements("Module"))
                {
                    string moduleName = xModule.Attribute("Alias")?.Value ??
                                       xModule.Attribute("Name")?.Value ?? "未命名模块";
                    string moduleType = xModule.Attribute("Name")?.Value ?? "";
                    string functionName = "";
                    var functionElement = xModule.Element("Function");
                    if (functionElement != null)
                    {
                        functionName = functionElement.Attribute("Name")?.Value ?? "";
                    }

                    modulesInfo.Add(new ModuleInfo
                    {
                        Name = moduleName,
                        Type = moduleType,
                        Function = functionName,
                        XmlElement = xModule
                    });
                }                

                if (modulesInfo.Count == 0)
                {
                    System.Windows.MessageBox.Show("XML文件中没有找到模块", "提示",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 显示模块选择窗口
                var dialog = new Luster.Motion.EditorUI.Views.ModuleSelectDialog(modulesInfo);
                if (dialog.ShowDialog() == true)
                {
                    var selectedModules = dialog.SelectedModules;
                    if (selectedModules.Count > 0)
                    {
                        // 将选中的模块粘贴到当前流程中
                        PasteSelectedModules(selectedModules);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"导入XML失败: {ex.Message}", "导入失败",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 将选中的模块粘贴到流程中
        /// </summary>
        private void PasteSelectedModules(List<ModuleInfo> selectedModules)
        {
            // 创建包含选中模块的XML
            var selectedXml = new System.Xml.Linq.XElement("SelectedModules");
            foreach (var moduleInfo in selectedModules)
            {
                selectedXml.Add(moduleInfo.XmlElement);
            }

            // 粘贴
            eventBus.OnPaste(selectedXml, eventBus.GetCurrent(), -1);
        }

        /// <summary>
        /// 执行导出XML
        /// </summary>
        private void ExecuteExportXml()
        {
            try
            {
                // 获取组合模块数据
                var compositions = eventBus.GetCompositions();
                if (compositions == null || !compositions.Any())
                {
                    MessageBox.Show("没有可导出的组合模块", "提示",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 创建组合模块的XML元素
                var compositionElement = new XElement("Compositions");
                foreach (var comp in compositions)
                {
                    var moduleElement = comp.ExportXml();
                    compositionElement.Add(moduleElement);
                }

                SaveCompositionToFileDirectly(compositionElement);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出组合模块失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 直接保存组合模块到文件
        /// </summary>
        private void SaveCompositionToFileDirectly(XElement compositionElement)
        {
            try
            {
                string orderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(orderPath, "Composition.xml");

                // 检查组合模块是否为空
                if (compositionElement == null || !compositionElement.Elements().Any())
                {
                    MessageBox.Show("组合模块为空，无法导出", "提示",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                XDocument doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XComment("提取组合模块"),
                    compositionElement
                 );
                doc.Save(filePath);

                MessageBox.Show($"组合模块已成功导出到：{filePath}", "导出成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存组合模块失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 数据集
        /// </summary>
        private ObservableCollection<ModuleNode> _modules;
        public ObservableCollection<ModuleNode> Modules
        {
            get { return _modules; }
            set { SetProperty(ref _modules, value); }
        }


        /// <summary>
        /// 模块选择
        /// </summary>
        private DelegateCommand<object> _doubleAddCommand;
        public DelegateCommand<object> DoubleAddCommand => _doubleAddCommand ?? (_doubleAddCommand = new DelegateCommand<object>((args) =>
        {
            if (!IsAdmin) return;

            MouseButtonEventArgs e = args as MouseButtonEventArgs;
            var source = VisualUpwardSearch<Border>(e.Source as DependencyObject);
            var node = (source as Border).DataContext as LNode;
            if (node != null)
            {
                if (e.ClickCount > 1)
                {
                    var module = node.Tag1 as IModule;
                    var function = node.Tag as IFunction;

                    if (function == null && node.Tag.ToString() == "Composition")
                    {
                        var newModule = eventBus.OnAddModule("Logic", "RefGroup");
                        if (newModule.TaskFunction is IRefComposition comp)
                        {
                            comp.SetRefModule(module as IMotionModule);
                        }
                    }
                    else
                    {
                        eventBus.OnAddModule(module.Name, function.Name);
                    }
                }
                else if (e.ClickCount == 1)
                {
                    DataObject dataObj;
                    dataObj = new DataObject(node);
                    DragDrop.DoDragDrop(source, dataObj, DragDropEffects.Move);
                }
            }
        }));


        /// <summary>
        /// 模块选择
        /// </summary>
        private DelegateCommand<object> _expandCommand;
        public DelegateCommand<object> ExpandedCommand => _expandCommand ?? (_expandCommand = new DelegateCommand<object>((obj) =>
        {
            ModuleNode node = obj as ModuleNode;
            if (node.IsExpand && node.Parent != null)
            {
                // 关闭其他Expanded
                foreach (var item in node.Parent.Childrens)
                {
                    item.IsExpand = node == item;
                }
            }
        }));


        /// <summary>
        /// 模块选择
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        {
            IMotionModule module = obj as IMotionModule;
            if (module != null)
            {
                _dialogService.ShowConfirm($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("DeleteCustomModule")}:{module.Alias}?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        eventBus.OnRemoveCompositions(module);
                    }
                });
            }
        }));


        /// <summary>
        /// 搜索父节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }
    }
}