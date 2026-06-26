using MathNetExtend.Converter;
using RpcLibrary.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathNetExtend.Model.Position
{
    /// <summary>
    /// 通用位置类型
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(PositionCustomConverter))]
    public class PositionCustom : PositionBase
    {
        public PositionCustom()
        {
            this.Cells = new List<PositionCustomCell>();
        }

        public PositionCustom(PositionCustom other)
        {
            this.Cells = new List<PositionCustomCell>();
            this.CopyFrom(other);
        }

        public override void CopyFrom(PositionBase obj)
        {
            if (obj is PositionCustom)
            {
                base.CopyFrom(obj);
                PositionCustom other = obj as PositionCustom;
                this.Cells.Clear();
                foreach (PositionCustomCell item in other.Cells)
                {
                    this.Cells.Add(new PositionCustomCell(item));
                }
            }
        }

        [System.ComponentModel.DisplayName("坐标类型")]
        public List<PositionCustomCell> Cells { get; set; }

        /// <summary>
        /// 获取当前位置类型
        /// </summary>
        /// <returns></returns>
        protected override PositionCodeType GetCurrentPosiCode()
        {
            PositionCodeType total = base.GetCurrentPosiCode();
            for (int i = 0; i < Cells.Count; i++) total = total | this.Cells[i].CoType;
            return total;
        }
        /// <summary>
        /// 根据坐标轴类型获取位置
        /// </summary>
        /// <param name="coType">坐标轴</param>
        /// <param name="posi">位置</param>
        /// <returns></returns>
        public override bool GetPosition(PositionCodeType coType, out double posi)
        {
            if (base.GetPosition(coType, out posi)) return true;
            if (!CheckCodeInLimit(coType)) return false;
            for (int i = 0; i < Cells.Count; i++)
                if (Cells[i].CoType == coType)
                {
                    posi = Cells[i].Value;
                    return true;
                }
            return false;
        }

        /// <summary>
        /// 根据坐标轴类型设置位置
        /// </summary>
        /// <param name="coType">坐标轴</param>
        /// <param name="posi">位置</param>
        /// <returns></returns>
        public override bool SetPosition(PositionCodeType coType, double posi)
        {
            if (base.SetPosition(coType, posi)) return true;
            if (!CheckCodeInLimit(coType)) return false;
            for (int i = 0; i < Cells.Count; i++)
                if (Cells[i].CoType == coType)
                {
                    Cells[i].Value = posi;
                    return true;
                }
            return false;
        }
        /// <summary>
        /// 根据坐标轴类型获取位置列表
        /// </summary>
        /// <param name="coType">坐标轴类型，多个坐标轴按位或</param>
        /// <param name="posiLis">位置列表</param>
        /// <returns></returns>
        public override bool GetPosition(PositionCodeType coType, out List<double> posiLis)
        {
            if (!base.GetPosition(coType, out posiLis)) return false;
            foreach (PositionCodeType item in Enum.GetValues(typeof(PositionCodeType)))
            {
                if ((coType & item) > 0)
                {
                    for (int i = 0; i < this.Cells.Count; i++)
                    {
                        if ((this.Cells[i].CoType & item) > 0)
                        {
                            posiLis.Add(this.Cells[i].Value);
                            break;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 获取一系列坐标轴的位置,排列顺序为默认顺序
        /// </summary>
        /// <param name="posiLis">坐标轴位置列表</param>
        /// <returns></returns>
        public override bool GetPosition(out List<double> posiLis)
        {
            if (!base.GetPosition(out posiLis)) return false;
            for (int i = 0; i < this.Cells.Count; i++)
                posiLis.Add(this.Cells[i].Value);
            return true;
        }
        public override string ToString()
        {
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < Cells.Count; i++)
            {
                strb.Append(Cells[i].ToString()).Append(",");
            }
            return strb.ToString().TrimEnd(',');
        }
        public override string ToString(string fmt)
        {
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < Cells.Count; i++)
            {
                strb.Append(Cells[i].ToString(fmt)).Append(",");
            }
            return strb.ToString().TrimEnd(',');
        }

        public override PositionBase Clone()
        {
            return new PositionCustom(this);
        }
        /// <summary>
        /// 从字符串中解析
        /// </summary>
        /// <param name="str">位置字符串</param>
        /// <returns>通用位置</returns>
        public static PositionCustom ParseString(string str)
        {
            string[] details = str.Split(new string[] { "," }, StringSplitOptions.None);
            PositionCustom posi = new PositionCustom();
            for (int i = 0; i < details.Length; i++)
            {
                posi.Cells.Add(PositionCustomCell.Parse(details[i]));
            }
            return posi;
        }

        /// <summary>
        /// 通用位置单元类
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class PositionCustomCell : FieldToPropertyTypeDescriptor
        {
            public PositionCustomCell() : this(PositionCodeType.Unknown, 0)
            {
            }
            public PositionCustomCell(PositionCodeType celType, double value)
            {
                this.CoType = celType;
                this.Value = value;
            }

            public PositionCustomCell(PositionCustomCell other)
            {
                this.CopyFrom(other);
            }

            public void CopyFrom(PositionCustomCell other)
            {
                this.CoType = other.CoType;
                this.Value = other.Value;
            }
            [System.ComponentModel.DisplayName("坐标类型")]
            [RpcLibrary.Attr.Permission(RpcLibrary.Users.UserRoleTypeEnum.Administrator)]
            public PositionCodeType CoType { get; set; }

            [System.ComponentModel.DisplayName("值")]
            public double Value { get; set; }
            public override string ToString()
            {
                return string.Format("{0}:{1}", CoType, Value);
            }
            public virtual string ToString(string fmt)
            {
                return string.Format("{0}:{1}", CoType, Value.ToString(fmt));
            }
            /// <summary>
            /// 从位置单元字符串中解析得到位置单元
            /// </summary>
            /// <param name="str">位置单元字符串</param>
            /// <returns>位置单元</returns>
            public static PositionCustomCell Parse(string str)
            {
                string[] details = str.Split(new string[] { ":" }, StringSplitOptions.None);
                if (details.Length != 2) throw new FormatException();
                return new PositionCustomCell()
                {
                    CoType = (PositionCodeType)Enum.Parse(typeof(PositionCodeType), details[0]),
                    Value = Convert.ToDouble(details[1]),
                };
            }
        }
    }
}