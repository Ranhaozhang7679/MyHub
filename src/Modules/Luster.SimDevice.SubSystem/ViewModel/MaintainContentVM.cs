#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MaintainContentVm
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel
* 文 件 名:       MaintainContentVm.cs
* 创建时间:       2022/12/9 9:10:18
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      9a8ae735-374c-4b1d-b7a9-f7a26c9e4280
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/9 9:10:18
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Motion.DataStruct.Interfaces;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class MaintainContentVM : PageVM
    {
        // 本地画笔
        private SolidColorBrush YellowBrush = new SolidColorBrush(Color.FromRgb(250, 204, 20));
        private SolidColorBrush GreenBrush = new SolidColorBrush(Color.FromRgb(47, 194, 90));
        private SolidColorBrush OrangeBrush = new SolidColorBrush(Color.FromRgb(245, 127, 101));
        private SolidColorBrush DarkBlueBrush = new SolidColorBrush(Color.FromRgb(82, 108, 201));
        private SolidColorBrush PoolBlueBrush = new SolidColorBrush(Color.FromRgb(87, 169, 253));

        /// <summary>
        /// 总寿命标题
        /// </summary>
        private string _fullTitle;
        public string FullTitle
        {
            get { return _fullTitle; }
            set { SetProperty(ref _fullTitle, value); }
        }

        /// <summary>
        /// 已使用标题
        /// </summary>
        private string _usedTitle;
        public string UsedTitle
        {
            get { return _usedTitle; }
            set { SetProperty(ref _usedTitle, value); }
        }

        /// <summary>
        /// 维护标题
        /// </summary>
        private string _maintaiTitle;
        public string MaintainTitle
        {
            get { return _maintaiTitle; }
            set { SetProperty(ref _maintaiTitle, value); }
        }

        /// <summary>
        /// 当前使用标题
        /// </summary>
        private string _currentTitle;
        public string CurrentTitle
        {
            get { return _currentTitle; }
            set { SetProperty(ref _currentTitle, value); }
        }

        /// <summary>
        /// 当前类型寿命单位
        /// </summary>
        private string _unit = "mm";
        public string Unit
        {
            get { return _unit; }
            set { SetProperty(ref _unit, value); }
        }

        /// <summary>
        /// 设置最大使用寿命
        /// </summary>
        private double _maxHP = 200000;
        public double MaxHP
        {
            get { return _maxHP; }
            set { SetProperty(ref _maxHP, value); }
        }

        /// <summary>
        /// 设置维护寿命
        /// </summary>
        private double _maintainHP = 5000;
        public double MaintainHP
        {
            get { return _maintainHP; }
            set { SetProperty(ref _maintainHP, value); }
        }

        /// <summary>
        /// 重置维护寿命
        /// </summary>
        public DelegateCommand<MaintainItemModel> ResetMaintainTimeCommand { get; set; }

        public DelegateCommand<MaintainDeviceTypeItem> SelectedCommand { get; set; }
        public DelegateCommand SetAllMaintainDeviceCommand { get; set; }

        /// <summary>
        /// 单独设置寿命
        /// </summary>
        public DelegateCommand<MaintainItemModel> SetSelectedMaintainDeviceCommand { get; set; }


        /// <summary>
        /// 设备类型列表
        /// </summary>
        public List<MaintainDeviceTypeItem> DeviceItems { get; set; }

        /// <summary>
        /// 当前选择的设备类型
        /// </summary>
        public MaintainDeviceTypeItem _selectedDeviceItem;
        public MaintainDeviceTypeItem SelectedDeviceItem
        {
            get { return _selectedDeviceItem; }
            set { SetProperty(ref _selectedDeviceItem, value); }
        }
        public ISimDeviceEngineUI simDeviceEngineUI;
        /// <summary>
        /// 设备列表
        /// </summary>
        public ObservableCollection<MaintainItemModel> MaintainModels { get; set; }
        protected MaintainContentVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            simDeviceEngineUI= _engine;
            MaintainModels = new ObservableCollection<MaintainItemModel>();
            DeviceItems = new List<MaintainDeviceTypeItem>();
            SetAllMaintainDeviceCommand = new DelegateCommand(SetAllMaintainDevice);
            ResetMaintainTimeCommand = new DelegateCommand<MaintainItemModel>(ResetMaintainTime);
            SelectedCommand = new DelegateCommand<MaintainDeviceTypeItem>(Selected);
            SetSelectedMaintainDeviceCommand = new DelegateCommand<MaintainItemModel>(SetSelectedMaintainDeviced);
            var deviceTypes = deviceEngine.GetDevices(typeof(IMaintain)).GroupBy(x => x.GetType()).ToDictionary(x => x.Key);
            foreach (var item in deviceTypes)
            {
                DeviceItems.Add(new MaintainDeviceTypeItem()
                {
                    ItemName = item.Key.Name,
                    ItemType = item.Key,
                });
            }
            if (DeviceItems.Count > 0)
            {
                SelectedDeviceItem = DeviceItems[0];
                InitDeviceModel(DeviceItems[0].ItemType);
            }
        }

        private void Selected(MaintainDeviceTypeItem item)
        {
            if (item != null) 
            {
                InitDeviceModel(item.ItemType);
            }
        }

        /// <summary>
        /// 一键设置全部设备维护寿命参数
        /// </summary>
        private void SetAllMaintainDevice()
        {
            foreach (var model in MaintainModels)
            {
                model.MaxHP = MaxHP;
                model.MaintainHP = MaintainHP;
                model.Percent = model.DeviceTag.GetPercent();
                model.ActualStr = model.DeviceTag.GetActual();
            }
        }

        /// <summary>
        /// 重置设备维护寿命
        /// </summary>
        /// <param name="model"></param>
        private void ResetMaintainTime(MaintainItemModel model)
        {
            if (model != null)
            {
                model.UsedHP = model.UsedHP + model.CurrentHP;
                model.CurrentHP = 0;
                model.Percent = model.DeviceTag.GetPercent();
                model.ActualStr = model.DeviceTag.GetActual();
                simDeviceEngineUI.MainTainClear(model);
            }
        }

        /// <summary>
        /// 根据类型初始化Model
        /// </summary>
        /// <param name="type"></param>
        private void InitDeviceModel(Type type)
        {
            MaintainModels.Clear();
            var devices = deviceEngine.GetDevices(type).ToList();
            Unit = (devices[0] as IMaintain).Unit;
            FullTitle = $"{Langs.LangProvider.GetLang("MaxScale")}({Unit})";
            UsedTitle = $"{Langs.LangProvider.GetLang("Used")}({Unit})";
            MaintainTitle = $"{Langs.LangProvider.GetLang("MaintainScale")}({Unit})";
            CurrentTitle = $"{Langs.LangProvider.GetLang("CurrentScale")}({Unit})";
            foreach (var device in devices)
            {
                MaintainModels.Add(new MaintainItemModel(device as IMaintain));
            }
            UpdateForeBrush();
        }

        /// <summary>
        /// 给不同model赋值色彩
        /// </summary>
        private void UpdateForeBrush()
        {
            if (MaintainModels.Count() > 0)
            {
                for (int i = 0; i < MaintainModels.Count(); i++)
                {
                    var colorIndex = i % 5;
                    switch (colorIndex)
                    {
                        case 0:
                            MaintainModels[i].ForeColor = YellowBrush;
                            break;
                        case 1:
                            MaintainModels[i].ForeColor = GreenBrush;
                            break;
                        case 2:
                            MaintainModels[i].ForeColor = OrangeBrush;
                            break;
                        case 3:
                            MaintainModels[i].ForeColor = DarkBlueBrush;
                            break;
                        case 4:
                            MaintainModels[i].ForeColor = PoolBlueBrush;
                            break;
                        default:
                            break;
                    }

                }
            }
        }

        private void SetSelectedMaintainDeviced(MaintainItemModel model)
        {
            model.MaxHP = MaxHP;
            model.MaintainHP = MaintainHP;
            model.Percent = model.DeviceTag.GetPercent();
            model.ActualStr = model.DeviceTag.GetActual();
        }
    }


}
