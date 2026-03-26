#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ExcelTool
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Tools
* 文 件 名:       ExcelTool.cs
* 创建时间:       2022/3/3 13:42:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      bbb82e01-eeff-4dd7-beef-cbacba3013db
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/3/3 13:42:05
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Aspose.Cells;
using Luster.Common.DataStruct.Extensions;

namespace Luster.Common.Tools
{
    public enum HorizontalAlign
    {
        Left = 2,
        Center = 1,
        Right = 0,
    }

    public enum VerticalAlign
    {
        Center = 0,
        Top = 1,
        Bottom = 2,
    }

    public enum FileFormat
    {
        Xlsx = 0,
        PDF = 1,
        CSV = 2,
        TIFF = 3
    }

    /// <summary>
    /// 单元格
    /// </summary>
    public class ExcelCell
    {
        /// <summary>
        /// 单元格内容
        /// </summary>
        public object Text { get; set; }

        /// <summary>
        /// 字体大小
        /// </summary>
        public int FontSize { get; set; }

        /// <summary>
        /// 字体颜色
        /// </summary>
        public Color FontColor { get; set; }

        /// <summary>
        /// 是否有边框
        /// </summary>
        public bool IsBorders { get; set; } = true;

        /// <summary>
        /// 水平位置
        /// </summary>
        public HorizontalAlign HorizontalAlign { get; set; }

        /// <summary>
        /// 垂直位置
        /// </summary>
        public VerticalAlign VerticalAlign { get; set; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// 单元格数据判断
        /// </summary>
        public Func<double, Color> CheckPass { get; set; }
    }

    public class ExcelTool
    {
        #region 字段

        /// <summary>
        /// 表头列单元格
        /// </summary>
        private List<ExcelCell> _columns = new List<ExcelCell>();

        /// <summary>
        /// Excel文件地址
        /// </summary>
        private string excelfilepath;

        /// <summary>
        /// Excel的Workbook对象
        /// </summary>
        private Workbook workbook;

        /// <summary>
        ///  Excel的Worksheet对象
        /// </summary>
        private Worksheet worksheet;

        /// <summary>
        /// 
        /// </summary>
        private CellsFactory cellsfactory;

        /// <summary>
        /// 单元格对象
        /// </summary>
        private Cells cells;

        /// <summary>
        /// 单元格样式
        /// </summary>
        private Style style;

        #endregion

        #region 属性

        private string projecttitle;

        /// <summary>
        /// 项目标题
        /// </summary>
        public string ProjectTitle
        {
            get { return projecttitle; }
            set { projecttitle = value; }
        }

        private Style commonCellStyle;

        /// <summary>
        /// 普通单元格样式
        /// </summary>
        private Style CommonCellStyle
        {
            get
            {
                if (commonCellStyle == null)
                {
                    commonCellStyle = cellsfactory.CreateStyle();
                    commonCellStyle = StyleCreate();
                }
                return commonCellStyle;
            }
        }

        private Style errorCellStyle;

        /// <summary>
        /// 错误单元格样式
        /// </summary>
        private Style ErrorCellStyle
        {
            get
            {
                if (errorCellStyle == null)
                {
                    errorCellStyle = cellsfactory.CreateStyle();
                    errorCellStyle = StyleCreate();
                    errorCellStyle.ForegroundColor = Color.Red;
                }
                return errorCellStyle;
            }
        }

        private Style correctCellStyle;

