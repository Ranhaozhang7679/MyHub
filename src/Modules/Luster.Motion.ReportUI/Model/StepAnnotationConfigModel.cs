using Prism.Mvvm;

namespace Luster.Motion.ReportUI.Model
{
    /// <summary>
    /// 步骤标注配置模型，定义单个工艺步骤的时间区间和显示信息
    /// </summary>
    public class StepAnnotationConfigModel : BindableBase
    {
        private string _name;
        /// <summary>
        /// 步骤名称（如"机械手下降"）
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private double _startTimeMs;
        /// <summary>
        /// 起始时间（毫秒）
        /// </summary>
        public double StartTimeMs
        {
            get => _startTimeMs;
            set => SetProperty(ref _startTimeMs, value);
        }

        private double _endTimeMs;
        /// <summary>
        /// 结束时间（毫秒）
        /// </summary>
        public double EndTimeMs
        {
            get => _endTimeMs;
            set => SetProperty(ref _endTimeMs, value);
        }

        private string _color = "#4CAF50";
        /// <summary>
        /// 标注颜色（十六进制，如"#4CAF50"）
        /// </summary>
        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }
    }
}
