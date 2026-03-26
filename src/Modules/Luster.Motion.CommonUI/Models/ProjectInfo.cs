using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.FileIO;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Linq;

namespace Luster.Motion.CommonUI.Models
{
    /// <summary>
    /// 工程对象
    /// </summary>
    public class ProjectInfo : BindableBase, IXMLParser
    {
        /// <summary>
        /// 工程全名
        /// </summary>
        private string _fullname;
        public string FullName { get => _fullname; set => SetProperty(ref _fullname, value); }

        /// <summary>
        /// 工程路径
        /// </summary>
        private string _projPath;
        public string ProjPath { get => _projPath; set => SetProperty(ref _projPath, value); }

        /// <summary>
        /// 工程名称
        /// </summary>
        private string _projName;
        public string ProjName { get => _projName; set => SetProperty(ref _projName, value); }

        /// <summary>
        /// 工程时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 配方列表
        /// </summary>
        private List<Recipe> _recipeList;
        public List<Recipe> RecipeList { get => _recipeList; set => SetProperty(ref _recipeList, value); }

        /// <summary>
        /// 工程是否激活
        /// </summary>
        private bool _isActive;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

        public ProjectInfo()
        {
            RecipeList = new List<Recipe>();
        }

        /// <summary>
        /// 新建工程
        /// </summary>
        /// <param name="projPath"></param>
        /// <param name="projName"></param>
        /// <param name="recipeName"></param>
        /// <returns></returns>
        /// <exception cref="FriendlyException"></exception>
        public static ProjectInfo NewProject(string projPath, string projName, string recipeName)
        {
            if (string.IsNullOrEmpty(projPath.Trim()))
            {
                throw new FriendlyException("工程路径不能为空！");
            }

            if (string.IsNullOrEmpty(projName.Trim()))
            {
                throw new FriendlyException("工程名不能为空！");
            }

            if (string.IsNullOrEmpty(recipeName.Trim()))
            {
                throw new FriendlyException("配方名不能为空");
            }

            // 工程名称
            string pPath = Path.Combine(projPath, projName);

            if (!Directory.Exists(pPath))
            {
                Directory.CreateDirectory(pPath);
            }

            var newProj = new ProjectInfo()
            {
                ProjName = projName,
                FullName = Path.Combine(projPath, projName, $"{projName}.msln"),
                Time = DateTime.Now,
                ProjPath = pPath
            };


            // 构建recipe
            newProj.AddNewRecipe(recipeName);
            return newProj;
        }

        /// <summary>
        /// 新建Recipe
        /// </summary>
        /// <param name="recipeName"></param>
        public void AddNewRecipe(string recipeName)
        {
            Recipe recipe = Recipe.NewRecipe(recipeName, this);
            List<Recipe> listrecipe = new List<Recipe>();
            listrecipe = new List<Recipe>(RecipeList.ToArray());
            listrecipe.Add(recipe);
            RecipeList = listrecipe;
        }

        public static ProjectInfo OpenExistProj(string projName, string projPath)
        {
            if (string.IsNullOrEmpty(projPath.Trim()))
            {
                throw new FriendlyException("工程路径不能为空！");
            }

            if (string.IsNullOrEmpty(projName.Trim()))
            {
                throw new FriendlyException("工程名不能为空！");
            }

            var newProj = new ProjectInfo()
            {
                ProjName = projName,
                FullName = projPath,
                Time = DateTime.Now,
                ProjPath = Path.GetDirectoryName(projPath),
            };
            // 构建recipe
            newProj.RecipeList = newProj.BuildRecipeList(projPath, newProj);
            return newProj;
        }



        /// <summary>
        /// 删除配方
        /// </summary>
        /// <param name="recipeName"></param>
        public void RemoveRecipe(string recipeName)
        {
            List<Recipe> listrecipe = new List<Recipe>();
            listrecipe = new List<Recipe>(RecipeList.ToArray());
            if (listrecipe.Any(u => u.Name == recipeName))
            {
                listrecipe.RemoveAll(u => u.Name == recipeName);
                RecipeList = listrecipe;
            }
        }

        #region 导入导出实现

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Name", (name) =>
            {
                ProjName = name;
            });

            xElement.GetAttribute("Path", (path) =>
            {
                FullName = path;
            });
            xElement.GetAttribute("Path", (path) =>
            {
                ProjPath = Path.GetDirectoryName(path);
            });
            xElement.GetAttribute("Time", (time) =>
            {
                Time = Convert.ToDateTime(time);
            });
            xElement.GetAttribute("IsActive", (isActive) =>
            {
                IsActive = Convert.ToBoolean(isActive);
            });
        }

        public XElement ExportXml()
        {
            XElement proj = new XElement("Solution",
            new XAttribute("Name", ProjName),
            new XAttribute("Path", FullName),
            new XAttribute("Time", Time),
            new XAttribute("IsActive", IsActive));

            return proj;
        }

        #endregion


        /// <summary>
        /// 构建当前配方列表
        /// </summary>
        /// <param name="slnPath"></param>
        public List<Recipe> BuildRecipeList(string slnPath, ProjectInfo proj)
        {
            RecipeList = new List<Recipe>();
            if (File.Exists(slnPath))
            {
                XElement xRoot = XElement.Load(slnPath);
                foreach (var item in xRoot.Elements("Recipe"))
                {
                    Recipe data = new Recipe();
                    data.ParserXml(item);
                    data.ProjInfo = proj;
                    RecipeList.Add(data);
                }
            }

            return RecipeList;
        }

        /// <summary>
        /// 获取项目配置文件目录
        /// </summary>
        /// <returns></returns>
        public string GetProjConfigDir()
        {
            string message = string.Empty;

            return Path.Combine(ProjPath, "Config");
        }
    }
}