        /// <summary>
        /// 正确单元格样式
        /// </summary>
        private Style CorrectCellStyle
        {
            get
            {
                if (correctCellStyle == null)
                {
                    correctCellStyle = cellsfactory.CreateStyle();
                    correctCellStyle = StyleCreate();
                    correctCellStyle.ForegroundColor = Color.Green;
                }
                return correctCellStyle;
            }
        }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="excelFile">Excel文件地址</param>
        public ExcelTool(string excelFile, bool isWrite = true)
        {
            excelfilepath = excelFile;

            if (isWrite)
            {
                if (File.Exists(excelFile))
                {
                    workbook = new Workbook(excelFile);
                }
                else
                {
                    workbook = new Workbook();
                    workbook.Save(excelFile);
                }

                worksheet = workbook.Worksheets[0];
                cells = worksheet.Cells;

                // 样式创建
                cellsfactory = new CellsFactory();
                style = cellsfactory.CreateStyle();
            }
            else
            {
                workbook = new Workbook(excelFile);
            }
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ExcelTool()
        {
            // 创建工作簿
            workbook = new Workbook();
            // 样式创建
            cellsfactory = new CellsFactory();
            style = cellsfactory.CreateStyle();
        }

        /// <summary>
        /// 添加一个标签
        /// </summary>
        /// <param name="sheetname"></param>
        /// <returns>返回对应的索引号</returns>
        public void AddWorksheet(string sheetname)
        {
            workbook.Worksheets.Add(sheetname);

            // 默认取最后一个
            worksheet = workbook.Worksheets[workbook.Worksheets.Count - 1];
            cells = GetCells(sheetname);
        }

        /// <summary>
        /// 通过名称获取sheet
        /// </summary>
        /// <param name="sheetname"></param>
        /// <returns></returns>
        private Worksheet GetWorkSheet(string sheetname)
        {
            Worksheet wSheet = null;
            foreach (Worksheet item in workbook.Worksheets)
            {
                if (item.Name.ToLower() == sheetname.ToLower())
                {
                    wSheet = item;
                    break;
                }
            }

            return wSheet;
        }

        /// <summary>
        /// 获取当前工作簿的Cells
        /// </summary>
        /// <param name="sheetname"></param>
        /// <returns></returns>
        private Cells GetCells(string sheetname)
        {
            var wSheet = GetWorkSheet(sheetname);
            return wSheet.Cells;
        }

        /// <summary>
        /// 获取当前工作簿的Cells
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private Cells GetCells(int sheet)
        {
            var wSheet = workbook.Worksheets[sheet];
            return wSheet.Cells;
        }

        #region 数据读取
        /// <summary>
        /// 通过Sheet获取Datable数据
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="startRow"></param>
        /// <param name="startCol"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">sheet不存在</exception>
        public DataTable GetTableBySheet(int sheet, int startRow, int startCol)
        {
            if (workbook.Worksheets.Count <= sheet)
            {
                return new DataTable();
            }

            worksheet = workbook.Worksheets[sheet];
            var totalRow = worksheet.Cells.Rows.Count - startRow;
            var totalCol = worksheet.Cells.Count - startCol;
            return worksheet.Cells.ExportDataTable(startRow, startCol, totalRow, totalCol);
        }


        public DataTable GetTableBySheet(string sheetname, int startRow, int startCol)
        {
            worksheet = GetWorkSheet(sheetname);

            if (worksheet != null)
            {
                var totalRow = worksheet.Cells.MaxDataRow - startRow + 1;
                var totalCol = worksheet.Cells.MaxDataColumn - startCol + 1;
                if (totalRow <= 0 || totalCol <= 0)
                {
                    return new DataTable();
                }

                return worksheet.Cells.ExportDataTable(startRow, startCol, totalRow, totalCol);
            }
            else
            {
                return new DataTable();
            }
        }

        /// <summary>
        /// 移除指定sheet页
        /// </summary>
        /// <param name="sheet">页数编号（从0开始）</param>
        public void RemoveWorkSheet(int sheet)
        {
            if (workbook.Worksheets.Count > sheet)
            {
                workbook.Worksheets.RemoveAt(sheet);
            }
        }

        /// <summary>
        /// 读取Excel 转换到List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startRow"></param>
        /// <param name="startCol"></param>
        /// <param name="sheetname"></param>
        /// <returns></returns>
        public List<T> GetListBySheet<T>(int startRow = 1, int startCol = 0, string sheetname = "")
        {
            if (string.IsNullOrEmpty(sheetname))
            {
                worksheet = workbook.Worksheets[0];
            }
            else
            {
                worksheet = GetWorkSheet(sheetname);
            }

            List<T> list = new List<T>();
            if (worksheet != null)
            {
                var totalRow = worksheet.Cells.MaxDataRow - startRow + 1;
                var totalCol = worksheet.Cells.MaxDataColumn - startCol + 1;
                if (totalRow <= 0 || totalCol <= 0)
                {
                    return list;
                }

                var type = typeof(T);
                var properties = CSVTool.GetProperties<T>();
                var dataTable = worksheet.Cells.ExportDataTable(startRow, startCol, totalRow, totalCol);

                int rowCount = dataTable.Rows.Count;
                for (int i = 0; i < rowCount; i++)
                {
                    var row = dataTable.Rows[i];
                    T obj = (T)Activator.CreateInstance(type);
                    
                    for (int j = 0; j < properties.Count; j++)
                    {
                        var prop = properties[j];
                        var cellVal = row.ItemArray[j];
                        
                        if (cellVal != null && cellVal.ToString()!= "")
                        {
                            var val = cellVal.ConvertTo(prop.PropertyType);
                            prop.SetValue(obj, val);
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            else
            {
                return list;
            }
        }

        #endregion

        #region 写入方法

        /// <summary>
        /// 创建单元格基本样式
        /// </summary>
        /// <returns></returns>
        private Style StyleCreate()
        {
            Style tempCellStyle = cellsfactory.CreateStyle();
            tempCellStyle.HorizontalAlignment = TextAlignmentType.Right;//文字居右
            tempCellStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin; //应用边界线 左边界线
            tempCellStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin; //应用边界线 右边界线
            tempCellStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin; //应用边界线 上边界线
            tempCellStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin; //应用边界线 下边界线
            tempCellStyle.Font.Name = "宋体";//设置字体
            tempCellStyle.Font.Size = 15;//设置字体大小
            tempCellStyle.Font.Color = Color.Black;//设置字体颜色
            tempCellStyle.Pattern = BackgroundType.Solid; //设置背景样式
            return tempCellStyle;
        }

        private Cell SetRange(int row, int startColumn, int totalRows, int totalColumns, object text)
        {
            Range range = worksheet.Cells.CreateRange(row, startColumn, totalRows, totalColumns);
            range.Merge();
            Cell cell = range[0, 0];
            cell.PutValue(text);
            return cell;
        }

        private Style StyleConvert(ExcelCell excelColumn)
        {
            Style style = cellsfactory.CreateStyle();
            //位置
            switch (excelColumn.HorizontalAlign)
            {
                case HorizontalAlign.Left:
                    {
                        style.HorizontalAlignment = TextAlignmentType.Left;
                    }
                    break;

                case HorizontalAlign.Center:
                    {
                        style.HorizontalAlignment = TextAlignmentType.Center;
                    }
                    break;

                case HorizontalAlign.Right:
                    {
                        style.HorizontalAlignment = TextAlignmentType.Right;
                    }
                    break;
            }
            switch (excelColumn.VerticalAlign)
            {
                case VerticalAlign.Center:
                    {
                        style.VerticalAlignment = TextAlignmentType.Center;
                    }
                    break;

                case VerticalAlign.Top:
                    {
                        style.VerticalAlignment = TextAlignmentType.Top;
                    }
                    break;

                case VerticalAlign.Bottom:
                    {
                        style.VerticalAlignment = TextAlignmentType.Bottom;
                    }
                    break;
            }
            //字体大小
            if (int.TryParse(excelColumn.FontSize.ToString(), out int v) && (excelColumn.FontSize > 0))
            {
                style.Font.Size = v;
            }
            else
            {
                style.Font.Size = 12;
            }
            //字体颜色
            if (excelColumn.FontColor != null)
            {
                style.Font.Color = excelColumn.FontColor;
            }
            else
            {
                style.Font.Color = Color.Black;
            }
            //背景颜色
            if (excelColumn.BackgroundColor != null)
            {
                style.Pattern = BackgroundType.Solid; //设置背景样式
                style.ForegroundColor = excelColumn.BackgroundColor;
            }
            else
            {
                style.Pattern = BackgroundType.None; //设置背景样式
            }
            //是否有边框
            if (excelColumn.IsBorders)
            {
                style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin; //应用边界线 左边界线
                style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin; //应用边界线 右边界线
                style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin; //应用边界线 上边界线
                style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin; //应用边界线 下边界线
            }
            return style;
        }

        /// <summary>
        /// 设置行高
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="height">高度</param>
        public void SetRowHeight(int row, int height)
        {
            cells.SetRowHeight(row, height);
        }

        /// <summary>
        /// 设置列宽
        /// </summary>
        /// <param name="col">列</param>
        /// <param name="width">宽度</param>
        public void SetColumnWidth(int column, int width)
        {
            cells.SetColumnWidth(column, width);
        }

        /// <summary>
        /// 通过指定行和列设置单元格文本信息
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="column">列</param>
        /// <param name="text">文本信息</param>
        public void SetText(int row, int column, string text)
        {
            cells[row, column].PutValue(text);
            cells[row, column].SetStyle(CommonCellStyle);
        }

        /// <summary>
        /// 通过指定行和列单元格
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="column">列</param>
        /// <param name="text">带格式的文本信息</param>
        public void SetText(int row, int column, ExcelCell text)
        {
            cells[row, column].PutValue(text.Text);
            cells[row, column].SetStyle(StyleConvert(text));
        }

        /// <summary>
        /// 合并单元格（可写入值）
        /// </summary>
        /// <param name="firstRow">起始行</param>
        /// <param name="firstColumn">起始列</param>
        /// <param name="totalRows">合并总行数</param>
        /// <param name="totalColumns">合并总列数</param>
        /// <param name="text">文本信息（默认不输入）</param>

        public void CellsMerge(int firstRow, int firstColumn, int totalRows, int totalColumns, string text = null)
        {
            Range range = worksheet.Cells.CreateRange(firstRow, firstColumn, totalRows, totalColumns);
            range.Merge();
            Cell cell = range[0, 0];
            cell.PutValue(text);
            cell.SetStyle(CommonCellStyle);
        }

        /// <summary>
        /// 合并一行的多列单元格并写入值
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="startColumn">起始列</param>
        /// <param name="totalColumns">合并总列数</param>
        /// <param name="text">文本信息</param>
        public void SetMergeColumns(int row, int startColumn, int totalColumns, string text)
        {
            Cell cell = SetRange(row, startColumn, 1, totalColumns, text);
            cell.SetStyle(CommonCellStyle);
        }

        /// <summary>
        /// 合并一行的多列单元格并写入值
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="startColumn">起始列</param>
        /// <param name="totalColumns">合并总列数</param>
        /// <param name="text">带格式的文本信息</param>
        public void SetMergeColumns(int row, int startColumn, int totalColumns, ExcelCell text)
        {
            Cell cell = SetRange(row, startColumn, 1, totalColumns, text.Text);
            cell.SetStyle(StyleConvert(text));
        }

        /// <summary>
        /// 合并一列的多行单元格并写入值
        /// </summary>
        /// <param name="startRow">起始行</param>
        /// <param name="column">列</param>
        /// <param name="totalRows">合并总行数</param>
        /// <param name="text">文本信息</param>
        public void SetMergeRows(int startRow, int column, int totalRows, string text)
        {
            Cell cell = SetRange(startRow, column, totalRows, 1, text);
            cell.SetStyle(CommonCellStyle);
        }

        /// <summary>
        /// 写入一行数据
        /// </summary>
        /// <param name="rowStart">起始行</param>
        /// <param name="columnStart">起始列</param>
        /// <param name="rowDatas">一行信息</param>
        public void WriteRowDatas(int rowStart, int columnStart, params object[] rowDatas)
        {
            if (rowDatas != null)
            {
                for (int i = 0; i < rowDatas.Length; i++)
                {
                    cells[rowStart, columnStart + i].PutValue(rowDatas[i]);
                    cells[rowStart, columnStart + i].SetStyle(CommonCellStyle);
                }
            }
        }

        /// <summary>
        /// 写入一行数据
        /// </summary>
        /// <param name="rowStart">起始行</param>
        /// <param name="columnStart">起始列</param>
        /// <param name="rowDatas">带格式的文本信息</param>
        public void WriteRowDatas(int rowStart, int columnStart, params ExcelCell[] rowDatas)
        {
            if (rowDatas != null)
            {
                for (int i = 0; i < rowDatas.Length; i++)
                {
                    cells[rowStart, columnStart + i].PutValue(rowDatas[i].Text);
                    cells[rowStart, columnStart + i].SetStyle(StyleConvert(rowDatas[i]));
                }
            }
        }

        /// <summary>
        /// 设置表头信息
        /// </summary>
        /// <param name="rowStart">起始行</param>
        /// <param name="columnStart">起始列</param>
        /// <param name="headers">表头信息</param>
        public void SetHeaders(int rowStart, int columnStart, params string[] headers)
        {
            if (headers != null)
            {
                for (int i = 0; i < headers.Length; i++)
                {
                    ExcelCell excel = new ExcelCell();
                    excel.Text = headers[i];
                    _columns.Add(excel);
                    cells[rowStart, columnStart + i].PutValue(headers[i]);
                    cells[rowStart, columnStart + i].SetStyle(CommonCellStyle);
                }
            }
        }

        /// <summary>
        /// 设置表头信息
        /// </summary>
        /// <param name="rowStart">起始行</param>
        /// <param name="columnStart">起始列</param>
        /// <param name="headers">带格式的表头信息</param>
        public void SetHeaders(int rowStart, int columnStart, params ExcelCell[] headers)
        {
            if (headers != null)
            {
                for (int i = 0; i < headers.Length; i++)
                {
                    _columns.Add(headers[i]);
                    cells[rowStart, columnStart + i].PutValue(headers[i].Text);
                    cells[rowStart, columnStart + i].SetStyle(StyleConvert(headers[i]));
                }
            }
        }

        /// <summary>
        /// 添加一行数据
        /// </summary>
        /// <param name="rowData">数据信息(信息需要与表头信息对应)</param>
        /// <param name="row">起始行(默认-1，自动计算行数)</param>
        /// <param name="col">起始列(默认-1，从0列开始)</param>
        /// <param name="isCheck">是否检测(默认 true，检测单元格数据)</param>
        public void AddRow(IDictionary<string, object> rowData, int row = -1, int col = -1, bool isCheck = true)
        {
            if (row <= -1)
            {
                row = cells.Rows.Count;
            }

            if (col <= -1)
            {
                col = 0;
            }

            int index = 0;
            foreach (var item in _columns)
            {
                var cellValue = rowData[item.Text.ToString()];
                if (cellValue != null)
                {
                    string strvalue = cellValue.ToString();
                    if ((item.CheckPass != null) && double.TryParse(strvalue, out var v) && (isCheck == true))
                    {
                        //现在CheckPass这里有问题
                        Color passColor = item.CheckPass(v);

                        var bgColorStyle = cellsfactory.CreateStyle();
                        bgColorStyle = StyleCreate();
                        bgColorStyle.ForegroundColor = passColor;
                        cells[row, col + index].SetStyle(bgColorStyle);
                        cells[row, col + index].PutValue(v);
                    }
                    else
                    {
                        cells[row, col + index].PutValue(strvalue);
                        cells[row, col + index].SetStyle(CommonCellStyle);
                    }
                }
                else
                {
                    //写入null
                    cells[row, col + index].PutValue(cellValue);
                    cells[row, col + index].SetStyle(CommonCellStyle);
                }
                index = index + 1;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save(string filePath = "")
        {
            worksheet.AutoFitColumns();
            worksheet.AutoFitRows();
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = excelfilepath;
            }
            workbook.Save(filePath);
        }

        /// <summary>
        /// 另存
        /// </summary>
        /// <param name="excelFile">另存文件地址</param>
        /// <param name="fileFormat">文件格式</param>
        public void SaveAs(string excelFile, FileFormat fileFormat)
        {
            worksheet.AutoFitColumns();
            worksheet.AutoFitRows();
            switch (fileFormat)
            {
                case FileFormat.Xlsx:
                    {
                        workbook.Save(excelFile, SaveFormat.Xlsx);
                    }
                    break;

                case FileFormat.PDF:
                    {
                        workbook.Save(excelFile, SaveFormat.Pdf);
                    }
                    break;

                case FileFormat.CSV:
                    {
                        workbook.Save(excelFile, SaveFormat.CSV);
                    }
                    break;

                case FileFormat.TIFF:
                    {
                        workbook.Save(excelFile, SaveFormat.TIFF);
                    }
                    break;
            }
        }

        /// <summary>
        /// 保存数据表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <param name="filename"></param>
        public void Save<T>(IList<T> datas, string filename = "")
        {
            var titles = CSVTool.GetTitles<T>(out var properties);

            // 配置标题
            SetRowHeight(0, 28);

            if (_columns == null || _columns.Count == 0)
            {
                SetHeaders(0, 0, titles.ToArray());
            }

            // 构建表内容
            int rowNum = datas.Count;
            for (int row = 1; row < rowNum + 1; row++)
            {
                var item = datas[row - 1];
                for (int col = 0; col < properties.Count; col++)
                {
                    var prop = properties[col];
                    var pVal = prop.GetValue(item);

                    cells[row, col].PutValue(pVal);
                    cells[row, col].SetStyle(CommonCellStyle);
                }
            }

            // 保存内容
            Save(filename);
        }
        #endregion
    }
}