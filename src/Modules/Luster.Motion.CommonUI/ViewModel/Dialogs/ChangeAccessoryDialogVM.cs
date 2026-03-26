#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ChangeAccessoryDialogVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.ViewModel.Dialogs
* 文 件 名:       ChangeAccessoryDialogVM.cs
* 创建时间:       2022/12/5 13:32:55
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      b28596f7-7ad7-48e7-9d59-36f6a959a5b8
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/5 13:32:55
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class ChangeAccessoryDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 辅料名称
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 批号
        /// </summary>
        private string _batchNo;
        [Required(ErrorMessage ="批次号不能为空")]
        public string BatchNo
        {
            get => _batchNo;
            set => SetProperty(ref _batchNo, value);
        }

        /// <summary>
        /// 数量
        /// </summary>
        private int _count;
        [Range(1, int.MaxValue)]
        [Required]
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        /// <summary>
        /// 工位名称
        /// </summary>
        private string _stationName;
        public string StationName
        {
            get => _stationName;
            set => SetProperty(ref _stationName, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("AccessoryName", out var name))
            {
                Name = name;
            }
            else
            {
                throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("MaterialNotObtained ")}！");
            }
        }

        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add("AccessoryName", Name);

            result.Parameters.Add("Count", Count);

            result.Parameters.Add("BatchNo", BatchNo);

            result.Parameters.Add("StationName", StationName);
        }
    }
}
