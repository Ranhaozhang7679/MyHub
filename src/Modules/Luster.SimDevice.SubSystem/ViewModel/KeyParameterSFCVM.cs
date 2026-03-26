using HandyControl.Data;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Module.Motion.Business.Functions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.Integration.SFC;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
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

namespace Luster.SimDevice.SubSystem.ViewModel
{
    // SFC参数行数据模型
    public class SFCKeyParameterRow : BindableBase
    {
        private string _stationName;
        private string _sfcMode;
        private string _isSFCEnable;
        private string _isFirstStation;
        private string _isForceStationName;
        private string _forceStationName;
        private string _forceStationType;
        private string _queryPrevStation;
        private string _isUnBind;
        private string _isAutoBuildWip;
        private string _sn;
        private string _isTwo;
        private string _lcfm;
        private string _carrierSN;
        private int _wipLength;
        private int _wipLength2;
        private string _isOK;
        private string _sfcUrl2;
        private string _useSFCUrl2;
        private string _errorCode;
        private string _errorMsg;
        private string _stationID;
        private string _checkFlag;
        private string _sfcStationCode;
        private string _test_station_name;
        private string _product;
        private string _stationDisplayName;
        private string _mac_address;
        private string _defaultNotPass;
        private string _ccdflag;
        private string _partname;
        private string _flexSN;
        private string _queryRealTime;
        private string _isTryrun;
        private string _isQueryRepair;
        private string _isUsePDCA;
        private string _alarmCode;
        private string _isNoLCFM;
        private string _returnOKValue;
        private string _flexStationID;
        private string _isStationLeft;
        private string _rnsStation;

        // 显示属性 - 支持双向绑定
        private bool _showIsSFCEnableCheckBox;
        private bool? _isSFCEnableCheckBoxValue;
        private string _isSFCEnableDisplayText;
        private bool _isSFCEnableIsBinding;

        private bool _showIsFirstStationCheckBox;
        private bool? _isFirstStationCheckBoxValue;
        private string _isFirstStationDisplayText;
        private bool _isFirstStationIsBinding;

        private bool _showIsForceStationNameCheckBox;
        private bool? _isForceStationNameCheckBoxValue;
        private string _isForceStationNameDisplayText;
        private bool _isForceStationNameIsBinding;

        private bool _showQueryPrevStationCheckBox;
        private bool? _queryPrevStationCheckBoxValue;
        private string _queryPrevStationDisplayText;
        private bool _queryPrevStationIsBinding;

        private bool _showIsUnBindCheckBox;
        private bool? _isUnBindCheckBoxValue;
        private string _isUnBindDisplayText;
        private bool _isUnBindIsBinding;

        private bool _showIsAutoBuildWipCheckBox;
        private bool? _isAutoBuildWipCheckBoxValue;
        private string _isAutoBuildWipDisplayText;
        private bool _isAutoBuildWipIsBinding;

        private bool _showIsTwoCheckBox;
        private bool? _isTwoCheckBoxValue;
        private string _isTwoDisplayText;
        private bool _isTwoIsBinding;

        private bool _showIsOKCheckBox;
        private bool? _isOKCheckBoxValue;
        private string _isOKDisplayText;
        private bool _isOKIsBinding;

        private bool _showUseSFCUrl2CheckBox;
        private bool? _useSFCUrl2CheckBoxValue;
        private string _useSFCUrl2DisplayText;
        private bool _useSFCUrl2IsBinding;

        private bool _showDefaultNotPassCheckBox;
        private bool? _defaultNotPassCheckBoxValue;
        private string _defaultNotPassDisplayText;
        private bool _defaultNotPassIsBinding;

        private bool _showIsQueryRepairCheckBox;
        private bool? _isQueryRepairCheckBoxValue;
        private string _isQueryRepairDisplayText;
        private bool _isQueryRepairIsBinding;

        private bool _showIsUsePDCACheckBox;
        private bool? _isUsePDCACheckBoxValue;
        private string _isUsePDCADisplayText;
        private bool _isUsePDCAIsBinding;

        private bool _showIsNoLCFMCheckBox;
        private bool? _isNoLCFMCheckBoxValue;
        private string _isNoLCFMDisplayText;
        private bool _isNoLCFMIsBinding;

        private bool _showIsStationLeftCheckBox;
        private bool? _isStationLeftCheckBoxValue;
        private string _isStationLeftDisplayText;
        private bool _isStationLeftIsBinding;

        private bool _showIsTryrunCheckBox;
        private bool? _isTryrunCheckBoxValue;
        private string _isTryrunDisplayText;
        private bool _isTryrunIsBinding;

        private string _forceStationNameDisplayText;
        private bool _forceStationNameIsBinding;

        private string _forceStationTypeDisplayText;
        private bool _forceStationTypeIsBinding;

        private string _sNDisplayText;
        private bool _sNIsBinding;

        private string _lcfmDisplayText;
        private bool _lcfmIsBinding;

        private string _carrierSNDisplayText;
        private bool _carrierSNIsBinding;

        private string _sfcUrl2DisplayText;
        private bool _sfcUrl2IsBinding;

        private string _errorCodeDisplayText;
        private bool _errorCodeIsBinding;

        private string _errorMsgDisplayText;
        private bool _errorMsgIsBinding;

        private string _stationIDDisplayText;
        private bool _stationIDIsBinding;

        private string _checkFlagDisplayText;
        private bool _checkFlagIsBinding;

        private string _sfcStationCodeDisplayText;
        private bool _sfcStationCodeIsBinding;

        private string _test_station_nameDisplayText;
        private bool _test_station_nameIsBinding;

        private string _productDisplayText;
        private bool _productIsBinding;

        private string _stationNameDisplayText;
        private bool _stationNameIsBinding;

        private string _mac_addressDisplayText;
        private bool _mac_addressIsBinding;

        private string _ccdflagDisplayText;
        private bool _ccdflagIsBinding;

        private string _partnameDisplayText;
        private bool _partnameIsBinding;

        private string _flexSNDisplayText;
        private bool _flexSNIsBinding;

        private string _queryRealTimeDisplayText;
        private bool _queryRealTimeIsBinding;

        private string _alarmCodeDisplayText;
        private bool _alarmCodeIsBinding;

        private string _returnOKValueDisplayText;
        private bool _returnOKValueIsBinding;

        private string _flexStationIDDisplayText;
        private bool _flexStationIDIsBinding;

        private string _rnsStationDisplayText;
        private bool _rnsStationIsBinding;

        // 源模块引用
        public IMotionModule SourceModule { get; set; }

        // 工站名称
        public string StationDisplayName
        {
            get => _stationDisplayName;
            set => SetProperty(ref _stationDisplayName, value);
        }

        // 动作类型
        public string SfcMode
        {
            get => _sfcMode;
            set => SetProperty(ref _sfcMode, value);
        }

        // SFC启用
        public string IsSFCEnable
        {
            get => _isSFCEnable;
            set
            {
                if (SetProperty(ref _isSFCEnable, value))
                {
                    UpdateIsSFCEnableDisplay();
                }
            }
        }

        // 是否是首站
        public string IsFirstStation
        {
            get => _isFirstStation;
            set
            {
                if (SetProperty(ref _isFirstStation, value))
                {
                    UpdateIsFirstStationDisplay();
                }
            }
        }

        // 是否强制本站名称
        public string IsForceStationName
        {
            get => _isForceStationName;
            set
            {
                if (SetProperty(ref _isForceStationName, value))
                {
                    UpdateIsForceStationNameDisplay();
                }
            }
        }

        // 本站名称
        public string ForceStationName
        {
            get => _forceStationName;
            set
            {
                if (SetProperty(ref _forceStationName, value))
                {
                    UpdateForceStationNameDisplay();
                }
            }
        }

        // 本站Type
        public string ForceStationType
        {
            get => _forceStationType;
            set
            {
                if (SetProperty(ref _forceStationType, value))
                {
                    UpdateForceStationTypeDisplay();
                }
            }
        }

        // 是否查询上一站结果
        public string QueryPrevStation
        {
            get => _queryPrevStation;
            set
            {
                if (SetProperty(ref _queryPrevStation, value))
                {
                    UpdateQueryPrevStationDisplay();
                }
            }
        }

        // 是否解绑治具码
        public string IsUnBind
        {
            get => _isUnBind;
            set
            {
                if (SetProperty(ref _isUnBind, value))
                {
                    UpdateIsUnBindDisplay();
                }
            }
        }

        // 根据工单自动生成WIP
        public string IsAutoBuildWip
        {
            get => _isAutoBuildWip;
            set
            {
                if (SetProperty(ref _isAutoBuildWip, value))
                {
                    UpdateIsAutoBuildWipDisplay();
                }
            }
        }

        // SN编码
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

        // 查两次
        public string IsTwo
        {
            get => _isTwo;
            set
            {
                if (SetProperty(ref _isTwo, value))
                {
                    UpdateIsTwoDisplay();
                }
            }
        }

        // LCFM
        public string LCFM
        {
            get => _lcfm;
            set
            {
                if (SetProperty(ref _lcfm, value))
                {
                    UpdateLCFMDisplay();
                }
            }
        }

        // 治具码
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

        // Wip码长度
        public int WipLength
        {
            get => _wipLength;
            set => SetProperty(ref _wipLength, value);
        }

        // Wip码长度2
        public int WipLength2
        {
            get => _wipLength2;
            set => SetProperty(ref _wipLength2, value);
        }

        // 上传Pass/Fail
        public string IsOK
        {
            get => _isOK;
            set
            {
                if (SetProperty(ref _isOK, value))
                {
                    UpdateIsOKDisplay();
                }
            }
        }

        // SFCUrl2
        public string SFCUrl2
        {
            get => _sfcUrl2;
            set
            {
                if (SetProperty(ref _sfcUrl2, value))
                {
                    UpdateSFCUrl2Display();
                }
            }
        }

