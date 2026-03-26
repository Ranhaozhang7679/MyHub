using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.Model
{
      public class ModuleModel:BindableBase
    {


        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                bool src = _isSelected;
                SetProperty(ref _isSelected, value);
                if(src!=value)
                    OnSelected(value);                
            }
        }


        public event Action<bool> SelectedChanged;
        public void OnSelected(bool isSelected)
        {
            SelectedChanged?.Invoke(isSelected);
        }


        /// <summary>
        /// 模块名称
        /// </summary>
        private string _moduleName;
        public string ModuleName
        {
            get => _moduleName; 
            set
            {
                string src = _moduleName;
                SetProperty(ref _moduleName, value);
            }
        }
    }
}
