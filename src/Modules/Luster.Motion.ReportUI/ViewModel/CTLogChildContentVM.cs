using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;

using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Luster.TaskFlow.Motion.Logic;
using System.Timers;
using System.Windows;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.Motion.DataStruct;
using TaiKeCommon;
using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.Defaults;

using SkiaSharp.Views.WPF;
//using LiveCharts.Wpf;
using LiveCharts;
using Luster.Common.Tools;
using Luster.Motion.ReportUI.Model;
using System.Windows.Forms;
using Luster.Common.DataAccess.Repositories;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Drawing;
using AxisPosition = LiveChartsCore.Measure.AxisPosition;
using LiveCharts.Wpf.Charts.Base;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Prism.Common;
using System.Text;
using System.ComponentModel;
//using Luster.SimDevice.SubSystem.Events;

namespace Luster.Motion.ReportUI.ViewModel
{
    #region 原始CSV一行
    internal sealed class CsvRow
    {
        public string SN { get; }
        public string 模块 { get; }
        public string 动作 { get; }
        public string 开始时间 { get; }
        public string 结束时间 { get; }
        public string Actual_CT { get; }
        public string Target_CT { get; }
        public string Gap { get; }

        public CsvRow(string sn, string 模块, string 动作, string 开始时间,
                      string 结束时间, string actualCt, string targetCt, string gap)
        {
            SN = sn;
            this.模块 = 模块;
            this.动作 = 动作;
            this.开始时间 = 开始时间;
            this.结束时间 = 结束时间;
            Actual_CT = actualCt;
            Target_CT = targetCt;
            Gap = gap;
        }
        #endregion
    }

    #region 表格一行
    public class GridRow //  : INotifyPropertyChanged
    {
        public string StartTime { get; set; }
        public string SN { get; set; }
        public bool IsTargetRow { get; set; } // true=TargetCT行
        public string TargetText { get; set; } = "TargetCT(s)"; // 行标题

        // 动作 -> Actual_CT
        private readonly Dictionary<string, string> _map = new Dictionary<string, string>();
        //public Dictionary<string, string> ActionCtMap { get; set; } = new Dictionary<string, string>();

        // 索引器：列绑定就靠它
        public string this[string actionName]
        {
            get
            {
                return _map.TryGetValue(actionName, out var v) ? v : string.Empty;
            }
        }

