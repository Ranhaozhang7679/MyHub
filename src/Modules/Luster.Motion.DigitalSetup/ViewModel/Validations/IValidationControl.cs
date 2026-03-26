using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Views;
using System;

namespace Luster.Motion.DigitalSetup.ViewModel.Validations
{
    /// <summary>
    /// 验证控件接口 - 所有验证类型的ViewModel都需要实现此接口
    /// </summary>
    public interface IValidationControl
    {
        /// <summary>
        /// 验证项名称
        /// </summary>
        string ValidationName { get; set; }

        /// <summary>
        /// 验证描述
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        bool IsRunning { get; set; }

        /// <summary>
        /// 最后运行时间
        /// </summary>
        DateTime LastRunTime { get; set; }

        /// <summary>
        /// 验证结果
        /// </summary>
        string ValidationResult { get; set; }

        /// <summary>
        /// 初始化验证控件
        /// </summary>
        /// <param name="name">验证项名称</param>
        void Initialize(string name);

        /// <summary>
        /// 从配置数据加载
        /// </summary>
        /// <param name="data">配置数据</param>
        void LoadFromConfigData(ValidationItemData data);

        /// <summary>
        /// 转换为配置数据
        /// </summary>
        /// <returns>配置数据</returns>
        ValidationItemData ToConfigData();

        /// <summary>
        /// 配置变化事件
        /// </summary>
        event EventHandler ConfigChanged;

        /// <summary>
        /// 验证状态变化事件
        /// </summary>
        event EventHandler<ValidationStatusChangedEventArgs> ValidationStatusChanged;
    }
    /// <summary>
    /// 验证状态变化事件参数
    /// </summary>
    public class ValidationStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 验证状态
        /// </summary>
        public ValidationStatus Status { get; set; }

        /// <summary>
        /// 验证结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        public ValidationStatusChangedEventArgs(ValidationStatus status, string message = "")
        {
            Status = status;
            Message = message;
        }

    }

    /// <summary>
    /// 验证状态枚举
    /// </summary>
    public enum ValidationStatus
    {
        Pending,
        Pass,
        Fail
    }
}
