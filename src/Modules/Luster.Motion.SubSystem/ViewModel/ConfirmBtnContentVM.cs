using LiveChartsCore.Defaults;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class ConfirmBtnContentVM : MotionVM, IDockContent
    {
        /// <summary>
        /// 确认按钮
        /// </summary>
        private List<IConfirmButton> _confirmButtons;

        /// <summary>
        /// 运控引擎
        /// </summary>
        private IMotionEngine _mEngine;

        /// <summary>
        /// 机台确认按
        /// </summary>
        private ObservableCollection<ButtonGroupModel> _buttons;
        public ObservableCollection<ButtonGroupModel> Buttons
        {
            get { return _buttons; }
            set
            {
                SetProperty(ref _buttons, value);
            }
        }

        /// <summary>
        /// 空构造函数，用于DockContent实例构建
        /// </summary>
        public ConfirmBtnContentVM() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bus"></param>
        public ConfirmBtnContentVM(ICommonBus bus, IMotionEngine motionEngine) : base(bus)
        {
            _mEngine = motionEngine;

            InitButtons();
        }

        /// <summary>
        /// 初始化按钮
        /// </summary>
        private void InitButtons()
        {
            Buttons = new ObservableCollection<ButtonGroupModel>();
            var btnModules = _mEngine.Where(u => u.Status != Luster.TaskFlow.Common.Enums.RunStatus.Skip && u.TaskFunction is IConfirmButton).ToList();
            foreach (var item in btnModules)
            {
                if (item.TaskFunction is IConfirmButton c)
                {
                    c.WaitConfirmEvent -= C_WaitConfirmEvent;
                    c.WaitConfirmEvent += C_WaitConfirmEvent;

                    var buttons = c.GetButtons().Select(u => new ButtonModel
                    {
                        Button = u,
                        Tag = c
                    }).ToList();

                    // 按钮
                    Buttons.Add(new ButtonGroupModel()
                    {
                        ButtonGroup = c.Module,
                        IsEnabled = false,
                        Tag = c,
                        Buttons = buttons
                    });
                }
            }
        }

        /// <summary>
        /// 等待按钮高亮
        /// </summary>
        /// <param name="group"></param>
        /// <param name="isOpen">是否高亮</param>
        private void C_WaitConfirmEvent(string group, bool isOpen = true)
        {
            // 高亮确认按钮
            foreach (var item in Buttons)
            {
                if (item.ButtonGroup == group)
                {
                    item.IsEnabled = isOpen;
                }
            }
        }

        /// <summary>
        /// 事件注册
        /// </summary>
        /// <param name="bus"></param>
        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                InitButtons();
            });
        }

        /// <summary>
        /// 确认按钮
        /// </summary>
        private DelegateCommand<ButtonModel> _confirmCommand;
        public DelegateCommand<ButtonModel> ConfirmCommand => _confirmCommand ?? (_confirmCommand = new DelegateCommand<ButtonModel>((btnContent) =>
        {
            // 配置
            btnContent.Tag.Confirm(btnContent.Button);
        }));

        public string Name => "ConfirmButton";

        public string RegionName { get; set; } = "ConfirmBtnContent";
    }

    /// <summary>
    /// 按钮模块
    /// </summary>
    public class ButtonGroupModel : BindableBase
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled; set
            {
                SetProperty(ref _isEnabled, value);
            }
        }

        /// <summary>
        /// 按钮组
        /// </summary>
        private string _buttonGroup;
        public string ButtonGroup
        {
            get => _buttonGroup; set
            {
                SetProperty(ref _buttonGroup, value);
            }
        }

        /// <summary>
        /// 按钮确认
        /// </summary>
        public IConfirmButton Tag { get; set; }

        /// <summary>
        /// 按钮集合
        /// </summary>
        private List<ButtonModel> _buttons;
        public List<ButtonModel> Buttons
        {
            get => _buttons; set
            {
                SetProperty(ref _buttons, value);
            }
        }
    }

    public class ButtonModel : BindableBase
    {
        /// <summary>
        /// 按钮组
        /// </summary>
        private string _button;
        public string Button
        {
            get => _button; set
            {
                SetProperty(ref _button, value);
            }
        }

        /// <summary>
        /// 按钮确认
        /// </summary>
        public IConfirmButton Tag { get; set; }
    }
}