using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.CommonUI.Models
{
    public class BarcodeRuleModel : BindableBase, IXMLParser
    {
        /// <summary>
        /// 检查长度
        /// </summary>
        private bool _checkLength = false;

        public bool CheckLength
        {
            get => _checkLength;
            set { SetProperty(ref _checkLength, value); }
        }

        /// <summary>
        /// 余量为0后，自动删除
        /// </summary>
        private bool _autoDelete = false;

        public bool AutoDelelte
        {
            get => _autoDelete;
            set { SetProperty(ref _autoDelete, value); }
        }

        /// <summary>
        /// 条码长度
        /// </summary>
        private int _length = 1;

        public int Length
        {
            get => _length;
            set { SetProperty(ref _length, value); }
        }

        /// <summary>
        /// 检查表达式
        /// </summary>
        private bool _checkExpression = false;

        public bool CheckExpression
        {
            get => _checkExpression;
            set { SetProperty(ref _checkExpression, value); }
        }

        /// <summary>
        /// 正则表达式
        /// </summary>
        private string _expression = string.Empty;

        public string Expression
        {
            get => _expression;
            set { SetProperty(ref _expression, value); }
        }

        public XElement ExportXml()
        {
            return this.ToXml("");
        }
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }

        public static BarcodeRuleModel InitBarcodeConfig()
        {
            return new BarcodeRuleModel()
            {
                CheckLength = false,
                Length = 1,
                CheckExpression = false,
                Expression = string.Empty,
                AutoDelelte = false,
            };
        }
    }
}
