#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DeviceDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel.Dialogs
* 文 件 名:       DeviceDialogVM.cs
* 创建时间:       2022/6/9 15:00:49
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ed3fac66-f055-47d9-97b1-de0af1e952ad
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/9 15:00:49
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets.ViewModel;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using Luster.Motion.SubSystem.Models;
using Luster.TaskFlow.Common.Attributes;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    /// <summary>
    /// Dialog
    /// </summary>
    public sealed class VersionDialogVM : MotionDialogVM
    {

        //总线
        ICommonBus commonBus;

        /// <summary>
        /// 版本列表数据
        /// </summary>
        private List<VersionModel> _versions;
        public List<VersionModel> Versions
        {
            get { return _versions; }
            set { SetProperty(ref _versions, value); }
        }

        /// <summary>
        /// 配方版本
        /// </summary>
        private List<VersionModel> _recipeVersions;
        public List<VersionModel> RecipeVersions
        {
            get { return _recipeVersions; }
            set { SetProperty(ref _recipeVersions, value); }
        }

        /// <summary>
        /// 版本
        /// </summary>
        public VersionDialogVM(ICommonBus cmbus)
        {
            Versions = new List<VersionModel>();
            RecipeVersions = new List<VersionModel>();
            commonBus = cmbus;
        }

        /// <summary>
        /// 选择对象
        /// </summary>
        private DelegateCommand<IVirtualDevice> _selectCommand;
        public DelegateCommand<IVirtualDevice> SelectCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<IVirtualDevice>(device =>
             {
                 IDialogResult r = new DialogResult(ButtonResult.OK);
                 r.Parameters.Add("Device", device);
                 r.Parameters.Add("SearchKey", SearchKey);
                 RaiseRequestClose(r);
             }));


        /// <summary>
        /// 查询关键字
        /// </summary>
        private string _searchKey;
        public string SearchKey
        {
            get { return _searchKey; }
            set { SetProperty(ref _searchKey, value); }
        }

        /// <summary>
        /// 对话框打开
        /// </summary>
        /// <param name="parameters">参数列表</param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            string jsonVersion = Path.Combine(Luster.Common.Tools.ReflectionTool.GetAssemblyPath(), "Config", "Version.json");
            if (!File.Exists(jsonVersion))
            {
                throw new Exception("版本配置说明文件不存在!");
            }

            // 读取json文件
            using (FileStream fileStream = new FileStream(jsonVersion, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string strJson = reader.ReadToEnd();
                var obj = Luster.Common.Tools.JsonTool.ToObject<Version>(strJson);
                Versions = obj.Versions;
            }

            string recipeVersionPath = commonBus.ProjInfo.FullName;
            if (string.IsNullOrEmpty(recipeVersionPath)) return;
            recipeVersionPath = recipeVersionPath.Substring(0, recipeVersionPath.LastIndexOf(@"\"));
            recipeVersionPath = Path.Combine(recipeVersionPath, "Config", "Version.json");
            if (File.Exists(recipeVersionPath))
            {
                // 读取json文件
                using (FileStream fileStream = new FileStream(recipeVersionPath, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string strJson = reader.ReadToEnd();
                    var obj = Luster.Common.Tools.JsonTool.ToObject<Version>(strJson);
                    RecipeVersions = obj.Versions;
                }
            }
        }
    }

    class Version
    {
        public List<VersionModel> Versions { get; set; }
    }

    public class VersionModel
    {
        public string Version { get; set; }

        public List<string> Contents { get; set; }
    }
}