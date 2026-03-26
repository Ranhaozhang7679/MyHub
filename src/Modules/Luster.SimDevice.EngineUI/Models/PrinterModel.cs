using Luster.Motion.DataStruct.DataModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class PrinterModel : BindableBase
    {
        /// <summary>
        /// 对应的ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                bool src = _isSelected;
                SetProperty(ref _isSelected, value);
                if (src != value)
                    OnSelected(value);
            }
        }

        private string _content;

        public string Content
        {
            get { return _content; }
            //set { _content = value; }
            set { SetProperty(ref _content, value); }
        }


        #region 常规参数
        /// <summary>
        /// 模块
        /// </summary>
        private string _module;
        public string Module { get => _module; set => SetProperty(ref _module, value); }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        /// <summary>
        /// 打印机名称
        /// </summary>
        private string _printerName;
        public string PrinterName
        {
            get => _printerName;
            set => SetProperty(ref _printerName, value);
        }

        /// <summary>
        /// 标签宽度
        /// </summary>
        private int _pageWidth;
        public int PageWidth { get => _pageWidth; set => SetProperty(ref _pageWidth, value); }

        /// <summary>
        /// 标签高度
        /// </summary>
        private int _pageHeight;
        public int PageHeight { get => _pageHeight; set => SetProperty(ref _pageHeight, value); }
        #endregion

        /// <summary>
        /// 选择发生了变更
        /// </summary>
        public event Action<bool> SelectedChanged;

        public void OnSelected(bool isSelected)
        {
            SelectedChanged?.Invoke(isSelected);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vFeeder"></param>
        public PrinterModel(VPrinter vPrinter)
        {
            ID = vPrinter.ID;
            IsSelected = false;
            Module = vPrinter.Module;
            Name = vPrinter.Name;
            PrinterName = vPrinter.PrinterName;
            PageWidth = vPrinter.PageWidth;
            PageHeight = vPrinter.PageHeight;
            Tag = vPrinter;
        }

        public VPrinter Tag { get; set; }

    }
}
