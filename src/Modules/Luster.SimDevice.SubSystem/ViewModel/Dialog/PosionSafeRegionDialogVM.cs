using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Virtual;
using Luster.Motion.DataStruct;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class PosionSafeRegionDialogVM : DialogVM
    {
        /// <summary>
        /// 轴对象
        /// </summary>
        private PositionItem _current;
        public PositionItem Current
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
        protected PosionSafeRegionDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
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
            if (parameters.TryGetValue<PositionItem>("PositionItem", out var current))
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
                var safeModel = new PosionSafeModel(pos,Current.Tag.Axis, Current.Tag);
                safeModel.Min = Min;
                safeModel.Max = Max;
                Current.Tag.Axis.AddPosionPosition(safeModel);
            }
        }
    }
}
