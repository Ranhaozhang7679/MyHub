#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VAxisMDialog
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       VAxisMDialog.cs
* 创建时间:       2022/6/14 13:14:41
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      217bc6d5-6def-4fd0-aacf-d223186088b8
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/14 13:14:41
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using HandyControl.Controls;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class VAxisMDialogVM : DialogVM
    {
        protected VAxisMDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            // 1.加载所有的虚拟虚拟轴
            VList = deviceEngine.GetDevices(typeof(VAxis));

            AxisNums = new List<KeyValue>()
            {
                new KeyValue(){Value=2, Desc="两轴"},
                new KeyValue(){Value=3, Desc="三轴"},
                new KeyValue(){Value=4, Desc="四轴"},
            };
        }

        #region 通用参数
        /// <summary>
        /// 模块
        /// </summary>
        private string _module;
        [Required]
        public string Module
        {
            get { return _module; }
            set { SetProperty(ref _module, value); }
        }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        [Required]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 轴数量
        /// </summary>
        private int _axisNumber = 2;
        public int AxisNumber
        {
            get => _axisNumber; set
            {
                SetProperty(ref _axisNumber, value);
                if (value == 4)
                {
                    IsThree = true;
                    IsFour = true;

                }
                else if (value == 3)
                {
                    IsThree = true;
                    IsFour = false;
                    if (Axis4 != null)
                    {
                        Axis4 = null;
                    }
                }
                else
                {
                    IsThree = false;
                    IsFour = false;
                    if (Axis4 != null)
                    {
                        Axis4 = null;
                    }
                    if (Axis3 != null)
                    {
                        Axis3 = null;
                    }
                }
            }
        }

        /// <summary>
        /// 第一个轴
        /// </summary>
        private VAxis _axis1;
        public VAxis Axis1
        {
            get { return _axis1; }
            set { SetProperty(ref _axis1, value); }
        }

        /// <summary>
        /// 第二个轴
        /// </summary>
        private VAxis _axis2;
        public VAxis Axis2
        {
            get { return _axis2; }
            set { SetProperty(ref _axis2, value); }
        }

        /// <summary>
        /// 第三个轴
        /// </summary>
        private VAxis _axis3;
        public VAxis Axis3
        {
            get { return _axis3; }
            set { SetProperty(ref _axis3, value); }
        }

        /// <summary>
        /// 第四个轴
        /// </summary>
        private VAxis _axis4;
        public VAxis Axis4
        {
            get { return _axis4; }
            set { SetProperty(ref _axis4, value); }
        }
        #endregion

        /// <summary>
        /// 三轴
        /// </summary>
        private bool _isThree;
        public bool IsThree
        {
            get { return _isThree; }
            set { SetProperty(ref _isThree, value); }
        }

        /// <summary>
        /// 四周
        /// </summary>
        private bool _isFour;
        public bool IsFour
        {
            get { return _isFour; }
            set { SetProperty(ref _isFour, value); }
        }
        /// <summary>
        /// 轴列表
        /// </summary>
        private List<IVirtualDevice> _vList;
        public List<IVirtualDevice> VList
        {
            get { return _vList; }
            set { SetProperty(ref _vList, value); }
        }


        /// <summary>
        /// 轴列表
        /// </summary>
        private List<KeyValue> _axisNums;
        public List<KeyValue> AxisNums
        {
            get { return _axisNums; }
            set { SetProperty(ref _axisNums, value); }
        }

        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            if (CheckRepeatAxis())
            {
                throw new DeviceException($"选中了相同的轴请重新选择!");
            }
            else
            {
                List<AxisItem> vList = new List<AxisItem>();
                if (Axis1 != null)
                {
                    vList.Add(new AxisItem()
                    {
                        Axis = Axis1,
                        AxisID = Axis1.ID,
                        Speed = Axis1.MoveSpeed
                    });
                }

                if (Axis2 != null)
                {
                    vList.Add(new AxisItem()
                    {
                        Axis = Axis2,
                        AxisID = Axis2.ID,
                        Speed = Axis2.MoveSpeed
                    });
                }

                if (Axis3 != null)
                {
                    vList.Add(new AxisItem()
                    {
                        Axis = Axis3,
                        AxisID = Axis3.ID,
                        Speed = Axis3.MoveSpeed
                    });
                }

                if (Axis4 != null)
                {
                    vList.Add(new AxisItem()
                    {
                        Axis = Axis4,
                        AxisID = Axis4.ID,
                        Speed = Axis4.MoveSpeed
                    });
                }

                VAxisM vAxisM = new VAxisM()
                {
                    Axises = vList,
                    ID = Guid.NewGuid(),
                    Name = Name,
                    Module = Module,
                };

                result.Parameters.Add("VAxisM", vAxisM);
            }
        }

        /// <summary>
        /// 检查是否有相同的轴
        /// </summary>
        /// <returns></returns>
        private bool CheckRepeatAxis()
        {
            var usedAxis = new List<VAxis>();
            if (Axis1 != null)
            {
                usedAxis.Add(Axis1);
            }
            if (Axis2 != null)
            {
                usedAxis.Add(Axis2);
            }
            if (Axis3 != null)
            {
                usedAxis.Add(Axis3);
            }
            if (Axis4 != null)
            {
                usedAxis.Add(Axis4);
            }

            if (usedAxis.Count == 2)
            {
                return Axis1.ID == Axis2.ID;
            }

            if (usedAxis.Count == 3)
            {
                return Axis1.ID == Axis2.ID || Axis1.ID == Axis3.ID || Axis2.ID == Axis3.ID;
            }

            if (usedAxis.Count == 4)
            {
                return Axis1.ID == Axis2.ID || Axis1.ID == Axis3.ID || Axis1.ID == Axis4.ID || Axis2.ID == Axis3.ID || Axis2.ID == Axis4.ID || Axis3.ID == Axis4.ID;
            }

            return false;
        }

    }
}