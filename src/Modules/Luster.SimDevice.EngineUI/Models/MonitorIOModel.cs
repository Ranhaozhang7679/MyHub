#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MonitorIOModel
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       MonitorIOModel.cs
* 创建时间:       2022/8/13 13:22:10
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      e42e16db-4baa-4c5f-bf50-a6d3dde6f4b9
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/13 13:22:10
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
    public class MonitorIOModel : BindableBase
    {
        /// <summary>
        /// 序号
        /// </summary>
        private int _index;
        public int Index 
        {
            get =>_index;
            set => SetProperty(ref _index, value);
        }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 监控值
        /// </summary>
        private bool _isTrue;
        public bool IsTrue
        {
            get => _isTrue;
            set
            {
                SetProperty(ref _isTrue, value);
                if (value)
                {
                    Tag.Montor = Motion.DataStruct.Enums.IOMonitor.True;
                }
                else
                {
                    Tag.Montor = Motion.DataStruct.Enums.IOMonitor.False;
                }
            }
        }

        /// <summary>
        /// 附加设备
        /// </summary>
        public VIO Tag { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="iO"></param>
        public MonitorIOModel(VIO iO)
        {
            Tag = iO;
            Index = iO.Index;
            Name = iO.Name;
            if (iO.Montor == Motion.DataStruct.Enums.IOMonitor.True)
            {
                IsTrue = true;
            }
            else 
            {
                IsTrue = false;
            }
        }
    }
}
