#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ImportAddressDialogVM
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       ImportAddressDialogVM.cs
* 创建时间:       2022/11/21 13:35:34
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      6d90ee20-dff2-4586-968a-9cb9cae53683
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/21 13:35:34
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class ImportAddressDialogVM : DialogVM
    {
        private VPlc _plc;
        public VPlc Plc
        {
            get => _plc;
            set => SetProperty(ref _plc, value);
        }

        /// <summary>
        /// 文件名称
        /// </summary>
        private string _excelFile;
        public string ExcelFile
        {
            get => _excelFile; set
            {
                SetProperty(ref _excelFile, value);
            }
        }

        public DelegateCommand SelectFileCommand { get; set; }

        protected ImportAddressDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            SelectFileCommand = new DelegateCommand(SelectFile);
        }

        private void SelectFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "(PLC地址配置文件)|*.xls;*.xlsx";
            if (fileDialog.ShowDialog() == true)
            {
                ExcelFile = fileDialog.FileName;
            }
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            var plc = parameters.GetValue<VPlc>("PlcDevice");
            if (plc != null)
            {
                Plc = plc;
            }
        }

        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);

            if (Plc != null)
            {
                var listAddress = new List<PlcAddr>();
                var excelTool = new ExcelTool(ExcelFile, false);
                // 导入DI
                var aTable = excelTool.GetTableBySheet(0, 1, 0);
                for (int i = 0; i < aTable.Rows.Count; i++)
                {
                    var address = aTable.Rows[i][0].ToString();
                    var dataType = (DataType)Convert.ToInt32(aTable.Rows[i][1]);
                    var addrType = (PlcAddrType)Convert.ToInt32(aTable.Rows[i][2]);
                    //var value = Convert.ToInt32(aTable.Rows[i][3]);
                    var desc = aTable.Rows[i][3].ToString();
                    var plcAddress = new PlcAddr()
                    {
                        Address = address,
                        DataType = dataType,
                        AddrType = addrType,
                        Desc = desc
                        //Value = value
                    };
                    listAddress.Add(plcAddress);
                }
                Plc.Address = listAddress;
            }

        }
    }
}