        // UseSFCUrl2
        public string UseSFCUrl2
        {
            get => _useSFCUrl2;
            set
            {
                if (SetProperty(ref _useSFCUrl2, value))
                {
                    UpdateUseSFCUrl2Display();
                }
            }
        }

        // NG码
        public string ErrorCode
        {
            get => _errorCode;
            set
            {
                if (SetProperty(ref _errorCode, value))
                {
                    UpdateErrorCodeDisplay();
                }
            }
        }

        // NG信息
        public string ErrorMsg
        {
            get => _errorMsg;
            set
            {
                if (SetProperty(ref _errorMsg, value))
                {
                    UpdateErrorMsgDisplay();
                }
            }
        }

        // stationID
        public string StationID
        {
            get => _stationID;
            set
            {
                if (SetProperty(ref _stationID, value))
                {
                    UpdateStationIDDisplay();
                }
            }
        }

        // check_flag
        public string CheckFlag
        {
            get => _checkFlag;
            set
            {
                if (SetProperty(ref _checkFlag, value))
                {
                    UpdateCheckFlagDisplay();
                }
            }
        }

        // sfcStationCode
        public string SfcStationCode
        {
            get => _sfcStationCode;
            set
            {
                if (SetProperty(ref _sfcStationCode, value))
                {
                    UpdateSfcStationCodeDisplay();
                }
            }
        }

        // test_station_name
        public string Test_station_name
        {
            get => _test_station_name;
            set
            {
                if (SetProperty(ref _test_station_name, value))
                {
                    UpdateTest_station_nameDisplay();
                }
            }
        }

        // product
        public string Product
        {
            get => _product;
            set
            {
                if (SetProperty(ref _product, value))
                {
                    UpdateProductDisplay();
                }
            }
        }

        // StationName
        public string StationNameProp
        {
            get => _stationName;
            set
            {
                if (SetProperty(ref _stationName, value))
                {
                    UpdateStationNameDisplay();
                }
            }
        }

        // mac_address
        public string Mac_address
        {
            get => _mac_address;
            set
            {
                if (SetProperty(ref _mac_address, value))
                {
                    UpdateMac_addressDisplay();
                }
            }
        }

        // 本站强制未做
        public string DefaultNotPass
        {
            get => _defaultNotPass;
            set
            {
                if (SetProperty(ref _defaultNotPass, value))
                {
                    UpdateDefaultNotPassDisplay();
                }
            }
        }

        // ccdflag
        public string Ccdflag
        {
            get => _ccdflag;
            set
            {
                if (SetProperty(ref _ccdflag, value))
                {
                    UpdateCcdflagDisplay();
                }
            }
        }

        // partname
        public string Partname
        {
            get => _partname;
            set
            {
                if (SetProperty(ref _partname, value))
                {
                    UpdatePartnameDisplay();
                }
            }
        }

        // FlexSN
        public string FlexSN
        {
            get => _flexSN;
            set
            {
                if (SetProperty(ref _flexSN, value))
                {
                    UpdateFlexSNDisplay();
                }
            }
        }

        // 实名制查询时间
        public string QueryRealTime
        {
            get => _queryRealTime;
            set
            {
                if (SetProperty(ref _queryRealTime, value))
                {
                    UpdateQueryRealTimeDisplay();
                }
            }
        }

        // isTryrun
        public string IsTryrun
        {
            get => _isTryrun;
            set
            {
                if (SetProperty(ref _isTryrun, value))
                {
                    UpdateIsTryrunDisplay();
                }
            }
        }

        // 查询维修
        public string IsQueryRepair
        {
            get => _isQueryRepair;
            set
            {
                if (SetProperty(ref _isQueryRepair, value))
                {
                    UpdateIsQueryRepairDisplay();
                }
            }
        }

        // 是否使用PDCA上传
        public string IsUsePDCA
        {
            get => _isUsePDCA;
            set
            {
                if (SetProperty(ref _isUsePDCA, value))
                {
                    UpdateIsUsePDCADisplay();
                }
            }
        }

        // 报警代码
        public string AlarmCode
        {
            get => _alarmCode;
            set
            {
                if (SetProperty(ref _alarmCode, value))
                {
                    UpdateAlarmCodeDisplay();
                }
            }
        }

        // 无LCFM
        public string IsNoLCFM
        {
            get => _isNoLCFM;
            set
            {
                if (SetProperty(ref _isNoLCFM, value))
                {
                    UpdateIsNoLCFMDisplay();
                }
            }
        }

        // SFCOK返回值
        public string ReturnOKValue
        {
            get => _returnOKValue;
            set
            {
                if (SetProperty(ref _returnOKValue, value))
                {
                    UpdateReturnOKValueDisplay();
                }
            }
        }

        // Flex工站ID
        public string FlexStationID
        {
            get => _flexStationID;
            set
            {
                if (SetProperty(ref _flexStationID, value))
                {
                    UpdateFlexStationIDDisplay();
                }
            }
        }

        // 左工位
        public string IsStationLeft
        {
            get => _isStationLeft;
            set
            {
                if (SetProperty(ref _isStationLeft, value))
                {
                    UpdateIsStationLeftDisplay();
                }
            }
        }

        // RnsStation
        public string RnsStation
        {
            get => _rnsStation;
            set
            {
                if (SetProperty(ref _rnsStation, value))
                {
                    UpdateRnsStationDisplay();
                }
            }
        }

        // 显示属性 - 支持双向绑定
        public bool ShowIsSFCEnableCheckBox
        {
            get => _showIsSFCEnableCheckBox;
            set => SetProperty(ref _showIsSFCEnableCheckBox, value);
        }

