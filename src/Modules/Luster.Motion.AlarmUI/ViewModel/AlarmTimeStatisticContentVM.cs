#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlarmTimeStatisticContent
* 机器名称:       L05590
* 命名空间:       Luster.Motion.AlarmUI.ViewModel
* 文 件 名:       AlarmTimeStatisticContent.cs
* 创建时间:       2023/6/25 9:48:23
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      c48bbff5-c604-4bac-9da9-08594a41930d
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/6/25 9:48:23
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using LiveCharts;
using LiveCharts.Wpf;
using Luster.Common.DataAccess.Repositories;
using Luster.Motion.AlarmUI.Model;
using Luster.Motion.CommonUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.AlarmUI.ViewModel
{
    public class AlarmTimeStatisticContentVM:AlarmContentVM
    {
        protected AlarmTimeStatisticContentVM(ICommonBus _commonBus, IDbManager dbManager, IRepository repository) : base(_commonBus, dbManager, repository)
        {
        }


        protected override void InitPieSeriesData()
        {
            if (CurrentDayAlarmVMs == null) return;
            var alarmgroupList = CurrentDayAlarmVMs.GroupBy(x => x.AlarmType)
                .Select(x => new { x.Key, Count = x.Count(), Time = x.Sum(t => t.AlarmLongTime), }).ToList();//按照AlarmType分组并计算分组后的总时间以及次数
            if (alarmgroupList == null) return;
            alarmgroupList = alarmgroupList.OrderByDescending(x => x.Time).Take(10).ToList();//排序取前10
            int m = 0;
            double sumTime = alarmgroupList.Sum(t => t.Time);
            AlarmGroupList = new ObservableCollection<AlarmGroupModel>();
            foreach (var alarmgroup in alarmgroupList)
            {
                var y = m % 10;
                var model = new AlarmGroupModel()
                {
                    AlarmType = alarmgroup.Key,
                    Time = Math.Round((double)(alarmgroup.Time / 60), 1),
                    Count = alarmgroup.Count,
                    Color = _localBrushes[y],
                    Percent = sumTime == 0 ? 0 : Math.Round((alarmgroup.Time / sumTime) * 100, 2),
                };
                m++;
                AlarmGroupList.Add(model);
            }
            PieSeriesCollection = new SeriesCollection();
            ChartValues<double> chartvalue = new ChartValues<double>();
            foreach (var item in AlarmGroupList)
            {
                chartvalue = new ChartValues<double>();
                chartvalue.Add(item.Percent);
                PieSeries series = new PieSeries();
                series.DataLabels = true;
                series.Values = chartvalue;
                series.Fill = item.Color;
                series.Title = item.AlarmType;
                PieSeriesCollection.Add(series);
            }
        }

    }
}