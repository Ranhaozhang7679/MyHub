#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HomeItemModel
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       HomeItemModel.cs
* 创建时间:       2022/7/29 9:11:57
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      f911ec2f-2b7d-420f-984d-c54456473dda
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/29 9:11:57
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Motion.DataStruct.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class HomeItemModel : BindableBase
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 是否需要归零
        /// </summary>
        private bool _needHome;
        public bool NeedHome
        {
            get => _needHome;
            set
            {
                if (_needHome != value)
                {
                    SetProperty(ref _needHome, value);
                    Tag.NeedHome = value;
                }
            }
        }

        private int _index;

        /// <summary>
        /// 归零顺序
        /// </summary>
        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        /// <summary>
        /// 附加对象
        /// </summary>
        public IHome Tag { get; set; }

        public HomeItemModel(IHome home)
        {
            Tag = home;
            Name = home.Name;
            NeedHome = home.NeedHome;
          
        }
    }
}