        public bool? IsSFCEnableCheckBoxValue
        {
            get => _isSFCEnableCheckBoxValue;
            set
            {
                if (SetProperty(ref _isSFCEnableCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsSFCEnable", value);
                }
            }
        }

        public string IsSFCEnableDisplayText
        {
            get => _isSFCEnableDisplayText;
            set => SetProperty(ref _isSFCEnableDisplayText, value);
        }

        public bool IsSFCEnableIsBinding
        {
            get => _isSFCEnableIsBinding;
            set => SetProperty(ref _isSFCEnableIsBinding, value);
        }

        public bool ShowIsFirstStationCheckBox
        {
            get => _showIsFirstStationCheckBox;
            set => SetProperty(ref _showIsFirstStationCheckBox, value);
        }

        public bool? IsFirstStationCheckBoxValue
        {
            get => _isFirstStationCheckBoxValue;
            set
            {
                if (SetProperty(ref _isFirstStationCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsFirstStation", value);
                }
            }
        }

        public string IsFirstStationDisplayText
        {
            get => _isFirstStationDisplayText;
            set => SetProperty(ref _isFirstStationDisplayText, value);
        }

        public bool IsFirstStationIsBinding
        {
            get => _isFirstStationIsBinding;
            set => SetProperty(ref _isFirstStationIsBinding, value);
        }

        public bool ShowIsForceStationNameCheckBox
        {
            get => _showIsForceStationNameCheckBox;
            set => SetProperty(ref _showIsForceStationNameCheckBox, value);
        }

        public bool? IsForceStationNameCheckBoxValue
        {
            get => _isForceStationNameCheckBoxValue;
            set
            {
                if (SetProperty(ref _isForceStationNameCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsForceStationName", value);
                }
            }
        }

        public string IsForceStationNameDisplayText
        {
            get => _isForceStationNameDisplayText;
            set => SetProperty(ref _isForceStationNameDisplayText, value);
        }

        public bool IsForceStationNameIsBinding
        {
            get => _isForceStationNameIsBinding;
            set => SetProperty(ref _isForceStationNameIsBinding, value);
        }

        public bool ShowQueryPrevStationCheckBox
        {
            get => _showQueryPrevStationCheckBox;
            set => SetProperty(ref _showQueryPrevStationCheckBox, value);
        }

        public bool? QueryPrevStationCheckBoxValue
        {
            get => _queryPrevStationCheckBoxValue;
            set
            {
                if (SetProperty(ref _queryPrevStationCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("QueryPrevStation", value);
                }
            }
        }

        public string QueryPrevStationDisplayText
        {
            get => _queryPrevStationDisplayText;
            set => SetProperty(ref _queryPrevStationDisplayText, value);
        }

        public bool QueryPrevStationIsBinding
        {
            get => _queryPrevStationIsBinding;
            set => SetProperty(ref _queryPrevStationIsBinding, value);
        }

        public bool ShowIsUnBindCheckBox
        {
            get => _showIsUnBindCheckBox;
            set => SetProperty(ref _showIsUnBindCheckBox, value);
        }

        public bool? IsUnBindCheckBoxValue
        {
            get => _isUnBindCheckBoxValue;
            set
            {
                if (SetProperty(ref _isUnBindCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsUnBind", value);
                }
            }
        }

        public string IsUnBindDisplayText
        {
            get => _isUnBindDisplayText;
            set => SetProperty(ref _isUnBindDisplayText, value);
        }

        public bool IsUnBindIsBinding
        {
            get => _isUnBindIsBinding;
            set => SetProperty(ref _isUnBindIsBinding, value);
        }

        public bool ShowIsAutoBuildWipCheckBox
        {
            get => _showIsAutoBuildWipCheckBox;
            set => SetProperty(ref _showIsAutoBuildWipCheckBox, value);
        }

        public bool? IsAutoBuildWipCheckBoxValue
        {
            get => _isAutoBuildWipCheckBoxValue;
            set
            {
                if (SetProperty(ref _isAutoBuildWipCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsAutoBuildWip", value);
                }
            }
        }

        public string IsAutoBuildWipDisplayText
        {
            get => _isAutoBuildWipDisplayText;
            set => SetProperty(ref _isAutoBuildWipDisplayText, value);
        }

        public bool IsAutoBuildWipIsBinding
        {
            get => _isAutoBuildWipIsBinding;
            set => SetProperty(ref _isAutoBuildWipIsBinding, value);
        }

        public bool ShowIsTwoCheckBox
        {
            get => _showIsTwoCheckBox;
            set => SetProperty(ref _showIsTwoCheckBox, value);
        }

        public bool? IsTwoCheckBoxValue
        {
            get => _isTwoCheckBoxValue;
            set
            {
                if (SetProperty(ref _isTwoCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsTwo", value);
                }
            }
        }

        public string IsTwoDisplayText
        {
            get => _isTwoDisplayText;
            set => SetProperty(ref _isTwoDisplayText, value);
        }

        public bool IsTwoIsBinding
        {
            get => _isTwoIsBinding;
            set => SetProperty(ref _isTwoIsBinding, value);
        }

        public bool ShowIsOKCheckBox
        {
            get => _showIsOKCheckBox;
            set => SetProperty(ref _showIsOKCheckBox, value);
        }

        public bool? IsOKCheckBoxValue
        {
            get => _isOKCheckBoxValue;
            set
            {
                if (SetProperty(ref _isOKCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsOK", value);
                }
            }
        }

        public string IsOKDisplayText
        {
            get => _isOKDisplayText;
            set => SetProperty(ref _isOKDisplayText, value);
        }

        public bool IsOKIsBinding
        {
            get => _isOKIsBinding;
            set => SetProperty(ref _isOKIsBinding, value);
        }

        public bool ShowUseSFCUrl2CheckBox
        {
            get => _showUseSFCUrl2CheckBox;
            set => SetProperty(ref _showUseSFCUrl2CheckBox, value);
        }

        public bool? UseSFCUrl2CheckBoxValue
        {
            get => _useSFCUrl2CheckBoxValue;
            set
            {
                if (SetProperty(ref _useSFCUrl2CheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("UseSFCUrl2", value);
                }
            }
        }

        public string UseSFCUrl2DisplayText
        {
            get => _useSFCUrl2DisplayText;
            set => SetProperty(ref _useSFCUrl2DisplayText, value);
        }

        public bool UseSFCUrl2IsBinding
        {
            get => _useSFCUrl2IsBinding;
            set => SetProperty(ref _useSFCUrl2IsBinding, value);
        }

        public bool ShowDefaultNotPassCheckBox
        {
            get => _showDefaultNotPassCheckBox;
            set => SetProperty(ref _showDefaultNotPassCheckBox, value);
        }

        public bool? DefaultNotPassCheckBoxValue
        {
            get => _defaultNotPassCheckBoxValue;
            set
            {
                if (SetProperty(ref _defaultNotPassCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("DefaultNotPass", value);
                }
            }
        }

        public string DefaultNotPassDisplayText
        {
            get => _defaultNotPassDisplayText;
            set => SetProperty(ref _defaultNotPassDisplayText, value);
        }

        public bool DefaultNotPassIsBinding
        {
            get => _defaultNotPassIsBinding;
            set => SetProperty(ref _defaultNotPassIsBinding, value);
        }

        public bool ShowIsQueryRepairCheckBox
        {
            get => _showIsQueryRepairCheckBox;
            set => SetProperty(ref _showIsQueryRepairCheckBox, value);
        }

        public bool? IsQueryRepairCheckBoxValue
        {
            get => _isQueryRepairCheckBoxValue;
            set
            {
                if (SetProperty(ref _isQueryRepairCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsQueryRepair", value);
                }
            }
        }

        public string IsQueryRepairDisplayText
        {
            get => _isQueryRepairDisplayText;
            set => SetProperty(ref _isQueryRepairDisplayText, value);
        }

        public bool IsQueryRepairIsBinding
        {
            get => _isQueryRepairIsBinding;
            set => SetProperty(ref _isQueryRepairIsBinding, value);
        }

        public bool ShowIsUsePDCACheckBox
        {
            get => _showIsUsePDCACheckBox;
            set => SetProperty(ref _showIsUsePDCACheckBox, value);
        }

        public bool? IsUsePDCACheckBoxValue
        {
            get => _isUsePDCACheckBoxValue;
            set
            {
                if (SetProperty(ref _isUsePDCACheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsUsePDCA", value);
                }
            }
        }

        public string IsUsePDCADisplayText
        {
            get => _isUsePDCADisplayText;
            set => SetProperty(ref _isUsePDCADisplayText, value);
        }

        public bool IsUsePDCAIsBinding
        {
            get => _isUsePDCAIsBinding;
            set => SetProperty(ref _isUsePDCAIsBinding, value);
        }

        public bool ShowIsNoLCFMCheckBox
        {
            get => _showIsNoLCFMCheckBox;
            set => SetProperty(ref _showIsNoLCFMCheckBox, value);
        }

        public bool? IsNoLCFMCheckBoxValue
        {
            get => _isNoLCFMCheckBoxValue;
            set
            {
                if (SetProperty(ref _isNoLCFMCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsNoLCFM", value);
                }
            }
        }

        public string IsNoLCFMDisplayText
        {
            get => _isNoLCFMDisplayText;
            set => SetProperty(ref _isNoLCFMDisplayText, value);
        }

        public bool IsNoLCFMIsBinding
        {
            get => _isNoLCFMIsBinding;
            set => SetProperty(ref _isNoLCFMIsBinding, value);
        }

        public bool ShowIsStationLeftCheckBox
        {
            get => _showIsStationLeftCheckBox;
            set => SetProperty(ref _showIsStationLeftCheckBox, value);
        }

        public bool? IsStationLeftCheckBoxValue
        {
            get => _isStationLeftCheckBoxValue;
            set
            {
                if (SetProperty(ref _isStationLeftCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsStationLeft", value);
                }
            }
        }

        public string IsStationLeftDisplayText
        {
            get => _isStationLeftDisplayText;
            set => SetProperty(ref _isStationLeftDisplayText, value);
        }

        public bool IsStationLeftIsBinding
        {
            get => _isStationLeftIsBinding;
            set => SetProperty(ref _isStationLeftIsBinding, value);
        }

        public bool ShowIsTryrunCheckBox
        {
            get => _showIsTryrunCheckBox;
            set => SetProperty(ref _showIsTryrunCheckBox, value);
        }

        public bool? IsTryrunCheckBoxValue
        {
            get => _isTryrunCheckBoxValue;
            set
            {
                if (SetProperty(ref _isTryrunCheckBoxValue, value))
                {
                    UpdateOriginalValueFromCheckBox("IsTryrun", value);
                }
            }
        }

        public string IsTryrunDisplayText
        {
            get => _isTryrunDisplayText;
            set => SetProperty(ref _isTryrunDisplayText, value);
        }

        public bool IsTryrunIsBinding
        {
            get => _isTryrunIsBinding;
            set => SetProperty(ref _isTryrunIsBinding, value);
        }

        public string ForceStationNameDisplayText
        {
            get => _forceStationNameDisplayText;
            set => SetProperty(ref _forceStationNameDisplayText, value);
        }

        public bool ForceStationNameIsBinding
        {
            get => _forceStationNameIsBinding;
            set => SetProperty(ref _forceStationNameIsBinding, value);
        }

        public string ForceStationTypeDisplayText
        {
            get => _forceStationTypeDisplayText;
            set => SetProperty(ref _forceStationTypeDisplayText, value);
        }

        public bool ForceStationTypeIsBinding
        {
            get => _forceStationTypeIsBinding;
            set => SetProperty(ref _forceStationTypeIsBinding, value);
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

        public string LCFMDisplayText
        {
            get => _lcfmDisplayText;
            set => SetProperty(ref _lcfmDisplayText, value);
        }

        public bool LCFMIsBinding
        {
            get => _lcfmIsBinding;
            set => SetProperty(ref _lcfmIsBinding, value);
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

        public string SFCUrl2DisplayText
        {
            get => _sfcUrl2DisplayText;
            set => SetProperty(ref _sfcUrl2DisplayText, value);
        }

        public bool SFCUrl2IsBinding
        {
            get => _sfcUrl2IsBinding;
            set => SetProperty(ref _sfcUrl2IsBinding, value);
        }

        public string ErrorCodeDisplayText
        {
            get => _errorCodeDisplayText;
            set => SetProperty(ref _errorCodeDisplayText, value);
        }

        public bool ErrorCodeIsBinding
        {
            get => _errorCodeIsBinding;
            set => SetProperty(ref _errorCodeIsBinding, value);
        }

        public string ErrorMsgDisplayText
        {
            get => _errorMsgDisplayText;
            set => SetProperty(ref _errorMsgDisplayText, value);
        }

        public bool ErrorMsgIsBinding
        {
            get => _errorMsgIsBinding;
            set => SetProperty(ref _errorMsgIsBinding, value);
        }

        public string StationIDDisplayText
        {
            get => _stationIDDisplayText;
            set => SetProperty(ref _stationIDDisplayText, value);
        }

        public bool StationIDIsBinding
        {
            get => _stationIDIsBinding;
            set => SetProperty(ref _stationIDIsBinding, value);
        }

        public string CheckFlagDisplayText
        {
            get => _checkFlagDisplayText;
            set => SetProperty(ref _checkFlagDisplayText, value);
        }

        public bool CheckFlagIsBinding
        {
            get => _checkFlagIsBinding;
            set => SetProperty(ref _checkFlagIsBinding, value);
        }

        public string SfcStationCodeDisplayText
        {
            get => _sfcStationCodeDisplayText;
            set => SetProperty(ref _sfcStationCodeDisplayText, value);
        }

        public bool SfcStationCodeIsBinding
        {
            get => _sfcStationCodeIsBinding;
            set => SetProperty(ref _sfcStationCodeIsBinding, value);
        }

        public string Test_station_nameDisplayText
        {
            get => _test_station_nameDisplayText;
            set => SetProperty(ref _test_station_nameDisplayText, value);
        }

        public bool Test_station_nameIsBinding
        {
            get => _test_station_nameIsBinding;
            set => SetProperty(ref _test_station_nameIsBinding, value);
        }

        public string ProductDisplayText
        {
            get => _productDisplayText;
            set => SetProperty(ref _productDisplayText, value);
        }

        public bool ProductIsBinding
        {
            get => _productIsBinding;
            set => SetProperty(ref _productIsBinding, value);
        }

        public string StationNameDisplayText
        {
            get => _stationNameDisplayText;
            set => SetProperty(ref _stationNameDisplayText, value);
        }

        public bool StationNameIsBinding
        {
            get => _stationNameIsBinding;
            set => SetProperty(ref _stationNameIsBinding, value);
        }

        public string Mac_addressDisplayText
        {
            get => _mac_addressDisplayText;
            set => SetProperty(ref _mac_addressDisplayText, value);
        }

        public bool Mac_addressIsBinding
        {
            get => _mac_addressIsBinding;
            set => SetProperty(ref _mac_addressIsBinding, value);
        }

        public string CcdflagDisplayText
        {
            get => _ccdflagDisplayText;
            set => SetProperty(ref _ccdflagDisplayText, value);
        }

        public bool CcdflagIsBinding
        {
            get => _ccdflagIsBinding;
            set => SetProperty(ref _ccdflagIsBinding, value);
        }

        public string PartnameDisplayText
        {
            get => _partnameDisplayText;
            set => SetProperty(ref _partnameDisplayText, value);
        }

        public bool PartnameIsBinding
        {
            get => _partnameIsBinding;
            set => SetProperty(ref _partnameIsBinding, value);
        }

        public string FlexSNDisplayText
        {
            get => _flexSNDisplayText;
            set => SetProperty(ref _flexSNDisplayText, value);
        }

        public bool FlexSNIsBinding
        {
            get => _flexSNIsBinding;
            set => SetProperty(ref _flexSNIsBinding, value);
        }

        public string QueryRealTimeDisplayText
        {
            get => _queryRealTimeDisplayText;
            set => SetProperty(ref _queryRealTimeDisplayText, value);
        }

        public bool QueryRealTimeIsBinding
        {
            get => _queryRealTimeIsBinding;
            set => SetProperty(ref _queryRealTimeIsBinding, value);
        }

        public string AlarmCodeDisplayText
        {
            get => _alarmCodeDisplayText;
            set => SetProperty(ref _alarmCodeDisplayText, value);
        }

        public bool AlarmCodeIsBinding
        {
            get => _alarmCodeIsBinding;
            set => SetProperty(ref _alarmCodeIsBinding, value);
        }

        public string ReturnOKValueDisplayText
        {
            get => _returnOKValueDisplayText;
            set => SetProperty(ref _returnOKValueDisplayText, value);
        }

        public bool ReturnOKValueIsBinding
        {
            get => _returnOKValueIsBinding;
            set => SetProperty(ref _returnOKValueIsBinding, value);
        }

        public string FlexStationIDDisplayText
        {
            get => _flexStationIDDisplayText;
            set => SetProperty(ref _flexStationIDDisplayText, value);
        }

        public bool FlexStationIDIsBinding
        {
            get => _flexStationIDIsBinding;
            set => SetProperty(ref _flexStationIDIsBinding, value);
        }

        public string RnsStationDisplayText
        {
            get => _rnsStationDisplayText;
            set => SetProperty(ref _rnsStationDisplayText, value);
        }

        public bool RnsStationIsBinding
        {
            get => _rnsStationIsBinding;
            set => SetProperty(ref _rnsStationIsBinding, value);
        }

        // 从复选框更新原始值的方法
        private void UpdateOriginalValueFromCheckBox(string propertyName, bool? checkBoxValue)
        {
            if (checkBoxValue.HasValue)
            {
                string newValue = checkBoxValue.Value ? "True" : "False";

                switch (propertyName)
                {
                    case "IsSFCEnable":
                        IsSFCEnable = newValue;
                        break;
                    case "IsFirstStation":
                        IsFirstStation = newValue;
                        break;
                    case "IsForceStationName":
                        IsForceStationName = newValue;
                        break;
                    case "QueryPrevStation":
                        QueryPrevStation = newValue;
                        break;
                    case "IsUnBind":
                        IsUnBind = newValue;
                        break;
                    case "IsAutoBuildWip":
                        IsAutoBuildWip = newValue;
                        break;
                    case "IsTwo":
                        IsTwo = newValue;
                        break;
                    case "IsOK":
                        IsOK = newValue;
                        break;
                    case "UseSFCUrl2":
                        UseSFCUrl2 = newValue;
                        break;
                    case "DefaultNotPass":
                        DefaultNotPass = newValue;
                        break;
                    case "IsQueryRepair":
                        IsQueryRepair = newValue;
                        break;
                    case "IsUsePDCA":
                        IsUsePDCA = newValue;
                        break;
                    case "IsNoLCFM":
                        IsNoLCFM = newValue;
                        break;
                    case "IsStationLeft":
                        IsStationLeft = newValue;
                        break;
                    case "IsTryrun":
                        IsTryrun = newValue;
                        break;
                }
            }
        }

        // 更新显示属性的方法
        private void UpdateIsSFCEnableDisplay()
        {
            IsSFCEnableIsBinding = IsBindingProperty(_isSFCEnable);
            IsSFCEnableDisplayText = GetDisplayText(_isSFCEnable);
            ShowIsSFCEnableCheckBox = ShouldShowCheckBox(_isSFCEnable);
            IsSFCEnableCheckBoxValue = GetCheckBoxState(_isSFCEnable);
        }

        private void UpdateIsFirstStationDisplay()
        {
            IsFirstStationIsBinding = IsBindingProperty(_isFirstStation);
            IsFirstStationDisplayText = GetDisplayText(_isFirstStation);
            ShowIsFirstStationCheckBox = ShouldShowCheckBox(_isFirstStation);
            IsFirstStationCheckBoxValue = GetCheckBoxState(_isFirstStation);
        }

        private void UpdateIsForceStationNameDisplay()
        {
            IsForceStationNameIsBinding = IsBindingProperty(_isForceStationName);
            IsForceStationNameDisplayText = GetDisplayText(_isForceStationName);
            ShowIsForceStationNameCheckBox = ShouldShowCheckBox(_isForceStationName);
            IsForceStationNameCheckBoxValue = GetCheckBoxState(_isForceStationName);
        }

        private void UpdateQueryPrevStationDisplay()
        {
            QueryPrevStationIsBinding = IsBindingProperty(_queryPrevStation);
            QueryPrevStationDisplayText = GetDisplayText(_queryPrevStation);
            ShowQueryPrevStationCheckBox = ShouldShowCheckBox(_queryPrevStation);
            QueryPrevStationCheckBoxValue = GetCheckBoxState(_queryPrevStation);
        }

        private void UpdateIsUnBindDisplay()
        {
            IsUnBindIsBinding = IsBindingProperty(_isUnBind);
            IsUnBindDisplayText = GetDisplayText(_isUnBind);
            ShowIsUnBindCheckBox = ShouldShowCheckBox(_isUnBind);
            IsUnBindCheckBoxValue = GetCheckBoxState(_isUnBind);
        }

        private void UpdateIsAutoBuildWipDisplay()
        {
            IsAutoBuildWipIsBinding = IsBindingProperty(_isAutoBuildWip);
            IsAutoBuildWipDisplayText = GetDisplayText(_isAutoBuildWip);
            ShowIsAutoBuildWipCheckBox = ShouldShowCheckBox(_isAutoBuildWip);
            IsAutoBuildWipCheckBoxValue = GetCheckBoxState(_isAutoBuildWip);
        }

        private void UpdateIsTwoDisplay()
        {
            IsTwoIsBinding = IsBindingProperty(_isTwo);
            IsTwoDisplayText = GetDisplayText(_isTwo);
            ShowIsTwoCheckBox = ShouldShowCheckBox(_isTwo);
            IsTwoCheckBoxValue = GetCheckBoxState(_isTwo);
        }

        private void UpdateIsOKDisplay()
        {
            IsOKIsBinding = IsBindingProperty(_isOK);
            IsOKDisplayText = GetDisplayText(_isOK);
            ShowIsOKCheckBox = ShouldShowCheckBox(_isOK);
            IsOKCheckBoxValue = GetCheckBoxState(_isOK);
        }

        private void UpdateUseSFCUrl2Display()
        {
            UseSFCUrl2IsBinding = IsBindingProperty(_useSFCUrl2);
            UseSFCUrl2DisplayText = GetDisplayText(_useSFCUrl2);
            ShowUseSFCUrl2CheckBox = ShouldShowCheckBox(_useSFCUrl2);
            UseSFCUrl2CheckBoxValue = GetCheckBoxState(_useSFCUrl2);
        }

        private void UpdateDefaultNotPassDisplay()
        {
            DefaultNotPassIsBinding = IsBindingProperty(_defaultNotPass);
            DefaultNotPassDisplayText = GetDisplayText(_defaultNotPass);
            ShowDefaultNotPassCheckBox = ShouldShowCheckBox(_defaultNotPass);
            DefaultNotPassCheckBoxValue = GetCheckBoxState(_defaultNotPass);
        }

        private void UpdateIsQueryRepairDisplay()
        {
            IsQueryRepairIsBinding = IsBindingProperty(_isQueryRepair);
            IsQueryRepairDisplayText = GetDisplayText(_isQueryRepair);
            ShowIsQueryRepairCheckBox = ShouldShowCheckBox(_isQueryRepair);
            IsQueryRepairCheckBoxValue = GetCheckBoxState(_isQueryRepair);
        }

        private void UpdateIsUsePDCADisplay()
        {
            IsUsePDCAIsBinding = IsBindingProperty(_isUsePDCA);
            IsUsePDCADisplayText = GetDisplayText(_isUsePDCA);
            ShowIsUsePDCACheckBox = ShouldShowCheckBox(_isUsePDCA);
            IsUsePDCACheckBoxValue = GetCheckBoxState(_isUsePDCA);
        }

        private void UpdateIsNoLCFMDisplay()
        {
            IsNoLCFMIsBinding = IsBindingProperty(_isNoLCFM);
            IsNoLCFMDisplayText = GetDisplayText(_isNoLCFM);
            ShowIsNoLCFMCheckBox = ShouldShowCheckBox(_isNoLCFM);
            IsNoLCFMCheckBoxValue = GetCheckBoxState(_isNoLCFM);
        }

        private void UpdateIsStationLeftDisplay()
        {
            IsStationLeftIsBinding = IsBindingProperty(_isStationLeft);
            IsStationLeftDisplayText = GetDisplayText(_isStationLeft);
            ShowIsStationLeftCheckBox = ShouldShowCheckBox(_isStationLeft);
            IsStationLeftCheckBoxValue = GetCheckBoxState(_isStationLeft);
        }

        private void UpdateIsTryrunDisplay()
        {
            IsTryrunIsBinding = IsBindingProperty(_isTryrun);
            IsTryrunDisplayText = GetDisplayText(_isTryrun);
            ShowIsTryrunCheckBox = ShouldShowCheckBox(_isTryrun);
            IsTryrunCheckBoxValue = GetCheckBoxState(_isTryrun);
        }

        private void UpdateForceStationNameDisplay()
        {
            ForceStationNameIsBinding = IsBindingProperty(_forceStationName);
            ForceStationNameDisplayText = GetDisplayText(_forceStationName);
        }

        private void UpdateForceStationTypeDisplay()
        {
            ForceStationTypeIsBinding = IsBindingProperty(_forceStationType);
            ForceStationTypeDisplayText = GetDisplayText(_forceStationType);
        }

        private void UpdateSNDisplay()
        {
            SNIsBinding = IsBindingProperty(_sn);
            SNDisplayText = GetDisplayText(_sn);
        }

        private void UpdateLCFMDisplay()
        {
            LCFMIsBinding = IsBindingProperty(_lcfm);
            LCFMDisplayText = GetDisplayText(_lcfm);
        }

        private void UpdateCarrierSNDisplay()
        {
            CarrierSNIsBinding = IsBindingProperty(_carrierSN);
            CarrierSNDisplayText = GetDisplayText(_carrierSN);
        }

        private void UpdateSFCUrl2Display()
        {
            SFCUrl2IsBinding = IsBindingProperty(_sfcUrl2);
            SFCUrl2DisplayText = GetDisplayText(_sfcUrl2);
        }

        private void UpdateErrorCodeDisplay()
        {
            ErrorCodeIsBinding = IsBindingProperty(_errorCode);
            ErrorCodeDisplayText = GetDisplayText(_errorCode);
        }

        private void UpdateErrorMsgDisplay()
        {
            ErrorMsgIsBinding = IsBindingProperty(_errorMsg);
            ErrorMsgDisplayText = GetDisplayText(_errorMsg);
        }

        private void UpdateStationIDDisplay()
        {
            StationIDIsBinding = IsBindingProperty(_stationID);
            StationIDDisplayText = GetDisplayText(_stationID);
        }

        private void UpdateCheckFlagDisplay()
        {
            CheckFlagIsBinding = IsBindingProperty(_checkFlag);
            CheckFlagDisplayText = GetDisplayText(_checkFlag);
        }

        private void UpdateSfcStationCodeDisplay()
        {
            SfcStationCodeIsBinding = IsBindingProperty(_sfcStationCode);
            SfcStationCodeDisplayText = GetDisplayText(_sfcStationCode);
        }

        private void UpdateTest_station_nameDisplay()
        {
            Test_station_nameIsBinding = IsBindingProperty(_test_station_name);
            Test_station_nameDisplayText = GetDisplayText(_test_station_name);
        }

        private void UpdateProductDisplay()
        {
            ProductIsBinding = IsBindingProperty(_product);
            ProductDisplayText = GetDisplayText(_product);
        }

        private void UpdateStationNameDisplay()
        {
            StationNameIsBinding = IsBindingProperty(_stationName);
            StationNameDisplayText = GetDisplayText(_stationName);
        }

        private void UpdateMac_addressDisplay()
        {
            Mac_addressIsBinding = IsBindingProperty(_mac_address);
            Mac_addressDisplayText = GetDisplayText(_mac_address);
        }

        private void UpdateCcdflagDisplay()
        {
            CcdflagIsBinding = IsBindingProperty(_ccdflag);
            CcdflagDisplayText = GetDisplayText(_ccdflag);
        }

        private void UpdatePartnameDisplay()
        {
            PartnameIsBinding = IsBindingProperty(_partname);
            PartnameDisplayText = GetDisplayText(_partname);
        }

        private void UpdateFlexSNDisplay()
        {
            FlexSNIsBinding = IsBindingProperty(_flexSN);
            FlexSNDisplayText = GetDisplayText(_flexSN);
        }

        private void UpdateQueryRealTimeDisplay()
        {
            QueryRealTimeIsBinding = IsBindingProperty(_queryRealTime);
            QueryRealTimeDisplayText = GetDisplayText(_queryRealTime);
        }

        private void UpdateAlarmCodeDisplay()
        {
            AlarmCodeIsBinding = IsBindingProperty(_alarmCode);
            AlarmCodeDisplayText = GetDisplayText(_alarmCode);
        }

        private void UpdateReturnOKValueDisplay()
        {
            ReturnOKValueIsBinding = IsBindingProperty(_returnOKValue);
            ReturnOKValueDisplayText = GetDisplayText(_returnOKValue);
        }

        private void UpdateFlexStationIDDisplay()
        {
            FlexStationIDIsBinding = IsBindingProperty(_flexStationID);
            FlexStationIDDisplayText = GetDisplayText(_flexStationID);
        }

        private void UpdateRnsStationDisplay()
        {
            RnsStationIsBinding = IsBindingProperty(_rnsStation);
            RnsStationDisplayText = GetDisplayText(_rnsStation);
        }

        private bool IsBindingProperty(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            if (value.StartsWith("绑定:"))
                return true;

            if (value.StartsWith("<") && value.Contains("RefName="))
                return true;

            return false;
        }

        private bool ShouldShowCheckBox(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            if (IsBindingProperty(value))
                return false;

            if (value == "True" || value == "False" || value == "true" || value == "false")
                return true;

            if (value == "是" || value == "否")
                return true;

            return false;
        }

        private bool? GetCheckBoxState(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (IsBindingProperty(value))
                return null;

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

            if (value.StartsWith("绑定:"))
            {
                var refName = value.Substring(3).Trim();
                return refName;
            }

            if (value.StartsWith("<") && value.Contains("RefName="))
            {
                try
                {
                    var match = Regex.Match(value, @"RefName=""([^""]+)""");
                    if (match.Success && match.Groups.Count > 1)
                    {
                        return match.Groups[1].Value;
                    }
                }
                catch
                {
                    try
                    {
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
                    }
                }
            }

            // 布尔值转换
            if (value == "True" || value == "true")
                return "是";

            if (value == "False" || value == "false")
                return "否";

            // 处理枚举值
            if (value == "Start") return "开始";
            if (value == "UnbindCarrier") return "治具解绑";
            if (value == "BindCarrier") return "治具绑定";
            if (value == "End") return "结束";
            if (value == "KeyMaterial") return "关键物料";
            if (value == "QueryBlackCarrier") return "查询载具黑名单";
            if (value == "AddBlackCarrier") return "加入载具黑名单";
            if (value == "A") return "卷料工单查询";
            if (value == "B") return "卷料是否可以组装CCDFlag=2";
            if (value == "C") return "卷料组装绑定CCDFlag=0";
            if (value == "D") return "卷料绑定确认";
            if (value == "E") return "CG1查询黑名单";
            if (value == "F") return "RecheckAOI查询检测配方";
            if (value == "G") return "RecheckAOI上传NG";
            if (value == "H") return "RecheckAOI上传OK";
            if (value == "I") return "CG5Flex与CG绑定";
            if (value == "J") return "CG5实名制查询";
            if (value == "K") return "CG6查询排线绑定";
            if (value == "QueryBlackMaterial") return "CG5查询物料黑名单";
            if (value == "L") return "CG1绑定LCFM";
            if (value == "M") return "CG5查询Flex黑名单";
            if (value == "N") return "通过载具码查询WIP";
            if (value == "CG5Flex绑定CG_68") return "CG5Flex绑定CG_68";

            return value;
        }
    }

    public class KeyParameterSFCVM : BindableBase
    {
        private readonly IDeviceEngine _deviceEngine;
        private readonly IEventAggregator _ea;
        private ObservableCollection<SFCKeyParameterRow> _parameterRows;
        private bool _isLoading;
        private int _totalModules;

        private ObservableCollection<string> _stationList;
        private string _selectedStation;
        private Dictionary<string, List<SFCKeyParameterRow>> _stationDataCache;

        public ObservableCollection<IMotionModule> totalSFCs;

        public ObservableCollection<SFCKeyParameterRow> ParameterRows
        {
            get => _parameterRows;
            set => SetProperty(ref _parameterRows, value);
        }

        public ObservableCollection<string> StationList
        {
            get => _stationList;
            set => SetProperty(ref _stationList, value);
        }

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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public int TotalModules
        {
            get => _totalModules;
            set => SetProperty(ref _totalModules, value);
        }

        public ICommand ExportCommand { get; }
        public ICommand RefreshCommand { get; }
        public DelegateCommand ApplyCommand { get; set; }

        public KeyParameterSFCVM(IDeviceEngine deviceEngine, IEventAggregator @event)
        {
            _deviceEngine = deviceEngine;
            _ea = @event;
            ParameterRows = new ObservableCollection<SFCKeyParameterRow>();
            StationList = new ObservableCollection<string>();
            _stationDataCache = new Dictionary<string, List<SFCKeyParameterRow>>();
            totalSFCs = new ObservableCollection<IMotionModule>();

            RefreshCommand = new DelegateCommand(RefreshData);
            ExportCommand = new DelegateCommand(ExportData);
            ApplyCommand = new DelegateCommand(OnApply);
        }

        private void ExportData()
        {
            try
            {
                if (ParameterRows == null || ParameterRows.Count == 0)
                {
                    MessageBox.Show("当前工站没有数据可导出", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV文件 (*.csv)|*.csv|所有文件 (*.*)|*.*",
                    FilterIndex = 1,
                    FileName = $"SFC参数配置_{SelectedStation ?? "未知工站"}_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    DefaultExt = ".csv",
                    Title = "导出当前工站SFC参数配置"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string csvContent = GenerateCsvContent();
                    File.WriteAllText(saveFileDialog.FileName, csvContent, Encoding.UTF8);
                    MessageBox.Show($"导出成功！\n文件保存至：{saveFileDialog.FileName}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateCsvContent()
        {
            var csvBuilder = new StringBuilder();

            csvBuilder.AppendLine($"导出时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csvBuilder.AppendLine($"工站名称：{SelectedStation ?? "未选择"}");
            csvBuilder.AppendLine();

            string[] headers = new string[]
            {
                "工站名称",
                "动作类型",
                "SFC启用",
                "首站",
                "强制本站名称",
                "本站名称",
                "本站Type",
                "查询上一站结果",
                "解绑治具码",
                "自动生成WIP",
                "SN编码",
                "查两次",
                "LCFM",
                "治具码",
                "Wip码长度",
                "Wip码长度2",
                "上传Pass/Fail",
                "SFCUrl2",
                "UseSFCUrl2",
                "NG码",
                "NG信息",
                "stationID",
                "check_flag",
                "sfcStationCode",
                "test_station_name",
                "product",
                "StationName",
                "mac_address",
                "本站强制未做",
                "ccdflag",
                "partname",
                "FlexSN",
                "实名制查询时间",
                "isTryrun",
                "查询维修",
                "使用PDCA",
                "报警代码",
                "无LCFM",
                "SFCOK返回值",
                "Flex工站ID",
                "左工位",
                "RnsStation"
            };

            csvBuilder.AppendLine(string.Join(",", headers));

            foreach (var row in ParameterRows)
            {
                var csvRow = new List<string>
                {
                    EscapeCsvField(row.StationDisplayName),
                    EscapeCsvField(row.SfcMode),
                    EscapeCsvField(GetBooleanDisplayText(row.IsSFCEnable)),
                    EscapeCsvField(GetBooleanDisplayText(row.IsFirstStation)),
                    EscapeCsvField(GetBooleanDisplayText(row.IsForceStationName)),
                    EscapeCsvField(row.ForceStationName),
                    EscapeCsvField(row.ForceStationType),
                    EscapeCsvField(GetBooleanDisplayText(row.QueryPrevStation)),
                    EscapeCsvField(GetBooleanDisplayText(row.IsUnBind)),
                    EscapeCsvField(GetBooleanDisplayText(row.IsAutoBuildWip)),
                    EscapeCsvField(row.SN),
                    EscapeCsvField(GetBooleanDisplayText(row.IsTwo)),
                    EscapeCsvField(row.LCFM),
                    EscapeCsvField(row.CarrierSN),
                    row.WipLength.ToString(),
                    row.WipLength2.ToString(),
                    EscapeCsvField(GetBooleanDisplayText(row.IsOK)),
                    EscapeCsvField(row.SFCUrl2),
                    EscapeCsvField(GetBooleanDisplayText(row.UseSFCUrl2)),
                    EscapeCsvField(row.ErrorCode),
                    EscapeCsvField(row.ErrorMsg),
                    EscapeCsvField(row.StationID),
                    EscapeCsvField(row.CheckFlag),
                    EscapeCsvField(row.SfcStationCode),
                    EscapeCsvField(row.Test_station_name),
                    EscapeCsvField(row.Product),
                    EscapeCsvField(row.StationNameProp),
                    EscapeCsvField(row.Mac_address),
                    EscapeCsvField(GetBooleanDisplayText(row.DefaultNotPass)),
                    EscapeCsvField(row.Ccdflag),
                    EscapeCsvField(row.Partname),
                    EscapeCsvField(row.FlexSN),
                    EscapeCsvField(row.QueryRealTime),
                    EscapeCsvField(GetBooleanDisplayText(row.IsTryrun)),
                    EscapeCsvField(GetBooleanDisplayText(row.IsQueryRepair)),
                    EscapeCsvField(GetBooleanDisplayText(row.IsUsePDCA)),
                    EscapeCsvField(row.AlarmCode),
                    EscapeCsvField(GetBooleanDisplayText(row.IsNoLCFM)),
                    EscapeCsvField(row.ReturnOKValue),
                    EscapeCsvField(row.FlexStationID),
                    EscapeCsvField(GetBooleanDisplayText(row.IsStationLeft)),
                    EscapeCsvField(row.RnsStation)
                };

                csvBuilder.AppendLine(string.Join(",", csvRow));
            }

            return csvBuilder.ToString();
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\r") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }

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
            OnUpdateRecipe(totalSFCs);
        }

        public void OnViewLoaded()
        {
            LoadSFCModulesFromMotionEngine();
        }

        private void OnStationChanged()
        {
            if (string.IsNullOrEmpty(SelectedStation) || !_stationDataCache.ContainsKey(SelectedStation))
            {
                ParameterRows.Clear();
                TotalModules = 0;
                return;
            }

            var rows = _stationDataCache[SelectedStation];
            ParameterRows.Clear();
            foreach (var row in rows)
            {
                ParameterRows.Add(row);
            }
            TotalModules = ParameterRows.Count;
        }

        private void RefreshData()
        {
            LoadSFCModulesFromMotionEngine();
        }

        private void LoadSFCModulesFromMotionEngine()
        {
            try
            {
                IsLoading = true;

                ParameterRows.Clear();
                StationList.Clear();
                _stationDataCache.Clear();
                totalSFCs.Clear();

                var sfcModules = _deviceEngine.GetSFCModulesFromMotionEngine();

                if (sfcModules == null || !sfcModules.Any())
                {
                    TotalModules = 0;
                    return;
                }

                var filteredModules = new ObservableCollection<IMotionModule>();
                foreach (var item in sfcModules)
                {
                    if (item is IMotionModule module && module.TaskFunction.GetType().FullName == typeof(SFCFlow).FullName)
                    {
                        filteredModules.Add(module);
                    }
                }

                if (filteredModules.Count == 0)
                {
                    TotalModules = 0;
                    return;
                }

                ConvertToSFCKeyParameterRow(filteredModules, out var rows);

                StationList.Clear();
                _stationDataCache.Clear();

                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    string stationDisplayName = row.StationDisplayName;

                    StationList.Add(stationDisplayName);
                    _stationDataCache[stationDisplayName] = new List<SFCKeyParameterRow> { row };
                }

                if (StationList.Count > 0)
                {
                    SelectedStation = StationList[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载SFC模块失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ConvertToSFCKeyParameterRow(ObservableCollection<IMotionModule> sfcModules, out ObservableCollection<SFCKeyParameterRow> parameterRows)
        {
            parameterRows = new ObservableCollection<SFCKeyParameterRow>();
            if (sfcModules == null) return;

            foreach (var module in sfcModules)
            {
                try
                {
                    var stationName = GetStationNameFromModule(module);
                    var moduleName = GetModuleNameFromModule(module);
                    var values = GetValuesFromModule(module);
                    var parameters = module.Parameters;

                    if (values != null)
                    {
                        var row = new SFCKeyParameterRow
                        {
                            StationDisplayName = $"{stationName} - {moduleName}",
                            SourceModule = module
                        };

                        ExtractParametersFromValues(row, values, parameters);
                        parameterRows.Add(row);

                        if (!totalSFCs.Contains(module))
                        {
                            totalSFCs.Add(module);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理SFC模块失败: {ex.Message}");
                }
            }
        }

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
            return module?.Alias ?? module?.GetType()?.Name ?? "未知模块";
        }

        private Dictionary<string, object> GetValuesFromModule(IMotionModule module)
        {
            var parameters = new Dictionary<string, object>();

            if (module?.Parameters == null) return parameters;

            var sfcKeys = new List<string>
            {
                "SfcMode", "IsSFCEnable", "IsFirstStation", "IsForceStationName", "ForceStationName",
                "ForceStationType", "QueryPrevStation", "IsUnBind", "IsAutoBuildWip", "SN",
                "IsTwo", "LCFM", "CarrierSN", "WipLength", "WipLength2", "IsOK", "SFCUrl2",
                "UseSFCUrl2", "ErrorCode", "ErrorMsg", "stationID", "checkFlag", "sfcStationCode",
                "test_station_name", "product", "StationName", "mac_address", "DefaultNotPass",
                "ccdflag", "partname", "FlexSN", "QueryRealTime", "isTryrun", "IsQueryRepair",
                "IsUsePDCA", "AlarmCode", "IsNoLCFM", "ReturnOKValue", "FlexStationID",
                "IsStationLeft", "RnsStation"
            };

            foreach (var key in sfcKeys)
            {
                if (module.Parameters.TryGetValue(key, out var param))
                {
                    object value = GetParameterValue(param);
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
                return param.Value ?? param.DefaultV;
            }
        }

        private object GetDefaultValue(ParameterAttribute param, string key)
        {
            if (param?.DefaultV != null)
            {
                return param.DefaultV;
            }

            switch (key)
            {
                case "IsSFCEnable":
                case "IsFirstStation":
                case "IsForceStationName":
                case "QueryPrevStation":
                case "IsUnBind":
                case "IsAutoBuildWip":
                case "IsTwo":
                case "UseSFCUrl2":
                case "DefaultNotPass":
                case "IsQueryRepair":
                case "IsUsePDCA":
                case "IsNoLCFM":
                case "IsTryrun":
                case "IsStationLeft":
                    return false;
                case "IsOK":
                    return true;
                case "WipLength":
                    return 23;
                case "WipLength2":
                    return 23;
                case "SfcMode":
                    return "Start";
                case "AlarmCode":
                    return "F99OOOO-01";
                case "QueryRealTime":
                    return "2020-12-12 12:12:12";
                default:
                    return string.Empty;
            }
        }

        private void ConvertToSFCFunction(ObservableCollection<SFCKeyParameterRow> parameterRows, out ObservableCollection<IMotionModule> sfcFunctions)
        {
            sfcFunctions = new ObservableCollection<IMotionModule>();
            if (parameterRows == null) return;

            foreach (var row in parameterRows)
            {
                if (row.SourceModule is IMotionModule module)
                {
                    UpdateModuleFromRow(row, module);
                    sfcFunctions.Add(module);
                }
            }
        }

        private void OnUpdateRecipe(ObservableCollection<IMotionModule> sfcFunctions)
        {
            var currentStationRows = new ObservableCollection<SFCKeyParameterRow>(ParameterRows);
            ConvertToSFCFunction(currentStationRows, out var modules);

            if (sfcFunctions != null)
            {
                foreach (var module in modules)
                {
                    _ea.GetEvent<ModuleUpdateEvent>().Publish(new ModuleUpdateModule { Module = module, UpdateType = ModuleUpdate.ParameterVal });
                }
            }
        }

        private void UpdateModuleFromRow(SFCKeyParameterRow row, IMotionModule module)
        {
            if (module?.Parameters == null) return;

            SetModuleParameter(module, "SfcMode", row.SfcMode);
            SetModuleParameter(module, "IsSFCEnable", row.IsSFCEnable);
            SetModuleParameter(module, "IsFirstStation", row.IsFirstStation);
            SetModuleParameter(module, "IsForceStationName", row.IsForceStationName);
            SetModuleParameter(module, "ForceStationName", row.ForceStationName);
            SetModuleParameter(module, "ForceStationType", row.ForceStationType);
            SetModuleParameter(module, "QueryPrevStation", row.QueryPrevStation);
            SetModuleParameter(module, "IsUnBind", row.IsUnBind);
            SetModuleParameter(module, "IsAutoBuildWip", row.IsAutoBuildWip);
            SetModuleParameter(module, "SN", row.SN);
            SetModuleParameter(module, "IsTwo", row.IsTwo);
            SetModuleParameter(module, "LCFM", row.LCFM);
            SetModuleParameter(module, "CarrierSN", row.CarrierSN);
            SetModuleParameter(module, "WipLength", row.WipLength);
            SetModuleParameter(module, "WipLength2", row.WipLength2);
            SetModuleParameter(module, "IsOK", row.IsOK);
            SetModuleParameter(module, "SFCUrl2", row.SFCUrl2);
            SetModuleParameter(module, "UseSFCUrl2", row.UseSFCUrl2);
            SetModuleParameter(module, "ErrorCode", row.ErrorCode);
            SetModuleParameter(module, "ErrorMsg", row.ErrorMsg);
            SetModuleParameter(module, "stationID", row.StationID);
            SetModuleParameter(module, "checkFlag", row.CheckFlag);
            SetModuleParameter(module, "sfcStationCode", row.SfcStationCode);
            SetModuleParameter(module, "test_station_name", row.Test_station_name);
            SetModuleParameter(module, "product", row.Product);
            SetModuleParameter(module, "StationName", row.StationNameProp);
            SetModuleParameter(module, "mac_address", row.Mac_address);
            SetModuleParameter(module, "DefaultNotPass", row.DefaultNotPass);
            SetModuleParameter(module, "ccdflag", row.Ccdflag);
            SetModuleParameter(module, "partname", row.Partname);
            SetModuleParameter(module, "FlexSN", row.FlexSN);
            SetModuleParameter(module, "QueryRealTime", row.QueryRealTime);
            SetModuleParameter(module, "isTryrun", row.IsTryrun);
            SetModuleParameter(module, "IsQueryRepair", row.IsQueryRepair);
            SetModuleParameter(module, "IsUsePDCA", row.IsUsePDCA);
            SetModuleParameter(module, "AlarmCode", row.AlarmCode);
            SetModuleParameter(module, "IsNoLCFM", row.IsNoLCFM);
            SetModuleParameter(module, "ReturnOKValue", row.ReturnOKValue);
            SetModuleParameter(module, "FlexStationID", row.FlexStationID);
            SetModuleParameter(module, "IsStationLeft", row.IsStationLeft);
            SetModuleParameter(module, "RnsStation", row.RnsStation);
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
                else if (type.IsEnum)
                {
                    try
                    {
                        string s = newValue.ToString();
                        // 处理SFCType枚举
                        if (s == "开始") s = "Start";
                        else if (s == "治具解绑") s = "UnbindCarrier";
                        else if (s == "治具绑定") s = "BindCarrier";
                        else if (s == "结束") s = "End";
                        else if (s == "关键物料") s = "KeyMaterial";
                        else if (s == "查询载具黑名单") s = "QueryBlackCarrier";
                        else if (s == "加入载具黑名单") s = "AddBlackCarrier";
                        else if (s == "卷料工单查询") s = "A";
                        else if (s == "卷料是否可以组装CCDFlag=2") s = "B";
                        else if (s == "卷料组装绑定CCDFlag=0") s = "C";
                        else if (s == "卷料绑定确认") s = "D";
                        else if (s == "CG1查询黑名单") s = "E";
                        else if (s == "RecheckAOI查询检测配方") s = "F";
                        else if (s == "RecheckAOI上传NG") s = "G";
                        else if (s == "RecheckAOI上传OK") s = "H";
                        else if (s == "CG5Flex与CG绑定") s = "I";
                        else if (s == "CG5实名制查询") s = "J";
                        else if (s == "CG6查询排线绑定") s = "K";
                        else if (s == "CG5查询物料黑名单") s = "QueryBlackMaterial";
                        else if (s == "CG1绑定LCFM") s = "L";
                        else if (s == "CG5查询Flex黑名单") s = "M";
                        else if (s == "通过载具码查询WIP") s = "N";
                        else if (s == "CG5Flex绑定CG_68") s = "CG5Flex绑定CG_68";

                        valueToSet = Enum.Parse(type, s);
                    }
                    catch
                    {
                        // 如果解析失败，尝试直接解析
                        try
                        {
                            valueToSet = Enum.Parse(type, newValue.ToString());
                        }
                        catch
                        {
                            // 忽略解析错误
                        }
                    }
                }

                param.Value = valueToSet;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新参数 {key} 失败: {ex.Message}");
            }
        }

        private void ExtractParametersFromValues(SFCKeyParameterRow row, Dictionary<string, object> values, Dictionary<string, ParameterAttribute> parameters = null)
        {
            foreach (var kvp in values)
            {
                SetSFCParameter(row, kvp.Key, kvp.Value, parameters);
            }
        }

        private void SetSFCParameter(SFCKeyParameterRow row, string key, object value, Dictionary<string, ParameterAttribute> parameters = null)
        {
            if (value == null) return;

            try
            {
                string stringValue = value.ToString();
                bool hasBinding = false;
                string bindingRefName = "";
                bool showCheckBox = false;
                bool? checkBoxValue = null;

                if (parameters != null && parameters.TryGetValue(key, out var paramAttr))
                {
                    if (paramAttr.RefOut != null)
                    {
                        stringValue = $"绑定: {paramAttr.RefOut.Name}";
                        hasBinding = true;
                        bindingRefName = paramAttr.RefOut.Name;
                    }
                    else
                    {
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
                        if (!hasBinding)
                        {
                            bool isSpecialParameter = key == "SN" || key == "LCFM" || key == "CarrierSN" ||
                                                      key == "SFCUrl2" || key == "ErrorCode" || key == "ErrorMsg" ||
                                                      key == "stationID" || key == "sfcStationCode" || key == "test_station_name" ||
                                                      key == "product" || key == "StationName" || key == "mac_address" ||
                                                      key == "AlarmCode" || key == "ReturnOKValue" || key == "FlexStationID" ||
                                                      key == "QueryRealTime" || key == "ccdflag" || key == "partname" ||
                                                      key == "FlexSN" || key == "RnsStation" || key == "ForceStationName" ||
                                                      key == "ForceStationType" || key == "WipLength" || key == "WipLength2";

                            if (isSpecialParameter)
                            {
                                showCheckBox = false;
                            }
                            else if (paramAttr.Type == typeof(bool))
                            {
                                showCheckBox = true;
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
                        if (paramAttr.Type == typeof(bool))
                        {
                            showCheckBox = true;
                        }
                    }
                }
                else if (value is XElement xmlElement)
                {
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
                    case "IsSFCEnable":
                        row.IsSFCEnableIsBinding = hasBinding;
                        row.ShowIsSFCEnableCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsSFCEnableCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "IsFirstStation":
                        row.IsFirstStationIsBinding = hasBinding;
                        row.ShowIsFirstStationCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsFirstStationCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "IsForceStationName":
                        row.IsForceStationNameIsBinding = hasBinding;
                        row.ShowIsForceStationNameCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsForceStationNameCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "QueryPrevStation":
                        row.QueryPrevStationIsBinding = hasBinding;
                        row.ShowQueryPrevStationCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.QueryPrevStationCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "IsUnBind":
                        row.IsUnBindIsBinding = hasBinding;
                        row.ShowIsUnBindCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsUnBindCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "IsAutoBuildWip":
                        row.IsAutoBuildWipIsBinding = hasBinding;
                        row.ShowIsAutoBuildWipCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsAutoBuildWipCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "IsTwo":
                        row.IsTwoIsBinding = hasBinding;
                        row.ShowIsTwoCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsTwoCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "IsOK":
                        row.IsOKIsBinding = hasBinding;
                        row.ShowIsOKCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsOKCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "UseSFCUrl2":
                        row.UseSFCUrl2IsBinding = hasBinding;
                        row.ShowUseSFCUrl2CheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.UseSFCUrl2CheckBoxValue = checkBoxValue.Value;
                        break;
                    case "DefaultNotPass":
                        row.DefaultNotPassIsBinding = hasBinding;
                        row.ShowDefaultNotPassCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.DefaultNotPassCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "IsQueryRepair":
                        row.IsQueryRepairIsBinding = hasBinding;
                        row.ShowIsQueryRepairCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsQueryRepairCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "IsUsePDCA":
                        row.IsUsePDCAIsBinding = hasBinding;
                        row.ShowIsUsePDCACheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsUsePDCACheckBoxValue = checkBoxValue.Value;
                        break;
                    case "IsNoLCFM":
                        row.IsNoLCFMIsBinding = hasBinding;
                        row.ShowIsNoLCFMCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsNoLCFMCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "IsStationLeft":
                        row.IsStationLeftIsBinding = hasBinding;
                        row.ShowIsStationLeftCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsStationLeftCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "isTryrun":
                        row.IsTryrunIsBinding = hasBinding;
                        row.ShowIsTryrunCheckBox = showCheckBox;
                        if (checkBoxValue.HasValue) row.IsTryrunCheckBoxValue = checkBoxValue.Value;
                        break;
                    case "ForceStationName":
                        row.ForceStationNameIsBinding = hasBinding;
                        break;
                    case "ForceStationType":
                        row.ForceStationTypeIsBinding = hasBinding;
                        break;
                    case "SN":
                        row.SNIsBinding = hasBinding;
                        break;
                    case "LCFM":
                        row.LCFMIsBinding = hasBinding;
                        break;
                    case "CarrierSN":
                        row.CarrierSNIsBinding = hasBinding;
                        break;
                    case "SFCUrl2":
                        row.SFCUrl2IsBinding = hasBinding;
                        break;
                    case "ErrorCode":
                        row.ErrorCodeIsBinding = hasBinding;
                        break;
                    case "ErrorMsg":
                        row.ErrorMsgIsBinding = hasBinding;
                        break;
                    case "stationID":
                        row.StationIDIsBinding = hasBinding;
                        break;
                    case "checkFlag":
                        row.CheckFlagIsBinding = hasBinding;
                        break;
                    case "sfcStationCode":
                        row.SfcStationCodeIsBinding = hasBinding;
                        break;
                    case "test_station_name":
                        row.Test_station_nameIsBinding = hasBinding;
                        break;
                    case "product":
                        row.ProductIsBinding = hasBinding;
                        break;
                    case "StationName":
                        row.StationNameIsBinding = hasBinding;
                        break;
                    case "mac_address":
                        row.Mac_addressIsBinding = hasBinding;
                        break;
                    case "ccdflag":
                        row.CcdflagIsBinding = hasBinding;
                        break;
                    case "partname":
                        row.PartnameIsBinding = hasBinding;
                        break;
                    case "FlexSN":
                        row.FlexSNIsBinding = hasBinding;
                        break;
                    case "QueryRealTime":
                        row.QueryRealTimeIsBinding = hasBinding;
                        break;
                    case "AlarmCode":
                        row.AlarmCodeIsBinding = hasBinding;
                        break;
                    case "ReturnOKValue":
                        row.ReturnOKValueIsBinding = hasBinding;
                        break;
                    case "FlexStationID":
                        row.FlexStationIDIsBinding = hasBinding;
                        break;
                    case "RnsStation":
                        row.RnsStationIsBinding = hasBinding;
                        break;
                }

                // 设置参数值
                switch (key)
                {
                    case "SfcMode":
                        row.SfcMode = GetEnumDisplayText(stringValue);
                        break;
                    case "IsSFCEnable":
                        row.IsSFCEnable = stringValue;
                        break;
                    case "IsFirstStation":
                        row.IsFirstStation = stringValue;
                        break;
                    case "IsForceStationName":
                        row.IsForceStationName = stringValue;
                        break;
                    case "ForceStationName":
                        row.ForceStationName = stringValue;
                        break;
                    case "ForceStationType":
                        row.ForceStationType = stringValue;
                        break;
                    case "QueryPrevStation":
                        row.QueryPrevStation = stringValue;
                        break;
                    case "IsUnBind":
                        row.IsUnBind = stringValue;
                        break;
                    case "IsAutoBuildWip":
                        row.IsAutoBuildWip = stringValue;
                        break;
                    case "SN":
                        row.SN = stringValue;
                        break;
                    case "IsTwo":
                        row.IsTwo = stringValue;
                        break;
                    case "LCFM":
                        row.LCFM = stringValue;
                        break;
                    case "CarrierSN":
                        row.CarrierSN = stringValue;
                        break;
                    case "WipLength":
                        if (TryParseInt(value, out int wipLength)) row.WipLength = wipLength;
                        break;
                    case "WipLength2":
                        if (TryParseInt(value, out int wipLength2)) row.WipLength2 = wipLength2;
                        break;
                    case "IsOK":
                        row.IsOK = stringValue;
                        break;
                    case "SFCUrl2":
                        row.SFCUrl2 = stringValue;
                        break;
                    case "UseSFCUrl2":
                        row.UseSFCUrl2 = stringValue;
                        break;
                    case "ErrorCode":
                        row.ErrorCode = stringValue;
                        break;
                    case "ErrorMsg":
                        row.ErrorMsg = stringValue;
                        break;
                    case "stationID":
                        row.StationID = stringValue;
                        break;
                    case "checkFlag":
                        row.CheckFlag = stringValue;
                        break;
                    case "sfcStationCode":
                        row.SfcStationCode = stringValue;
                        break;
                    case "test_station_name":
                        row.Test_station_name = stringValue;
                        break;
                    case "product":
                        row.Product = stringValue;
                        break;
                    case "StationName":
                        row.StationNameProp = stringValue;
                        break;
                    case "mac_address":
                        row.Mac_address = stringValue;
                        break;
                    case "DefaultNotPass":
                        row.DefaultNotPass = stringValue;
                        break;
                    case "ccdflag":
                        row.Ccdflag = stringValue;
                        break;
                    case "partname":
                        row.Partname = stringValue;
                        break;
                    case "FlexSN":
                        row.FlexSN = stringValue;
                        break;
                    case "QueryRealTime":
                        row.QueryRealTime = stringValue;
                        break;
                    case "isTryrun":
                        row.IsTryrun = stringValue;
                        break;
                    case "IsQueryRepair":
                        row.IsQueryRepair = stringValue;
                        break;
                    case "IsUsePDCA":
                        row.IsUsePDCA = stringValue;
                        break;
                    case "AlarmCode":
                        row.AlarmCode = stringValue;
                        break;
                    case "IsNoLCFM":
                        row.IsNoLCFM = stringValue;
                        break;
                    case "ReturnOKValue":
                        row.ReturnOKValue = stringValue;
                        break;
                    case "FlexStationID":
                        row.FlexStationID = stringValue;
                        break;
                    case "IsStationLeft":
                        row.IsStationLeft = stringValue;
                        break;
                    case "RnsStation":
                        row.RnsStation = stringValue;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置参数 {key} 失败: {ex.Message}");
            }
        }

        private string GetEnumDisplayText(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            // 如果是绑定属性，直接返回
            if (value.StartsWith("绑定:")) return value;

            // 处理SFCType枚举
            if (value == "Start" || value == "SFCFlow+SFCType.Start") return "开始";
            if (value == "UnbindCarrier" || value == "SFCFlow+SFCType.UnbindCarrier") return "治具解绑";
            if (value == "BindCarrier" || value == "SFCFlow+SFCType.BindCarrier") return "治具绑定";
            if (value == "End" || value == "SFCFlow+SFCType.End") return "结束";
            if (value == "KeyMaterial" || value == "SFCFlow+SFCType.KeyMaterial") return "关键物料";
            if (value == "QueryBlackCarrier" || value == "SFCFlow+SFCType.QueryBlackCarrier") return "查询载具黑名单";
            if (value == "AddBlackCarrier" || value == "SFCFlow+SFCType.AddBlackCarrier") return "加入载具黑名单";
            if (value == "A" || value == "SFCFlow+SFCType.A") return "卷料工单查询";
            if (value == "B" || value == "SFCFlow+SFCType.B") return "卷料是否可以组装CCDFlag=2";
            if (value == "C" || value == "SFCFlow+SFCType.C") return "卷料组装绑定CCDFlag=0";
            if (value == "D" || value == "SFCFlow+SFCType.D") return "卷料绑定确认";
            if (value == "E" || value == "SFCFlow+SFCType.E") return "CG1查询黑名单";
            if (value == "F" || value == "SFCFlow+SFCType.F") return "RecheckAOI查询检测配方";
            if (value == "G" || value == "SFCFlow+SFCType.G") return "RecheckAOI上传NG";
            if (value == "H" || value == "SFCFlow+SFCType.H") return "RecheckAOI上传OK";
            if (value == "I" || value == "SFCFlow+SFCType.I") return "CG5Flex与CG绑定";
            if (value == "J" || value == "SFCFlow+SFCType.J") return "CG5实名制查询";
            if (value == "K" || value == "SFCFlow+SFCType.K") return "CG6查询排线绑定";
            if (value == "QueryBlackMaterial" || value == "SFCFlow+SFCType.QueryBlackMaterial") return "CG5查询物料黑名单";
            if (value == "L" || value == "SFCFlow+SFCType.L") return "CG1绑定LCFM";
            if (value == "M" || value == "SFCFlow+SFCType.M") return "CG5查询Flex黑名单";
            if (value == "N" || value == "SFCFlow+SFCType.N") return "通过载具码查询WIP";
            if (value == "CG5Flex绑定CG_68" || value == "SFCFlow+SFCType.CG5Flex绑定CG_68") return "CG5Flex绑定CG_68";

            return value;
        }

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
    }
}