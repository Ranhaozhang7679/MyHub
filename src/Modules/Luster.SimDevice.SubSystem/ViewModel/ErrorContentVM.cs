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
using Luster.SimDevice.SubSystem.Langs;
using Luster.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        private List<ErrorItemModel> GetErrorListForType(Type type)
        {
            var list = new List<ErrorItemModel>();
            var devices = deviceEngine.GetDevices(type).ToList();
            foreach (var device in devices)
            {
                if (device is IDeviceError error && ((device is VIO vio && vio.Behavior == IOBehavior.Input) || device is not VIO))
                {
                    foreach (var item in error.Errors)
                    {
                        list.Add(new ErrorItemModel(error, item));
                    }
                }
            }
            return list;
        }

        private string GetDisplayName(MaintainDeviceTypeItem deviceItem)
        {
            var displayName = LangProvider.GetLang(deviceItem.ItemName);
            return string.IsNullOrEmpty(displayName) ? deviceItem.ItemName : displayName;
        }

        private void ExportTotalCommand()
        {
            try
            {
                string exportDir = @"D:\Hive";
                if (!Directory.Exists(exportDir))
                    Directory.CreateDirectory(exportDir);

                int exportedCount = 0;
                foreach (var deviceItem in DeviceItems)
                {
                    var errorList = GetErrorListForType(deviceItem.ItemType);
                    if (errorList.Count > 0)
                    {
                        string displayName = GetDisplayName(deviceItem);
                        string fileName = Path.Combine(exportDir, $"{displayName}.xls");
                        var excel = new ExcelTool();
                        WriteToSheet(excel, errorList, "DI");
                        excel.Save(fileName);
                        exportedCount++;
                    }
                }

                if (exportedCount > 0)
                    MessageBox.Show($"已导出 {exportedCount} 个文件到 {exportDir}", "导出完成");
                else
                    MessageBox.Show("没有可导出的数据", "提示");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败：{ex.Message}", "错误");
            }
        }

        private void ImportFromFile(string fileName, List<ErrorItemModel> errorList)
        {
            var excel = new ExcelTool(fileName, false);
            var diTable = excel.GetTableBySheet("DI", 1, 0);
            for (int i = 0; i < errorList.Count && i < diTable.Rows.Count; i++)
            {
                errorList[i].Name = diTable.Rows[i][1].ToString();
                errorList[i].ErrorCode = diTable.Rows[i][2].ToString();
                errorList[i].ErrorForeignMessage = diTable.Rows[i][3].ToString();
                if (diTable.Columns.Count > 4)
                    errorList[i].AlarmCategory = diTable.Rows[i][4]?.ToString();
                if (diTable.Columns.Count > 5)
                    errorList[i].RepairAction = diTable.Rows[i][5]?.ToString();
            }
        }

        private void ImportTotalCommand()
        {
            try
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                dialog.Title = "选择包含报警配置文件的文件夹";
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    MessageBox.Show("未选择导入文件夹，操作已取消", "提示");
                    return;
                }

                string selectedFolder = dialog.FileName;
                int importedCount = 0;

                foreach (var deviceItem in DeviceItems)
                {
                    string displayName = GetDisplayName(deviceItem);
                    string fileName = Path.Combine(selectedFolder, $"{displayName}.xls");
                    if (!File.Exists(fileName))
                        fileName = Path.Combine(selectedFolder, $"{displayName}.xlsx");

                    if (!File.Exists(fileName))
                        continue;

                    // 获取设备列表，从文件导入数据
                    var errorList = GetErrorListForType(deviceItem.ItemType);
                    if (errorList.Count == 0)
                        continue;

                    ImportFromFile(fileName, errorList);

                    // 如果是当前选中的设备类型，清空界面并用导入的数据重新填充
                    if (SelectedDeviceItem != null && deviceItem.ItemType == SelectedDeviceItem.ItemType)
                    {
                        ErrorDatas.Clear();
                        foreach (var item in errorList)
                            ErrorDatas.Add(item);
                    }

                    importedCount++;
                }

                deviceEngine.Save();

                if (importedCount > 0)
                    MessageBox.Show($"已导入 {importedCount} 个文件的配置", "导入完成");
                else
                    MessageBox.Show("未找到匹配的配置文件", "提示");
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("导入文件失败：文件正在被其他程序占用，请关闭文件后重试", "错误");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入失败：{ex.Message}", "错误");
            }
        }

    }
}
