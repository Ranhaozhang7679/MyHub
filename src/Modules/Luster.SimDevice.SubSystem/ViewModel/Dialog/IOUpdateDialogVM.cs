using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.SimDevice.EngineUI.Models;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class IOUpdateDialogVM : DialogVM
    {
        protected IOUpdateDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            IOTypes = typeof(IOType).EnumToDataSource();
            IOBehaviors = typeof(IOBehavior).EnumToDataSource();
        }

        /// <summary>
        /// IO类型集合
        /// </summary>
        public List<KeyValue> IOTypes { get; set; }

        /// <summary>
        /// IO行为集合
        /// </summary>
        public List<KeyValue> IOBehaviors { get; set; }

        private string _module;
        [Required]
        public string Module
        {
            get => _module; set
            {
                SetProperty(ref _module, value);
            }
        }

        private string _name;
        [Required]
        public string Name
        {
            get => _name; set
            {
                SetProperty(ref _name, value);
            }
        }

        private string _subDefinite;
        [Required]
        public string SubDefinite
        {
            get => _subDefinite; set
            {
                SetProperty(ref _subDefinite, value);
            }
        }


        private IOModel IOModel { get; set; }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.TryGetValue<IOModel>("IOModel", out var iOModel))
            {
                IOModel = iOModel;
                Module = iOModel.Module;
                Name = iOModel.Name;
                SubDefinite= iOModel.SubDefinite;
            }
        }

        protected override void Ok(IDialogResult result)
        {
            IOModel.Module = Module;
            IOModel.Name = Name;
            IOModel.SubDefinite = SubDefinite;
            result.Parameters.Add("IOModel", IOModel);
        }

    }
}
