#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PositionItem
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       PositionItem.cs
* 创建时间:       2022/12/15 16:12:16
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      c1348aed-3a59-4b82-9420-7cee16cfdccc
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/15 16:12:16
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Motion.DataStruct.DataModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class PositionItem : BindableBase
    {

        private string _name;
        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                var src = _name;
                SetProperty(ref _name, value);
                if (src != value && Tag != null)
                {
                    Tag.Name = value;
                }
            }
        }

        private double _position;
        /// <summary>
        /// 点位位置
        /// </summary>
        public double Position
        {
            get { return _position; }
            set
            {
                var src = _position;
                SetProperty(ref _position, value);
                if (src != value && Tag != null)
                {
                    Tag.Axis.Engine.UpdatePostion(Tag, value);
                }
            }
        }

        /// <summary>
        /// 轴点位对象
        /// </summary>
        public AxisPosition Tag { get; set; }
    }
}
