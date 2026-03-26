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
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class ImportExDialogVM : DialogVM
    {
        protected ImportExDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            // 初始化控制块
            MotionCardList = deviceEngine.GetRealDevices(typeof(IMotionCard));
            excelTool = null;
        }

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
        /// 分组
        /// </summary>
        private string _excelFile;
        [Required]
        public string ExcelFile
        {
            get => _excelFile; set
            {
                SetProperty(ref _excelFile, value);
            }
        }
        #endregion

        /// <summary>
        /// Excel帮助类
        /// </summary>
        private ExcelTool excelTool = null;

        private DelegateCommand _selectFileCommand;
        public DelegateCommand SelectFileCommand => _selectFileCommand ?? (_selectFileCommand = new DelegateCommand(() =>
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "(IO配置文件)|*.xls;*.xlsx";
            if (fileDialog.ShowDialog() == true)
            {
                ExcelFile = fileDialog.FileName;
            }
        }));

        private bool isImportIO = false;

        /// <summary>
        /// 弹窗
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.TryGetValue<bool>("IsIO", out var io))
            {
                isImportIO = io;
            }
        }

        protected override void Ok(IDialogResult result)
        {
            excelTool = new ExcelTool(ExcelFile, false);
            if (isImportIO)
            {
                // 导入DI
                var diTable = excelTool.GetTableBySheet("DI", 1, 0);
                ImportIO(diTable, IOBehavior.Input);

                // 导入DO
                var doTable = excelTool.GetTableBySheet("DO", 1, 0);
                ImportIO(doTable, IOBehavior.Output);

                // 导入AI
                var aiTable = excelTool.GetTableBySheet("AI", 1, 0);
                ImportIO(aiTable, IOBehavior.Input, IOType.Analog);

                // 导入AO
                var aoTable = excelTool.GetTableBySheet("AO", 1, 0);
                ImportIO(aoTable, IOBehavior.Output, IOType.Analog);
            }
            else
            {
                var axisTable = excelTool.GetTableBySheet("AxisPara", 1, 0);
                for (int i = 0; i < axisTable.Rows.Count; i++)
                {
                    // LC卡应该依据电控给的结果来
                    int axisNo = int.Parse(axisTable.Rows[i][0].ToString());
                    string name = axisTable.Rows[i][2].ToString();
                    string module = axisTable.Rows[i][3].ToString();
                    int acc = int.Parse(axisTable.Rows[i][4].ToString());
                    int dec = int.Parse(axisTable.Rows[i][5].ToString());
                    int speed = int.Parse(axisTable.Rows[i][6].ToString());
                    int pulse = int.Parse(axisTable.Rows[i][7].ToString());
                    int homeHigh = int.Parse(axisTable.Rows[i][8].ToString());
                    int homeLow = int.Parse(axisTable.Rows[i][9].ToString());
                    int homeOffset = int.Parse(axisTable.Rows[i][10].ToString());
                    string homeMethod = axisTable.Rows[i][11].ToString();
                    string homePri = axisTable.Rows[i][12].ToString();

                    int.TryParse(axisTable.Rows[i][13].ToString(), out var jogSpeed);
                    float.TryParse(axisTable.Rows[i][14].ToString(), out var softML);
                    float.TryParse(axisTable.Rows[i][15].ToString(), out var softPL);
                    int.TryParse(axisTable.Rows[i][16].ToString(), out var stime);
                    int.TryParse(axisTable.Rows[i][17].ToString(), out var axisBand);
                    int.TryParse(axisTable.Rows[i][18].ToString(), out var homeAcc);
                    int.TryParse(axisTable.Rows[i][19].ToString(), out var homeDec);

                    int axisType = 0;
                    if (axisTable.Rows[i].ItemArray.Length > 20)
                    {
                        int.TryParse(axisTable.Rows[i][20].ToString(), out axisType);
                    }

                    VAxis axis = new VAxis()
                    {
                        ID = Guid.NewGuid(),
                        DeviceID = MDevice.ID,
                        Name = name,
                        Module = module,
                        AxisNo = axisNo,
                        Acc = acc,
                        Dec = dec,
                        MoveSpeed = speed,
                        JogSpeed = jogSpeed,
                        PerPluse = pulse,
                        SoftLimitML = softML,
                        SoftLimitPL = softPL,
                        Stime = stime,
                        AxisBand = axisBand,
                        HomeMode = (HomeMode)Enum.Parse(typeof(HomeMode), homeMethod, false),
                        HomeAcc = (uint)homeAcc,
                        HomeDec = (uint)homeDec,
                        HomePriority = (Priority)Enum.Parse(typeof(Priority), homePri, false),
                        HomeOffset = homeOffset,
                        HomeSpeedHigh = (uint)homeHigh,
                        HomeSpeedLow = (uint)homeLow,
                        AxisType = (AxisType)Enum.Parse(typeof(AxisType), axisType.ToString(), false),
                    };

                    deviceEngine.AddVirtual(axis);
                }
            }
        }

        /// <summary>
        /// 导入IO
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="behavior"></param>
        /// <param name="ioType"></param>
        private void ImportIO(DataTable dt, IOBehavior behavior, IOType ioType = IOType.Digital)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i][2].ToString();
                string subDefinite = dt.Rows[i][3].ToString();
                string module = dt.Rows[i][4].ToString();
                VIO vIO = new VIO()
                {
                    ID = Guid.NewGuid(),
                    DeviceID = MDevice.ID,
                    CardName = MDevice.Name,
                    Module = module,
                    Name = name,
                    SubDefinite= subDefinite,
                    IOType = ioType,
                    Behavior = behavior,
                    Mode = DeviceMode.Virtual,
                    Index = i + 1, // IO 从1开始
                    DefaultValue = 0,
                };

                deviceEngine.AddVirtual(vIO);
            }
        }
    }
}