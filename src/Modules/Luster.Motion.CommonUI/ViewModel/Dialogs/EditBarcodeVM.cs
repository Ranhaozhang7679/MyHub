using Luster.Common.DataStruct;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.SubSystem.Models;
using Microsoft.VisualBasic.ApplicationServices;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class EditBarcodeVM:MotionDialogVM
    {
        public static BarcodeRuleModel _rulemodel;

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

        public EditBarcodeVM()
        {
            BarcodeConfig _barcodeConfig= new BarcodeConfig();
            _barcodeConfig.LoadBarcodeConfig();
            _rulemodel = _barcodeConfig.Barcodes;
            CheckLength = _rulemodel.CheckLength;
            CheckExpression = _rulemodel.CheckExpression;
            Expression = _rulemodel.Expression;
            Length = _rulemodel.Length;
        }



        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            if (_rulemodel != null)
            {
                
            }   
            else
            {
                _rulemodel = new BarcodeRuleModel();
            }
            _rulemodel.CheckLength = CheckLength;
            _rulemodel.CheckExpression = CheckExpression;
            _rulemodel.Length = Length;
            _rulemodel.Expression = Expression;
            result.Parameters.Add("Barcode", _rulemodel);
        }

        protected override void CloseDialog(string parameter)
        {
            base.CloseDialog(parameter);
        }
    }
}
