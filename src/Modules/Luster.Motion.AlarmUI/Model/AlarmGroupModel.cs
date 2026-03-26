#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogItemVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.AlarmUI.ViewModel
* 文 件 名:       LogItemVM.cs
* 创建时间:       2022/7/15 10:47:31
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      e59c39a0-65a8-4403-ba08-75c9028c6584
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/15 10:47:31
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.Motion.AlarmUI.Model
{
    public class AlarmGroupModel: BindableBase
    {
        /// <summary>
        /// 颜色标识
        /// </summary>
        private Brush _color;
        public Brush Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }

        /// <summary>
        /// 报警类型
        /// </summary>
        private string _alarmType;
        public string AlarmType
        {
            get { return _alarmType; }
            set { SetProperty(ref _alarmType, value); }
        }

        /// <summary>
        /// 时间
        /// </summary>
        private double _time;
        public double Time
        {
            get { return _time; }
            set { SetProperty(ref _time, value); }
        }

        /// <summary>
        /// 次数
        /// </summary>
        private int _count;
        public int Count
        {
            get { return _count; }
            set { SetProperty(ref _count, value); }
        }

        /// <summary>
        /// 时间占比
        /// </summary>
        private double _percent;
        public double Percent
        {
            get { return _percent; }
            set { SetProperty(ref _percent, value); }
        }
    }
}
