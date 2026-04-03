using Luster.Motion.DigitalSetup.Datas;
using Prism.Mvvm;

namespace Luster.Motion.DigitalSetup.Datas
{
    /// <summary>
    /// 工站配置数据模型
    /// </summary>
    public class StationConfig : BindableBase
    {
        private string _name;
        /// <summary>
        /// 工站名称
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _selectedGlobalKey;
        /// <summary>
        /// 选中的全局变量Key
        /// </summary>
        public string SelectedGlobalKey
        {
            get => _selectedGlobalKey;
            set => SetProperty(ref _selectedGlobalKey, value);
        }

        private CheckStatus _checkStatus = CheckStatus.NotChecked;
        /// <summary>
        /// 工站点检状态
        /// </summary>
        public CheckStatus CheckStatus
        {
            get => _checkStatus;
            set => SetProperty(ref _checkStatus, value);
        }
    }
}
