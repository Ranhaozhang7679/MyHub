using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Luster.Motion.DigitalSetup.Datas
{
    /// <summary>
    /// LAD 工站配置数据模型 - 包含工站信息和对应的 LAD 更新配置
    /// </summary>
    public class LADStationConfig : BindableBase
    {
        private string _stationName;
        private ViewModel.LADUpdateConfig _ladConfig;
        private CheckStatus _checkStatus;

        /// <summary>
        /// 工站名称
        /// </summary>
        public string StationName
        {
            get => _stationName;
            set => SetProperty(ref _stationName, value);
        }

        /// <summary>
        /// 该工站的 LAD 更新配置
        /// </summary>
        public ViewModel.LADUpdateConfig LadConfig
        {
            get => _ladConfig;
            set => SetProperty(ref _ladConfig, value);
        }

        /// <summary>
        /// 该工站的点检状态
        /// </summary>
        public CheckStatus CheckStatus
        {
            get => _checkStatus;
            set => SetProperty(ref _checkStatus, value);
        }

        /// <summary>
        /// 配置文件路径（用于保存/加载）
        /// </summary>
        public string ConfigFilePath { get; set; }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static LADStationConfig CreateDefault(string stationName)
        {
            return new LADStationConfig
            {
                StationName = stationName,
                LadConfig = new ViewModel.LADUpdateConfig
                {
                    ConfigFile1 = "",
                    ConfigFile2 = "",
                    PythonScriptPath = "",
                    PythonExePath = "",
                    SelectedParameters = new List<string>(),
                    MappingItems = new List<ViewModel.MappingItem>
                    {
                        new ViewModel.MappingItem { TxtKey = "Install_Force", ExcelKey = "1# Paste Force", StartRow = "23", MaxRow = "18", MinRow = "20" },
                        new ViewModel.MappingItem { TxtKey = "Install_Gap_X", ExcelKey = "X1", StartRow = "23", MaxRow = "18", MinRow = "20" },
                        new ViewModel.MappingItem { TxtKey = "Install_Gap_Y", ExcelKey = "Y1", StartRow = "23", MaxRow = "18", MinRow = "20" },
                        new ViewModel.MappingItem { TxtKey = "Install_CC", ExcelKey = "1# CC ", StartRow = "23", MaxRow = "18", MinRow = "20" }
                    }
                },
                CheckStatus = CheckStatus.NotChecked
            };
        }
    }
}
