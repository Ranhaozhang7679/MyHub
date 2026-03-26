using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HandyControl.Controls;
using Luster.Motion.ReportUI.ViewModel;

namespace Luster.Motion.ReportUI.Views
{
    /// <summary>
    /// ChartTorqueContent.xaml 的交互逻辑
    /// </summary>
    public partial class CTLogChildContent : UserControl
    {
        public CTLogChildContent()
        {
            InitializeComponent();
            // 后台代码改监听 DataContextChanged
            DataContextChanged += OnDataContextChanged;
        }
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is CTLogChildContentVM vm)
                BuildColumns(vm);
        }
        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        //{
        //    base.OnRenderSizeChanged(sizeInfo);
        //    if (DataContext is CTLogChildContentVM vm && dataGrid.Columns.Count == 0)
        //        BuildColumns(vm);
        //}

        private void BuildColumns(CTLogChildContentVM vm)
        {
            dataGrid.Columns.Clear();
            Style centerHead = (Style)dataGrid.FindResource("CenterHeaderStyle");
            // 固定列
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "开始时间",
                Binding = new Binding("StartTime")
            });
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "SN",
                Binding = new Binding("SN")
            });

            // 动态动作列
            foreach (var hdr in vm.ActionHeaders)
            {
                var col = new DataGridTextColumn
                {
                    Header = hdr,
                    Width = 60, // 固定列宽
                    Binding = new Binding("[" + hdr + "]")   // 关键：索引器
                    {
                        TargetNullValue = "",
                        FallbackValue = ""
                    },
                    HeaderStyle = centerHead
                };
                // 关键：包含“等待”就刷成彩色背景
                if (hdr.Contains("等待"))
                {
                    // 表头背景
                    col.HeaderStyle = new Style(typeof(DataGridColumnHeader))
                    {
                        BasedOn = centerHead,
                        Setters = {
                    new Setter(DataGridColumnHeader.BackgroundProperty,
                               new SolidColorBrush(Colors.Orange))
                        }
                    };
                    // 单元格背景（可选）
                    col.CellStyle = new Style(typeof(DataGridCell))
                    {
                        Setters = {
                    new Setter(DataGridCell.BackgroundProperty,
                               new SolidColorBrush(Colors.LightPink))
                        },
                        Triggers = {
                                new DataTrigger
                                {
                        Binding = new Binding("IsTargetRow"),
                        Value   = true,
                        Setters = {
                            // 用空值把前面的黄色擦掉，恢复浅绿（或任意你想保留的颜色）
                            new Setter(DataGridCell.BackgroundProperty,
                                       new SolidColorBrush(Colors.Transparent))
                                }
                            }
                        }
                    };
                }

                dataGrid.Columns.Add(col);
            }

            // 动态动作列
            //foreach (var hdr in vm.ActionHeaders)
            //{
            //    dataGrid.Columns.Add(new DataGridTextColumn
            //    {
            //        Header = hdr,
            //        Binding = new Binding($"ActionCtMap[{hdr}]")
            //        {
            //            TargetNullValue = "",
            //            FallbackValue = ""
            //        }
            //    });
            //}
        }
    }
}
