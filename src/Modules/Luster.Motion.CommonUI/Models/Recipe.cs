#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Recipe
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Models
* 文 件 名:       Recipe.cs
* 创建时间:       2022/5/31 14:49:20
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f42d1d61-603c-467e-9b29-101a112f3c39
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/31 14:49:20
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Microsoft.VisualBasic.Devices;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.CommonUI.Models
{
    /// <summary>
    /// 配方类
    /// </summary>
    public class Recipe : BindableBase, IXMLParser
    {
        /// <summary>
        /// 配方名称
        /// </summary>
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        /// <summary>
        /// 工程信息
        /// </summary>
        private ProjectInfo _projInfo;
        public ProjectInfo ProjInfo { get => _projInfo; set => SetProperty(ref _projInfo, value); }

        /// <summary>
        /// 配方是否激活
        /// </summary>
        private bool _isActive;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

        /// <summary>
        /// 获取完整名称
        /// </summary>
        /// <returns></returns>
        public string GetFullname()
        {
            return Path.Combine(ProjInfo.ProjPath, Name, $"{Name}.recipe");
        }

        public string GetFullData()
        {
            return Path.Combine(ProjInfo.ProjPath, Name, $"{Name}.data");
        }

        public string GetDbDir()
        {
            return Path.Combine(ProjInfo.ProjPath, Name, "DB");
        }

        /// <summary>
        /// 获取配方对的路径
        /// </summary>
        /// <returns></returns>
        public string GetRecipePath()
        {
            return Path.Combine(ProjInfo.ProjPath, Name);
        }

        /// <summary>
        /// 新建配方
        /// </summary>
        /// <param name="recipeName"></param>
        /// <param name="recipePath"></param>
        public static Recipe NewRecipe(string recipeName, ProjectInfo p)
        {
            var recipe = new Recipe()
            {
                Name = recipeName,
                ProjInfo = p
            };

            if (!Directory.Exists(recipe.GetRecipePath()))
            {
                Directory.CreateDirectory(recipe.GetRecipePath());
            }
            if (!File.Exists(recipe.GetFullname()))//判断.recipe是否存在
            {
                // 生成一个空的Recipe对象
                XElement xRoot = new XElement("Recipe", new XAttribute("Name", recipeName));//创建节点
                xRoot.Save(recipe.GetFullname());
            }
            return recipe;
        }

        /// <summary>
        /// 修改配方
        /// </summary>
        /// <param name="oldrecipe">原配方名</param>
        /// <param name="recipeName">新配方名</param>
        /// <param name="recipePath">配方路径</param>
        public void UpdateRecipe(string oldrecipe, string recipeName)
        {
            var src = ProjInfo.RecipeList.FirstOrDefault(u => u.Name == oldrecipe);
            if (src != null)
            {
                src.Name = recipeName;

                Computer computer = new Computer();
                string recipepath = GetFullname();
                computer.FileSystem.RenameDirectory(GetRecipePath(), recipeName);
            }
        }

        #region 实现导入导出
        public XElement ExportXml()
        {
            XElement xrecipe = new XElement("Recipe",
                   new XAttribute("Name", Name),
                   new XAttribute("IsActive", IsActive));

            return xrecipe;
        }

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Name", (name) =>
            {
                Name = name;
            });

            xElement.GetAttribute("IsActive", (isActive) =>
            {
                IsActive = Convert.ToBoolean(isActive);
            });
        }
        #endregion

        /// <summary>
        /// 获取保存路径
        /// </summary>
        /// <returns></returns>
        public string GetSavePath()
        {
            string dir = Path.Combine(ProjInfo.ProjPath, ".luster", Name);
            if (!Directory.Exists(dir))
            {
                Luster.Common.Tools.FolderTool.CreateDir(dir, true);
            }

            return dir;
        }
    }
}