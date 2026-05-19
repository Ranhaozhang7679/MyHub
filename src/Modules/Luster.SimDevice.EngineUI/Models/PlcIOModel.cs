using Prism.Mvvm;

namespace Luster.SimDevice.EngineUI.Models
{
    /// <summary>
    /// PLC Bool IO 监控卡片数据模型
    /// </summary>
    public class PlcIOModel : BindableBase
    {
        /// <summary>
        /// 序号
        /// </summary>
        private int _index;
        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        /// <summary>
        /// PLC 地址
        /// </summary>
        private string _address;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 是否为输出（可写）
        /// </summary>
        private bool _isOutput;
        public bool IsOutput
        {
            get => _isOutput;
            set => SetProperty(ref _isOutput, value);
        }

        /// <summary>
        /// Bool 值
        /// </summary>
        private bool _boolValue;
        public bool BoolValue
        {
            get => _boolValue;
            set => SetProperty(ref _boolValue, value);
        }

        /// <summary>
        /// Short 值（实时读取值）
        /// </summary>
        private short _shortValue;
        public short ShortValue
        {
            get => _shortValue;
            set => SetProperty(ref _shortValue, value);
        }

        /// <summary>
        /// 写入值（用户输入）
        /// </summary>
        private short _writeValue;
        public short WriteValue
        {
            get => _writeValue;
            set => SetProperty(ref _writeValue, value);
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PlcIOModel(string address, string name, bool isOutput, int index = 0)
        {
            Address = address;
            Name = string.IsNullOrEmpty(name) ? address : name;
            IsOutput = isOutput;
            Index = index;
            BoolValue = false;
        }
    }
}
