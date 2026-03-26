using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Models;
using Luster.TaskFlow.Common.Attributes;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Models;

namespace Luster.Motion.EditorUI.ViewModel.Dialogs
{
    public class StationDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 数组
        /// </summary>
        private ObservableCollection<ModuleCT> _stations;
        public ObservableCollection<ModuleCT> Stations
        {
            get => _stations;
            set => SetProperty(ref _stations, value);
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        private DataType _dataType;
        public DataType DataType
        {
            get { return _dataType; }
            set { SetProperty(ref _dataType, value); }
        }

        /// <summary>
        /// 参数特征
        /// </summary>
        private ParameterAttribute pAttr;

        /// <summary>
        /// 数据类型
        /// </summary>
        private DataType dType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <exception cref="FriendlyException"></exception>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.TryGetValue<List<IMotionModule>>("Stations", out var stations))
            {
                Stations = new ObservableCollection<ModuleCT>();
                foreach (var item in stations)
                {
                    Stations.Add(new ModuleCT(item));
                }
            }
        }

        /// <summary>
        /// 添加
        /// </summary>
        private DelegateCommand<object> _selectCommand;
        public DelegateCommand<object> SelectCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj != null && obj is ModuleCT m)
            {
                LStation lStation = new LStation()
                {
                    Alias = m.Name,
                    ParamKey = m.Name,
                    ID = m.Tag.ID,
                    Tag = m.Tag
                };

                IDialogResult r = new DialogResult(ButtonResult.OK);
                r.Parameters.Add("Station", lStation);
                RaiseRequestClose(r);
            }
        }));
    }
}