        // 暴露一个方法供 VM 填充
        public void SetActionCt(string action, string ct)
        {
            _map[action] = ct;
            // 触发假属性，让绑定重新读取
            OnPropertyChanged("Item[" + action + "]");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
    #endregion

    #region VM
    public partial class CTLogChildContentVM// : ObservableObject<T>
    {
        /// <summary>
        /// 文件名（用作 Tab Header）
        /// </summary>
        public string FileName { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        // 这就是缺少的方法
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 父级 VM 会注入删除动作
        public DelegateCommand CloseCmd { get; private set; }

        public ObservableCollection<GridRow> GridRows { get; private set; } = new ObservableCollection<GridRow>();

        private List<string> _actionHeaders = new List<string>();
        public List<string> ActionHeaders
        {
            get { return _actionHeaders; }
            private set
            {
                _actionHeaders = value;
                OnPropertyChanged("ActionHeaders");   // 通知 UI 重新读取
            }
        }

        //private List<string> _actionHeaders = new List<string>(); // 列头顺序
        //public IReadOnlyList<string> ActionHeaders => _actionHeaders;

        /// <summary>
        /// 构造函数：解析 CSV 并填充集合
        /// </summary>
        public CTLogChildContentVM(string csvPath, Action<CTLogChildContentVM> removeAction)
        {
            FileName = Path.GetFileNameWithoutExtension(csvPath);
            CloseCmd = new DelegateCommand(() => removeAction(this));
            BuildGrid(csvPath);
        }

        private void BuildGrid(string path)
        {
            // 1. 读原始数据（最多100行）
            var rows = ReadCsvRows(path).Take(100).ToList();

            // 2. 拆成“完整序列”
            var sequences = SplitSequences(rows).Take(5).ToList(); // 最多5组

            // 3. 提取不重复动作名（按第一次出现顺序）
            _actionHeaders = sequences
                             .SelectMany(seq => seq.Select(r => r.动作))
                             .Distinct()
                             .ToList();

            // 4. 一行序列 -> 一行GridRow
            GridRows.Clear();
            // 1. 构造 Target 行（第一组序列）
            var targetRow = new GridRow { IsTargetRow = true };
            if (sequences.Count > 0)
            {
                foreach (var r in sequences[0])
                    targetRow.SetActionCt(r.动作, r.Target_CT);
            }
            //targetRow.SN = "TargetCT(s)";
            GridRows.Add(targetRow);
            // 2. 正常数据行
            foreach (var seq in sequences)
            {
                var first = seq.First();
                var gr = new GridRow
                {
                    StartTime = first.开始时间,
                    SN = first.SN
                };

                foreach (var r in seq)
                    gr.SetActionCt(r.动作, r.Actual_CT);

                GridRows.Add(gr);
            }

            //GridRows = new ObservableCollection<GridRow>(
            //    sequences.Select(seq => new GridRow
            //    {
            //        StartTime = seq.First().开始时间,
            //        SN = seq.First().SN,
            //        ActionCtMap = seq.ToDictionary(r => r.动作, r => r.Actual_CT)
            //    }));
        }

        /* ---------- 读CSV ---------- */
        private IEnumerable<CsvRow> ReadCsvRows(string path)
        {
            using (var reader = new StreamReader(path, Encoding.UTF8))
            {
                string line;
                var headers = reader.ReadLine()?.Split(',') ?? Array.Empty<string>(); // 跳过标题
                while ((line = reader.ReadLine()) != null)
                {
                    var cols = line.Split(',');
                    if (cols.Length < 8) continue; // 字段不足直接跳过
                    yield return new CsvRow(
                        cols[0], cols[1], cols[2], cols[3],
                        cols[4], cols[5], cols[6], cols[7]);
                }
            }
        }

        /* ---------- 拆序列 ---------- */
        private IEnumerable<List<CsvRow>> SplitSequences(IEnumerable<CsvRow> rows)
        {
            List<CsvRow> current = null;
            foreach (var r in rows)
            {
                if (r.动作.Contains("工站开始"))
                {
                    current = new List<CsvRow> { r };
                }
                else if (current != null)
                {
                    current.Add(r);
                    if (r.动作.Contains("工站结束"))
                    {
                        yield return current;
                        current = null;
                    }
                }
            }
        }


        /// <summary>
        /// 实际解析逻辑
        /// </summary>
        private void LoadCsv(string path)
        {
            var rows = File.ReadLines(path)
                           .Skip(1)   // 跳过标题
                           .Select(l => l.Split(','))
                           .Select(a => new
                           {
                               TargetCt = a.Length > 0 ? a[0].Trim() : "",
                               Action = a.Length > 1 ? a[1].Trim() : "",
                               ActualCt = a.Length > 2 ? a[2].Trim() : ""
                           })
                           .ToList();

            //TargetCtRow = new ObservableCollection<string>(rows.Select(r => r.TargetCt));
            //ActionRow = new ObservableCollection<CellVm>(
            //                  rows.Select(r => new CellVm
            //                  {
            //                      Text = r.Action,
            //                      IsWait = r.Action?.Contains("等待") == true
            //                  }));

            //int half = (int)Math.Ceiling(rows.Count / 2.0);
            //ActualCtRow1 = new ObservableCollection<string>(rows.Take(half).Select(r => r.ActualCt));
            //ActualCtRow2 = new ObservableCollection<string>(rows.Skip(half).Select(r => r.ActualCt));
        }
    }
    #endregion
    ///// <summary>
    ///// 单行单元格 VM（支持黄/蓝切换）
    ///// </summary>
    //public partial class CellVm : ObservableObject
    //{
    //    [ObservableProperty] private string? text;
    //    [ObservableProperty] private bool isWait;
    //}
}
