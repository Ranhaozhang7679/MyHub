using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class VPrinterDialogVM : ModuleDialogVM
    {
        protected VPrinterDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            //初始化真实打印机列表
            PrinterList = deviceEngine.GetRealDevices(typeof(IPrinter)).Select(u => u as IPrinter).ToList();
        }

        #region 通用参数
        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        [Required]
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        /// <summary>
        /// 标签宽度
        /// </summary>
        private int _pageWidth;
        [Range(1,1000)]
        public int PageWidth { get => _pageWidth; set => SetProperty(ref _pageWidth, value); }

        /// <summary>
        /// 标签高度
        /// </summary>
        private int _pageHeight;
        [Range(1, 1000)]
        public int PageHeight { get => _pageHeight; set => SetProperty(ref _pageHeight, value); }
        #endregion

        /// <summary>
        /// 真实打印机
        /// </summary>
        private IPrinter _mPrinter;
        public IPrinter MPrinter
        {
            get => _mPrinter;
            set => SetProperty(ref _mPrinter, value);
        }

        /// <summary>
        /// 真实打印机列表
        /// </summary>
        public List<IPrinter> PrinterList { get; set; }

        public PrinterModel printerModel;

        /// <summary>
        /// 修改标志位
        /// </summary>
        private bool _editFlag = false;

        /// <summary>
        /// 覆写对话框打开方法
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (parameters.TryGetValue<PrinterModel>("PrinterModel", out var vModel) && vModel != null)
            {
                _editFlag = true;
                printerModel = vModel;
                Module = vModel.Module;
                Name = vModel.Name;
                MPrinter = deviceEngine.GetDeviceByID(vModel.Tag .DeviceID ) as IPrinter ;
                PageHeight = vModel.PageHeight;
                PageWidth = vModel.PageWidth;
            }
        }

        /// <summary>
        /// 覆写窗体关闭方法
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);


            if (_editFlag)
            {
                _editFlag = false;
                printerModel.Name = Name;
                printerModel.Module = Module;
                printerModel.PrinterName= MPrinter.Name;
                printerModel.PageWidth = PageWidth;
                printerModel.PageHeight = PageHeight;

                //更新Device
                var device = deviceEngine.GetVirtualByID(printerModel.ID) as VPrinter;
                device.Name = Name;
                device.Module = Module;
                device.PrinterName = MPrinter.Name;
                device.PageWidth = PageWidth;
                device.PageHeight = PageHeight;
            }
            else
            {
                //构建虚拟设备对象
                VPrinter printer = new VPrinter()
                {
                    ID = Guid.NewGuid(),
                    DeviceID = MPrinter.ID,
                    Name = Name,
                    Module = Module,
                    PrinterName = MPrinter.Name,
                    PageHeight = PageHeight,
                    PageWidth = PageWidth,
                };

                // 添加对象
                deviceEngine.AddVirtual(printer);
            }
        }
    }
}
