#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CacheDataItem
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       CacheDataItem.cs
* 创建时间:       2022/11/22 17:19:34
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      ecba967b-6241-46bc-8f52-de69d9995886
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/22 17:19:34
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Models
{
    public class CacheDataItem : BindableBase
    {
        /// <summary>
        /// 缓存数据唯一标识
        /// </summary>
        private string _dataId;
        public string DataId
        {
            get => _dataId;
            set => SetProperty(ref _dataId, value);
        }

        /// <summary>
        /// 创建事件
        /// </summary>
        private DateTime _createTime;
        public DateTime CreateTime
        {
            get => _createTime;
            set => SetProperty(ref _createTime, value);
        }

        private bool _isRemoved;
        public bool IsRemoved
        {
            get => _isRemoved;
            set => SetProperty(ref _isRemoved, value);
        }

        /// <summary>
        /// 数据字典
        /// </summary>
        private Dictionary<string, object> _data = new Dictionary<string, object>();

        public Dictionary<string, object> Data 
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }        
    }
}
