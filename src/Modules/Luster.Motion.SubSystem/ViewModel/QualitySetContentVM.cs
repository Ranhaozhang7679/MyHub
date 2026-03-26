#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       QualitySetContent
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       QualitySetContent.cs
* 创建时间:       2022/7/20 15:35:00
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      b984cab6-c894-450a-b163-3e088824fecc
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/20 15:35:00
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Luster.Common.Assets.Extension;
using System.Windows.Controls;
using Luster.Common.Assets;
using Luster.Common.DataAccess.Tables;
using Luster.Motion.CommonUI.Models;
using Luster.SimDevice.SubSystem.Events;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class QualitySetContentVM : BaseVM, IConfirmNavigationRequest
    {

        private bool _itemChanged = false;
        private ICommonBus _commonBus;
        private IDbManager _dbManager;
        /// <summary>
        /// 输出来源
        /// </summary>
        public List<LNode> SourceNodes { get; set; }

        // 输出项列表
        private List<MapData> _mapDatas;

        // 新增输出项列表
        private List<MapData> _newMapDatas;

        // 移除输出项列表
        private List<MapData> _removeDatas;

        /// <summary>
        /// 质量标准
        /// </summary>

        private ObservableCollection<QualityItemModel> _qualityItemModels;

        public ObservableCollection<QualityItemModel> QualityItemModels
        {
            get => _qualityItemModels;
            set => SetProperty(ref _qualityItemModels, value);
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

        /// <summary>
        /// 删除
        /// </summary>
        public DelegateCommand<OutputItemModel> DeleteCommand { get; set; }

        /// <summary>
        /// 接收鼠标拖拽
        /// </summary>
        public DelegateCommand<object> RecieveDrogCommand { get; set; }     

        public DelegateCommand CancelQualitySetCommand { get; set; }

        public DelegateCommand<object> SaveQualitySetCommand { get; set; }

        /// <summary>
        /// 弹窗
        /// </summary>

        private IDialogService _dialog;

        public QualitySetContentVM(ICommonBus commonBus, IDbManager dbManager, IDialogService dialogService)
        {
            _commonBus = commonBus;
            _dbManager = dbManager;
            SourceNodes = _commonBus.GetOutDataTree();
            _mapDatas = new List<MapData>(_commonBus.GetMapDatas());
            _newMapDatas = new List<MapData>();
            _removeDatas = new List<MapData>();
            _dialog = dialogService;
            InitItemModels(_mapDatas);
            MatchQuality();
            DeleteCommand = new DelegateCommand<OutputItemModel>(Delete);
            RecieveDrogCommand = new DelegateCommand<object>(RecieveDrog);
            SaveQualitySetCommand = new DelegateCommand<object>(SaveQualitySet);       
            CancelQualitySetCommand = new DelegateCommand(CancelQualitySet);
        }

        /// <summary>
        /// 匹配已存储的标准
        /// </summary>
        private void MatchQuality()
        {
            if (_commonBus.CurrentRecipe == null) return;
            var qModels = _dbManager.GetQualityItems(_commonBus.CurrentRecipe.Name).ToList();
            if (qModels.Count() > 0)
            {
                foreach (var item in QualityItemModels)
                {
                    var model = qModels.FirstOrDefault(x => x.MapId == item.MapId && x.Name == item.AliasName);
                    if (model != null)
                    {
                        qModels.Remove(model);
                    }
                }
                // 删除数据库中此配方中的多余项
                if (qModels.Count > 0)
                {
                    _dbManager.DeleteQualityItems(qModels, _commonBus.CurrentRecipe.Name);
                }
            }
        }

        /// <summary>
        /// 还原当前设置
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void CancelQualitySet()
        {
            _itemChanged = false;
            _commonBus.OnNavigate(PageModel.Pages.FirstOrDefault(x => x.Name == "Home"));
        }

        /// <summary>
        /// 保存当前设置
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void SaveQualitySet(object obj)
        {
            var needNavigate = (string)obj;
            var toleranceList = new List<TbLTolerance>();
            var timenow = DateTime.Now;
            var recipeName = _commonBus.CurrentRecipe.Name;

            //CheckAliasName();

            //foreach (var model in QualityItemModels)
            //{
            //    toleranceList.Add(new TbLTolerance()
            //    {
            //        ID = model.Id,
            //        Name = model.AliasName,
            //        MapId = model.MapId,
            //        RecipeName = recipeName,
            //        Standard = model.StandardValue,
            //        Compensate = model.CompensationValue,
            //        ToleranceMax = model.PositiveStandardDeviation,
            //        ToleranceMin = model.NegativeStandardDeviation,
            //        CreateTime = timenow,
            //    });
            //}

            foreach (var model in OutputItemModels)
            {
                var mapdata = _mapDatas.FirstOrDefault(x => x.ID == model.Id && x.DataSource == model.Source);
                if (mapdata != null)
                {
                    mapdata.StandardValue=model.StandardValue;
                    mapdata.PositivestandardDeviation = model.PositiveStandardDeviation;
                    mapdata.NegativestandardDeviation = model.NegativeStandardDeviation;
                    mapdata.CompensationValue=model.CompensationValue;
                    mapdata.NgReason = model.NgReason;
                }
            }

            _itemChanged = false;
            _dbManager.AddTbLTolerance(toleranceList);
            _commonBus.EventBus.GetEvent<MapUpdateEvent>().Publish(_mapDatas);
            _commonBus.UpdayeMapDataSource(_mapDatas, _newMapDatas, _removeDatas);
            _commonBus.OnSaveRecipe();
            if (needNavigate.ToUpper() == "TRUE")
                _commonBus.OnNavigate(PageModel.Pages.FirstOrDefault(x=>x.Name == "Home"));
        }

        /// <summary>
        /// 检查别名确保唯一
        /// </summary>
        /// <exception cref="Exception"></exception>
        //private void CheckAliasName()
        //{
        //    foreach (var item in OutputItemModels.GroupBy(x => x.AliasName).ToDictionary(x => x.Key, x => x))
        //    {
        //        if (item.Value.Count() > 1)
        //        {
        //            throw new Exception($"存在一样别名:\"{item.Key}\"，请修改新加入项的别名");
        //        }
        //    }
        //}

        /// <summary>
        /// 接收鼠标拖拽事件
        /// </summary>
        /// <param name="obj"></param>
        private void RecieveDrog(object obj)
        {
            var source = obj as DragEventArgs;
            var node = source.Data.GetData(typeof(LNode)) as LNode;
            if (node != null)
            {
                AddOutput(node);
            }
        }

        /// <summary>
        /// 添加数据节点
        /// </summary>
        /// <param name="node"></param>
        private void AddOutput(LNode node)
        {
            if (node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    AddOutput(child);
                }
            }
            else
            {
                AddOutputItem(node.Tag);
            }
        }

        /// <summary>
        /// 跟新输出表格UI
        /// </summary>
        /// <param name="map"></param>
        /// <exception cref="Exception"></exception>
        private void AddOutputItem(object map)
        {
            var mapData = map as MapData;
            if (mapData != null)
            {
                var model = OutputItemModels.FirstOrDefault(x => x.Id == mapData.ID && x.Source == mapData.Key);
                if (model != null)
                {
                    throw new Exception($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("Source")}{mapData.DataSource},{Luster.Motion.Assests.Langs.LangProvider.GetLang("OutPutItem")}{mapData.Alias}{Luster.Motion.Assests.Langs.LangProvider.GetLang("AlreadyExists")}");
                }

                if (mapData.DataType.IsNumeric() || mapData.DataType == typeof(LTolerance) || mapData.DataType == typeof(string))
                {
                    _mapDatas.Add(mapData);
                    _newMapDatas.Add(mapData);
                    var outModel = new OutputItemModel(mapData);
                    
                    outModel.CheckedChange += OutModel_CheckedChange;

                    OutputItemModels.Add(outModel);
                    if (mapData.DataType == typeof(LTolerance))
                    {
                        QualityItemModels.Add(new QualityItemModel()
                        {
                            AliasName = outModel.AliasName,
                            MapId = outModel.Id,
                        });
                    }
                    _itemChanged = true;
                }
            }
        }

        private void OutModel_CheckedChange(OutputItemModel item)
        {
            //if (!item.IsChecked)
            //{
            //    _itemChanged = true;
            //    return;
            //}
            //foreach (var model in OutputItemModels)
            //{
            //    if (!(model.Id == item.Id && model.Source == item.Source && model.AliasName == item.AliasName))
            //    {
            //        model.IsChecked = false;
            //    }
            //}
            _mapDatas.ForEach(x => x.IsMonitor = false);
            var map = _mapDatas.FirstOrDefault(x => x.ID == item.Id && x.DataSource == item.Source && x.Alias == item.AliasName);
            map.IsMonitor = true;
            _itemChanged = true;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        private void Delete(OutputItemModel model)
        {
            if (model == null)
            {
                return;
            }

            _dialog.ShowConfirm($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("ConfirmDelete")}:{model.AliasName}?{Luster.Motion.Assests.Langs.LangProvider.GetLang("RestartTakesEffect")}!", r =>
            {
                model.CheckedChange -= OutModel_CheckedChange;
                OutputItemModels.Remove(model);

                // 判断是否为新增的
                var tempMap = _newMapDatas.FirstOrDefault(x => x.ID == model.Id && x.DataSource == model.Source);
                if (tempMap != null)
                {
                    _newMapDatas.Remove(tempMap);
                }

                var map = _mapDatas.FirstOrDefault(x => x.ID == model.Id && x.DataSource == model.Source);
                if (map != null)
                {
                    _mapDatas.Remove(map);
                    _removeDatas.Add(map);
                    _itemChanged = true;
                }               

                if (model.SourceType != (typeof(LTolerance))) return;
                var qualityModel = QualityItemModels.FirstOrDefault(x => x.MapId == model.Id && x.AliasName == model.AliasName);
                if (qualityModel != null)
                {
                    QualityItemModels.Remove(qualityModel);
                    if (qualityModel.Id > 0)
                    {
                        var recipeName = _commonBus.CurrentRecipe.Name;
                        var tolerance = new TbLTolerance()
                        {
                            ID = qualityModel.Id,
                            Name = qualityModel.AliasName,
                            MapId = qualityModel.MapId,
                            RecipeName = recipeName,
                            Standard = qualityModel.StandardValue,
                            Compensate = qualityModel.CompensationValue,
                            ToleranceMax = qualityModel.PositiveStandardDeviation,
                            ToleranceMin = qualityModel.NegativeStandardDeviation,
                        };
                        _dbManager.DeleteQualityItem(tolerance, _commonBus.CurrentRecipe.Name);
                    }
                }
            });
        }

        /// <summary>
        /// 拖拽排序后更新数据源
        /// </summary>
        public void RefreshData()
        {
            var tempMapList = new List<MapData>();
            var tempQualityList = new List<QualityItemModel>();
            foreach (var item in OutputItemModels)
            {
                var tempMap = _mapDatas.FirstOrDefault(x => x.ID == item.Id && x.DataSource == item.Source);
                if (tempMap != null)
                {
                    tempMapList.Add(tempMap);
                }

                var tempQuality = QualityItemModels.FirstOrDefault(x => x.MapId == item.Id && x.AliasName == item.AliasName);
                if (tempQuality != null)
                {
                    tempQualityList.Add(tempQuality);
                }
            }
            if (tempMapList.Count > 0)
            {
                _itemChanged = true;
                _mapDatas = tempMapList;
                QualityItemModels = new ObservableCollection<QualityItemModel>(tempQualityList);
            }

        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        /// <param name="mapDatas"></param>
        private void InitItemModels(List<MapData> mapDatas)
        {
            OutputItemModels = new ObservableCollection<OutputItemModel>();
            QualityItemModels = new ObservableCollection<QualityItemModel>();
            if (mapDatas != null && mapDatas.Count > 0)
            {
                foreach (var map in mapDatas)
                {
                    var outModel = new OutputItemModel(map);
                    
                    outModel.CheckedChange += OutModel_CheckedChange;

                    OutputItemModels.Add(outModel);

                    if (map.DataType == typeof(LTolerance))
                    {
                        QualityItemModels.Add(new QualityItemModel()
                        {
                            AliasName = outModel.AliasName,
                            MapId = outModel.Id,
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 禁止编辑plc地址
        /// </summary>
        private DelegateCommand<object> _forbidEditCommand;
        public DelegateCommand<object> ForbidEditCommand => _forbidEditCommand ?? (_forbidEditCommand = new DelegateCommand<object>((obj) =>
        {
            var editArgs = obj as DataGridBeginningEditEventArgs;
            if (editArgs != null)
            {
                OutputItemModel model = editArgs.Row.Item as OutputItemModel;
                //if (model.SourceType.Name=="Boolean")
                //{
                //    editArgs.Cancel = true;
                //}
            }
        }));

        #region 导航接口实现
        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            if (_itemChanged)
            {
                _dialog.ShowConfirm($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("OutPutItemUnSave")}！", (r) =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        SaveQualitySet("False");
                    }
                });
            }
            continuationCallback(true);
        }
        #endregion
    }
}
