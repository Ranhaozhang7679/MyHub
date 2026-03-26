using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class SetOutPutChartDialogVM : MotionDialogVM
    {
        private ICommonBus _commonBus;

        private IDbManager _dbManager;

        // 输出项列表
        private List<MapData> _mapDatas;




        public SetOutPutChartDialogVM(ICommonBus commonBus, IDbManager dbManager)
        {
            this._commonBus = commonBus;
            _dbManager = dbManager;
            ChartDataList = new ObservableCollection<ChartDataModel>();
            OutputItemModels = new ObservableCollection<OutputItemModel>();
            OutputSelectModels = new ObservableCollection<OutputItemModel>();
            _mapDatas = new List<MapData>(_commonBus.GetMapDatas());
            SourceNodes = _commonBus.GetOutDataTree();

        }

        /// <summary>
        /// 输出Item
        /// </summary>
        private ObservableCollection<OutputItemModel> _outputItemModels;
        public ObservableCollection<OutputItemModel> OutputItemModels
        {
            get => _outputItemModels;
            set => SetProperty(ref _outputItemModels, value);
        }

        private ObservableCollection<OutputItemModel> _outputSelectModels;
        public ObservableCollection<OutputItemModel> OutputSelectModels
        {
            get => _outputSelectModels;
            set => SetProperty(ref _outputSelectModels, value);
        }


        private ObservableCollection<ChartDataModel> _chartDataList;
        public ObservableCollection<ChartDataModel> ChartDataList
        {
            get { return _chartDataList; }
            set { SetProperty(ref _chartDataList, value); }
        }


        /// <summary>
        /// 标题
        /// </summary>
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        /// <summary>
        /// 当前Chart
        /// </summary>
        private string _currentChart;
        public string CurrentChart
        {
            get { return _currentChart; }
            set { SetProperty(ref _currentChart, value); }
        }


        /// <summary>
        /// 实时显示
        /// </summary>
        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }


        /// <summary>
        /// 输出来源
        /// </summary>
        public List<LNode> SourceNodes { get; set; }


        private void InitOutPutAV()
        {
            OutputItemModels = new ObservableCollection<OutputItemModel>();
            if (_mapDatas != null && _mapDatas.Count > 0)
            {
                foreach (var map in _mapDatas)
                {
                    var outModel = new OutputItemModel(map);
                    OutputItemModels.Add(outModel);
                }
            }
        }

        /// <summary>
        /// 添加Chart
        /// </summary>
        private DelegateCommand _addChartCommand;
        public DelegateCommand AddChartCommand => _addChartCommand ?? (_addChartCommand = new DelegateCommand(() =>
        {
            var model = new ChartDataModel()
            {
                Name = "Chart" + (ChartDataList.Count + 1).ToString(),
                IsSelected = true,
                Width = 250,
                Height = 250,
                UpperLimit = 1.2,
                LowerLimit = 0.8,
                SpaceFactor = 0.1,
                IsRealTime = false,
            };
            IsVisible = model.IsRealTime;
            ChartDataList.Add(model);
        }));

        /// <summary>
        /// 行切换事件
        /// </summary>
        private DelegateCommand<object> _selectChangeCommand;
        public DelegateCommand<object> SelectChangeCommand => _selectChangeCommand ?? (_selectChangeCommand = new DelegateCommand<object>((o) =>
        {
            if (o == null) { return; }
            var model = o as SelectionChangedEventArgs;
            if (model != null)
            {
                if (model.AddedItems.Count > 0 && model.RemovedItems.Count == 0)
                {
                    var currentChart = model.AddedItems[0] as ChartDataModel;
                    CurrentChart = currentChart.Name;
                    InitOutPutAV();
                    if (currentChart.SelectList != null && currentChart.SelectList.Count > 0)
                    {
                        OutputSelectModels.Clear();
                        currentChart.SelectList.ForEach(u => OutputSelectModels.Add(u));
                    }
                }
                else
                {
                    if (model.AddedItems.Count != 0)
                    {
                        var getmodel = model.RemovedItems[0] as ChartDataModel;
                        var selectmodel = ChartDataList.FirstOrDefault(u => u.Name == getmodel.Name);
                        if (OutputSelectModels != null)
                        {
                            selectmodel.SelectList = OutputSelectModels.ToList();
                        }

                        var newmodel = model.AddedItems[0] as ChartDataModel;
                        CurrentChart = newmodel.Name;
                        var checkmodel = ChartDataList.FirstOrDefault(u => u.Name == newmodel.Name);
                        OutputSelectModels.Clear();
                        if (checkmodel.SelectList != null)
                        {
                            checkmodel.SelectList.ForEach(x => OutputSelectModels.Add(x));
                        }
                    }
                }
            }

        }));

        /// <summary>
        /// 接收数据
        /// </summary>
        private DelegateCommand<object> _reciveDropCommand;
        public DelegateCommand<object> ReciveDropCommand => _reciveDropCommand ?? (_reciveDropCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj == null) { return; }
            var source = obj as System.Windows.DragEventArgs;
            if (source != null)
            {
                var node = source.Data.GetData(typeof(OutputItemModel)) as OutputItemModel;
                if (node != null)
                {
                    var mode = OutputSelectModels.FirstOrDefault(u => u.AliasName == node.AliasName);
                    if (mode != null)
                    {
                        throw new Common.DataStruct.FriendlyException(node.AliasName + $"{Luster.Motion.Assests.Langs.LangProvider.GetLang("AlreadyExists")}！");
                    }
                    else
                    {
                        OutputSelectModels.Add(node);
                    }
                }
            }
        }));

        private DelegateCommand<OutputItemModel> _removeCommand;
        public DelegateCommand<OutputItemModel> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<OutputItemModel>((o) =>
        {
            if (o == null) { return; }
            OutputSelectModels.Remove(o);
        }));

        private DelegateCommand<ChartDataModel> _deleteCommand;
        public DelegateCommand<ChartDataModel> DeleteCommand => _deleteCommand ?? (_deleteCommand = new DelegateCommand<ChartDataModel>((o) =>
        {
            if (o == null) { return; }
            ChartDataList.Remove(o);
        }));

        private DelegateCommand<object> _cellChangeCommand;
        public DelegateCommand<object> CellChangeCommand => _cellChangeCommand ?? (_cellChangeCommand = new DelegateCommand<object>((o) =>
        {
            if (o == null) { return; }
            var arg = o as DataGridCellEditEndingEventArgs;
            if (arg != null)
            {
                if (arg.EditAction == DataGridEditAction.Commit)
                {
                    var col = arg.Column as DataGridTextColumn;
                    if (col != null)
                    {
                        if (col.SortMemberPath == "Name")
                        {
                            var txtcontrol = arg.EditingElement as System.Windows.Controls.TextBox;
                            CurrentChart = txtcontrol.Text;
                        }
                    }
                    //var col1 = arg.Column as DataGridCheckBoxColumn;
                    //if (col1 != null)
                    //{
                    //    var ckbcontrol = arg.EditingElement as System.Windows.Controls.CheckBox;
                    //    IsVisible = ckbcontrol.IsChecked;
                    //    return;
                    //}
                }
            }

            var arg1 = o as ChartDataModel;
            if(arg1!=null)
            {
              IsVisible = arg1.IsRealTime;
            }
        }));

        private DelegateCommand<object> _cellCheckCommand;
        public DelegateCommand<object> CellCheckCommand => _cellCheckCommand ?? (_cellCheckCommand = new DelegateCommand<object>((o) =>
        {
            var chartDataModel= o as ChartDataModel;
            if (chartDataModel!=null)
            {
                IsVisible = chartDataModel.IsRealTime;
            }
        }));





        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            if (CurrentChart != null)
            {
                var selectmodel = ChartDataList.FirstOrDefault(u => u.Name == CurrentChart);
                if (OutputSelectModels != null)
                {
                    if (selectmodel != null)
                    {
                        selectmodel.SelectList = OutputSelectModels.ToList();
                    }

                }
            }
            List<ChartDataModel> listData = new List<ChartDataModel>();
            result.Parameters.Add("ChartDataList", ChartDataList.ToList());
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

            if (parameters.TryGetValue<List<ChartDataModel>>("ChartList", out var chartlist))
            {
                if (chartlist != null)
                {
                    chartlist.ForEach(x => ChartDataList.Add(x));
                }

            }

        }
    }

}
