#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ReportType
* 机器名称:       X5854
* 命名空间:       Luster.Motion.ReportUI.Model
* 文 件 名:       ReportType.cs
* 创建时间:       2022/10/18 9:02:15
* 作    者:       X5854
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      cacb98ce-67d3-40f2-8ed6-9917dc5ee69c
* 登录用户:       夏翔
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2024/9/18 9:02:15
* 修 改 人:		  X5854
************************************************************************************/
#endregion
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.ReportUI.Model
{
    public class TaikeModel : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        private double _no;

        public double No
        {
            get => _no;
            set => SetProperty(ref _no, value);
        }
        private double _torque1;

        public double Torque1
        {
            get => _torque1;
            set => SetProperty(ref _torque1, value);
        }
        /// <summary> 
        /// 角度
        /// </summary> 
        private double _angle1;

        public double Angle1
        {
            get => _angle1;
            set => SetProperty(ref _angle1, value);
        }
    }
    public class TotalPressModel : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        private double _no;

        public double No
        {
            get => _no;
            set => SetProperty(ref _no, value);
        }

        private double _time;

        public double Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }


        /// <summary>
        /// 压力
        /// </summary>
        private double _press;

        public double Press
        {
            get => _press;
            set => SetProperty(ref _press, value);
        }

        /// <summary>
        /// 位置
        /// </summary>
        private double _position;

        public double Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }
    }


    public class TimeTorqueAngleModel : BindableBase
    {
        /// <summary>
        /// 序号/时间索引（来自CSV列 No）
        /// </summary>
        private double _no;
        public double No
        {
            get => _no;
            set => SetProperty(ref _no, value);
        }

        /// <summary>
        /// 扭矩（来自CSV列 Torque1）
        /// </summary>
        private double _torque1;
        public double Torque1
        {
            get => _torque1;
            set => SetProperty(ref _torque1, value);
        }

        /// <summary>
        /// 角度（来自CSV列 Angle1）
        /// </summary>
        private double _angle1;
        public double Angle1
        {
            get => _angle1;
            set => SetProperty(ref _angle1, value);
        }
    }


    public class CowlingForceModel:BindableBase
    {
        private double _time;

        public double Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }


        /// <summary> 
        /// 压力
        /// </summary> 
        private double _force;

        public double Force
        {
            get => _force;
            set => SetProperty(ref _force, value);
        }
    }
}
