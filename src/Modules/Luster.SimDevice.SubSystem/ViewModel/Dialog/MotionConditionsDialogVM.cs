#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MotionConditionsDialogVM
* 机器名称:       L05590
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       MotionConditionsDialogVM.cs
* 创建时间:       2023/07/17 16:57:30
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      8458c4e3-4ded-40c0-8cde-e5d011499b9c
* 登录用户:       鲁帆
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/07/17 16:57:30
* 修 改 人:		  L05590
************************************************************************************/
#endregion
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class MotionConditionsDialogVM : DialogVM
    {
        private ISimDeviceEngineUI _deviceengine;
         List<VIO> DIList;
         List<VIO> DOList;


        #region 属性

        private ObservableCollection<AxisModel> _axisList;
        public ObservableCollection<AxisModel> AxisList
        {
            get { return _axisList; }
            set { SetProperty(ref _axisList, value); }
        }

        private string _actionName;
        public string ActionName
        {
            get { return _actionName; }
            set { SetProperty(ref _actionName, value); }
        }

        /// <summary>
        /// 点位位置
        /// </summary>
        private double _axisMin1;
        public double AxisMin1
        {
            get { return _axisMin1; }
            set { SetProperty(ref _axisMin1, value); }
        }

        private double _axisMax1;
        public double AxisMax1
        {
            get { return _axisMax1; }
            set { SetProperty(ref _axisMax1, value); }
        }

        private double _axisMin2;
        public double AxisMin2
        {
            get { return _axisMin2; }
            set { SetProperty(ref _axisMin2, value); }
        }

        private double _axisMax2;
        public double AxisMax2
        {
            get { return _axisMax2; }
            set { SetProperty(ref _axisMax2, value); }
        }


        private double _axisMin3;
        public double AxisMin3
        {
            get { return _axisMin3; }
            set { SetProperty(ref _axisMin3, value); }
        }

        private double _axisMax3;
        public double AxisMax3
        {
            get { return _axisMax3; }
            set { SetProperty(ref _axisMax3, value); }
        }

        private AxisModel _selectAxis1;
        public AxisModel SelectAxis1
        {
            get { return _selectAxis1; }
            set { SetProperty(ref _selectAxis1, value); }
        }

        private AxisModel _selectAxis2;
        public AxisModel SelectAxis2
        {
            get { return _selectAxis2; }
            set { SetProperty(ref _selectAxis2, value); }
        }

        private AxisModel _selectAxis3;
        public AxisModel SelectAxis3
        {
            get { return _selectAxis3; }
            set { SetProperty(ref _selectAxis3, value); }
        }

        /// <summary>
        /// IO列表
        /// </summary>
        private ObservableCollection<VIO> _ioList1;
        public ObservableCollection<VIO> IOList1
        {
            get { return _ioList1; }
            set { SetProperty(ref _ioList1, value); }
        }

        private ObservableCollection<VIO> _ioList2;
        public ObservableCollection<VIO> IOList2
        {
            get { return _ioList2; }
            set { SetProperty(ref _ioList2, value); }
        }

        private ObservableCollection<VIO> _ioList3;
        public ObservableCollection<VIO> IOList3
        {
            get { return _ioList3; }
            set { SetProperty(ref _ioList3, value); }
        }

        /// <summary>
        /// IO in or Out
        /// </summary>
        private bool _isIn1;
        public bool IsIn1
        {
            get { return _isIn1; }
            set {
                SetProperty(ref _isIn1, value);
                IOList1.Clear();
                if (value)
                {    
                    DIList.ForEach(x => IOList1.Add(x));
                }
                else
                {
                    DOList.ForEach(x => IOList1.Add(x));
                }
            }
        }
        /// <summary>
        /// IO in or Out
        /// </summary>
        private bool _isIn2;
        public bool IsIn2
        {
            get { return _isIn2; }
            set
            {
                SetProperty(ref _isIn2, value);
                IOList2.Clear();
                if (value)
                {
                    DIList.ForEach(x => IOList2.Add(x));
                }
                else
                {
                    DOList.ForEach(x => IOList2.Add(x));
                }
            }
        }

        private bool _isIn3;
        public bool IsIn3
        {
            get { return _isIn3; }
            set
            {
                SetProperty(ref _isIn3, value);
                IOList3.Clear();
                if (value)
                {
                    DIList.ForEach(x => IOList3.Add(x));
                }
                else
                {
                    DOList.ForEach(x => IOList3.Add(x));
                }
            }
        }

        private bool _isIOCheck1;
        public bool IsIOCheck1
        {
            get { return _isIOCheck1; }
            set { SetProperty(ref _isIOCheck1, value); }
        }
        private bool _isIOCheck2;
        public bool IsIOCheck2
        {
            get { return _isIOCheck2; }
            set { SetProperty(ref _isIOCheck2, value); }
        }

        private bool _isIOCheck3;
        public bool IsIOCheck3
        {
            get { return _isIOCheck3; }
            set { SetProperty(ref _isIOCheck3, value); }
        }

        /// <summary>
        /// 选中IO
        /// </summary>
        private VIO _selectIO1;
        public VIO SelectIO1 
        {
            get { return _selectIO1; }
            set { SetProperty(ref _selectIO1, value); }
        }
        private VIO _selectIO2;
        public VIO SelectIO2
        {
            get { return _selectIO2; }
            set { SetProperty(ref _selectIO2, value); }
        }
        private VIO _selectIO3;
        public VIO SelectIO3
        {
            get { return _selectIO3; }
            set { SetProperty(ref _selectIO3, value); }
        }
        #endregion

        protected MotionConditionsDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            _deviceengine= _engine;
            var ioList = deviceEngine.GetDevices(typeof(VIO)).Select(u => u as VIO).ToList();
            IOList1 = new ObservableCollection<VIO>();
            IOList2 = new ObservableCollection<VIO>();
            IOList3 = new ObservableCollection<VIO>();
            DIList = ioList.Where(u => u.Behavior == IOBehavior.Input).ToList();
            DOList = ioList.Where(u => u.Behavior == IOBehavior.Output).ToList();
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            AxisList = new ObservableCollection<AxisModel>();
            if (parameters.TryGetValue<List<AxisModel>>("AxisList", out var list))
            {
       
                list.ForEach(m=> AxisList.Add(m));
            }
            if (parameters.TryGetValue<string>("Action", out var name))
            {
                ActionName = name;
            }
            if (parameters.TryGetValue<AxisModel>("Axis", out var model))
            {
                if (model != null&&model.Tag!=null)
                {
                    List<VIOMoveCheckModel> listIOLimit=model.Tag.MoveIOLimit;
                    List<VAxisMoveDisModel> listAxisLimit=model.Tag.MoveDisLimit;
            
                    if (listAxisLimit != null)
                    {
                        var models = listAxisLimit.FirstOrDefault(x=>x.ActionName==ActionName);
                        if (models != null && models.ListDisLimit!=null)
                        {
                            for (int i = 0; i < models.ListDisLimit.Count; i++)
                            {
                                if (i == 0)
                                {
                                    var axis = AxisList.FirstOrDefault(m=>m.Name== models.ListDisLimit[i].Axis);
                                    if (axis != null)
                                    {
                                        SelectAxis1=axis;
                                    }
                                    AxisMin1 = models.ListDisLimit[i].MinPos;
                                    AxisMax1 = models.ListDisLimit[i].MaxPos;
                                }
                                if (i == 1)
                                {
                                    var axis = AxisList.FirstOrDefault(m => m.Name == models.ListDisLimit[i].Axis);
                                    if (axis != null)
                                    {
                                        SelectAxis2 = axis;
                                    }
                                    AxisMin2 = models.ListDisLimit[i].MinPos;
                                    AxisMax2 = models.ListDisLimit[i].MaxPos;
                                }
                                if (i == 2)
                                {
                                    var axis = AxisList.FirstOrDefault(m => m.Name == models.ListDisLimit[i].Axis);
                                    if (axis != null)
                                    {
                                        SelectAxis3 = axis;
                                    }
                                    AxisMin3 = models.ListDisLimit[i].MinPos;
                                    AxisMax3 = models.ListDisLimit[i].MaxPos;
                                }
                            }
                        }
                       
                    }

                    if (listIOLimit != null)
                    {
                        var models = listIOLimit.FirstOrDefault(x => x.ActionName == ActionName);
                        IsIn1 = true;
                        IsIn2 = true;
                        IsIn3 = true;
                        if (models != null&& models.ListIOCheck!=null)
                        {
                            for (int i = 0; i < models.ListIOCheck.Count; i++)
                            {
                                if (models.ListIOCheck[i].vIO != null)
                                {
                                    if (i == 0)
                                    {
                                        IsIOCheck1 = models.ListIOCheck[i].CheckValue;
                                        IsIn1 = models.ListIOCheck[i].vIO.Behavior == IOBehavior.Input ? true : false;
                                        var iomodel = IOList1.FirstOrDefault(m => m.Name == models.ListIOCheck[i].vIO.Name);
                                        if (iomodel != null)
                                        {
                                            SelectIO1 = iomodel;
                                        }
                                    }
                                    if (i == 1)
                                    {
                                        IsIOCheck2 = models.ListIOCheck[i].CheckValue;
                                        IsIn2 = models.ListIOCheck[i].vIO.Behavior == IOBehavior.Input ? true : false;
                                        var iomodel = IOList2.FirstOrDefault(m => m.Name == models.ListIOCheck[i].vIO.Name);
                                        if (iomodel != null)
                                        {
                                            SelectIO2 = iomodel;
                                        }
                                    }
                                    if (i == 2)
                                    {
                                        IsIOCheck3 = models.ListIOCheck[i].CheckValue;
                                        IsIn3 = models.ListIOCheck[i].vIO.Behavior == IOBehavior.Input ? true : false;
                                        var iomodel = IOList3.FirstOrDefault(m => m.Name == models.ListIOCheck[i].vIO.Name);
                                        if (iomodel != null)
                                        {
                                            SelectIO3 = iomodel;
                                        }
                                    }
                                }
                                else
                                {
                                    IsIn1 = true;
                                    IsIn2 = true;
                                    IsIn3 = true;
                                }

                            }
                        }
                    
                    }
                 
                }

            }

      


        }

        private DelegateCommand<string> _ioCommand;
        public DelegateCommand<string> IOCommand => _ioCommand ?? (_ioCommand = new DelegateCommand<string>((ioinfo) =>
        {
            if (ioinfo == null) return;
            if (ioinfo == "InIO1")
            {
                IsIn1 = true;
                IOList1.Clear();
                DIList.ForEach(x=>IOList1.Add(x));
            }
            if (ioinfo == "InIO2")
            {
                IsIn2 = true;
                IOList2.Clear();
                DIList.ForEach(x => IOList2.Add(x));
            }
            if (ioinfo == "InIO3")
            {
                IsIn3 = true;
                IOList3.Clear();
                DIList.ForEach(x => IOList3.Add(x));
            }
            if (ioinfo == "OutIO1")
            {
                IsIn1 = false;
                IOList1.Clear();
                DOList.ForEach(x => IOList1.Add(x));
            }
            if (ioinfo == "OutIO2")
            {
                IsIn2 = false;
                IOList2.Clear();
                DOList.ForEach(x => IOList2.Add(x));
            }
            if (ioinfo == "OutIO3")
            {
                IsIn3 = false;
                IOList3.Clear();
                DOList.ForEach(x => IOList3.Add(x));
            }

        }));

        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            List<MoveDisModel> listAxisdisLimit = new List<MoveDisModel>();
            if (SelectAxis1 != null)
            {
                var dismodel = new MoveDisModel()
                {
                    Axis= SelectAxis1.Name,
                    MinPos=AxisMin1,
                    MaxPos=AxisMax1
                };
                listAxisdisLimit.Add(dismodel);
            }
            if (SelectAxis2 != null)
            {
                var dismodel = new MoveDisModel()
                {
                    Axis = SelectAxis2.Name,
                    MinPos = AxisMin2,
                    MaxPos = AxisMax2
                };
                listAxisdisLimit.Add(dismodel);
            }
            if (SelectAxis3 != null)
            {
                var dismodel = new MoveDisModel()
                {
                    Axis = SelectAxis3.Name,
                    MinPos = AxisMin3,
                    MaxPos = AxisMax3
                };
                listAxisdisLimit.Add(dismodel);
            }
            var MoveDismodel = new VAxisMoveDisModel()
            {
                ActionName = ActionName,
                ListDisLimit= listAxisdisLimit
            };

            List<MoveCheckModel> listIOLimit = new List<MoveCheckModel>();
            if (SelectIO1 != null)
            {
                var iomodel = new MoveCheckModel()
                {
                    vIO = SelectIO1,
                    CheckValue=IsIOCheck1,
                };
                listIOLimit.Add(iomodel);
            }
            if (SelectIO2!= null)
            {
                var iomodel = new MoveCheckModel()
                {
                    vIO = SelectIO2,
                    CheckValue = IsIOCheck2,
                };
                listIOLimit.Add(iomodel);
            }
            if (SelectIO3!=null)
            {
                var iomodel = new MoveCheckModel()
                {
                    vIO = SelectIO3,
                    CheckValue = IsIOCheck3,
                };
                listIOLimit.Add(iomodel);
            }

            var MoveCheckmodel = new VIOMoveCheckModel()
            {
                ActionName = ActionName,
                ListIOCheck = listIOLimit
            };

            result.Parameters.Add(nameof(MoveDismodel), MoveDismodel);
            result.Parameters.Add(nameof(MoveCheckmodel), MoveCheckmodel);
        }

        private DelegateCommand<string> _clearCommand;
        public DelegateCommand<string> ClearCommand => _clearCommand ?? (_clearCommand = new DelegateCommand<string>((param) =>
        {
            if (param == null) return;
            if (param == "axis1")
            {
                SelectAxis1 = null;
                AxisMin1 = 0;
                AxisMax1 = 0;
            }
            if (param == "axis2")
            {
                SelectAxis2 = null;
                AxisMin2 = 0;
                AxisMax2 = 0;
            }
            if (param == "axis3")
            {
                SelectAxis3 = null;
                AxisMin3 = 0;
                AxisMax3 = 0;
            }
            if (param == "io1")
            {
                SelectIO1 = null;
                IsIn1 = true;
                IsIOCheck1 = false;
            }
            if (param == "io2")
            {
                SelectIO2 = null;
                IsIn2 = true;
                IsIOCheck2 = false;
            }
            if (param == "io3")
            {
                SelectIO3 = null;
                IsIn3 = true;
                IsIOCheck3 = false;
            }

        }));

    }
}
