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
using Luster.Common.Tools;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Microsoft.Win32;
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
    public class ErrorContentVM : PageVM
    {
        /// 重置维护寿命
        /// </summary>
        public DelegateCommand<ErrorItemModel> ResetMaintainTimeCommand { get; set; }

        public DelegateCommand SetAllMaintainDeviceCommand { get; set; }

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

        public DelegateCommand BatchExportCommand { get; set; }
        public DelegateCommand BatchImportCommand { get; set; }


        public ISimDeviceEngineUI simDeviceEngineUI;
        /// <summary>
        /// 设备列表
        /// </summary>
        public ObservableCollection<ErrorItemModel> ErrorDatas { get; set; }

        protected ErrorContentVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            simDeviceEngineUI = _engine;
            DeviceItems = new List<MaintainDeviceTypeItem>();
            ErrorDatas = new ObservableCollection<ErrorItemModel>();

            var deviceTypes = deviceEngine.GetDevices(typeof(IDeviceError)).GroupBy(x => x.GetType()).ToDictionary(x => x.Key);
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
            BatchExportCommand = new DelegateCommand(ExportTotalCommand);
            BatchImportCommand = new DelegateCommand(ImportTotalCommand);
        }

        private void Selected(MaintainDeviceTypeItem item)
        {
            if (item != null)
            {
                InitDeviceModel(item.ItemType);
            }
        }



        /// <summary>
        /// 根据类型初始化Model
        /// </summary>
        /// <param name="type"></param>
        private void InitDeviceModel(Type type)
        {
            ErrorDatas.Clear();
            var devices = deviceEngine.GetDevices(type).ToList();
            foreach (var device in devices)
            {
                if (device is IDeviceError error &&((device is VIO vio &&vio.Behavior==IOBehavior.Input)||device is not VIO))
                {
                    foreach (var item in error.Errors)
                    {
                        ErrorDatas.Add(new ErrorItemModel(error, item));
                    }
                }
            }
        }

        private DelegateCommand<MaintainDeviceTypeItem> _selectedCommand;
        public DelegateCommand<MaintainDeviceTypeItem> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<MaintainDeviceTypeItem>(item =>
        {
            InitDeviceModel(item.ItemType);
        }));


        private void WriteToSheet(ExcelTool excel, List<ErrorItemModel> errorList, string sheetName)
        {
            excel.RemoveWorkSheet(0);
            excel.AddWorksheet(sheetName);
            var header = new string[6] { "设备名称", "名称", "报警代码",  "报警配置", "报警种类", "维修动作"};
            var data = new object[6];
            excel.SetHeaders(0, 0, header);
            for (int i = 0; i < errorList.Count(); i++)
            {
                data[0] = errorList[i].DeviceName;
                data[1] = errorList[i].Name;
                data[2] = errorList[i].ErrorCode;
                data[3] = errorList[i].ErrorForeignMessage;
                data[4] = errorList[i].AlarmCategory;
                data[5] = errorList[i].RepairAction;
                excel.WriteRowDatas(i + 1, 0, data);
            }
        }

        private void BatchExport(List<ErrorItemModel> items)
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "XLS|*.xls";
            var result = saveFile.ShowDialog().Value;
            if (result)
            {
                var filename = saveFile.FileName;
                var excel = new ExcelTool();
                WriteToSheet(excel, items, "DI");
                excel.Save(filename);
            }
        }

        private void ExportTotalCommand()
        {
            if (ErrorDatas.Count() > 0)
            {
                BatchExport(ErrorDatas.ToList());
            }
        }

        private void BatchImport(List<ErrorItemModel> items)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "(配置文件)|*.xls;*.xlsx";
            if (fileDialog.ShowDialog() == true)
            {
                //var fileName = fileDialog.FileName;
                var excel = new ExcelTool(fileDialog.FileName, false);
                var diTable = excel.GetTableBySheet("DI", 1, 0);
                int i = 0;
                foreach (var item in items)
                {
                    item.Name = diTable.Rows[i][1].ToString();
                    item.ErrorCode = diTable.Rows[i][2].ToString();
                    item.ErrorForeignMessage = diTable.Rows[i][3].ToString();
                    if (diTable.Columns.Count > 4)
                        item.AlarmCategory = diTable.Rows[i][4]?.ToString();
                    if (diTable.Columns.Count > 5)
                        item.RepairAction = diTable.Rows[i][5]?.ToString();
                    i++;
                }

                // 导入完成后立即保存，防止重启丢失
                deviceEngine.Save();
            }
        }

        private void ImportTotalCommand()
        {
            if (ErrorDatas.Count() > 0)
            {
                BatchImport(ErrorDatas.ToList());
            }
        }

    }
}
