using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Module.Motion.Business.Functions;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.EditorUI.Events;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.SubSystem.ViewModel;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Prism.Ioc;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    // PDCA参数行数据模型
    public class PDCAKeyParameterRow : BindableBase
    {
        private string _stationName;
        private string _commDevice;
        private string _isEnable;
        private int _timeOutMS;
        private string _isAutoDisConnect;
        private string _pdcaMode;
        private string _isCGDisplaySN;
        private string _cgDisplaySN;
        private string _isCPKMode;
        private string _isGRRMode;
        private string _sn;
        private string _wipFile;
        private int _wipLength;
        private string _carrierSN;
        private double _cycleTime;
        private int _workId;
        private string _sourceImagePath;
        private string _desImagePath;
        private string _isImageRol;
        private string _prodData;
        private string _is3ANG;
        private string _isContainLine;
        private string _isManual;
        private string _isEnableCarrier;

        // 显示属性 - 支持双向绑定
        private bool _showIsEnableCheckBox;
        private bool? _isEnableCheckBoxValue;
        private string _isEnableDisplayText;
        private bool _isEnableIsBinding;

        private bool _showIsAutoDisConnectCheckBox;
        private bool? _isAutoDisConnectCheckBoxValue;
        private string _isAutoDisConnectDisplayText;
        private bool _isAutoDisConnectIsBinding;

        private bool _showIsCGDisplaySNCheckBox;
        private bool? _isCGDisplaySNCheckBoxValue;
        private string _isCGDisplaySNDisplayText;
        private bool _isCGDisplaySNIsBinding;

        private bool _showIsCPKModeCheckBox;
        private bool? _isCPKModeCheckBoxValue;
        private string _isCPKModeDisplayText;
        private bool _isCPKModeIsBinding;

        private bool _showIsGRRModeCheckBox;
        private bool? _isGRRModeCheckBoxValue;
        private string _isGRRModeDisplayText;
        private bool _isGRRModeIsBinding;

        private string _cgDisplaySNDisplayText;
        private bool _cgDisplaySNIsBinding;

        private string _sNDisplayText;
        private bool _sNIsBinding;

        private string _carrierSNDisplayText;
        private bool _carrierSNIsBinding;

        private string _sourceImagePathDisplayText;
        private bool _sourceImagePathIsBinding;

        private bool _showIs3ANGCheckBox;
        private bool? _is3ANGCheckBoxValue;
        private string _is3ANGDisplayText;

        private bool _showIsContainLineCheckBox;
        private bool? _isContainLineCheckBoxValue;
        private string _isContainLineDisplayText;

        private bool _showIsManualCheckBox;
        private bool? _isManualCheckBoxValue;
        private string _isManualDisplayText;

        private bool _showIsEnableCarrierCheckBox;
        private bool? _isEnableCarrierCheckBoxValue;
        private string _isEnableCarrierDisplayText;

        private bool _isProdDataExpanded;
        public bool IsProdDataExpanded
        {
            get => _isProdDataExpanded;
            set => SetProperty(ref _isProdDataExpanded, value);
        }
        // 工站名称
        public string StationName
        {
            get => _stationName;
            set => SetProperty(ref _stationName, value);
        }

        // 源模块引用
        public IMotionModule SourceModule { get; set; }

        // 通信服务器地址
        public string CommDevice
        {
            get => _commDevice;
            set => SetProperty(ref _commDevice, value);
        }

        // 启用
        public string IsEnable
        {
            get => _isEnable;
            set
            {
                if (SetProperty(ref _isEnable, value))
                {
                    UpdateIsEnableDisplay();
                }
            }
        }

        // PDCA读取超时时间(ms)
        public int TimeOutMS
        {
            get => _timeOutMS;
            set => SetProperty(ref _timeOutMS, value);
        }

        // PDCA读取失败后是否主动断连
        public string IsAutoDisConnect
        {
            get => _isAutoDisConnect;
            set
            {
                if (SetProperty(ref _isAutoDisConnect, value))
                {
                    UpdateIsAutoDisConnectDisplay();
                }
            }
        }

        // 动作类型
        public string PDCAMode
        {
            get => _pdcaMode;
            set => SetProperty(ref _pdcaMode, value);
        }

        // 启用Display_SN上传项
        public string IsCGDisplaySN
        {
            get => _isCGDisplaySN;
            set
            {
                if (SetProperty(ref _isCGDisplaySN, value))
                {
                    UpdateIsCGDisplaySNDisplay();
                }
            }
        }

        // Display_SN
        public string CGDisplaySN
        {
            get => _cgDisplaySN;
            set
            {
                if (SetProperty(ref _cgDisplaySN, value))
                {
                    UpdateCGDisplaySNDisplay();
                }
            }
        }

        // CPK模式
        public string IsCPKMode
        {
            get => _isCPKMode;
            set
            {
                if (SetProperty(ref _isCPKMode, value))
                {
                    UpdateIsCPKModeDisplay();
                }
            }
        }

        // GRR模式
        public string IsGRRMode
        {
            get => _isGRRMode;
            set
            {
                if (SetProperty(ref _isGRRMode, value))
                {
                    UpdateIsGRRModeDisplay();
                }
            }
        }

        // SN
        public string SN
        {
            get => _sn;
            set
            {
                if (SetProperty(ref _sn, value))
                {
                    UpdateSNDisplay();
                }
            }
        }

        // 虚拟码文件
        public string WIPFile
        {
            get => _wipFile;
            set => SetProperty(ref _wipFile, value);
        }

        // WIP长度
        public int WIPLength
        {
            get => _wipLength;
            set => SetProperty(ref _wipLength, value);
        }

        // CarrierSN
        public string CarrierSN
        {
            get => _carrierSN;
            set
            {
                if (SetProperty(ref _carrierSN, value))
                {
                    UpdateCarrierSNDisplay();
                }
            }
        }

        // CycleTime
        public double CycleTime
        {
            get => _cycleTime;
            set => SetProperty(ref _cycleTime, value);
        }

        // 工位号
        public int WorkId
        {
            get => _workId;
            set => SetProperty(ref _workId, value);
        }

        // 原图片路径
        public string SourceImagePath
        {
            get => _sourceImagePath;
            set
            {
                if (SetProperty(ref _sourceImagePath, value))
                {
                    UpdateSourceImagePathDisplay();
                }
            }
        }

        // 目标图片文件夹名称
        public string DesImagePath
        {
            get => _desImagePath;
            set => SetProperty(ref _desImagePath, value);
        }

        // 图片名称规则
        public string IsImageRol
        {
            get => _isImageRol;
            set => SetProperty(ref _isImageRol, value);
        }

        // 过程数据
        public string ProdData
        {
            get => _prodData;
            set => SetProperty(ref _prodData, value);
        }

        // 是否3ANG
        public string Is3ANG
        {
            get => _is3ANG;
            set
            {
                if (SetProperty(ref _is3ANG, value))
                {
                    UpdateIs3ANGDisplay();
                }
            }
        }

        // 是否包含下划线
        public string IsContainLine
        {
            get => _isContainLine;
            set
            {
                if (SetProperty(ref _isContainLine, value))
                {
                    UpdateIsContainLineDisplay();
                }
            }
        }

        // 手动输入AELimite
        public string IsManual
        {
            get => _isManual;
            set
            {
                if (SetProperty(ref _isManual, value))
                {
                    UpdateIsManualDisplay();
                }
            }
        }

        // 启用载具查询WIP
        public string IsEnableCarrier
        {
            get => _isEnableCarrier;
            set
            {
                if (SetProperty(ref _isEnableCarrier, value))
                {
                    UpdateIsEnableCarrierDisplay();
                }
            }
        }

        // 显示属性 - 支持双向绑定
        public bool ShowIsEnableCheckBox
        {
            get => _showIsEnableCheckBox;
            set => SetProperty(ref _showIsEnableCheckBox, value);
        }

        public bool? IsEnableCheckBoxValue
        {
            get => _isEnableCheckBoxValue;
            set
            {
                if (SetProperty(ref _isEnableCheckBoxValue, value))
                {
                    // 当复选框值改变时，更新原始值
                    UpdateOriginalValueFromCheckBox("IsEnable", value);
                }
            }
        }

        public string IsEnableDisplayText
        {
            get => _isEnableDisplayText;
            set => SetProperty(ref _isEnableDisplayText, value);
        }

        public bool IsEnableIsBinding
        {
            get => _isEnableIsBinding;
            set => SetProperty(ref _isEnableIsBinding, value);
        }

        public bool ShowIsAutoDisConnectCheckBox
        {
            get => _showIsAutoDisConnectCheckBox;
            set => SetProperty(ref _showIsAutoDisConnectCheckBox, value);
        }

        public bool? IsAutoDisConnectCheckBoxValue
        {
            get => _isAutoDisConnectCheckBoxValue;
            set
            {
                if (SetProperty(ref _isAutoDisConnectCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsAutoDisConnect", value);
                }
            }
        }

        public string IsAutoDisConnectDisplayText
        {
            get => _isAutoDisConnectDisplayText;
            set => SetProperty(ref _isAutoDisConnectDisplayText, value);
        }

        public bool IsAutoDisConnectIsBinding
        {
            get => _isAutoDisConnectIsBinding;
            set => SetProperty(ref _isAutoDisConnectIsBinding, value);
        }

        public bool ShowIsCGDisplaySNCheckBox
        {
            get => _showIsCGDisplaySNCheckBox;
            set => SetProperty(ref _showIsCGDisplaySNCheckBox, value);
        }

        public bool? IsCGDisplaySNCheckBoxValue
        {
            get => _isCGDisplaySNCheckBoxValue;
            set
            {
                if (SetProperty(ref _isCGDisplaySNCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsCGDisplaySN", value);
                }
            }
        }

        public string IsCGDisplaySNDisplayText
        {
            get => _isCGDisplaySNDisplayText;
            set => SetProperty(ref _isCGDisplaySNDisplayText, value);
        }

        public bool IsCGDisplaySNIsBinding
        {
            get => _isCGDisplaySNIsBinding;
            set => SetProperty(ref _isCGDisplaySNIsBinding, value);
        }

        public bool ShowIsCPKModeCheckBox
        {
            get => _showIsCPKModeCheckBox;
            set => SetProperty(ref _showIsCPKModeCheckBox, value);
        }

        public bool? IsCPKModeCheckBoxValue
        {
            get => _isCPKModeCheckBoxValue;
            set
            {
                if (SetProperty(ref _isCPKModeCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsCPKMode", value);
                }
            }
        }

        public string IsCPKModeDisplayText
        {
            get => _isCPKModeDisplayText;
            set => SetProperty(ref _isCPKModeDisplayText, value);
        }

        public bool IsCPKModeIsBinding
        {
            get => _isCPKModeIsBinding;
            set => SetProperty(ref _isCPKModeIsBinding, value);
        }

        public bool ShowIsGRRModeCheckBox
        {
            get => _showIsGRRModeCheckBox;
            set => SetProperty(ref _showIsGRRModeCheckBox, value);
        }

        public bool? IsGRRModeCheckBoxValue
        {
            get => _isGRRModeCheckBoxValue;
            set
            {
                if (SetProperty(ref _isGRRModeCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsGRRMode", value);
                }
            }
        }

        public string IsGRRModeDisplayText
        {
            get => _isGRRModeDisplayText;
            set => SetProperty(ref _isGRRModeDisplayText, value);
        }

        public bool IsGRRModeIsBinding
        {
            get => _isGRRModeIsBinding;
            set => SetProperty(ref _isGRRModeIsBinding, value);
        }

        public string CGDisplaySNDisplayText
        {
            get => _cgDisplaySNDisplayText;
            set => SetProperty(ref _cgDisplaySNDisplayText, value);
        }

        public bool CGDisplaySNIsBinding
        {
            get => _cgDisplaySNIsBinding;
            set => SetProperty(ref _cgDisplaySNIsBinding, value);
        }

        public string SNDisplayText
        {
            get => _sNDisplayText;
            set => SetProperty(ref _sNDisplayText, value);
        }

        public bool SNIsBinding
        {
            get => _sNIsBinding;
            set => SetProperty(ref _sNIsBinding, value);
        }

        public string CarrierSNDisplayText
        {
            get => _carrierSNDisplayText;
            set => SetProperty(ref _carrierSNDisplayText, value);
        }

        public bool CarrierSNIsBinding
        {
            get => _carrierSNIsBinding;
            set => SetProperty(ref _carrierSNIsBinding, value);
        }

        public string SourceImagePathDisplayText
        {
            get => _sourceImagePathDisplayText;
            set => SetProperty(ref _sourceImagePathDisplayText, value);
        }

        public bool SourceImagePathIsBinding
        {
            get => _sourceImagePathIsBinding;
            set => SetProperty(ref _sourceImagePathIsBinding, value);
        }

        public bool ShowIs3ANGCheckBox
        {
            get => _showIs3ANGCheckBox;
            set => SetProperty(ref _showIs3ANGCheckBox, value);
        }

        public bool? Is3ANGCheckBoxValue
        {
            get => _is3ANGCheckBoxValue;
            set
            {
                if (SetProperty(ref _is3ANGCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("Is3ANG", value);
                }
            }
        }

        public string Is3ANGDisplayText
        {
            get => _is3ANGDisplayText;
            set => SetProperty(ref _is3ANGDisplayText, value);
        }

        public bool ShowIsContainLineCheckBox
        {
            get => _showIsContainLineCheckBox;
            set => SetProperty(ref _showIsContainLineCheckBox, value);
        }

        public bool? IsContainLineCheckBoxValue
        {
            get => _isContainLineCheckBoxValue;
            set
            {
                if (SetProperty(ref _isContainLineCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsContainLine", value);
                }
            }
        }

        public string IsContainLineDisplayText
        {
            get => _isContainLineDisplayText;
            set => SetProperty(ref _isContainLineDisplayText, value);
        }

        public bool ShowIsManualCheckBox
        {
            get => _showIsManualCheckBox;
            set => SetProperty(ref _showIsManualCheckBox, value);
        }

        public bool? IsManualCheckBoxValue
        {
            get => _isManualCheckBoxValue;
            set
            {
                if (SetProperty(ref _isManualCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsManual", value);
                }
            }
        }

        public string IsManualDisplayText
        {
            get => _isManualDisplayText;
            set => SetProperty(ref _isManualDisplayText, value);
        }

        public bool ShowIsEnableCarrierCheckBox
        {
            get => _showIsEnableCarrierCheckBox;
            set => SetProperty(ref _showIsEnableCarrierCheckBox, value);
        }

        public bool? IsEnableCarrierCheckBoxValue
        {
            get => _isEnableCarrierCheckBoxValue;
            set
            {
                if (SetProperty(ref _isEnableCarrierCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsEnableCarrier", value);
                }
            }
        }

        public string IsEnableCarrierDisplayText
        {
            get => _isEnableCarrierDisplayText;
            set => SetProperty(ref _isEnableCarrierDisplayText, value);
        }

        // 文件路径截断显示
        public string WIPFileShort => GetShortPath(WIPFile, 30);
        public string SourceImagePathShort => GetShortPath(SourceImagePath, 30);
        public string DesImagePathShort => GetShortPath(DesImagePath, 30);

        // 从复选框更新原始值的方法
        private void UpdateOriginalValueFromCheckBox(string propertyName, bool? checkBoxValue)
        {
            if (checkBoxValue.HasValue)
            {
                string newValue = checkBoxValue.Value ? "True" : "False";

                // 根据属性名更新对应的原始值
                switch (propertyName)
                {
                    case "IsEnable":
                        IsEnable = newValue;
                        break;
                    case "IsAutoDisConnect":
                        IsAutoDisConnect = newValue;
                        break;
                    case "IsCGDisplaySN":
                        IsCGDisplaySN = newValue;
                        break;
                    case "IsCPKMode":
                        IsCPKMode = newValue;
                        break;
                    case "IsGRRMode":
                        IsGRRMode = newValue;
                        break;
                    case "Is3ANG":
                        Is3ANG = newValue;
                        break;
                    case "IsContainLine":
                        IsContainLine = newValue;
                        break;
                    case "IsManual":
                        IsManual = newValue;
                        break;
                    case "IsEnableCarrier":
                        IsEnableCarrier = newValue;
                        break;
                }
            }
        }

        // 更新显示属性的方法
        private void UpdateIsEnableDisplay()
        {
            IsEnableIsBinding = IsBindingProperty(_isEnable);
            IsEnableDisplayText = GetDisplayText(_isEnable);
            ShowIsEnableCheckBox = ShouldShowCheckBox(_isEnable);
            IsEnableCheckBoxValue = GetCheckBoxState(_isEnable);
        }

        private void UpdateIsAutoDisConnectDisplay()
        {
            IsAutoDisConnectIsBinding = IsBindingProperty(_isAutoDisConnect);
            IsAutoDisConnectDisplayText = GetDisplayText(_isAutoDisConnect);
            ShowIsAutoDisConnectCheckBox = ShouldShowCheckBox(_isAutoDisConnect);
            IsAutoDisConnectCheckBoxValue = GetCheckBoxState(_isAutoDisConnect);
        }

        private void UpdateIsCGDisplaySNDisplay()
        {
            IsCGDisplaySNIsBinding = IsBindingProperty(_isCGDisplaySN);
            IsCGDisplaySNDisplayText = GetDisplayText(_isCGDisplaySN);
            ShowIsCGDisplaySNCheckBox = ShouldShowCheckBox(_isCGDisplaySN);
            IsCGDisplaySNCheckBoxValue = GetCheckBoxState(_isCGDisplaySN);
        }

        private void UpdateIsCPKModeDisplay()
        {
            IsCPKModeIsBinding = IsBindingProperty(_isCPKMode);
            IsCPKModeDisplayText = GetDisplayText(_isCPKMode);
            ShowIsCPKModeCheckBox = ShouldShowCheckBox(_isCPKMode);
            IsCPKModeCheckBoxValue = GetCheckBoxState(_isCPKMode);
        }

        private void UpdateIsGRRModeDisplay()
        {
            IsGRRModeIsBinding = IsBindingProperty(_isGRRMode);
            IsGRRModeDisplayText = GetDisplayText(_isGRRMode);
            ShowIsGRRModeCheckBox = ShouldShowCheckBox(_isGRRMode);
            IsGRRModeCheckBoxValue = GetCheckBoxState(_isGRRMode);
        }

        private void UpdateCGDisplaySNDisplay()
        {
            CGDisplaySNIsBinding = IsBindingProperty(_cgDisplaySN);
            CGDisplaySNDisplayText = GetDisplayText(_cgDisplaySN);
        }

        private void UpdateSNDisplay()
        {
            SNIsBinding = IsBindingProperty(_sn);
            SNDisplayText = GetDisplayText(_sn);
        }

        private void UpdateCarrierSNDisplay()
        {
            CarrierSNIsBinding = IsBindingProperty(_carrierSN);
            CarrierSNDisplayText = GetDisplayText(_carrierSN);
        }

        private void UpdateSourceImagePathDisplay()
        {
            SourceImagePathIsBinding = IsBindingProperty(_sourceImagePath);
            SourceImagePathDisplayText = GetDisplayText(_sourceImagePath);
        }

        private void UpdateIs3ANGDisplay()
        {
            Is3ANGDisplayText = GetDisplayText(_is3ANG);
            ShowIs3ANGCheckBox = ShouldShowCheckBox(_is3ANG);
            Is3ANGCheckBoxValue = GetCheckBoxState(_is3ANG);
        }

        private void UpdateIsContainLineDisplay()
        {
            IsContainLineDisplayText = GetDisplayText(_isContainLine);
            ShowIsContainLineCheckBox = ShouldShowCheckBox(_isContainLine);
            IsContainLineCheckBoxValue = GetCheckBoxState(_isContainLine);
        }

        private void UpdateIsManualDisplay()
        {
            IsManualDisplayText = GetDisplayText(_isManual);
            ShowIsManualCheckBox = ShouldShowCheckBox(_isManual);
            IsManualCheckBoxValue = GetCheckBoxState(_isManual);
        }

        private void UpdateIsEnableCarrierDisplay()
        {
            IsEnableCarrierDisplayText = GetDisplayText(_isEnableCarrier);
            ShowIsEnableCarrierCheckBox = ShouldShowCheckBox(_isEnableCarrier);
            IsEnableCarrierCheckBoxValue = GetCheckBoxState(_isEnableCarrier);
        }

        // 辅助方法
        private bool IsBindingProperty(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            // 检查是否为绑定属性（以"绑定:"开头）
            if (value.StartsWith("绑定:"))
                return true;

            // 检查是否为 XML 格式的绑定属性
            if (value.StartsWith("<") && value.Contains("RefName="))
                return true;

            return false;
        }

        private bool ShouldShowCheckBox(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            // 如果是绑定属性，不显示复选框
            if (IsBindingProperty(value))
                return false;

            // 如果是布尔值字符串，显示复选框
            if (value == "True" || value == "False" || value == "true" || value == "false")
                return true;

            // 如果是"是"/"否"，显示复选框
            if (value == "是" || value == "否")
                return true;

            return false;
        }

        private bool? GetCheckBoxState(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (IsBindingProperty(value))
                return null; // 绑定属性不显示复选框状态

            if (value == "True" || value == "true" || value == "是")
                return true;

            if (value == "False" || value == "false" || value == "否")
                return false;

            return null;
        }

        private string GetDisplayText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // 处理 "绑定:" 前缀 - 这是关键修复
            if (value.StartsWith("绑定:"))
            {
                // 直接返回 RefName 部分，例如 "Extend_治具LCG"
                var refName = value.Substring(3).Trim();
                return refName;
            }

            // 解析 XML 格式的绑定属性，提取 RefName
            if (value.StartsWith("<") && value.Contains("RefName="))
            {
                try
                {
                    // 使用正则表达式提取 RefName 的值
                    var match = Regex.Match(value, @"RefName=""([^""]+)""");
                    if (match.Success && match.Groups.Count > 1)
                    {
                        return match.Groups[1].Value;
                    }
                }
                catch
                {
                    // 解析失败，尝试使用 XML 解析
                    try
                    {
                        // 将字符串包装成有效的 XML
                        var xmlString = $"<root>{value}</root>";
                        var xmlDoc = XDocument.Parse(xmlString);
                        var element = xmlDoc.Root.Elements().FirstOrDefault();

                        if (element != null)
                        {
                            var refNameAttr = element.Attribute("RefName");
                            if (refNameAttr != null)
                            {
                                return refNameAttr.Value;
                            }
                        }
                    }
                    catch
                    {
                        // 解析失败，返回原值
                    }
                }
            }

            // 布尔值转换
            if (value == "True" || value == "true")
                return "是";

            if (value == "False" || value == "false")
                return "否";

            return value;
        }

        private string GetShortPath(string fullPath, int maxLength)
        {
            if (string.IsNullOrEmpty(fullPath) || fullPath.Length <= maxLength)
                return fullPath;

            try
            {
                var fileName = Path.GetFileName(fullPath);
                var directory = Path.GetDirectoryName(fullPath);

                if (string.IsNullOrEmpty(fileName))
                    return "...";

                if (fileName.Length >= maxLength - 3)
                    return "..." + fileName.Substring(fileName.Length - (maxLength - 3));

                var availableLength = maxLength - fileName.Length - 3;
                if (availableLength <= 0)
                    return "..." + fileName;

                var shortDir = directory.Length > availableLength
                    ? "..." + directory.Substring(directory.Length - availableLength)
                    : directory;

                return Path.Combine(shortDir, fileName);
            }
            catch
            {
                return fullPath.Length > maxLength ? "..." + fullPath.Substring(fullPath.Length - maxLength + 3) : fullPath;
            }
        }
    }

    // 工站信息类
    public class StationInfo
    {
        public string Name { get; set; }
        public List<PDCAKeyParameterRow> Parameters { get; set; }

        public StationInfo(string name)
        {
            Name = name;
            Parameters = new List<PDCAKeyParameterRow>();
        }
    }

    internal class KeyParameterContentVM : PageVM
    {
        private readonly IDeviceEngine _deviceEngine;
        private readonly IEventAggregator _ea;
        private ObservableCollection<PDCAKeyParameterRow> _parameterRows;
        private bool _isLoading;
        private int _totalModules;

        private ObservableCollection<string> _stationList;
        private string _selectedStation;
        private Dictionary<string, List<PDCAKeyParameterRow>> _stationDataCache;

        public ObservableCollection<IMotionModule> totalAEs;
        public override bool IsShowAdd => false;
        public override bool IsShowRemove => false;
        public override bool IsShowAuto => false;
        public ObservableCollection<PDCAKeyParameterRow> ParameterRows
        {
            get => _parameterRows;
            set => SetProperty(ref _parameterRows, value);
        }

        // 工站列表
        public ObservableCollection<string> StationList
        {
            get => _stationList;
            set => SetProperty(ref _stationList, value);
        }

        // 当前选中工站
        public string SelectedStation
        {
            get => _selectedStation;
            set
            {
                if (SetProperty(ref _selectedStation, value))
                {
                    OnStationChanged();
                }
            }
        }

        // 加载状态
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // 模块总数
        public int TotalModules
        {
            get => _totalModules;
            set => SetProperty(ref _totalModules, value);
        }

        // 命令
        public ICommand ExportCommand { get; }
        public ICommand RefreshCommand { get; }
        public DelegateCommand ApplyCommand { get; set; }

        public KeyParameterContentVM(IDeviceEngine deviceEngine, IEventAggregator @event) : base(GetEngineUI(deviceEngine))
        {
            _deviceEngine = deviceEngine;
            _ea = @event;
            ParameterRows = new ObservableCollection<PDCAKeyParameterRow>();
            StationList = new ObservableCollection<string>();
            _stationDataCache = new Dictionary<string, List<PDCAKeyParameterRow>>();
            totalAEs = new ObservableCollection<IMotionModule>();

            // 初始化命令
            RefreshCommand = new DelegateCommand(RefreshData);
            ExportCommand = new DelegateCommand(ExportData);
            ApplyCommand = new DelegateCommand(OnApply);
        }

        private static ISimDeviceEngineUI GetEngineUI(IDeviceEngine deviceEngine)
        {
            if (deviceEngine == null)
                throw new ArgumentNullException(nameof(deviceEngine));
            if (deviceEngine is ISimDeviceEngineUI engineUI)
                return engineUI;
            try
            {
                return ContainerLocator.Container.Resolve<ISimDeviceEngineUI>();
            }
            catch
            {
                throw new InvalidOperationException($"无法将 IDeviceEngine 转换为 ISimDeviceEngineUI，请检查依赖注入配置。");
            }
        }
        /// <summary>
        /// 导出数据为CSV文件
        /// </summary>
        private void ExportData()
        {
            try
            {
                if (ParameterRows == null || ParameterRows.Count == 0)
                {
                    MessageBox.Show("当前工站没有数据可导出", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 创建保存文件对话框
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV文件 (*.csv)|*.csv|所有文件 (*.*)|*.*",
                    FilterIndex = 1,
                    FileName = $"PDCA参数配置_{SelectedStation ?? "未知工站"}_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    DefaultExt = ".csv",
                    Title = "导出当前工站PDCA参数配置"
                };

                // 显示保存文件对话框
                if (saveFileDialog.ShowDialog() == true)
                {
                    // 生成CSV内容
                    string csvContent = GenerateCsvContent();

                    // 写入文件
                    File.WriteAllText(saveFileDialog.FileName, csvContent, Encoding.UTF8);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 生成CSV文件内容
        /// </summary>
        private string GenerateCsvContent()
        {
            var csvBuilder = new StringBuilder();

            // 添加标题行 - 导出时间、操作人、当前工站
            csvBuilder.AppendLine($"导出时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csvBuilder.AppendLine($"工站名称：{SelectedStation ?? "未选择"}");
            csvBuilder.AppendLine();

            // 添加表头
            string[] headers = new string[]
            {
                "工站名称",
                "通信服务器地址",
                "启用",
                "PDCA读取超时时间(ms)",
                "PDCA读取失败后是否主动断连",
                "动作类型",
                "启用Display_SN上传项",
                "Display_SN",
                "CPK模式",
                "GRR模式",
                "SN",
                "虚拟码文件",
                "WIP长度",
                "CarrierSN",
                "CycleTime",
                "工位号",
                "原图片路径",
                "目标图片文件夹名称",
                "图片名称规则",
                "过程数据",
                "是否3ANG",
                "是否包含下划线",
                "手动输入AELimite",
                "启用载具查询WIP"
            };

            csvBuilder.AppendLine(string.Join(",", headers));

            // 添加数据行
            for (int i = 0; i < ParameterRows.Count; i++)
            {
                var row = ParameterRows[i];
                var csvRow = new List<string>
                {
                    EscapeCsvField(row.StationName),
                    EscapeCsvField(row.CommDevice),
                    EscapeCsvField(GetBooleanDisplayText(row.IsEnable)),
                    row.TimeOutMS.ToString(),
                    EscapeCsvField(GetBooleanDisplayText(row.IsAutoDisConnect)),
                    EscapeCsvField(row.PDCAMode),
                    EscapeCsvField(GetBooleanDisplayText(row.IsCGDisplaySN)),
                    EscapeCsvField(row.CGDisplaySN),
                    EscapeCsvField(GetBooleanDisplayText(row.IsCPKMode)),
                    EscapeCsvField(GetBooleanDisplayText(row.IsGRRMode)),
                    EscapeCsvField(row.SN),
                    EscapeCsvField(row.WIPFile),
                    row.WIPLength.ToString(),
                    EscapeCsvField(row.CarrierSN),
                    row.CycleTime.ToString("F2"),
                    row.WorkId.ToString(),
                    EscapeCsvField(row.SourceImagePath),
                    EscapeCsvField(row.DesImagePath),
                    EscapeCsvField(row.IsImageRol),
                    EscapeCsvField(row.ProdData),
                    EscapeCsvField(GetBooleanDisplayText(row.Is3ANG)),
                    EscapeCsvField(GetBooleanDisplayText(row.IsContainLine)),
                    EscapeCsvField(GetBooleanDisplayText(row.IsManual)),
                    EscapeCsvField(GetBooleanDisplayText(row.IsEnableCarrier))
                };

                csvBuilder.AppendLine(string.Join(",", csvRow));
            }

            return csvBuilder.ToString();
        }

        /// <summary>
        /// 转义CSV字段（处理逗号、引号等特殊字符）
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // 如果字段包含逗号、双引号或换行符，需要用双引号包围并转义双引号
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\r") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }

        /// <summary>
        /// 获取布尔值的显示文本
        /// </summary>
        private string GetBooleanDisplayText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value == "True" || value == "true" || value == "是")
                return "是";

            if (value == "False" || value == "false" || value == "否")
                return "否";

            return value;
        }

        private void OnApply()
        {
            OnUpdateRecipe(totalAEs);
        }

        /// <summary>
        /// 页面加载时调用
        /// </summary>
        public void OnViewLoaded()
        {
            LoadPDCAModulesFromMotionEngine();
        }

        /// <summary>
        /// 工站变更事件处理
        /// </summary>
        private void OnStationChanged()
        {
            if (string.IsNullOrEmpty(SelectedStation) || !_stationDataCache.ContainsKey(SelectedStation))
            {
                ParameterRows.Clear();
                TotalModules = 0;
                return;
            }

            // 从缓存中获取对应工站的数据
            var rows = _stationDataCache[SelectedStation];
            ParameterRows.Clear();
            foreach (var row in rows)
            {
                ParameterRows.Add(row);
            }
            TotalModules = ParameterRows.Count;
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        private void RefreshData()
        {
            LoadPDCAModulesFromMotionEngine();
        }

        /// <summary>
        /// 从MotionEngine加载PDCA模块
        /// </summary>
        private void LoadPDCAModulesFromMotionEngine()
        {
            try
            {
                IsLoading = true;

                // 清空缓存和列表
                ParameterRows.Clear();
                StationList.Clear();
                _stationDataCache.Clear();
                totalAEs.Clear();

                // 使用新方法获取所有PDCA模块
                var pdcaModules = _deviceEngine.GetPDCAModulesFromMotionEngine();
                foreach (var item in pdcaModules)
                {
                    var str = item.GetType().ToString();
                    if (item is IMotionModule module && module.TaskFunction.GetType().FullName == typeof(PDCAELimit).FullName)
                    {
                        dynamic t = item;
                        totalAEs.Add(t);
                    }
                }

                if (pdcaModules == null || !pdcaModules.Any())
                {
                    TotalModules = 0;
                    return;
                }

                // 转换为PDCA参数行
                ConvertToPDCAKeyParameterRow(totalAEs, out var rows);

                // 清空当前列表
                StationList.Clear();
                _stationDataCache.Clear();

                // 为每个模块创建一个下拉选项
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    string stationDisplayName = row.StationName;

                    // 直接添加到下拉列表
                    StationList.Add(stationDisplayName);

                    // 每个选项只包含对应的这一行数据
                    _stationDataCache[stationDisplayName] = new List<PDCAKeyParameterRow> { row };
                }

                // 默认选择第一个工站
                if (StationList.Count > 0)
                {
                    SelectedStation = StationList[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载PDCA模块失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ConvertToPDCAKeyParameterRow(ObservableCollection<IMotionModule> pDCAELimits, out ObservableCollection<PDCAKeyParameterRow> parameterRows)
        {
            parameterRows = new ObservableCollection<PDCAKeyParameterRow>();
            if (pDCAELimits == null) return;

            foreach (var module in pDCAELimits)
            {
                try
                {
                    // 解析模块信息
                    var stationName = GetStationNameFromModule(module);
                    var moduleName = GetModuleNameFromModule(module);
                    var values = GetValuesFromModule(module);
                    var parameters = module.Parameters;

                    if (values != null)
                    {
                        var row = new PDCAKeyParameterRow
                        {
                            StationName = $"{stationName} - {moduleName}",
                            SourceModule = module
                        };

                        // 从Values字典中提取参数，同时检查是否有绑定属性
                        ExtractParametersFromValues(row, values, parameters);
                        parameterRows.Add(row);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理PDCA模块失败: {ex.Message}");
                }
            }
        }

        #region 

        private string GetStationNameFromModule(IMotionModule module)
        {
            try
            {
                if (module.Station == null)
                {
                    module.UpdateStation();
                }

                var station = module.Station as IMotionModule;
                return station?.Alias ?? "未知工站";
            }
            catch
            {
                return "未知工站";
            }
        }

        private string GetModuleNameFromModule(IMotionModule module)
        {
            return module?.Alias ?? module?.GetType()?.Name ?? "未知模块"; ;
        }

        /// <summary>
        /// 提取PDCA参数
        /// </summary>
        private Dictionary<string, object> GetValuesFromModule(IMotionModule module)
        {
            var parameters = new Dictionary<string, object>();

            if (module?.Parameters == null) return parameters;

            // PDCA参数列表
            var pdcaKeys = new List<string>
            {
                "CommDevice", "IsEnable", "TimeOutMS", "IsAutoDisConnect",
                "PDCAMode", "IsCGDisplaySN", "CGDisplaySN", "IsCPKMode",
                "IsGRRMode", "SN", "WIPFile", "WIP_Length", "CarrierSN",
                "CycleTime", "WorkId", "SourceImagePath", "DesImagePath",
                "IsImageRol", "ProdData", "Is3ANG", "IsContainLine",
                "IsManual", "IsEnableCarrier"
            };

            foreach (var key in pdcaKeys)
            {
                if (module.Parameters.TryGetValue(key, out var param))
                {
                    // 获取参数值
                    object value = GetParameterValue(param);

                    // 如果值为空，使用默认值
                    if (value == null)
                    {
                        value = GetDefaultValue(param, key);
                    }

                    parameters[key] = value;
                }
                else
                {
                    parameters[key] = GetDefaultValue(null, key);
                }
            }

            return parameters;
        }

        /// <summary>
        /// 获取参数值
        /// </summary>
        private object GetParameterValue(ParameterAttribute param)
        {
            if (param == null) return null;

            if (param.RefOut != null)
            {
                string refName = param.RefOut.Name ?? "未知引用";
                return $"绑定: {refName}";
            }

            try
            {
                string errMsg;
                var value = param.GetValue(out errMsg);

                if (!string.IsNullOrEmpty(errMsg))
                {
                    return param.DefaultV ?? param.Value;
                }

                return value;
            }
            catch (Exception)
            {
                // 异常情况下返回 Value 或 DefaultV
                return param.Value ?? param.DefaultV;
            }
        }

        /// <summary>
        /// 获取默认值
        /// </summary>
        private object GetDefaultValue(ParameterAttribute param, string key)
        {
            // 优先使用参数的DefaultV
            if (param?.DefaultV != null)
            {
                return param.DefaultV;
            }
            // 使用预设默认值
            switch (key)
            {
                case "IsEnable":
                case "IsAutoDisConnect":
                case "IsCGDisplaySN":
                case "IsCPKMode":
                case "IsGRRMode":
                case "Is3ANG":
                case "IsContainLine":
                case "IsManual":
                case "IsEnableCarrier":
                    return false;
                case "TimeOutMS":
                    return 5000;
                case "WIP_Length":
                case "WorkId":
                    return 0;
                case "CycleTime":
                    return 0.0;
                case "PDCAMode":
                    return "Whole";
                case "CommDevice":
                    return "未设置";
                default:
                    return string.Empty;
            }
        }

        #endregion

        private void ConvertToPDCAELimit(ObservableCollection<PDCAKeyParameterRow> parameterRows, out ObservableCollection<IMotionModule> pDCAELimits)
        {
            pDCAELimits = new ObservableCollection<IMotionModule>();
            if (parameterRows == null) return;

            foreach (var row in parameterRows)
            {
                if (row.SourceModule is IMotionModule module)
                {
                    // 直接更新模块的参数
                    UpdateModuleFromRow(row, module);
                    pDCAELimits.Add(module);
                }
            }
        }

        private void OnUpdateRecipe(ObservableCollection<IMotionModule> pDCAELimits)
        {
            // 更新当前工站的所有数据
            var currentStationRows = new ObservableCollection<PDCAKeyParameterRow>(ParameterRows);
            ConvertToPDCAELimit(currentStationRows, out var modules);

            if (pDCAELimits != null)
            {
                // 只更新当前工站的模块
                foreach (var module in modules)
                {
                    _ea.GetEvent<ModuleUpdateEvent>().Publish(new ModuleUpdateModule { Module = module, UpdateType = ModuleUpdate.ParameterVal });
                }
            }
        }

        private void UpdateModuleFromRow(PDCAKeyParameterRow row, IMotionModule module)
        {
            if (module?.Parameters == null) return;

            SetModuleParameter(module, "IsEnable", row.IsEnable);
            SetModuleParameter(module, "TimeOutMS", row.TimeOutMS);
            SetModuleParameter(module, "IsAutoDisConnect", row.IsAutoDisConnect);
            SetModuleParameter(module, "PDCAMode", row.PDCAMode);
            SetModuleParameter(module, "IsCGDisplaySN", row.IsCGDisplaySN);
            SetModuleParameter(module, "CGDisplaySN", row.CGDisplaySN);
            SetModuleParameter(module, "IsCPKMode", row.IsCPKMode);
            SetModuleParameter(module, "IsGRRMode", row.IsGRRMode);
            SetModuleParameter(module, "SN", row.SN);
            SetModuleParameter(module, "WIPFile", row.WIPFile);
            SetModuleParameter(module, "WIP_Length", row.WIPLength);
            SetModuleParameter(module, "CarrierSN", row.CarrierSN);
            SetModuleParameter(module, "CycleTime", row.CycleTime);
            SetModuleParameter(module, "WorkId", row.WorkId);
            SetModuleParameter(module, "SourceImagePath", row.SourceImagePath);
            SetModuleParameter(module, "DesImagePath", row.DesImagePath);
            SetModuleParameter(module, "IsImageRol", row.IsImageRol);
            SetModuleParameter(module, "ProdData", row.ProdData);
            SetModuleParameter(module, "Is3ANG", row.Is3ANG);
            SetModuleParameter(module, "IsContainLine", row.IsContainLine);
            SetModuleParameter(module, "IsManual", row.IsManual);
            SetModuleParameter(module, "IsEnableCarrier", row.IsEnableCarrier);
            SetModuleParameter(module, "CommDevice", row.CommDevice);
        }

        private void SetModuleParameter(IMotionModule module, string key, object newValue)
        {
            if (!module.Parameters.TryGetValue(key, out var param)) return;

            try
            {
                var type = param.Type;
                object valueToSet = newValue;

                if (type == typeof(bool))
                {
                    if (newValue is string s)
                    {
                        if (bool.TryParse(s, out bool b)) valueToSet = b;
                        else if (s == "是" || s == "True" || s == "true") valueToSet = true;
                        else if (s == "否" || s == "False" || s == "false") valueToSet = false;
                    }
                    else if (newValue is bool b) valueToSet = b;
                }
                else if (type == typeof(int))
                {
                    if (int.TryParse(newValue?.ToString(), out int res)) valueToSet = res;
                }
                else if (type == typeof(double))
                {
                    if (double.TryParse(newValue?.ToString(), out double res)) valueToSet = res;
                }
                else if (type.IsEnum)
                {
                    try
                    {
                        string s = newValue.ToString();
                        if (s == "开始") s = "Start";
                        else if (s == "获取WIP") s = "GetWIP";
                        else if (s == "图片拷贝") s = "CopyImage";
                        else if (s == "数据发送") s = "SendData";
                        else if (s == "所有动作") s = "Whole";
                        else if (s == "结束") s = "End";
                        else if (s == "CCTV文件拷贝") s = "CCTV";

                        valueToSet = Enum.Parse(type, s);
                    }
                    catch
                    {

                    }
                }
                else if (type == typeof(LPath))
                {
                    valueToSet = new LPath(newValue?.ToString() ?? "");
                }

                // 设置参数值
                param.Value = valueToSet;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新参数 {key} 失败: {ex.Message}");
            }
        }


        /// <summary>
        /// 从Values字典中提取参数，检查绑定属性
        /// </summary>
        private void ExtractParametersFromValues(PDCAKeyParameterRow row, Dictionary<string, object> values, Dictionary<string, ParameterAttribute> parameters = null)
        {
            // 直接遍历所有值进行设置
            foreach (var kvp in values)
            {
                SetPDCAParameter(row, kvp.Key, kvp.Value, parameters);
            }
        }

        /// <summary>
        /// 设置PDCA参数，处理绑定属性
        /// </summary>
        private void SetPDCAParameter(PDCAKeyParameterRow row, string key, object value, Dictionary<string, ParameterAttribute> parameters = null)
        {
            if (value == null) return;

            try
            {
                string stringValue = value.ToString();
                bool hasBinding = false;
                string bindingRefName = "";
                bool showCheckBox = false;
                bool? checkBoxValue = null;

                // 检查是否有绑定属性
                if (parameters != null && parameters.TryGetValue(key, out var paramAttr))
                {
                    // 检查是否有实际绑定
                    if (paramAttr.RefOut != null)
                    {
                        // 有绑定，显示绑定信息
                        stringValue = $"绑定: {paramAttr.RefOut.Name}";
                        hasBinding = true;
                        bindingRefName = paramAttr.RefOut.Name;
                    }
                    else
                    {
                        // 没有绑定，尝试从值中解析布尔值
                        if (paramAttr.Type == typeof(bool))
                        {
                            if (bool.TryParse(stringValue, out bool boolValue))
                            {
                                checkBoxValue = boolValue;
                            }
                            else if (stringValue == "是")
                            {
                                checkBoxValue = true;
                            }
                            else if (stringValue == "否")
                            {
                                checkBoxValue = false;
                            }
                        }
                    }

                    // 处理布尔类型参数显示文本
                    if (paramAttr.Type == typeof(bool) && !hasBinding)
                    {
                        if (checkBoxValue.HasValue)
                        {
                            stringValue = checkBoxValue.Value ? "True" : "False";
                        }
                        else if (stringValue == "是")
                        {
                            stringValue = "True";
                            checkBoxValue = true;
                        }
                        else if (stringValue == "否")
                        {
                            stringValue = "False";
                            checkBoxValue = false;
                        }
                    }

                    if (paramAttr.CanRef == ParamRef.Ref)
                    {
                        // 有 CanRef == Ref 属性
                        if (!hasBinding)
                        {
                            // 检查是否是需要特殊处理的参数（显示文本框而不是复选框）
                            bool isSpecialParameter = key == "TimeOutMS" || key == "SN" || key == "WIPFile" ||
                                                      key == "CarrierSN" || key == "CycleTime" || key == "WorkId" ||
                                                      key == "SourceImagePath" || key == "DesImagePath" ||
                                                      key == "IsImageRol" || key == "ProdData" || key == "CGDisplaySN" ||
                                                      key == "CommDevice";

                            if (isSpecialParameter)
                            {
                                // 没有绑定时显示文本框
                                showCheckBox = false;
                            }
                            else if (paramAttr.Type == typeof(bool))
                            {
                                // 没有绑定时显示复选框
                                showCheckBox = true;
                                // 如果还没有设置值，使用默认值 False
                                if (!checkBoxValue.HasValue)
                                {
                                    checkBoxValue = false;
                                    stringValue = "False";
                                }
                            }
                        }
                    }
                    else
                    {
                        // 没有 CanRef == Ref 属性
                        if (paramAttr.Type == typeof(bool))
                        {
                            // 对于布尔类型，总是显示复选框
                            showCheckBox = true;
                        }
                    }
                }
                else if (value is XElement xmlElement)
                {
                    // 处理 XElement 对象
                    stringValue = xmlElement.ToString();
                    var refNameAttr = xmlElement.Attribute("RefName");
                    if (refNameAttr != null)
                    {
                        hasBinding = true;
                        bindingRefName = refNameAttr.Value;
                        stringValue = $"绑定: {bindingRefName}";
                    }
                }
                else if (value is string strValue && strValue.StartsWith("<") && strValue.Contains("RefName="))
                {
                    // 处理 XML 字符串
                    try
                    {
                        var xmlString = $"<root>{strValue}</root>";
                        var xmlDoc = XDocument.Parse(xmlString);
                        var element = xmlDoc.Root.Elements().FirstOrDefault();
                        var refNameAttr = element?.Attribute("RefName");

                        if (refNameAttr != null)
                        {
                            hasBinding = true;
                            bindingRefName = refNameAttr.Value;
                            stringValue = $"绑定: {bindingRefName}";
                        }
                    }
                    catch
                    {

                    }
                }

                // 根据参数名设置相应的属性
                switch (key)
                {
                    case "IsEnable":
                        row.IsEnableIsBinding = hasBinding;
                        row.ShowIsEnableCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue)
                        {
                            row.IsEnableCheckBoxValue = checkBoxValue.Value;
                        }
                        break;
                    case "IsAutoDisConnect":
                        row.IsAutoDisConnectIsBinding = hasBinding;
                        row.ShowIsAutoDisConnectCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue)
                        {
                            row.IsAutoDisConnectCheckBoxValue = checkBoxValue.Value;
                        }
                        break;
                    case "IsCGDisplaySN":
                        row.IsCGDisplaySNIsBinding = hasBinding;
                        row.ShowIsCGDisplaySNCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue)
                        {
                            row.IsCGDisplaySNCheckBoxValue = checkBoxValue.Value;
                        }
                        break;
                    case "CGDisplaySN":
                        row.CGDisplaySNIsBinding = hasBinding;
                        break;
                    case "IsCPKMode":
                        row.IsCPKModeIsBinding = hasBinding;
                        row.ShowIsCPKModeCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue)
                        {
                            row.IsCPKModeCheckBoxValue = checkBoxValue.Value;
                        }
                        break;
                    case "IsGRRMode":
                        row.IsGRRModeIsBinding = hasBinding;
                        row.ShowIsGRRModeCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue)
                        {
                            row.IsGRRModeCheckBoxValue = checkBoxValue.Value;
                        }
                        break;
                    case "Is3ANG":
                        row.ShowIs3ANGCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue)
                        {
                            row.Is3ANGCheckBoxValue = checkBoxValue.Value;
                        }
                        break;
                    case "SN":
                        row.SNIsBinding = hasBinding;
                        break;
                    case "CarrierSN":
                        row.CarrierSNIsBinding = hasBinding;
                        break;
                    case "SourceImagePath":
                        row.SourceImagePathIsBinding = hasBinding;
                        break;
                    case "IsContainLine":
                        row.ShowIsContainLineCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue)
                        {
                            row.IsContainLineCheckBoxValue = checkBoxValue.Value;
                        }
                        break;
                    case "IsManual":
                        row.ShowIsManualCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue)
                        {
                            row.IsManualCheckBoxValue = checkBoxValue.Value;
                        }
                        break;
                    case "IsEnableCarrier":
                        row.ShowIsEnableCarrierCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue)
                        {
                            row.IsEnableCarrierCheckBoxValue = checkBoxValue.Value;
                        }
                        break;
                }

                // 设置参数值
                switch (key)
                {
                    case "CommDevice":
                        row.CommDevice = stringValue;
                        break;

                    case "IsEnable":
                        row.IsEnable = stringValue;
                        break;

                    case "TimeOutMS":
                        if (TryParseInt(value, out int timeout)) row.TimeOutMS = timeout;
                        break;

                    case "IsAutoDisConnect":
                        row.IsAutoDisConnect = stringValue;
                        break;

                    case "PDCAMode":
                        // 使用 GetEnumDescription 获取显示文本
                        string displayText = GetEnumDescription(value);
                        row.PDCAMode = displayText;
                        break;

                    case "IsCGDisplaySN":
                        row.IsCGDisplaySN = stringValue;
                        break;

                    case "CGDisplaySN":
                        row.CGDisplaySN = stringValue;
                        break;

                    case "IsCPKMode":
                        row.IsCPKMode = stringValue;
                        break;

                    case "IsGRRMode":
                        row.IsGRRMode = stringValue;
                        break;

                    case "SN":
                        row.SN = stringValue;
                        break;

                    case "WIPFile":
                        row.WIPFile = stringValue;
                        break;

                    case "WIP_Length":
                        if (TryParseInt(value, out int wipLength)) row.WIPLength = wipLength;
                        break;

                    case "CarrierSN":
                        row.CarrierSN = stringValue;
                        break;

                    case "CycleTime":
                        if (TryParseDouble(value, out double cycleTime)) row.CycleTime = cycleTime;
                        break;

                    case "WorkId":
                        if (TryParseInt(value, out int workId)) row.WorkId = workId;
                        break;

                    case "SourceImagePath":
                        row.SourceImagePath = stringValue;
                        break;

                    case "DesImagePath":
                        row.DesImagePath = stringValue;
                        break;

                    case "IsImageRol":
                        row.IsImageRol = stringValue;
                        break;

                    case "ProdData":
                        row.ProdData = stringValue;
                        break;

                    case "Is3ANG":
                        row.Is3ANG = stringValue;
                        break;

                    case "IsContainLine":
                        row.IsContainLine = stringValue;
                        break;

                    case "IsManual":
                        row.IsManual = stringValue;
                        break;

                    case "IsEnableCarrier":
                        row.IsEnableCarrier = stringValue;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置参数 {key} 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 尝试解析整数
        /// </summary>
        private bool TryParseInt(object value, out int result)
        {
            result = 0;
            if (value == null) return false;

            if (value is int intValue)
            {
                result = intValue;
                return true;
            }

            return int.TryParse(value.ToString(), out result);
        }

        /// <summary>
        /// 尝试解析浮点数
        /// </summary>
        private bool TryParseDouble(object value, out double result)
        {
            result = 0;
            if (value == null) return false;

            if (value is double doubleValue)
            {
                result = doubleValue;
                return true;
            }

            if (value is float floatValue)
            {
                result = floatValue;
                return true;
            }

            return double.TryParse(value.ToString(), out result);
        }

        /// <summary>
        /// 获取枚举描述
        /// </summary>
        private string GetEnumDescription(object value)
        {
            if (value == null) return string.Empty;

            try
            {
                // 如果是字符串，处理动作类型的转换
                if (value is string stringValue)
                {
                    if (stringValue == "Start" || stringValue == "PDCAType.Start") return "开始";
                    if (stringValue == "GetWIP" || stringValue == "PDCAType.GetWIP") return "获取WIP";
                    if (stringValue == "CopyImage" || stringValue == "PDCAType.CopyImage") return "图片拷贝";
                    if (stringValue == "SendData" || stringValue == "PDCAType.SendData") return "数据发送";
                    if (stringValue == "Whole" || stringValue == "PDCAType.Whole") return "所有动作";
                    if (stringValue == "End" || stringValue == "PDCAType.End") return "结束";
                    if (stringValue == "CCTV" || stringValue == "PDCAType.CCTV") return "CCTV文件拷贝";

                    if (stringValue == "开始" || stringValue == "获取WIP" || stringValue == "图片拷贝" ||
                        stringValue == "数据发送" || stringValue == "所有动作" || stringValue == "结束" ||
                        stringValue == "CCTV文件拷贝")
                        return stringValue;
                }

                // 如果是枚举类型
                var type = value.GetType();
                if (type.IsEnum)
                {
                    // 获取枚举值的字符串表示
                    string enumString = value.ToString();

                    if (enumString == "Start") return "开始";
                    if (enumString == "GetWIP") return "获取WIP";
                    if (enumString == "CopyImage") return "图片拷贝";
                    if (enumString == "SendData") return "数据发送";
                    if (enumString == "Whole") return "所有动作";
                    if (enumString == "End") return "结束";
                    if (enumString == "CCTV") return "CCTV文件拷贝";

                    var field = type.GetField(enumString);
                    if (field == null) return enumString;

                    var descAttr = field.GetCustomAttribute<DescriptionAttribute>();
                    return descAttr?.Description ?? enumString;
                }
                return value.ToString();
            }
            catch
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        private object GetPropertyValue(object obj, string propertyName)
        {
            try
            {
                if (obj == null) return null;
                var type = obj.GetType();
                var property = type.GetProperty(propertyName);

                if (property == null)
                {
                    var field = type.GetField(propertyName);
                    if (field != null)
                    {
                        return field.GetValue(obj);
                    }
                    return null;
                }

                return property.GetValue(obj);
            }
            catch
            {
                return null;
            }
        }
    }
}