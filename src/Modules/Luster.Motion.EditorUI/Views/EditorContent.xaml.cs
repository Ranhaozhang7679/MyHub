using Luster.Common.Assets.Converter;
using Luster.Common.DataStruct;
using Luster.Control.Wpf.Motion.Flow;
using Luster.Motion.EditorUI.ViewModel;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
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
using langs = Luster.Motion.Assests.Langs;

namespace Luster.Motion.EditorUI.Views
{
    /// <summary>
    /// FlowContent.xaml 的交互逻辑
    /// </summary>
    public partial class EditorContent : UserControl
    {
        /// <summary>
        /// 右键菜单
        /// </summary>
        private ContextMenu contextMenu;

        /// <summary>
        /// 构造函数
        /// </summary>
        public EditorContent()
        {
            InitializeComponent();

            contextMenu = new ContextMenu();
            contextMenu.PlacementTarget = flowEditor;

            // 右键菜单
            flowEditor.AddHandler(FlowEditor.ItemSelectedEvent, new RoutedEventHandler(MouseDownRight), true);
            flowEditor.AddHandler(FlowEditor.MouseRightButtonDownEvent, new RoutedEventHandler(MouseEmptyRightDown), true);
            flowEditor.AddHandler(FlowEditor.LineSelectedEvent, new RoutedEventHandler(LineDownRight), true);
            flowEditor.AddHandler(FlowEditor.MouseLeftButtonDownEvent, new RoutedEventHandler(MouseDownLeft), true);

            //可接受焦点，必须先设这个属性,否则InputCommands 无效 
            Focusable = true;
            this.Loaded += FlowContent_Loaded;

            //TxtSearchKey.Focusable = true;
            //TxtSearchKey.Loaded += (s, e) => Keyboard.Focus(TxtSearchKey);

            //MaskGrid.IsVisibleChanged += MaskGrid_IsVisibleChanged;
        }

        private void MaskGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (bool.TryParse(e.NewValue.ToString(), out var visible))
            //{
            //    if (visible)
            //    {
            //        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
            //            new Action(() => TxtSearchKey.Focus()));
            //    }
            //    else
            //    {
            //        Keyboard.Focus(this);
            //    }
            //}
        }

