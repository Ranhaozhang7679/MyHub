using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class ModuleNameDialogVM : DialogVM
    {
        /// <summary>
        /// 报警代码
        /// </summary>
        private string _name;
        [Required]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        protected ModuleNameDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            
        }

        protected override void Ok(IDialogResult result)
        {
            result.Parameters.Add("Name", Name);
        }
    }
}
