using Prism.Mvvm;
using System.Collections.Generic;

namespace Luster.SimDevice.EngineUI.Models
{
    public class PlcIOTabData : BindableBase
    {
        private string _tabName;
        public string TabName
        {
            get => _tabName;
            set => SetProperty(ref _tabName, value);
        }

        public List<PlcIOModel> Addresses { get; } = new List<PlcIOModel>();
    }
}