        /// <summary>
        /// 加载完成后，必须设置交点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlowContent_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
        }

        /// <summary>
        /// 右键空白处
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MouseEmptyRightDown(object sender, RoutedEventArgs args)
        {
            contextMenu.Items.Clear();

            //
            AddStationType();

            // 粘贴
            AddPasteItems();

            // 导出图像
            AddExportImage();

            //导出流程树
            AddExportFlowTree();

            contextMenu.IsOpen = true;
        }


        /// <summary>
        /// 动态右键菜单功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MouseDownRight(object sender, RoutedEventArgs args)
        {
            FlowItemDownArgs downArgs = args as FlowItemDownArgs;
            var sItems = downArgs.SelectItems;
            if (sItems.Count == 0) return;

            if (downArgs.MouseDownArgs.ChangedButton != MouseButton.Right) return;

            // 右键菜单
            var context = flowEditor.DataContext;
            if (downArgs.SelectItems.Count == 1)
            {
                BuildSingleMenu(downArgs.FlowItem as FlowItem);
                contextMenu.IsOpen = true;
            }
            else
            {
                BuildMultiMenu(flowEditor.DataContext);
                contextMenu.IsOpen = true;
            }
        }

        private void MouseDownLeft(object sender, RoutedEventArgs args)
        {
            Keyboard.Focus(this);
        }


        /// <summary>
        /// 连线右键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void LineDownRight(object sender, RoutedEventArgs args)
        {
            FlowLineDownArgs downArgs = args as FlowLineDownArgs;
            if (downArgs.Fline == null || downArgs.MouseDownArgs.ChangedButton != MouseButton.Right) return;

            contextMenu.Items.Clear();

            MenuItem removeItem = new MenuItem();
            removeItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Remove);
            removeItem.Icon = GetFontText("\xe69e");
            removeItem.InputGestureText = "Delete";
            BuildCommand(removeItem, "RemoveLineCommand");
            removeItem.CommandParameter = downArgs.Fline;
            contextMenu.Items.Add(removeItem);
            contextMenu.IsOpen = true;
        }

        /// <summary>
        /// 获取旋转的模块
        /// </summary>
        /// <returns></returns>
        private List<IMotionModule> GetSelectItems()
        {
            return flowEditor.GetSelectedItems().Select(u => u.Tag as IMotionModule).ToList();
        }

        /// <summary>
        /// 构建右键单击命令
        /// </summary>
        /// <param name="context"></param>
        private void BuildSingleMenu(FlowItem flowItem)
        {
            contextMenu.Items.Clear();

            // 运行菜单
            if (flowItem.Motion.Status != RunStatus.Skip && flowItem.Motion.Status != RunStatus.Running)
            {
                AddRunItem();
            }

            if (flowItem.Motion.Status != RunStatus.Running)
            {
                AddSkipItem();
                contextMenu.Items.Add(new Separator());
            }

            AddRemoveItem();
            AddParamterItem(flowItem);

            //if (flowItem.Motion.Status != RunStatus.Skip)
            //{
            //    AddCopyItems();
            //}
            AddCopyItems();

            AddShowLog(flowItem);

            AddGoRef(flowItem);

            AddByRef(flowItem);

            AddHolo3D(flowItem);

            // 新窗口打开
            AddOpenInNewWindow(flowItem);

            // 查找同名工站
            AddFindDuplicate(flowItem);
        }


        private void AddShowLog(FlowItem flowItem)
        {
            if (flowEditor.IsDataFlow)
            {
                contextMenu.Items.Add(new Separator());
                MenuItem refItem = new MenuItem();
                refItem.Header = "显示LOG";
                BuildCommand(refItem, "ShowLogCommand");
                refItem.CommandParameter = flowItem.Tag;
                refItem.Icon = GetFontText("\xe6b1", Brushes.Red);
                contextMenu.Items.Add(refItem);
            }

        }

        private void AddGoRef(FlowItem flowItem)
        {
            var refs = flowItem.Motion.GetReferences();
            if (refs.Count == 0) return;

            contextMenu.Items.Add(new Separator());

            // 引用
            foreach (var item in refs)
            {
                var module = flowItem.Motion.TaskModules[item.RefID];
                if (module!=null)
                {
                    MenuItem refItem = new MenuItem();
                    refItem.Header = module.Alias;
                    BuildCommand(refItem, "GoToRefCommand");
                    refItem.CommandParameter = module;
                    refItem.Icon = GetFontText("\xe699", Brushes.Blue);
                    contextMenu.Items.Add(refItem);
                }
               
            }
        }

        /// <summary>
        /// 添加被引用模块
        /// </summary>
        /// <param name="flowItem"></param>
        private void AddByRef(FlowItem flowItem)
        {
            var byRefs = flowItem.Motion.GetByReferences();
            if (byRefs.Count > 0)
            {
                contextMenu.Items.Add(new Separator());

                // 引用
                foreach (var item in byRefs)
                {
                    MenuItem refItem = new MenuItem();
                    var refM = flowItem.Motion.TaskModules[item.RefID];
                    if (refM == null) return;

                    refItem.Header = refM.Alias;
                    BuildCommand(refItem, "GoToRefCommand");
                    refItem.CommandParameter = refM;
                    refItem.Icon = GetFontText("\xe6b1", Brushes.Blue);
                    contextMenu.Items.Add(refItem);
                }
            }
        }

        /// <summary>
        /// 查找同名工站
        /// </summary>
        private void AddFindDuplicate(FlowItem flowItem)
        {
            if (flowEditor.IsDataFlow && flowItem.Motion.TaskFunction is IStation)
            {
                contextMenu.Items.Add(new Separator());
                MenuItem item = new MenuItem();
                item.Header = "查找同名工站";
                BuildCommand(item, "FindDuplicateCommand");
                item.Icon = GetFontText("\xe693", Brushes.Orange);
                contextMenu.Items.Add(item);
            }
        }

        /// <summary>
        /// 多选
        /// </summary>
        private void BuildMultiMenu(object context)
        {
            contextMenu.Items.Clear();
            AddRunItem();
            AddSkipItem();
            AddRemoveItem();
            AddCopyItems();
            AddExtractItems();

            // 提取分组
            AddComposition();

            AddCompareLook();
        }

        /// <summary>
        /// 构造字体图标
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private TextBlock GetFontText(string icon, Brush foreBrush = null)
        {
            TextBlock txtBlock = new TextBlock();
            txtBlock.Text = icon;
            txtBlock.Style = this.FindResource("MotionIconSmall") as Style;

            if (foreBrush != null)
            {
                txtBlock.Foreground = foreBrush;
            }

            //var prop = DependencyProperty.Register("Property", typeof(string), typeof(Trigger), new PropertyMetadata("IsMouseOver"));
            //txtBlock.Style.Triggers.Add(new Trigger()
            //{
            //    Property = prop,
            //    Value = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
            //});

            return txtBlock;
        }

        /// <summary>
        /// 通用构建命令方法
        /// </summary>
        /// <param name="item">菜单</param>
        /// <param name="cmdName">命令名称</param>
        private void BuildCommand(MenuItem item, string cmdName)
        {
            BindingOperations.SetBinding(item, MenuItem.CommandProperty,
             new Binding(cmdName)
             {
                 Source = flowEditor.DataContext,
             });
        }

        /// <summary>
        /// 权限配置
        /// </summary>
        /// <param name="item"></param>
        /// <param name="minRole"></param>
        private void SetPermission(MenuItem item, SystemRole minRole = SystemRole.Integrator)
        {
            // 是否启用
            BindingOperations.SetBinding(item, MenuItem.IsEnabledProperty, new Binding("SysRole")
            {
                Source = flowEditor.DataContext,
                ConverterParameter = minRole.ToString(),
                Converter = new RoleEnabledCoverter()
            });
        }

        private void AddRunItem()
        {
            // 运行
            MenuItem runItem = new MenuItem();
            runItem.Header = langs.LangProvider.GetLang(langs.LangKeys.RunOne);
            runItem.Icon = GetFontText("\xe644");
            BuildCommand(runItem, "RunOneCommand");

            // 工程师可以单步运行
            SetPermission(runItem, SystemRole.Integrator);

            runItem.InputGestureText = "F6";
            contextMenu.Items.Add(runItem);
        }

        /// <summary>
        /// 模块忽略
        /// </summary>
        private void AddSkipItem()
        {
            // 忽略
            MenuItem skipItem = new MenuItem();
            string skipText = langs.LangKeys.Skip;
            bool isSkip = true;
            var sItem = flowEditor.GetSelectedItems()[0];
            if (sItem != null && sItem.Tag is IMotionModule module && module.Status == RunStatus.Skip)
            {
                skipText = langs.LangKeys.CancelSkip;
                isSkip = false;
            }

            skipItem.Header = langs.LangProvider.GetLang(skipText);
            BuildCommand(skipItem, "SkipCommand");
            SetPermission(skipItem, SystemRole.Admin);
            skipItem.CommandParameter = GetSelectItems().Where(u => isSkip ? u.Status != RunStatus.Skip : u.Status == RunStatus.Skip);
            skipItem.Icon = GetFontText("\xe6a1");
            contextMenu.Items.Add(skipItem);
        }

        /// <summary>
        /// 3D点云打开
        /// </summary>
        private void AddHolo3D(FlowItem flowItem)
        {
            if (flowItem.Motion.TaskFunction is IHolo3D)
            {
                contextMenu.Items.Add(new Separator());

                MenuItem holoItem = new MenuItem();
                holoItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Holo3D);
                holoItem.Icon = GetFontText("\xe65a");
                BuildCommand(holoItem, "OpenHolo3DCommand");
                holoItem.CommandParameter = flowItem.Motion;
                contextMenu.Items.Add(holoItem);
            }
        }

        /// <summary>
        /// 复制粘贴
        /// </summary>
        private void AddCopyItems()
        {
            // 复制
            MenuItem copyItem = new MenuItem();
            copyItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Copy);
            copyItem.InputGestureText = "Ctrl+C";
            copyItem.Icon = GetFontText("\xe63a");
            BuildCommand(copyItem, "CopyCommand");
            SetPermission(copyItem, SystemRole.Admin);

            copyItem.CommandParameter = GetSelectItems();
            contextMenu.Items.Add(copyItem);

        }

        /// <summary>
        /// 复制粘贴
        /// </summary>
        private void AddPasteItems()
        {
            // 粘贴
            MenuItem pasteItem = new MenuItem();
            pasteItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Paste);
            pasteItem.InputGestureText = "Ctrl+V";
            BuildCommand(pasteItem, "PasteCommand");
            pasteItem.Icon = GetFontText("\xe633");

            // 设置可见性绑定
            BindingOperations.SetBinding(pasteItem, MenuItem.IsEnabledProperty,
              new Binding($"CanPaste")
              {
                  Source = flowEditor.DataContext,
                  Mode = BindingMode.TwoWay,
                  UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                  Converter = new BooleanToVisibilityConverter(),
              });
            contextMenu.Items.Add(pasteItem);
        }

        /// <summary>
        /// 添加工站状态
        /// </summary>
        private void AddStationType()
        {
            // 如果是数据流程，则不能进行提取
            if (flowEditor.IsDataFlow) return;

            // 本站有料
            MenuItem thisHaveItem = new MenuItem();
            thisHaveItem.Header = langs.LangProvider.GetLang(langs.LangKeys.ThisHave);
            thisHaveItem.Icon = GetFontText("\xe600");
            BuildCommand(thisHaveItem, "StationCommand");
            thisHaveItem.CommandParameter = "ThisHave";
            SetPermission(thisHaveItem, SystemRole.Admin);
            contextMenu.Items.Add(thisHaveItem);

            // 本站要料
            MenuItem thisGetItem = new MenuItem();
            thisGetItem.Header = langs.LangProvider.GetLang(langs.LangKeys.ThisGet);
            thisGetItem.Icon = GetFontText("\xe600");
            BuildCommand(thisGetItem, "StationCommand");
            SetPermission(thisGetItem, SystemRole.Admin);
            thisGetItem.CommandParameter = "ThisGet";
            contextMenu.Items.Add(thisGetItem);
        }

        /// <summary>
        /// 提取
        /// </summary>
        private void AddExtractItems()
        {
            // 如果是数据流程，则不能进行提取
            if (flowEditor.IsDataFlow) return;

            if (flowEditor.GetSelectedItems().Count >= 2)
            {
                contextMenu.Items.Add(new Separator());

                // 分组提取
                MenuItem stepItem = new MenuItem();
                stepItem.Header = langs.LangProvider.GetLang(langs.LangKeys.ExtractStepGroup);
                stepItem.Icon = GetFontText("\xe65e");
                BuildCommand(stepItem, "ExtractCommand");
                stepItem.CommandParameter = "Group";
                SetPermission(stepItem, SystemRole.Admin);
                contextMenu.Items.Add(stepItem);

                // 异步
                MenuItem asyncItem = new MenuItem();
                asyncItem.Header = langs.LangProvider.GetLang(langs.LangKeys.ExtractAsyncGroup);
                asyncItem.Icon = GetFontText("\xe6a2");
                BuildCommand(asyncItem, "ExtractCommand");
                SetPermission(asyncItem, SystemRole.Admin);
                asyncItem.CommandParameter = "AsyncGroup";
                contextMenu.Items.Add(asyncItem);


                // 分支
                MenuItem switchItem = new MenuItem();
                switchItem.Header = langs.LangProvider.GetLang(langs.LangKeys.ExtractNGGroup);
                switchItem.Icon = GetFontText("\xe6a3");
                BuildCommand(switchItem, "ExtractCommand");
                SetPermission(switchItem, SystemRole.Admin);
                switchItem.CommandParameter = "NGGroup";
                contextMenu.Items.Add(switchItem);
            }
        }

        /// <summary>
        /// 提取
        /// </summary>
        private void AddComposition()
        {
            // 如果是数据流程，则不能进行提取
            if (flowEditor.IsDataFlow) return;

            if (flowEditor.GetSelectedItems().Count >= 2)
            {
                contextMenu.Items.Add(new Separator());

                // 分组提取
                MenuItem stepItem = new MenuItem();
                stepItem.Header = langs.LangProvider.GetLang(langs.LangKeys.ExtractModule);
                stepItem.Icon = GetFontText("\xe65e");
                BuildCommand(stepItem, "CompositionCommand");
                SetPermission(stepItem, SystemRole.Admin);
                stepItem.CommandParameter = "ExtractModule";
                contextMenu.Items.Add(stepItem);
            }
        }

        /// <summary>
        /// 添加删除命令
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddRemoveItem()
        {
            // 异步
            MenuItem removeItem = new MenuItem();
            removeItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Remove);
            removeItem.Icon = GetFontText("\xe620");
            removeItem.InputGestureText = "Delete";
            BuildCommand(removeItem, "RemoveCommand");
            SetPermission(removeItem, SystemRole.Admin);
            removeItem.CommandParameter = GetSelectItems();
            contextMenu.Items.Add(removeItem);
        }

        /// <summary>
        /// 添加扩展属性
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddParamterItem(FlowItem flowItem)
        {
            if (flowItem == null) return;

            IMotionModule module = flowItem.Motion;
            if (module != null && module.TaskFunction != null && module.TaskFunction.DynParam)
            {
                contextMenu.Items.Add(new Separator());
                MenuItem inItem = new MenuItem();
                inItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Add);
                inItem.Icon = GetFontText("\xe742");
                inItem.InputGestureText = "AddInParam";
                BuildCommand(inItem, "AddInCommand");
                SetPermission(inItem, SystemRole.Admin);

                inItem.CommandParameter = module;
                contextMenu.Items.Add(inItem);
            }
        }

        /// <summary>
        /// 添加对比查看
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddCompareLook()
        {
            // 如果是数据流程，则不能进行提取
            if (!flowEditor.IsDataFlow) return;

            if (flowEditor.GetSelectedItems().Count >= 2)
            {
                contextMenu.Items.Add(new Separator());
                MenuItem multiLook = new MenuItem();
                multiLook.Header = langs.LangProvider.GetLang(langs.LangKeys.CompareLook);
                multiLook.Icon = GetFontText("\xe62e");
                BuildCommand(multiLook, "CompareLookCommand");
                contextMenu.Items.Add(multiLook);
            }
        }


        /// <summary>
        /// 添加对比查看
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddExportImage()
        {
            // 如果是数据流程，则不能进行提取
            if (!flowEditor.IsDataFlow) return;

            contextMenu.Items.Add(new Separator());
            MenuItem exportItem = new MenuItem();
            exportItem.Header = langs.LangProvider.GetLang(langs.LangKeys.ExportImage);
            exportItem.Icon = GetFontText("\xe64d");
            BuildCommand(exportItem, "ExportImageCommand");
            contextMenu.Items.Add(exportItem);
        }

        /// <summary>
        /// 添加导出流程图
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddExportFlowTree()
        {
            // 如果是数据流程，则不能进行提取
            if (!flowEditor.IsDataFlow)
            {
                return;
            }

            contextMenu.Items.Add(new Separator());
            MenuItem exportItem = new MenuItem();
            exportItem.Header = langs.LangProvider.GetLang(langs.LangKeys.ExportFlowTree);
            exportItem.Icon = GetFontText("\xe634");
            BuildCommand(exportItem, "ExportFlowCommand");
            contextMenu.Items.Add(exportItem);
        }

        /// <summary>
        /// 新窗口打开菜单项（仅对自由工站、测试工站、多分支、if判断、并行显示）
        /// </summary>
        private void AddOpenInNewWindow(FlowItem flowItem)
        {
            if (flowItem?.Motion?.TaskFunction == null) return;

            var func = flowItem.Motion.TaskFunction;
            if (func is IFreeStation || func is ITestStation || func is ISwitch
                || func is IBranch || func is IParallel)
            {
                contextMenu.Items.Add(new Separator());
                MenuItem openItem = new MenuItem();
                openItem.Header = "新窗口打开";
                openItem.Icon = GetFontText("\xe75e");
                BuildCommand(openItem, "OpenInNewWindowCommand");
                openItem.CommandParameter = flowItem.Motion;
                contextMenu.Items.Add(openItem);
            }
        }
    }
}
