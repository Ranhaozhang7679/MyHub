using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Luster.Motion.EditorUI.ViewModels
{
    /// <summary>
    /// 五轴手动控制 ViewModel（TES-132）。
    /// 仅做 UI 层 VM + 命令接线（占位），不接真运动 API；真运动调度归 TES-124。
    /// </summary>
    public class FiveAxisManualControlViewModel : BindableBase
    {
        // 坐标系枚举：机械/粗略/精确/工件（TES-130 契约）
        public enum CoordinateFrame
        {
            Machine,
            Rough,
            Accurate,
            Workpiece
        }

        // 坐标系下拉项（Value=enum, Display=中文显示）
        public sealed class CoordinateFrameOption
        {
            public CoordinateFrame Value { get; set; }
            public string Display { get; set; }
        }

        private CoordinateFrame _selectedCoordinateFrame = CoordinateFrame.Machine;
        private bool _isJogMode = true; // 默认频率模式
        private double _movePara = 0.5; // 默认第 4 档（与 XAML 原 SelectedIndex=3 对齐）

        /// <summary>坐标系选项集合（供 ComboBox ItemsSource 绑定）。</summary>
        public ObservableCollection<CoordinateFrameOption> CoordinateFrameOptions { get; }

        /// <summary>
        /// 运行参数 7 档预设（供 ComboBox ItemsSource 绑定）。
        /// 双语义：频率模式=速度（mm/s），步进模式=步距（mm）；解释归 TES-124。
        /// </summary>
        public ObservableCollection<double> MoveParaOptions { get; }

        /// <summary>当前坐标系。</summary>
        public CoordinateFrame SelectedCoordinateFrame
        {
            get => _selectedCoordinateFrame;
            set => SetProperty(ref _selectedCoordinateFrame, value);
        }

        /// <summary>Jog 模式：true=频率（连续 Jog），false=步进（Step）。</summary>
        public bool IsJogMode
        {
            get => _isJogMode;
            set
            {
                if (SetProperty(ref _isJogMode, value))
                {
                    RaisePropertyChanged(nameof(IsStepMode));
                }
            }
        }

        /// <summary>步进模式（只读，= !IsJogMode，供步进 RadioButton 反向绑定）。</summary>
        public bool IsStepMode => !_isJogMode;

        /// <summary>运行参数（频率=速度 / 步进=步距）。双语义解释归 TES-124。</summary>
        public double MovePara
        {
            get => _movePara;
            set => SetProperty(ref _movePara, value);
        }

        // 10 个 Jog 命令（离散式，XAML 已绑死；命令体占位，不调真运动 API）
        public DelegateCommand JogXPlusCommand { get; }
        public DelegateCommand JogXMinusCommand { get; }
        public DelegateCommand JogYPlusCommand { get; }
        public DelegateCommand JogYMinusCommand { get; }
        public DelegateCommand JogZPlusCommand { get; }
        public DelegateCommand JogZMinusCommand { get; }
        public DelegateCommand JogAPlusCommand { get; }
        public DelegateCommand JogAMinusCommand { get; }
        public DelegateCommand JogCPlusCommand { get; }
        public DelegateCommand JogCMinusCommand { get; }

        public FiveAxisManualControlViewModel()
        {
            CoordinateFrameOptions = new ObservableCollection<CoordinateFrameOption>
            {
                new CoordinateFrameOption { Value = CoordinateFrame.Machine, Display = "机械" },
                new CoordinateFrameOption { Value = CoordinateFrame.Rough, Display = "粗略" },
                new CoordinateFrameOption { Value = CoordinateFrame.Accurate, Display = "精确" },
                new CoordinateFrameOption { Value = CoordinateFrame.Workpiece, Display = "工件" },
            };

            MoveParaOptions = new ObservableCollection<double> { 0.01, 0.05, 0.1, 0.5, 1, 2, 5 };

            JogXPlusCommand = new DelegateCommand(() => OnJog("X", "+"));
            JogXMinusCommand = new DelegateCommand(() => OnJog("X", "-"));
            JogYPlusCommand = new DelegateCommand(() => OnJog("Y", "+"));
            JogYMinusCommand = new DelegateCommand(() => OnJog("Y", "-"));
            JogZPlusCommand = new DelegateCommand(() => OnJog("Z", "+"));
            JogZMinusCommand = new DelegateCommand(() => OnJog("Z", "-"));
            JogAPlusCommand = new DelegateCommand(() => OnJog("A", "+"));
            JogAMinusCommand = new DelegateCommand(() => OnJog("A", "-"));
            JogCPlusCommand = new DelegateCommand(() => OnJog("C", "+"));
            JogCMinusCommand = new DelegateCommand(() => OnJog("C", "-"));
        }

        /// <summary>
        /// Jog 命令占位实现：仅记录意图，不调用真运动 API。
        /// 真运动调度（press-hold / VAxis / MotorComponent）归 TES-124。
        /// </summary>
        private void OnJog(string axis, string direction)
        {
            Debug.WriteLine($"[TES-132 占位] Jog {axis} {direction} (Frame={SelectedCoordinateFrame}, JogMode={IsJogMode}, MovePara={MovePara})");
        }
    }
}
