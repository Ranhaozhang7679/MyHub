#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FileConfigContentVM
* 机器名称:       L05590
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       FileConfigContentVM.cs
* 创建时间:       2023/6/19 15:34:14
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      3447a0c9-b933-404c-a2c7-e7563a365f8f
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/6/19 15:34:14
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.TaskFlow.Engine;
using Luster.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class FileConfigContentVM: MotionPageVM
    {
        private IMotionController _mController;

        public FileConfigContentVM(ICommonBus commonBus, IMotionController mController) : base(commonBus)
        {
            _mController=mController;
            InitData(commonBus.CurrentRecipe);
        }



        #region 属性
        /// <summary>
        /// 是否保存
        /// </summary>
        private bool _isSave = false;
        public bool IsSave
        {
            get { return _isSave; }
            set
            {
                SetProperty(ref _isSave, value);
                commonBus.IsNeedSave = value;
            }
        }

        /// <summary>
        /// 图像路径
        /// </summary>
        private string _picPath;
        public string PicPath
        {
            get { return _picPath; }
            set
            {
                SetProperty(ref _picPath, value);
                _mController.FileConfig.PicSavePath = value;
                IsSave = true;
            }
        }
        /// <summary>
        /// 图像保存天数
        /// </summary>
        private int _picSaveDays;
        public int PicSaveDays
        {
            get { return _picSaveDays; }
            set
            {
                SetProperty(ref _picSaveDays, value);
                _mController.FileConfig.PicSaveDays = value;
                IsSave = true;
            }
        }


        /// <summary>
        /// 数据库路径
        /// </summary>
        private string _dBPath;
        public string DBPath
        {
            get { return _dBPath; }
            set
            {
                SetProperty(ref _dBPath, value);
                _mController.FileConfig.DBSavePath = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 数据库转储路径
        /// </summary>
        private string _dumpDBPath;
        public string DumpDBPath
        {
            get { return _dumpDBPath; }
            set
            {
                SetProperty(ref _dumpDBPath, value);
                _mController.FileConfig.DumpDBPath = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 数据库保存天数
        /// </summary>
        private int _dBSaveDays;
        public int DBSaveDays
        {
            get { return _dBSaveDays; }
            set
            {
                SetProperty(ref _dBSaveDays, value);
                _mController.FileConfig.DBSaveDays = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 日志备份天数
        /// </summary>
        private int _logBackUpDays;
        public int LogBackUpDays
        {
            get { return _logBackUpDays; }
            set
            {
                SetProperty(ref _logBackUpDays, value);
                _mController.FileConfig.LogBackUpDays = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 配方备份天数
        /// </summary>
        private int _recipeBackUpDays;
        public int RecipeBackUpDays
        {
            get { return _recipeBackUpDays; }
            set
            {
                SetProperty(ref _recipeBackUpDays, value);
                _mController.FileConfig.RecipeBackUpDays = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// DB备份天数
        /// </summary>
        private int _dbBackUpDays;
        public int DBBackUpDays
        {
            get { return _dbBackUpDays; }
            set
            {
                SetProperty(ref _dbBackUpDays, value);
                _mController.FileConfig.DBBackUpDays = value;
                IsSave = true;
            }
        }


        /// <summary>
        /// 最新周保养时间
        /// </summary>
        private DateTime _lastWeekMaintenanceDate;
        public DateTime LastWeekMaintenanceDate
        {
            get { return _lastWeekMaintenanceDate; }
            set
            {
                SetProperty(ref _lastWeekMaintenanceDate, value);
            }
        }

        /// <summary>
        /// 最新月保养时间
        /// </summary>
        private DateTime _lastMonthMaintenanceDate;
        public DateTime LastMonthMaintenanceDate
        {
            get { return _lastMonthMaintenanceDate; }
            set
            {
                SetProperty(ref _lastMonthMaintenanceDate, value);
            }
        }

        /// <summary>
        /// 宕机原因
        /// </summary>
        private string _downTimeCauses;
        public string DownTimeCauses
        {
            get { return _downTimeCauses; }
            set
            {
                SetProperty(ref _downTimeCauses, value);
                _mController.FileConfig.DownTimeCauses = value;
                IsSave =true;
            }
        }

        /// <summary>
        /// 消耗物品
        /// </summary>
        private string _sparePartsUsed;
        public string SparePartsUsed
        {
            get { return _sparePartsUsed; }
            set
            {
                SetProperty(ref _sparePartsUsed, value);
                _mController.FileConfig.SparePartsUsed = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 检查步骤
        /// </summary>
        private string _actionLists;
        public string ActionLists
        {
            get { return _actionLists; }
            set
            {
                SetProperty(ref _actionLists, value);
                _mController.FileConfig.ActionLists = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 消耗物品
        /// </summary>
        private string _toolLists;
        public string ToolLists
        {
            get { return _toolLists; }
            set
            {
                SetProperty(ref _toolLists, value);
                _mController.FileConfig.ToolLists = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 消耗物品
        /// </summary>
        private string _settingLists;
        public string SettingLists
        {
            get { return _settingLists; }
            set
            {
                SetProperty(ref _settingLists, value);
                _mController.FileConfig.SettingLists = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 日志存放路径
        /// </summary>
        private string _logsSavePath;
        public string LogsSavePath
        {
            get { return _logsSavePath; }
            set
            {
                SetProperty(ref _logsSavePath, value);
                _mController.FileConfig.LogsSavePath = value;
                IsSave = true;
            }
        }


        #endregion

        private void InitData(Recipe r)
        {
            if ( commonBus.ProjInfo != null && !string.IsNullOrEmpty(commonBus.ProjInfo.ProjPath))
            {
                string fileConfig = Path.Combine(r.ProjInfo.ProjPath, "Config", "FileConfig.xml");
                _mController.FileConfig.LoadFileConfig(fileConfig);//加载配置
            }


            PicPath = _mController.FileConfig.PicSavePath ?? "";
            DBPath = _mController.FileConfig.DBSavePath ?? "";
            LogsSavePath= _mController.FileConfig.LogsSavePath ?? "";
            DumpDBPath = _mController.FileConfig.DumpDBPath ?? "";
            PicSaveDays = _mController.FileConfig.PicSaveDays == 0 ? 0 : _mController.FileConfig.PicSaveDays;
            DBSaveDays = _mController.FileConfig.DBSaveDays == 0 ? 90 : _mController.FileConfig.DBSaveDays;
            LogBackUpDays = _mController.FileConfig.LogBackUpDays == 0 ? 10 : _mController.FileConfig.LogBackUpDays;
            RecipeBackUpDays = _mController.FileConfig.RecipeBackUpDays == 0 ? 10 : _mController.FileConfig.RecipeBackUpDays;
            DBBackUpDays = _mController.FileConfig.DBBackUpDays == 0 ? 90 : _mController.FileConfig.DBBackUpDays;
            LastWeekMaintenanceDate=_mController.FileConfig.LastWeekMaintenanceDate;
            LastMonthMaintenanceDate = _mController.FileConfig.LastMonthMaintenanceDate;
            DownTimeCauses= _mController.FileConfig.DownTimeCauses;
            SparePartsUsed= _mController.FileConfig.SparePartsUsed;
            ActionLists = _mController.FileConfig.ActionLists;
            ToolLists = _mController.FileConfig.ToolLists;
            SettingLists = _mController.FileConfig.SettingLists;

            if (commonBus.CurrentRecipe != null)
            {
                if (DBPath == "")
                {
                    DBPath = commonBus.CurrentRecipe.GetRecipePath() + @"\db\recipe.db";
                }
                if (DumpDBPath == "")
                {
                    DumpDBPath = commonBus.CurrentRecipe.GetRecipePath() + @"\dbBack";
                }
            }
        }

        /// <summary>
        /// 浏览选择路径
        /// </summary>
        private DelegateCommand<string> _selectPathCommand;
        public DelegateCommand<string> SelectPathCommand => _selectPathCommand ?? (_selectPathCommand = new DelegateCommand<string>((sPath) =>
        {
            Luster.WindowsAPICodePack.Dialogs.CommonOpenFileDialog dialog = new WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                commonBus.IsNeedSave = true;
                if (sPath == "Pic")
                {
                    PicPath = dialog.FileName;
                    _mController.FileConfig.PicSavePath = dialog.FileName;
                }
                if (sPath == "DB")
                {
                    DBPath = dialog.FileName;
                    _mController.FileConfig.DBSavePath = dialog.FileName;
                }
                if (sPath == "DumpDB")
                {
                    DumpDBPath = dialog.FileName;
                    _mController.FileConfig.DumpDBPath = dialog.FileName;
                }
                if (sPath == "Log")
                {
                    LogsSavePath = dialog.FileName;
                    _mController.FileConfig.LogsSavePath = dialog.FileName;
                }
            }
        }));
    }
}