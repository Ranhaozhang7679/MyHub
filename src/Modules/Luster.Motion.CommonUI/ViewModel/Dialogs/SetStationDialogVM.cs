using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class SetStationDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 模块引擎
        /// </summary>
        private IMotionEngine _engine;

        public SetStationDialogVM(IMotionEngine motionEngine)
        {
            _engine = motionEngine;
            Stations = new ObservableCollection<IMotionModule>();
            BuildStation();
        }

        /// <summary>
        /// 工站集
        /// </summary>
        private ObservableCollection<IMotionModule> _stations;
        public ObservableCollection<IMotionModule> Stations
        {
            get { return _stations; }
            set { SetProperty(ref _stations, value); }
        }

        /// <summary>
        /// 获取工站
        /// </summary>
        private void BuildStation()
        {
            List<IMotionModule> listStation = _engine.GetStations();
            var list = listStation.OrderBy(t => t.Sort).ToList();
            Stations.Clear();
            list.ForEach(x => Stations.Add(x));
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add("Stations", Stations);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var tvalue))
            {
                Title = tvalue;
                
            }
            else
            {
                Title = string.Empty;
            }

        }

        /// <summary>
        /// 工站是否显示
        /// </summary>
        private DelegateCommand<IMotionModule> _checkCommand;
        public DelegateCommand<IMotionModule> CheckCommand => _checkCommand ?? (_checkCommand = new DelegateCommand<IMotionModule>((module) =>
        {
            if (module != null)
            {
                IStation station = module.TaskFunction as IStation;
                if (Stations != null)
                {
                    foreach (var item in Stations)
                    {
                        if (item.Alias == module.Alias)
                        {
                            if (station.Visible)
                            {
                                station.Visible = false;
                            }
                            else
                            {
                                station.Visible = true;
                            }
                        }
                    }
                }
            }
        }));
    }
}
