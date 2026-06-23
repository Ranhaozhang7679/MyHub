using Prism.Mvvm;

namespace Luster.Motion.LightTuning.Functions
{
    /// <summary>
    /// 单通道亮度表行（DataGrid 编辑 + 实时回读）。
    /// 对齐源端 <c>LightParamProfile</c>（<c>LightParamProfile.cs:33-39</c>）的 Delay/Width 两字段，
    /// 源端用 PropertyGrid 展开数组编辑，目标端用 DataGrid 表格编辑（HandyControl）。
    /// </summary>
    public class LightChannelItem : BindableBase
    {
        private int _delay = 80;
        private int _width = 50;
        private int _feedback;

        /// <summary>通道索引（0 起）</summary>
        public int Channel { get; set; }

        /// <summary>光源延时（源端 LightParamProfile.Delay，默认 "80"）</summary>
        public int Delay
        {
            get => _delay;
            set => SetProperty(ref _delay, value);
        }

        /// <summary>光源脉宽/亮度（源端 LightParamProfile.Width，默认 "50"；下发 SetChannelAndVal 的 intensity）</summary>
        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        /// <summary>实时回读亮度（GetChannelIntensity 返回值，只读展示）</summary>
        public int Feedback
        {
            get => _feedback;
            set => SetProperty(ref _feedback, value);
        }
    }
}
