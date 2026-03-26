#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IODialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       IODialogVM.cs
* 创建时间:       2022/4/26 14:20:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6e74db86-a141-462c-8c56-f72e64505019
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/26 14:20:45
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class IODialogVM : DialogVM
    {
        protected IODialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            IOTypes = typeof(IOType).EnumToDataSource();
            IOBehaviors = typeof(IOBehavior).EnumToDataSource();

            // 初始化控制块
            MotionCardList = deviceEngine.GetRealDevices(typeof(IMotionCard));
        }

        /// <summary>
        /// IO类型集合
        /// </summary>
        public List<KeyValue> IOTypes { get; set; }

        /// <summary>
        /// IO行为集合
        /// </summary>
        public List<KeyValue> IOBehaviors { get; set; }

        /// <summary>
        /// 轴卡列表
        /// </summary>
        private List<IDevice> motionCardList;
        public List<IDevice> MotionCardList
        {
            get { return motionCardList; }
            set { SetProperty(ref motionCardList, value); }
        }


        #region 通用参数
        /// <summary>
        /// 模组名称
        /// </summary>
        private IDevice _mDevice;
        [Required]
        public IDevice MDevice { get => _mDevice; set => SetProperty(ref _mDevice, value); }

        /// <summary>
        /// 起始编号
        /// </summary>
        private int _startNo = 1;
        [Range(1, 10000)]
        public int StartNo { get => _startNo; set => SetProperty(ref _startNo, value); }

        /// <summary>
        /// 一次创建IO的数量
        /// </summary>
        private int _number = 1;
        [Range(1, 10000)]
        public int Number
        {
            get => _number; set
            {
                SetProperty(ref _number, value);
            }
        }

        /// <summary>
        /// 分组
        /// </summary>
        private string _module;
        [Required]
        public string Module
        {
            get => _module; set
            {
                SetProperty(ref _module, value);
            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name = "DI";
        [Required]
        public string Name
        {
            get => _name; set
            {
                SetProperty(ref _name, value);
            }
        }

        /// <summary>
        /// 类型
        /// </summary>
        private IOType _ioType;
        public IOType IoType { get => _ioType; set => SetProperty(ref _ioType, value); }

        /// <summary>
        /// 行为
        /// </summary>
        private IOBehavior _ioBehavior;
        public IOBehavior IOBehavior
        {
            get => _ioBehavior; set
            {
                SetProperty(ref _ioBehavior, value);
                IsShowDefault = value == IOBehavior.Output;
                Name = GetNamePrefix();
            }
        }

        /// <summary>
        /// 是否显示默认值
        /// </summary>
        private bool _isShowDefault;
        public bool IsShowDefault
        {
            get { return _isShowDefault; }
            set { SetProperty(ref _isShowDefault, value); }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        private double _defaultV = 0;

        public double DefaultValue { get => _defaultV; set => SetProperty(ref _defaultV, value); }

        #endregion

        /// <summary>
        /// 自动扫描
        /// </summary>
        private DelegateCommand _scanCommand;
        public DelegateCommand ScanCommand => _scanCommand ?? (_scanCommand = new DelegateCommand(() =>
        {
            // 是否在线
            if (SimEngineUI.DeviceMode != DeviceMode.Real)
            {
                SimEngineUI.OnAlert(LogType.Warning, "请先调整在线模式");
                return;
            }

            // 是否选择设备
            if (MDevice == null)
            {
                SimEngineUI.OnAlert(LogType.Info, "请先选择控制卡");
                return;
            }

            if (MDevice is IMotionCard card)
            {
                if (IoType == IOType.Digital)
                {
                    card.ScanDigitalIO(out var dIn, out var dOut);
                    if (IOBehavior == IOBehavior.Output)
                    {
                        Number = (int)dOut;
                    }
                    else
                    {
                        Number = (int)dIn;
                    }

                    SimEngineUI.OnAlert(LogType.Info, $"共扫描出数字输入信号:{dIn}，输出信号:{dOut}");
                }
                else if (IoType == IOType.Analog)
                {
                    card.ScanDigitalIO(out var dIn, out var dOut);
                    if (IOBehavior == IOBehavior.Output)
                    {
                        Number = (int)dOut;
                    }
                    else
                    {
                        Number = (int)dIn;
                    }


                    SimEngineUI.OnAlert(LogType.Info, $"共扫描出模拟输入信号:{dIn}，输出信号:{dOut}");
                }
            }
        }));

        /// <summary>
        /// 获取名称前缀
        /// </summary>
        /// <returns></returns>
        private string GetNamePrefix()
        {
            StringBuilder prefix = new StringBuilder();
            if (IoType == IOType.Digital)
            {
                prefix.Append("D");
            }
            else
            {
                prefix.Append("A");
            }

            if (IOBehavior == IOBehavior.Output)
            {
                prefix.Append("O");
            }
            else
            {
                prefix.Append("I");
            }

            return prefix.ToString();
        }

        protected override void Ok(IDialogResult result)
        {
            // 设置当前的类型
            result.Parameters.Add("IOType", IoType);

            // 获取已有IO，确认index是否有重复
            var existIOs = deviceEngine.GetDevices(typeof(VIO))
                .Select(u => u as VIO)
                .Where(u => u.CardName == MDevice.Name)
                .Where(u => u.Behavior == IOBehavior)
                .Where(u => u.IOType == IoType)
                .Select(u => u.Index).ToList();

            bool isExist = false;
            int endNo = StartNo + Number;
            for (int i = StartNo; i < endNo; i++)
            {
                if (existIOs.Contains(i))
                {
                    isExist = true;
                    break;
                }
            }

            if (isExist)
            {
                throw new DeviceException($"IO类型:{IoType.GetDescription()} IO索引和已有索引冲突!");
            }

            // 构建虚拟设备对象
            for (int i = StartNo; i < endNo; i++)
            {
                string axisName = $"{Name}_{i}";
                VIO axis = new VIO()
                {
                    ID = Guid.NewGuid(),
                    DeviceID = MDevice.ID,
                    CardName = MDevice.Name,
                    Module = Module,
                    Name = axisName,
                    IOType = IoType,
                    Behavior = IOBehavior,
                    Mode = DeviceMode.Virtual,
                    Index = i,
                    DefaultValue = DefaultValue,
                };

                // 添加对象
                deviceEngine.AddVirtual(axis);
            }
        }
    }
}