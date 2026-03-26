#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ChartDataModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       ChartDataModel.cs
* 创建时间:       2022/10/25 13:39:20
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      43a5c26f-d6c3-4bfb-986c-b15794f24a8f
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/25 13:39:20
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.CommonUI.Models
{
    public class ChartDataModel:BindableBase, IXMLParser
    {

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
       
        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 宽度
        /// </summary>
        private double _width;
        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        /// <summary>
        /// 高度
        /// </summary>
        private double _height;
        public double Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

        /// <summary>
        /// 上限
        /// </summary>
        private double _upperLimit;
        public double UpperLimit
        {
            get { return _upperLimit; }
            set { SetProperty(ref _upperLimit, value); }
        }

        /// <summary>
        /// 下限
        /// </summary>
        private double _lowerLimit;
        public double LowerLimit
        {
            get { return _lowerLimit; }
            set { SetProperty(ref _lowerLimit, value); }
        }

        /// <summary>
        /// 间距
        /// </summary>
        private double _spaceFactor;
        public double SpaceFactor
        {
            get { return _spaceFactor; }
            set { SetProperty(ref _spaceFactor, value); }
        }

        /// <summary>
        /// 实时刷新
        /// </summary>
        private bool _isRealTime;
        public bool IsRealTime
        {
            get { return _isRealTime; }
            set { SetProperty(ref _isRealTime, value); }
        }



        /// <summary>
        /// 选择的数据
        /// </summary>
        private List<OutputItemModel> _selectList;
        [Ignore]
        public List<OutputItemModel> SelectList
        {
            get { return _selectList; }
            set { SetProperty(ref _selectList, value); }
        }

        public void SaveChartConfig(string chartConfig)
        {
            if (!Directory.Exists(Path.GetDirectoryName(chartConfig)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(chartConfig));//创建路径
            }

            XElement xe = ExportXml();
            xe.Save(chartConfig);
        }

        public XElement ExportXml()
        {
            var root=this.ToXml("ChartConfig");
            if (SelectList != null)
            {
                var clsassElement = new XElement("SelectList");
                // 添加用户Feature
                foreach (var model in SelectList)
                {
                    var element = model.ExportXml();
                    clsassElement.Add(element);
                }
                root.Add(clsassElement);
            }
            return root;
        }

        public void ParserXml(XElement xElement)
        {
            SelectList = new List<OutputItemModel>();
            var element = xElement.Elements("SelectList");
            if (element != null)
            {
                foreach (var xItems in element.Elements("OutputItemModel"))
                {
                    var tagItems = xItems.Elements("Tag");
                    if (tagItems != null)
                    {
                        MapData mapData = new MapData();
                        OutputItemModel output = new OutputItemModel(mapData);
                        output.ParserXml(xItems);
                        SelectList.Add(output);
                    }
                   
             
                }
            }
            element.Remove();
            this.FromXml(xElement);
        }
    }
}