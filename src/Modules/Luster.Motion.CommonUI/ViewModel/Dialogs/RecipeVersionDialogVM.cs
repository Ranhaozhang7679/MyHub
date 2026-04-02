using Luster.Common.Tools;
using Luster.Motion.CommonUI.ViewModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public sealed class RecipeVersionDialogVM : MotionDialogVM
    {
        ICommonBus commonBus;

        private string _versionNoPrefix;
        /// <summary>
        /// 版本号前段（最后一段之前的部分），用户可编辑
        /// </summary>
        public string VersionNoPrefix
        {
            get { return _versionNoPrefix; }
            set
            {
                if (SetProperty(ref _versionNoPrefix, value))
                {
                    HasVersionWarning = false;
                    VersionWarning = string.Empty;
                }
            }
        }

        private string _versionNoDaySuffix;
        /// <summary>
        /// 版本号最后一段（当天日期），只读
        /// </summary>
        public string VersionNoDaySuffix
        {
            get { return _versionNoDaySuffix; }
            set { SetProperty(ref _versionNoDaySuffix, value); }
        }

        /// <summary>
        /// 完整版本号（前段 + "." + 日期后缀），用于保存和校验
        /// </summary>
        public string VersionNo
        {
            get
            {
                if (string.IsNullOrEmpty(VersionNoPrefix))
                    return VersionNoDaySuffix ?? string.Empty;
                return $"{VersionNoPrefix}.{VersionNoDaySuffix}";
            }
        }

        private string _latestVersionNo;
        public string LatestVersionNo
        {
            get { return _latestVersionNo; }
            set { SetProperty(ref _latestVersionNo, value); }
        }

        private string _versionWarning;
        public string VersionWarning
        {
            get { return _versionWarning; }
            set { SetProperty(ref _versionWarning, value); }
        }

        private bool _hasVersionWarning;
        public bool HasVersionWarning
        {
            get { return _hasVersionWarning; }
            set { SetProperty(ref _hasVersionWarning, value); }
        }

        private ObservableCollection<ChangeLineItem> _changeLines;
        public ObservableCollection<ChangeLineItem> ChangeLines
        {
            get { return _changeLines; }
            set { SetProperty(ref _changeLines, value); }
        }

        private List<string> _versionTypes;
        public List<string> VersionTypes
        {
            get { return _versionTypes; }
            set { SetProperty(ref _versionTypes, value); }
        }

        private string _selectedVersionType;
        public string SelectedVersionType
        {
            get { return _selectedVersionType; }
            set
            {
                if (SetProperty(ref _selectedVersionType, value))
                {
                    LoadVersionNo(value);
                }
            }
        }

        private string _modifiedBy;
        public string ModifiedBy
        {
            get { return _modifiedBy; }
        }

        private string _modifiedTime;
        public string ModifiedTime
        {
            get { return _modifiedTime; }
        }

        public RecipeVersionDialogVM(ICommonBus cmbus)
        {
            commonBus = cmbus;
            VersionTypes = new List<string> { "配方", "PLC" };
            SelectedVersionType = "配方";
            ChangeLines = new ObservableCollection<ChangeLineItem> { new ChangeLineItem() };
            _modifiedBy = cmbus.CurrentUser?.UserName ?? string.Empty;
            _modifiedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void LoadVersionNo(string versionType)
        {
            // 日期后缀始终为当天日期
            string dayStr = DateTime.Now.Day.ToString();

            string recipeVersionPath = commonBus.ProjInfo?.FullName;
            if (string.IsNullOrEmpty(recipeVersionPath))
            {
                LatestVersionNo = string.Empty;
                VersionNoPrefix = string.Empty;
                VersionNoDaySuffix = dayStr.PadLeft(2, '0');
                return;
            }
            recipeVersionPath = recipeVersionPath.Substring(0, recipeVersionPath.LastIndexOf(@"\"));
            string fileName = versionType == "PLC" ? "PLCVersion.json" : "Version.json";
            string filePath = Path.Combine(recipeVersionPath, "Config", fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        string strJson = reader.ReadToEnd();
                        var obj = JsonTool.ToObject<VersionConfig>(strJson);
                        if (obj?.Versions != null && obj.Versions.Count > 0)
                        {
                            LatestVersionNo = obj.Versions[0].Version;
                            SplitVersionToFields(LatestVersionNo, dayStr);
                        }
                        else
                        {
                            LatestVersionNo = string.Empty;
                            VersionNoPrefix = string.Empty;
                            VersionNoDaySuffix = dayStr.PadLeft(2, '0');
                        }
                    }
                }
                catch
                {
                    LatestVersionNo = string.Empty;
                    VersionNoPrefix = string.Empty;
                    VersionNoDaySuffix = dayStr.PadLeft(2, '0');
                }
            }
            else
            {
                LatestVersionNo = string.Empty;
                VersionNoPrefix = string.Empty;
                VersionNoDaySuffix = dayStr.PadLeft(2, '0');
            }
        }

        /// <summary>
        /// 将版本号拆分为前段和日期后缀
        /// 例如 V1.02.03.09 + day=1 → Prefix="V1.02.03", Suffix="01"
        /// 跨月时（上次日期 > 今天日期），前段最后一个数字段自动+1
        /// 例如 V1.02.03.28 + day=3 → Prefix="V1.02.04", Suffix="03"
        /// </summary>
        private void SplitVersionToFields(string version, string dayStr)
        {
            if (string.IsNullOrEmpty(version))
            {
                VersionNoPrefix = string.Empty;
                VersionNoDaySuffix = dayStr.PadLeft(2, '0');
                return;
            }

            int lastDot = version.LastIndexOf('.');
            if (lastDot >= 0)
            {
                string prefix = version.Substring(0, lastDot);
                int lastWidth = version.Length - lastDot - 1;
                string lastSegStr = version.Substring(lastDot + 1);

                // 跨月检测：上次最后一段（日期）> 今天日期，说明跨月了，倒数第二段+1
                int.TryParse(lastSegStr, out int oldDay);
                int today = DateTime.Now.Day;
                if (oldDay > today)
                {
                    prefix = IncrementLastSegment(prefix);
                }

                VersionNoPrefix = prefix;
                VersionNoDaySuffix = dayStr.PadLeft(Math.Max(lastWidth, 2), '0');
            }
            else
            {
                // 只有一段，整段作为前缀无意义，前缀留空
                VersionNoPrefix = string.Empty;
                VersionNoDaySuffix = dayStr.PadLeft(2, '0');
            }
        }

        /// <summary>
        /// 将前段最后一个数字段+1，位数溢出时向前进位，本段归0
        /// 第一段允许位数增长（如 V9→V10，99→100）
        /// 例如 "V1.02.03" → "V1.02.04"
        /// 例如 "V1.02.99" → "V1.03.00"（99+1溢出，进位到上一段）
        /// 例如 "V1.99.99" → "V2.00.00"
        /// 例如 "V9.99.99" → "V10.00.00"（第一段允许增长位数）
        /// </summary>
        private static string IncrementLastSegment(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return prefix;

            // 提取第一段的字母前缀（如 "V"）
            var parts = prefix.Split('.');
            string letterPrefix = string.Empty;
            for (int i = 0; i < parts[0].Length; i++)
            {
                if (char.IsDigit(parts[0][i]))
                {
                    letterPrefix = parts[0].Substring(0, i);
                    parts[0] = parts[0].Substring(i);
                    break;
                }
            }

            // 解析每段的数值和原始位宽
            var segments = new int[parts.Length];
            var widths = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                widths[i] = parts[i].Length;
                int.TryParse(parts[i], out segments[i]);
            }

            // 从最后一段开始+1，溢出则归0并向前进位
            int carry = 1;
            for (int i = segments.Length - 1; i >= 0 && carry > 0; i--)
            {
                segments[i] += carry;
                carry = 0;

                if (i > 0)
                {
                    // 非第一段：位数增加则溢出进位
                    int maxVal = (int)Math.Pow(10, widths[i]);
                    if (segments[i] >= maxVal)
                    {
                        segments[i] = 0;
                        carry = 1;
                    }
                }
                // 第一段：允许位数增长，不进位
            }

            // 组装结果
            var result = new string[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                result[i] = segments[i].ToString().PadLeft(widths[i], '0');
            }
            return letterPrefix + string.Join(".", result);
        }

        /// <summary>
        /// 校验版本号前段格式：
        /// 1. 整体格式为 [英文字母]数字.数字.数字... ，第一段前可有英文字母前缀，其余段纯数字
        /// 2. 总段数（含日期后缀）不超过6段
        /// </summary>
        private bool ValidateFormat()
        {
            string prefix = VersionNoPrefix?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(prefix))
            {
                // 前段为空，跳过版本管理，不需要校验格式
                return true;
            }

            var segments = prefix.Split('.');

            // 总段数 = 前段段数 + 1（日期后缀），不超过6段
            if (segments.Length + 1 > 6)
            {
                HasVersionWarning = true;
                VersionWarning = "版本号总段数不能超过6段";
                return false;
            }

            for (int i = 0; i < segments.Length; i++)
            {
                if (string.IsNullOrEmpty(segments[i]))
                {
                    HasVersionWarning = true;
                    VersionWarning = "版本号格式不正确，不能包含空段";
                    return false;
                }

                if (i == 0)
                {
                    // 第一段：可选英文字母前缀 + 数字
                    if (!Regex.IsMatch(segments[i], @"^[A-Za-z]*\d+$"))
                    {
                        HasVersionWarning = true;
                        VersionWarning = "版本号格式不正确，仅第一段前可包含英文字母，其余必须为数字";
                        return false;
                    }
                }
                else
                {
                    // 其余段：纯数字
                    if (!Regex.IsMatch(segments[i], @"^\d+$"))
                    {
                        HasVersionWarning = true;
                        VersionWarning = "版本号格式不正确，各段必须为纯数字（仅第一段前可包含英文字母）";
                        return false;
                    }
                }
            }

            HasVersionWarning = false;
            VersionWarning = string.Empty;
            return true;
        }

        private bool ValidateVersionNo()
        {
            if (string.IsNullOrEmpty(LatestVersionNo) || string.IsNullOrEmpty(VersionNo))
            {
                HasVersionWarning = false;
                VersionWarning = string.Empty;
                return true;
            }

            int compareResult = CompareVersion(VersionNo, LatestVersionNo);
            if (compareResult < 0)
            {
                HasVersionWarning = true;
                VersionWarning = $"版本号不能低于当前版本 {LatestVersionNo}";
                return false;
            }
            else
            {
                HasVersionWarning = false;
                VersionWarning = string.Empty;
                return true;
            }
        }

        /// <summary>
        /// 去除版本号中的非数字前缀，返回纯数字部分
        /// </summary>
        private static string StripPrefix(string version)
        {
            for (int i = 0; i < version.Length; i++)
            {
                if (char.IsDigit(version[i]))
                    return version.Substring(i);
            }
            return version;
        }

        private static int CompareVersion(string v1, string v2)
        {
            var parts1 = StripPrefix(v1).Split('.');
            var parts2 = StripPrefix(v2).Split('.');
            int maxLen = Math.Max(parts1.Length, parts2.Length);
            for (int i = 0; i < maxLen; i++)
            {
                int n1 = 0, n2 = 0;
                if (i < parts1.Length) int.TryParse(parts1[i], out n1);
                if (i < parts2.Length) int.TryParse(parts2[i], out n2);
                if (n1 > n2) return 1;
                if (n1 < n2) return -1;
            }
            return 0;
        }

        private DelegateCommand _addLineCommand;
        public DelegateCommand AddLineCommand => _addLineCommand ?? (_addLineCommand = new DelegateCommand(() =>
        {
            ChangeLines.Add(new ChangeLineItem());
        }));

        private DelegateCommand _saveVersionCommand;
        public DelegateCommand SaveVersionCommand => _saveVersionCommand ?? (_saveVersionCommand = new DelegateCommand(() =>
        {
            if (string.IsNullOrWhiteSpace(VersionNoPrefix))
            {
                // 前段为空，跳过版本管理，直接关闭对话框让调用方执行保存
                IDialogResult rr = new DialogResult(ButtonResult.OK);
                rr.Parameters.Add("VersionNo", string.Empty);
                rr.Parameters.Add("VersionType", SelectedVersionType);
                rr.Parameters.Add("ChangeContents", new List<string>());
                rr.Parameters.Add("ModifiedBy", ModifiedBy);
                rr.Parameters.Add("ModifiedTime", ModifiedTime);
                RaiseRequestClose(rr);
                return;
            }

            // 格式防呆校验
            if (!ValidateFormat())
            {
                return;
            }

            var contents = new List<string>();
            int index = 1;
            foreach (var line in ChangeLines)
            {
                if (!string.IsNullOrWhiteSpace(line.Text))
                {
                    contents.Add($"{index}、{line.Text}");
                    index++;
                }
            }

            // 有变更内容时才校验版本号大小
            if (contents.Count > 0 && !ValidateVersionNo())
            {
                return;
            }

            IDialogResult r = new DialogResult(ButtonResult.OK);
            r.Parameters.Add("VersionNo", VersionNo);
            r.Parameters.Add("VersionType", SelectedVersionType);
            r.Parameters.Add("ChangeContents", contents);
            r.Parameters.Add("ModifiedBy", ModifiedBy);
            r.Parameters.Add("ModifiedTime", ModifiedTime);
            RaiseRequestClose(r);
        }));

        private DelegateCommand _cancelVersionCommand;
        public DelegateCommand CancelVersionCommand => _cancelVersionCommand ?? (_cancelVersionCommand = new DelegateCommand(() =>
        {
            RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
        }));

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            LoadVersionNo(SelectedVersionType);
        }
    }

    public class ChangeLineItem : BindableBase
    {
        private string _text;
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }
    }
}
