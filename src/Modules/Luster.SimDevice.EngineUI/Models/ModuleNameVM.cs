using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class ModuleNameVM : BindableBase
    {
       /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name; set
            {
                SetProperty(ref _name, value);
                if (Tag != null)
                {
                    Tag.Name = value;
                }
            }
        }

        public ModuleNameModel Tag { get; set; }

        /// 构造函数
        /// </summary>
        /// <param name="vIO"></param>
        public ModuleNameVM(ModuleNameModel pModule)
        {
            Tag = pModule;
            Name = pModule.Name;
        }

    }
}
