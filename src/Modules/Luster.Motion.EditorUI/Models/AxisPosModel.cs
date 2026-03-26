#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AxisPosModel
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.EditorUI.Models
* 文 件 名:       AxisPosModel.cs
* 创建时间:       2022/12/15 14:56:51
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ac44aa25-b5b4-423d-b94d-534e38b82b8c
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/15 14:56:51
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.Models
{
    public class AxisPosItemModel : BindableBase
    {
        /// <summary>
        /// 属性变更
        /// </summary>
        public event Action<string, object, object> AxisPropertyChanged;

        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        private double _position;
        public double Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        /// <summary>
        /// 速度
        /// </summary>
        private double _speed;
        public double Speed
        {
            get { return _speed; }
            set
            {
                double srcVal = _speed;
                SetProperty(ref _speed, value);

                if (srcVal != value)
                {
                    Tag.Speed = value;
                    AxisPropertyChanged?.Invoke(Name, srcVal, value);
                }
            }
        }

        /// <summary>
        /// 速度
        /// </summary>
        private double _acc;
        public double Acc
        {
            get { return _acc; }
            set
            {
                double srcVal = _acc;
                SetProperty(ref _acc, value);

                if (srcVal != value)
                {
                    Tag.Acc = value;
                }
            }
        }

        /// <summary>
        /// 速度
        /// </summary>
        private double _dec;
        public double Dec
        {
            get { return _dec; }
            set
            {
                double srcVal = _dec;
                SetProperty(ref _dec, value);

                if (srcVal != value)
                {
                    Tag.Dec = value;
                }
            }
        }

        /// <summary>
        /// 优先级
        /// </summary>
        private Priority _priority;
        public Priority Priority
        {
            get { return _priority; }
            set
            {
                var srcVal = _priority;
                SetProperty(ref _priority, value);
                if (srcVal != value)
                {
                    Tag.MovePriority = value;
                }
            }
        }

        public AxisPosItem Tag { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="posItem"></param>
        public AxisPosItemModel(AxisPosItem posItem)
        {
            // 点位名称
            Name = $"[{posItem.Axis.Name}] {posItem.AxisPostion.Name}";
            Tag = posItem;
            Priority = Tag.MovePriority;
            Acc = Tag.Acc;
            Dec = Tag.Dec;
            Speed = Tag.Speed;
            Position = Tag.Position;
        }
    }
}