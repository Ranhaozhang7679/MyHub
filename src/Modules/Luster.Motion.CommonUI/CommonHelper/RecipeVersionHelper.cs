using Luster.Common.Tools;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel.Dialogs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Luster.Motion.CommonUI
{
    public static class RecipeVersionHelper
    {
        public static void UpdateRecipeVersion(string versionNo, List<string> contents, string fileName, ProjectInfo projInfo, string modifiedBy = null, string modifiedTime = null)
        {
            string recipeVersionPath = projInfo?.FullName;
            if (string.IsNullOrEmpty(recipeVersionPath)) return;
            recipeVersionPath = recipeVersionPath.Substring(0, recipeVersionPath.LastIndexOf(@"\"));
            string basePath = Path.Combine(recipeVersionPath, "Config");

            string filePath = Path.Combine(basePath, fileName);
            VersionConfig config;

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                config = JsonConvert.DeserializeObject<VersionConfig>(json) ?? new VersionConfig { Versions = new List<VersionModel>() };
            }
            else
            {
                config = new VersionConfig { Versions = new List<VersionModel>() };
            }

            // 将修改人和修改时间追加到变更说明最后
            if (!string.IsNullOrEmpty(modifiedBy))
            {
                contents.Add($"修改人：{modifiedBy}");
            }
            if (!string.IsNullOrEmpty(modifiedTime))
            {
                contents.Add($"修改时间：{modifiedTime}");
            }

            // 若版本号已存在，将变更内容追加到已有条目，序号顺延；否则新增条目
            var existing = config.Versions.Find(v => v.Version == versionNo);
            if (existing != null)
            {
                // 找到已有内容中最大的序号
                int maxIndex = 0;
                foreach (var line in existing.Contents)
                {
                    var match = Regex.Match(line, @"^(\d+)[、.：:]\s*");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int num) && num > maxIndex)
                    {
                        maxIndex = num;
                    }
                }

                // 获取已有序号的位宽（如 "01" 宽度为2），用于零填充
                int indexWidth = 1;
                foreach (var line in existing.Contents)
                {
                    var mw = Regex.Match(line, @"^(\d+)[、.：:]\s*");
                    if (mw.Success && mw.Groups[1].Value.Length > indexWidth)
                    {
                        indexWidth = mw.Groups[1].Value.Length;
                    }
                }

                // 将新内容的序号顺延，保持零填充位宽
                var renumbered = new List<string>();
                foreach (var line in contents)
                {
                    var match = Regex.Match(line, @"^(\d+)[、.：:]\s*(.*)");
                    if (match.Success)
                    {
                        maxIndex++;
                        renumbered.Add($"{maxIndex.ToString().PadLeft(indexWidth, '0')}、{match.Groups[2].Value}");
                    }
                    else
                    {
                        renumbered.Add(line);
                    }
                }
                existing.Contents.AddRange(renumbered);
            }
            else
            {
                var newVersion = new VersionModel
                {
                    Version = versionNo,
                    Contents = contents
                };
                config.Versions.Insert(0, newVersion);
            }

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            string output = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(filePath, output);
        }
    }
}
