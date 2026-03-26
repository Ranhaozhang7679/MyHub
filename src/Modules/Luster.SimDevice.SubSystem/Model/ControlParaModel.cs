using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace Luster.SimDevice.SubSystem.Model
{
     public class ControlParaModel:BindableBase, IXMLParser
    {
        /// <summary>
        /// 对应的基元ID
        /// </summary>
        public Guid PrimID { get; set; }

        //等级
        private GradeType _grade;
        public GradeType Grade
        {
            get { return _grade; }
            set { SetProperty(ref _grade, value); }
        }

        //类型
        private string _type;
        public string Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        //名称
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }


        //模组
        private string _module;
        public string Module
        {
            get { return _module; }
            set { SetProperty(ref _module, value); }
        }


        //基元
        private string _prim;
        public string Prim
        {
            get { return _prim; }
            set { SetProperty(ref _prim, value); }
        }

        //实际值
        private string  _actualVal;
        public string ActualVal
        {
            get { return _actualVal; }
            set {
                string src = _actualVal;
                SetProperty(ref _actualVal, value);
                if (src != value)
                    OnValueChanged(PrimID, _name, _actualVal);
                SetProperty(ref _actualVal, value); }
        }

        //上限值
        private string _upLimitVal;
        public string UpLimitVal
        {
            get { return _upLimitVal; }
            set { SetProperty(ref _upLimitVal, value); }
        }

        //上限值
        private string _lowLimitVal;
        public string LowLimitVal
        {
            get { return _lowLimitVal; }
            set { SetProperty(ref _lowLimitVal, value); }
        }

        //标准值
        private string _standardVal;
        public string StandardVal
        {
            get { return _standardVal; }
            set { SetProperty(ref _standardVal, value); }
        }



        /// <summary>
        /// 值发生变化
        /// </summary>
        public event Action<Guid, string,string> ValueChanged;
        public void OnValueChanged(Guid guid, string name, string value)
        {
            ValueChanged?.Invoke(guid,name,value);
        }






        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public XElement ExportXml()
        {
            XElement xPara = new XElement("ControlPara");
            xPara.SetAttributeValue("PrimID", PrimID);
            xPara.SetAttributeValue("Name", Name);  
            xPara.SetAttributeValue("Grade", Grade);
            return xPara;
        }
        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ParserXml(XElement xElement)
        {
            if(!xElement.Name.ToString().Contains("Extend"))
            {
                xElement.Parent.GetAttribute("ID", id => PrimID = Guid.Parse(id));
                Grade = GradeType.Low;
                UpLimitVal = "0";
                LowLimitVal = "0";
                StandardVal = "0";
                xElement.GetAttribute("Module", module => Module = module);
                xElement.GetAttribute("Prim", prim => Prim = prim);
                xElement.GetAttribute("Type", type => Type = type);
                xElement.GetAttribute("CN", cn => Name = cn);

                //Name = xElement.Name.ToString();
                xElement.GetAttribute("Value", value => ActualVal = value);
                xElement.GetAttribute("Max", upVal => UpLimitVal = upVal);
                xElement.GetAttribute("Min", lowVal => LowLimitVal =  lowVal);
                xElement.GetAttribute("Default", staVal => StandardVal =staVal);
            }
            else
            {
                 PrimID=Guid.Empty;
                Grade = GradeType.Low;
                UpLimitVal = "0";
                LowLimitVal = "0";
                StandardVal = "0";
                Module = "System";
                Prim = "全局";
                xElement.GetAttribute("Type", type => Type = type);
                xElement.GetAttribute("Alias", alias => Name = alias);
                xElement.GetAttribute("Value", value => ActualVal = value);
                xElement.GetAttribute("Max", upVal => UpLimitVal = upVal);
                xElement.GetAttribute("Min", lowVal => LowLimitVal = lowVal);
                xElement.GetAttribute("Default", staVal => StandardVal = staVal);
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
