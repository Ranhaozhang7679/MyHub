#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PCylinderModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       PCylinderModel.cs
* 创建时间:       2022/8/10 14:20:19
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4f66097e-9a10-41b8-a538-9eb39bf42a42
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/10 14:20:19
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.VDevice;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class PointPosModel : BindableBase
    {
        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                bool src = _isSelected;
                SetProperty(ref _isSelected, value);
            }
        }

        private int _pointNo;
        public int PointNo
        {
            get => _pointNo; set
            {
                SetProperty(ref _pointNo, value);
                if (Tag != null)
                {
                    Tag.PointNo = value;
                }
            }
        }

        private PClinderCmdType _cmdType;
        public PClinderCmdType CmdType
        {
            get => _cmdType; set
            {
                SetProperty(ref _cmdType, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.CmdType = value;
                }

                IsMove = (value == PClinderCmdType.Abs || value == PClinderCmdType.Rel);
            }
        }

        private double _Acc;
        public double Acc
        {
            get => _Acc; set
            {
                SetProperty(ref _Acc, value);
                if (Tag != null)
                {
                    Tag.Acc = value;
                }
            }
        }

        private double _Dec;
        public double Dec
        {
            get => _Dec; set
            {
                SetProperty(ref _Dec, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.Dec = value;
                }
            }
        }

        /// <summary>
        /// 速度
        /// </summary>
        private double _Speed;
        public double Speed
        {
            get => _Speed; set
            {
                SetProperty(ref _Speed, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.Speed = value;
                }
            }
        }


        private double _pos;
        public double Pos
        {
            get => _pos; set
            {
                SetProperty(ref _pos, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.Pos = value;
                }
            }
        }

        /// <summary>
        /// 定位区间
        /// </summary>
        private double _range;
        public double Range
        {
            get => _range; set
            {
                SetProperty(ref _range, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.Range = value;
                }
            }
        }

        /// <summary>
        /// 回零高速
        /// </summary>
        private double _pressure;
        public double Pressure
        {
            get => _pressure; set
            {
                SetProperty(ref _pressure, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.Pressure = value;
                }
            }
        }

        /// <summary>
        /// 推压距离
        /// </summary>
        private double _preDistance;
        public double PreDistance
        {
            get => _preDistance; set
            {
                SetProperty(ref _preDistance, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.PreDistance = value;
                }
            }
        }

        /// <summary>
        /// 回零高速
        /// </summary>
        private double _delay;
        public double Delay
        {
            get => _delay; set
            {
                SetProperty(ref _delay, value); if (Tag != null && _isTwoWay)
                {
                    Tag.Delay = value;
                }
            }
        }

        /// <summary>
        /// 下一步指令
        /// </summary>
        private int _next;
        public int Next
        {
            get => _next; set
            {
                SetProperty(ref _next, value); if (Tag != null && _isTwoWay)
                {
                    Tag.Next = value;
                }
            }
        }


        /// <summary>
        /// 下一步指令
        /// </summary>
        private bool _isMove;
        public bool IsMove
        {
            get => _isMove; set
            {
                SetProperty(ref _isMove, value);
            }
        }

        private bool _isTwoWay = false;

        public PointPos Tag { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vAxis"></param>
        public PointPosModel(PointPos pp, bool isTwoWay = true)
        {
            _isTwoWay = isTwoWay;
            Update(pp);
        }

        public void Update(PointPos pp)
        {
            Tag = pp;
            IsSelected = false;
            PointNo = pp.PointNo;
            CmdType = pp.CmdType;
            Acc = pp.Acc;
            Dec = pp.Dec;
            Speed = pp.Speed;
            Pos = pp.Pos;
            Pressure = pp.Pressure;
            Range = pp.Range;
            PreDistance = pp.PreDistance;
            Delay = pp.Delay;
            Next = pp.Next;
        }
    }
}