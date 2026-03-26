using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.SubSystem.ViewModel;
using Luster.Motion.TaskFlow.Engine;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Luster.Motion.SubSystem.Views
{
    /// <summary>
    /// ModuleDisplayContent.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleDisplayContent : UserControl
    {
        public ModuleDisplayContent()
        {
            
            InitializeComponent();
            CreateContent();
            LoadLayout();
            if (DataContext is ModuleDisplayContentVM viewModel)
            {
                viewModel.ModuleChangedEvent -= ViewModel_ModuleChangedEvent;
                viewModel.ModuleChangedEvent += ViewModel_ModuleChangedEvent;
                viewModel.ModuleSaveEvent -= ViewModel_ModuleSaveEvent;
                viewModel.ModuleSaveEvent += ViewModel_ModuleSaveEvent; 
                viewModel.LanguageChangedEvent -= ViewModel_LanguageChangedEvent;
                viewModel.LanguageChangedEvent += ViewModel_LanguageChangedEvent;
            }
        }


        private void CreateContent()
        {
            if (DataContext is ModuleDisplayContentVM vModel)
            {
                layoutDocPane.Children.Clear();
                foreach (var item in vModel.ModuleConfigures)
                {
                    LayoutAnchorable doc = new LayoutAnchorable();
                    if (item.Name == Luster.Motion.Assests.Langs.LangProvider.GetLang("Log"))//日志默认显示且不可被移除
                    {
                        doc.CanHide = false;
                    }
                    else
                    {
                        doc.CanClose = true;
                    }
                    doc.IsMaximized = true;
                    doc.ContentId = item.Name;
                    doc.CanShowOnHover = false;
                    if (item.DockName != null)
                    {
                        doc.Title = Luster.Motion.Assests.Langs.LangProvider.GetLang(item.DockName);
                    }

                   
                    doc.Closed -= Doc_Closed;
                    doc.Closed += Doc_Closed;
                    ContentControl content = new ContentControl();
                    if (item.ControlName != null)
                    {
                        Type type = null;
                        if (item.ControlName == "LogContent")
                        {
                            type = Assembly.LoadFrom("Luster.Motion.CommonUI.dll").GetTypes().Where(x => x.Name.Contains(item.ControlName)).FirstOrDefault();
                        }
                        else
                        {
                            type = GetType().Assembly.GetTypes().Where(x => x.Name.Contains(item.ControlName)).FirstOrDefault();
                        }
                        if (type != null)
                        {
                            var controlcontent = Activator.CreateInstance(type);
                            content.Content = controlcontent;
                            content.Margin = new Thickness(0);
                            //content.MaxHeight = gridDock.Height;
                            doc.Content = content;
                            layoutDocPane.Children.Add(doc);
                        }
                    }
                }
                ModuleDock.Layout.Children.ToList().Clear();
                var layoutpanel = ModuleDock.Layout.Children.ToList()[0] as LayoutPanel;
                layoutpanel.Children.Clear();
                layoutpanel.Children.Add(layoutDocPane);
            }
        }

        /// <summary>
        /// 关闭模块
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Doc_Closed(object sender, EventArgs e)
        {
            var module = sender as LayoutAnchorable;
            var vm = DataContext as ModuleDisplayContentVM;
            if (vm != null)
            {
                vm.DeleteModule = module.ContentId;
            }
        }

        private void ViewModel_ModuleChangedEvent()
        {
            Dispatcher.Invoke(() =>
            {
                CreateContent();
            });
        }

        /// <summary>
        /// 语言切换重新加载
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void ViewModel_LanguageChangedEvent()
        {
            Dispatcher.Invoke(() =>
            {
                CreateContent();
            });
        }


        /// <summary>
        /// 布局保存
        /// </summary>
        private void ViewModel_ModuleSaveEvent()
        {
            Dispatcher.Invoke(() =>
            {
                var vm = DataContext as ModuleDisplayContentVM;
                if (vm != null)
                {
                    XmlLayoutSerializer serializer = new XmlLayoutSerializer(ModuleDock);
                    using (var stream = new StreamWriter(vm.LayOutPath))
                    {
                        serializer.Serialize(stream);
                    }
                }
            });
        }

        /// <summary>
        /// 加载布局
        /// </summary>
        private void LoadLayout()
        {
            
            var vm = DataContext as ModuleDisplayContentVM;
            if (vm != null)
            {
                if (File.Exists(vm.LayOutPath))
                {
                    try
                    {
                        XmlLayoutSerializer serializer = new XmlLayoutSerializer(ModuleDock);
                        using (var stream = new StreamReader(vm.LayOutPath))
                        {
                            serializer.Deserialize(stream);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                var layoutpanel = ModuleDock.Layout.Children.ToList()[0] as LayoutPanel;
                foreach (var item in layoutpanel.Children)
                {
                    if (item is LayoutAnchorablePaneGroup)
                    {
                        var panelgroup = item as LayoutAnchorablePaneGroup;
                        ResgitEvent(panelgroup);
                    }
                    else if (item is LayoutAnchorablePane)
                    {
                        var panelgroup = item as LayoutAnchorablePane;
                        ResgitEvent(panelgroup);
                    }
                }
            }
        }

        /// <summary>
        /// 注册删除事件
        /// </summary>
        private void ResgitEvent(LayoutAnchorablePane pane)
        {
            if (pane != null)
            {
                foreach (var module in pane.Children)
                {
                    if (module is LayoutAnchorable)
                    {
                        var doc = module as LayoutAnchorable;
                        doc.Closed -= Doc_Closed;
                        doc.Closed += Doc_Closed;
                    }
                }
            }
        }

        /// <summary>
        /// 注册删除事件
        /// </summary>
        private void ResgitEvent(LayoutAnchorablePaneGroup panelgroup)
        {
            if (panelgroup != null)
            {
                foreach (var module in panelgroup.Children)
                {
                    if (module is LayoutAnchorablePane)
                    {
                        foreach (var docment in module.Children)
                        {
                            var doc = docment as LayoutAnchorable;
                            doc.Closed -= Doc_Closed;
                            doc.Closed += Doc_Closed;
                        }
                    }
                    if (module is LayoutAnchorablePaneGroup)
                    {
                        var anchorablepanelgroup = module as LayoutAnchorablePaneGroup;
                        ResgitEvent(anchorablepanelgroup);
                    }
                }
            }
        }

        private void gridDock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (gridDock.ActualHeight > 0)
            {
                var layoutpanel = ModuleDock.Layout.Children.ToList()[0] as LayoutPanel;
                if (layoutpanel != null)
                {
                    foreach (var item in layoutpanel.Children)
                    {
                        if (item is LayoutAnchorablePaneGroup)
                        {
                            var panelgroup = item as LayoutAnchorablePaneGroup;
                            SetControlSize(panelgroup);
                        }
                        else if (item is LayoutAnchorablePane)
                        {
                            var panelgroup = item as LayoutAnchorablePane;
                            SetControlSize(panelgroup);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 设置控件大小 --防止控件大小随展示内容变大
        /// </summary>
        private void SetControlSize(LayoutAnchorablePane pane)
        {
            if (pane != null)
            {
                foreach (var module in pane.Children)
                {
                    if (module is LayoutAnchorable)
                    {
                        var doc = module as LayoutAnchorable;
                        if (doc == null) continue;
                        var content = doc.Content as ContentControl;
                        content.MaxHeight = gridDock.ActualHeight - 65;
                    }
                }
            }
        }

        /// <summary>
        /// 设置控件大小
        /// </summary>
        private void SetControlSize(LayoutAnchorablePaneGroup panelgroup)
        {
            if (panelgroup != null)
            {
                foreach (var module in panelgroup.Children)
                {
                    if (module is LayoutAnchorablePane)
                    {
                        foreach (var docment in module.Children)
                        {
                            var doc = docment as LayoutAnchorable;
                            var content = doc.Content as ContentControl;
                            content.MaxHeight = gridDock.ActualHeight - 65;
                        }
                    }
                    if (module is LayoutAnchorablePaneGroup)
                    {
                        var anchorablepanelgroup = module as LayoutAnchorablePaneGroup;
                        SetControlSize(anchorablepanelgroup);
                    }
                }
            }
        }
    }
}
