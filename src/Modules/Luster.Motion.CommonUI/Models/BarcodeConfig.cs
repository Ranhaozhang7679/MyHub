using Luster.Common.DataStruct.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualBasic.ApplicationServices;
using Luster.Motion.CommonUI.Models;
using System.IO;

namespace Luster.Motion.SubSystem.Models
{
    public class BarcodeConfig :IXMLParser
    {
        private string _barcodeSettingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "BarcodeSettingConfig.xml");
        public BarcodeRuleModel Barcodes;
        public XElement ExportXml()
        {
            return Barcodes.ExportXml();
        }

        public void ParserXml(XElement xElement)
        {
            var barcode = new BarcodeRuleModel();
            barcode.ParserXml(xElement);
            Barcodes = barcode;
        }

            /// <summary>
            /// 保存
            /// </summary>
        public void SaveBarcodeConfig()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_barcodeSettingPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_barcodeSettingPath));//创建路径
            }
            XElement xe = ExportXml();
            xe.Save(_barcodeSettingPath);
        }

        /// <summary>
        /// 加载
        /// </summary>
        public void LoadBarcodeConfig()
        {
            if (File.Exists(_barcodeSettingPath))
            {
                XElement barcodeConfig = XElement.Load(_barcodeSettingPath);
                ParserXml(barcodeConfig);
            }
            else
            {
                Barcodes = BarcodeRuleModel.InitBarcodeConfig();
                SaveBarcodeConfig();
            }
        }
    }
}
