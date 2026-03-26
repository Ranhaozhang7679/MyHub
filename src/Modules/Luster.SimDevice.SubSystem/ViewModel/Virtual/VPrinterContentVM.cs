using Luster.Common.Assets;
using Luster.Common.Tools.Tools;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    internal class VPrinterContentVM : PageVM
    {
        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 显示移除按钮
        /// </summary>
        public override bool IsShowRemove => true;

        /// <summary>
        /// 显示参数
        /// </summary>
        private bool _isShowParam;
        public bool IsShowParam
        {
            get { return _isShowParam; }
            set { SetProperty(ref _isShowParam, value); }
        }

        protected VPrinterContentVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            LoadDevices();
        }

        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices(string key = "")
        {
            var devices = deviceEngine.GetDevices(typeof(VPrinter));
            PrinterList = new ObservableCollection<PrinterModel>();
            foreach (var device in devices)
            {
                var vPrinter = device as VPrinter;
                var printerModel = new PrinterModel(vPrinter);
                printerModel.SelectedChanged += PrinterModel_SelectedChanged;
                if (string.IsNullOrEmpty(key))
                {
                    PrinterList.Add(printerModel);
                }
                else
                {
                    if (vPrinter.Module.Contains(key) || vPrinter.Name.Contains(key))
                    {
                        PrinterList.Add(printerModel);
                    }
                }
            }
        }

        /// <summary>
        /// 打印机列表
        /// </summary>
        private ObservableCollection<PrinterModel> printerList;
        public ObservableCollection<PrinterModel> PrinterList
        {
            get { return printerList; }
            set { SetProperty(ref printerList, value); }
        }

        /// <summary>
        /// 选择所有
        /// </summary>
        private bool _isSelectAll;
        public bool IsSelectAll
        {
            get { return _isSelectAll; }
            set { SetProperty(ref _isSelectAll, value); }
        }

        /// <summary>
        /// 如果来源与Header点击事件变化
        /// </summary>
        private bool isHeaderClick = false;

        private void PrinterModel_SelectedChanged(bool isSelected)
        {
            // 防止重复触发，如果是来源于行
            if (!isHeaderClick)
            {
                IsSelectAll = PrinterList.Count(u => u.IsSelected) == PrinterList.Count();

                // 将Item添加到Selected
                UpdateSelected();
            }
        }


        /// <summary>
        /// 更新选中项目
        /// </summary>
        private void UpdateSelected()
        {
            // 点击的情况
            SelectedList = new ObservableCollection<PrinterModel>();
            foreach (var item in PrinterList)
            {
                if (item.IsSelected)
                {
                    SelectedList.Add(item);
                }
            }
        }

        /// <summary>
        /// 按钮切换
        /// </summary>
        private DelegateCommand<object> _selectAllCommand;
        public DelegateCommand<object> SelectAllCommand => _selectAllCommand ?? (_selectAllCommand = new DelegateCommand<object>((dgControl) =>
        {
            isHeaderClick = true;
            var selectCount = PrinterList.Count(u => u.IsSelected);

            foreach (var item in PrinterList)
            {
                item.IsSelected = selectCount != PrinterList.Count;
            }

            UpdateSelected();
            isHeaderClick = false;
        }));

        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<string> _searchCommand;
        public DelegateCommand<string> SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand<string>((key) =>
        {
            string trimKey = key.Trim();

            LoadDevices(trimKey);
        }));

        /// <summary>
        /// 打印机选中列表
        /// </summary>
        private ObservableCollection<PrinterModel> selectedList;
        public ObservableCollection<PrinterModel> SelectedList
        {
            get { return selectedList; }
            set { SetProperty(ref selectedList, value); }
        }

        /// <summary>
        /// 增加新的打印机
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowPrinterDialog(null, r =>
            {
                LoadDevices();
            });
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        public override void RemoveItem()
        {
            if (SelectedList.Count == 0)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, "请先选择要进行删除的项");
            }

            // 删除确认
            dialogService.ShowConfirm($"确认删除{SelectedList.Count}项?", (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var lstIds = new List<Guid>();
                    foreach (var item in SelectedList)
                    {
                        PrinterList.Remove(item);
                        lstIds.Add(item.ID);
                    }

                    // 删除DeviceEngine中
                    deviceEngine.ReomoveVirtual(lstIds.ToArray());
                    SelectedList.Clear();

                    // 全部清除的时候，将
                    IsSelectAll = false;
                }
            });
        }

        /// <summary>
        /// 编辑指令
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((item) =>
        {
            dialogService.ShowPrinterDialog(item as PrinterModel, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    LoadDevices();
                }
            });
        }));

        /// <summary>
        /// 打印机打印
        /// </summary>
        private DelegateCommand<PrinterModel> _printerPrintCommand;
        public DelegateCommand<PrinterModel> PrinterPrintCommand => _printerPrintCommand ?? (_printerPrintCommand = new DelegateCommand<PrinterModel>((printer) =>
        {
            printer.Tag.Print(printer.Content, 5000);
        }));
    }

}
