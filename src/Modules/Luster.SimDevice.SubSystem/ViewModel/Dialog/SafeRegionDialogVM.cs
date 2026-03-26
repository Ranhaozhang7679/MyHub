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

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class SafeRegionDialogVM : DialogVM
    {
        /// <summary>
        /// 轴对象
        /// </summary>
        private AxisModel _current;
        public AxisModel Current
        {
            get { return _current; }
            set { SetProperty(ref _current, value); }
        }

        /// <summary>
        /// 最小范围 单位是mm
        /// </summary>
        private double _min = 0;
        public double Min
        {
            get => _min; set
            {
                SetProperty(ref _min, value);
            }
        }

        /// <summary>
        /// 最大范围 单位是mm
        /// </summary>
        private double _max = 100;
        public double Max
        {
            get => _max; set
            {
                SetProperty(ref _max, value);
            }
        }

        /// <summary>
        /// 安全数据
        /// </summary>
        private ObservableCollection<KeyValue> _safeDatas;
        public ObservableCollection<KeyValue> SafeDatas
        {
            get { return _safeDatas; }
            set { _safeDatas = value; }
        }

        /// <summary>
        /// 虚拟设备
        /// </summary>
        private IVirtualDevice _checkVirtul;
        public IVirtualDevice SafePos
        {
            get { return _checkVirtul; }
            set { SetProperty(ref _checkVirtul, value); }
        }

        private List<IVirtualDevice> allAxis;

        /// <summary>
        /// 安全区域对话框
        /// </summary>
        /// <param name="_engine"></param>
        protected SafeRegionDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            SafeDatas = new ObservableCollection<KeyValue>();
            allAxis = new List<IVirtualDevice>();
            var postions = _engine.Engine.GetDevices(typeof(IPosition));
            foreach (var item in postions)
            {
                allAxis.Add(item);

                SafeDatas.Add(new KeyValue()
                {
                    Value = item,
                    Desc = item.Name,
                    Key = item.ID.ToString()
                });
            }
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.TryGetValue<AxisModel>("AxisModel", out var current))
            {
                Current = current;
            }
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="result"></param>
        /// <exception cref="FriendlyException"></exception>
        protected override void Ok(IDialogResult result)
        {
            if (Current == null)
            {
                throw new FriendlyException("依赖设备不能为空!");
            }

            if (Current != null && SafePos != null && SafePos is IPosition pos)
            {
                var safeModel = new SafeModel(pos, Current.Tag);
                safeModel.Min = Min;
                safeModel.Max = Max;
                Current.Tag.AddPosition(safeModel);
            }
        }
    }
}