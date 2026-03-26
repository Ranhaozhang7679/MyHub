#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       QualityItemModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       QualityItemModel.cs
* 创建时间:       2022/7/20 17:27:16
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      1ae16c64-024c-4815-9d92-b22d12380e25
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/20 17:27:16
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.Models
{
    public class QualityItemModel : BindableBase
    {
        public long Id { get; set; }

        /// <summary>
        /// 唯一标识
        /// </summary>
        private Guid _mapId;
        public Guid MapId
        {
            get => _mapId;
            set => SetProperty(ref _mapId, value);
        }

        /// <summary>
        /// 映射名称
        /// </summary>
        private string _aliasName;
        public string AliasName
        {
            get => _aliasName;
            set => SetProperty(ref _aliasName, value);
        }

        /// <summary>
        /// 标准值
        /// </summary>
        private double _standardValue;
        public double StandardValue
        {
            get => _standardValue;
            set => SetProperty(ref _standardValue, value);
        }

        /// <summary>
        /// 负公差
        /// </summary>
        private double _negativestandardDeviation;
        public double NegativeStandardDeviation
        {
            get => _negativestandardDeviation;
            set => SetProperty(ref _negativestandardDeviation, value);
        }

        /// <summary>
        /// 正公差
        /// </summary>
        private double _positivestandardDeviation;
        public double PositiveStandardDeviation
        {
            get => _positivestandardDeviation;
            set => SetProperty(ref _positivestandardDeviation, value);
        }

        /// <summary>
        /// 补偿值
        /// </summary>
        private double _compensationValue;
        public double CompensationValue 
        {
            get => _compensationValue;
            set => SetProperty(ref _compensationValue, value);
        }

    }
}
