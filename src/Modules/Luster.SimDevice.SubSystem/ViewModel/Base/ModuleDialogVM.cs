#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Base
* 文 件 名:       ModuleDialogVM.cs
* 创建时间:       2022/6/22 17:42:38
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a89bd9a9-71da-4b83-8f7c-82fdac32092f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/22 17:42:38
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class ModuleDialogVM : DialogVM
    {
        protected ModuleDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            DIList = new List<VIO>();
            DOList = new List<VIO>();

            ModuleChanged("");
        }

        /// <summary>
        /// 模块类型
        /// </summary>
        private string _module;
        [Required]
        public string Module
        {
            get { return _module; }
            set
            {
                SetProperty(ref _module, value);
                ModuleChanged(value);
            }
        }

        private List<VIO> _diList;
        private List<VIO> _doList;

        /// <summary>
        /// 输入结果
        /// </summary>
        public List<VIO> DIList { get => _diList; set => SetProperty(ref _diList, value); }

        /// <summary>
        /// 输出结果
        /// </summary>
        public List<VIO> DOList { get => _doList; set => SetProperty(ref _doList, value); }

        /// <summary>
        /// 气缸
        /// </summary>
        private List<VCylinder> _cylinders;
        public List<VCylinder> CylinderList { get => _cylinders; set => SetProperty(ref _cylinders, value); }

        /// <summary>
        /// 真空
        /// </summary>
        private List<VVacuum> _vacuumList;
        public List<VVacuum> VVacuumList { get => _vacuumList; set => SetProperty(ref _vacuumList, value); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        protected virtual void ModuleChanged(string module)
        {
            var ioList = deviceEngine.GetDevices(typeof(VIO)).Select(u => u as VIO).ToList();
            if (!string.IsNullOrEmpty(module))
            {
                ioList = ioList.Where(u => u.IOType == IOType.Digital && u.Module == module).ToList();
            }

            DIList = ioList.Where(u => u.Behavior == IOBehavior.Input).ToList();
            DOList = ioList.Where(u => u.Behavior == IOBehavior.Output).ToList();

            // 如果有用到气缸，重写该方法
        }
    }
}