using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class CreateProjectDialogVM : MotionDialogVM
    {
        private ICommonBus _commonBus;
        public CreateProjectDialogVM(ICommonBus commonBus)
        {
            this._commonBus = commonBus;
        }

        /// <summary>
        /// 工程名称
        /// </summary>
        private string _projectName;
        [Required]
        public string ProjectName
        {
            get { return _projectName; }
            set { SetProperty(ref _projectName, value); }
        }

        /// <summary>
        /// 工程路径
        /// </summary>
        private string _projectPath;
        [Required]
        public string ProjectPath
        {
            get { return _projectPath; }
            set { SetProperty(ref _projectPath, value); }
        }

        /// <summary>
        /// 配方名称
        /// </summary>
        private string _recipe;
        [Required]
        public string Recipe
        {
            get { return _recipe; }
            set { SetProperty(ref _recipe, value); }
        }
        /// <summary>
        /// 是否编辑
        /// </summary>
        private bool _isEnable;
        public bool IsEnable
        {
            get { return _isEnable; }
            set { SetProperty(ref _isEnable, value); }
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
        /// 选择路径
        /// </summary>
        private DelegateCommand _selectPathCommand;
        public DelegateCommand SelectPathCommand => _selectPathCommand ?? (_selectPathCommand = new DelegateCommand(() =>
        {
            Luster.WindowsAPICodePack.Dialogs.CommonOpenFileDialog dialog = new WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ProjectPath = dialog.FileName;
            }
        }));

        /// <summary>
        /// 配方变换
        /// </summary>
        private DelegateCommand _recipeChangeCommand;
        public DelegateCommand RecipeChangeCommand => _recipeChangeCommand ?? (_recipeChangeCommand = new DelegateCommand(() =>
        {
            if (IsEnable)
            {
                ProjectName = Recipe;
            }
        }));



        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            ProjectInfo info = new ProjectInfo();
            if (IsProjExist(ProjectName))
            {
                if (Title != Luster.Motion.Assests.Langs.LangProvider.GetLang("NewRecipe"))
                {
                    throw new Exception($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("ProjectAlreadyExists")}！");
                }
            }
            result.Parameters.Add("ProjectName", ProjectName);
            result.Parameters.Add("ProjectPath", ProjectPath);
            result.Parameters.Add("Recipe", Recipe);
        }

        public bool IsProjExist(string ProjName)
        {
            bool isExist = false;
            bool isContainer=_commonBus.ProjectList.Any(u => u.ProjName.ToLower() == ProjName.ToLower());
            if (isContainer)
            {
                var proj = _commonBus.ProjectList.FirstOrDefault(u => u.ProjName == ProjName);
                if (proj!=null)
                {
                    if (Directory.Exists(proj.ProjPath))
                    {
                        isExist = true;
                    }
                    else
                    {
                        isExist = false;
                        _commonBus.ProjectList.Remove(proj);
                    }       
                }
            }
            return isExist;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var tvalue))
            {
                Title = tvalue;
                if (Title == Luster.Motion.Assests.Langs.LangProvider.GetLang("NewRecipe"))
                {
                    IsEnable = false;
                }
                else
                {
                    IsEnable = true;
                }
            }
            else
            {
                Title = string.Empty;
            }
            if (parameters.TryGetValue<string>("ProjectName", out var value))
            {
                ProjectName = value;
            }
            else
            {
                ProjectName = string.Empty;
            }

            if (parameters.TryGetValue<string>("ProjectPath", out var pathvalue))
            {
                ProjectPath = pathvalue;
            }
            else
            {
                ProjectPath = string.Empty;
            }
            if (parameters.TryGetValue<string>("Recipe", out var recipevalue))
            {
                Recipe = recipevalue;
            }
            else
            {
                Recipe = string.Empty;
            }
        }
    }

}
