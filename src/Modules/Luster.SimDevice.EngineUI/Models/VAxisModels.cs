#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VAxisModels
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       VAxisModels.cs
* 创建时间:       2022/6/14 14:16:59
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      dc3086ed-6eff-4c86-a280-aa66373aca08
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/14 14:16:59
* 修 改 人:		  L05123
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
    /// <summary>
    /// 多轴模块
    /// </summary>
    public class VAxisModels : BindableBase
    {
        /// <summary>
        /// 模组名称
        /// </summary>
        private string _module;
        public string Module
        {
            get => _module; set
            {
                SetProperty(ref _module, value);
                if (Tag != null)
                {
                    Tag.Module = value;
                }
            }
        }


        /// <summary>
        /// 模组名称
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name; set
            {
                SetProperty(ref _name, value); 
                if (Tag != null)
                {
                    Tag.Name = value;
                }
            }
        }

        /// <summary>
        /// 轴数量
        /// </summary>
        private int _number;
        public int Number { get => _number; set => SetProperty(ref _number, value); }


        /// <summary>
        /// 轴模组名称
        /// </summary>
        private string _alias;
        public string Alias { get => _alias; set => SetProperty(ref _alias, value); }

        public VAxisM Tag { get; set; }

        public VAxisModels(VAxisM v)
        {
            Tag = v;
            Module = v.Module;
            Name = v.Name;
            Number = v.Axises.Count();

            if (v.Axises.Any(u => u.Axis == null))
            {
                return;
            }

            Alias = String.Join(",", v.Axises.Select(u => u.Axis.Name).ToArray());
        }
    }
}