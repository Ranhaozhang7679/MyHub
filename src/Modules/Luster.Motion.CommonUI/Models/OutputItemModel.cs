#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       OutputItemModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       OutputItemModel.cs
* 创建时间:       2022/7/20 17:44:15
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      18aebd78-e15e-4464-a57d-fcbf4fa3ea5f
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/20 17:44:15
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Attributes;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.CommonUI.Models
{
    public class OutputItemModel : BindableBase, IXMLParser
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 映射名称
        /// </summary>
        private string _aliasName;
        public string AliasName
        {
            get => _aliasName;
            set
            {
                SetProperty(ref _aliasName, value);
                if (Tag != null)
                {
                    Tag.Alias = value;
                    if (Tag.Parameter != null)
                    {
                        Tag.Parameter.Alias = value;
                    }
                }
            }
        }

        /// <summary>
        /// 来源
        /// </summary>
        private string _source;
        public string Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        /// <summary>
        /// 来源类型
        /// </summary>
        private Type _sourceType;
        public Type SourceType
        {
            get => _sourceType;
            set => SetProperty(ref _sourceType, value);
        }


        /// <summary>
        /// 标准值
        /// </summary>
        private double _standardValue;
        public double StandardValue
        {
            get => _standardValue;
            set => SetProperty(ref _standardValue, value);
        }

        /// <summary>
        /// 负公差
        /// </summary>
        private double _negativestandardDeviation;
        public double NegativeStandardDeviation
        {
            get => _negativestandardDeviation;
            set => SetProperty(ref _negativestandardDeviation, value);
        }

        /// <summary>
        /// 正公差
        /// </summary>
        private double _positivestandardDeviation;
        public double PositiveStandardDeviation
        {
            get => _positivestandardDeviation;
            set => SetProperty(ref _positivestandardDeviation, value);
        }

        /// <summary>
        /// 补偿值
        /// </summary>
        private double _compensationValue;
        public double CompensationValue
        {
            get => _compensationValue;
            set => SetProperty(ref _compensationValue, value);
        }

        /// <summary>
        /// NgReason
        /// </summary>
        private string _ngReason;
        public string NgReason
        {
            get => _ngReason;
            set => SetProperty(ref _ngReason, value);
        }

        //private bool _isChecked;
        //public bool IsChecked
        //{
        //    get { return _isChecked; }
        //    set
        //    {
        //        SetProperty(ref _isChecked, value);

        //        if (Tag != null)
        //        {
        //            Tag.IsMonitor = value;
        //        }
        //    }
        //}

        public event Action<OutputItemModel> CheckedChange;

        /// <summary>
        /// 参数更新
        /// </summary>
        public MapData Tag { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mapData"></param>
        public OutputItemModel(MapData mapData)
        {
            Tag = mapData;
            AliasName = mapData.Alias;
            Id = mapData.ID;
            Source = mapData.DataSource;
            SourceType = mapData.DataType;
            StandardValue = mapData.StandardValue;
            NegativeStandardDeviation = mapData.NegativestandardDeviation;
            PositiveStandardDeviation=mapData.PositivestandardDeviation;
            CompensationValue = mapData.CompensationValue;
            NgReason=mapData.NgReason;
            //IsChecked = mapData.IsMonitor;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            //this.FromXml(xElement);
            xElement.GetElement("Id", id => Id = Guid.Parse(id));
            xElement.GetElement("AliasName", aliasName => AliasName = aliasName);
            xElement.GetElement("Source", d => Source = d);
            xElement.GetElement("StandardValue", standvalue => StandardValue = Convert.ToDouble(standvalue));
            xElement.GetElement("PositiveStandardDeviation", maxlimit => NegativeStandardDeviation = Convert.ToDouble(maxlimit));
            xElement.GetElement("PositiveStandardDeviation", minlimit => PositiveStandardDeviation = Convert.ToDouble(minlimit));
            xElement.GetElement("CompensationValue", compensationValue => CompensationValue = Convert.ToDouble(compensationValue));
            xElement.GetElement("NgReason", ngreason => NgReason = ngreason);
            //xElement.GetElement("IsChecked", d => IsChecked = bool.Parse(d));
            xElement.GetElement("SourceType", sourceType =>
            {
                var type = ParameterAttribute.GetTypeByDataType(sourceType);
                if (type != null)
                {
                    SourceType = type;
                }
            });
            //xElement.GetElement("Tag", tag =>
            //{
            //    var element = xElement.Elements("Tag").ToList();
            //    var mapdata =new MapData() ;
            //    mapdata.ParserXml(element[0]);
            //     Tag = mapdata;
            //});

        }



        public XElement ExportXml()
        {
            return this.ToXml();
        }

    }
}
