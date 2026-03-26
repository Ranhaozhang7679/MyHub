using Luster.Motion.EditorUI.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Luster.Controls.Wpf;
using Luster.Motion.EditorUI.Extensions;
using Microsoft.VisualBasic.Devices;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.CommonUI;
using HandyControl.Data;
using Luster.Motion.CommonUI.Models;
using System.Windows.Forms;
using Luster.WindowsAPICodePack.Dialogs;
using System.Windows;
using Luster.Common.Assets;
using System.Security.AccessControl;
using Prism.Events;
using Luster.Motion.CommonUI.Events;
using System.Threading;
using Luster.Common.DataStruct;
using Luster.Motion.TaskFlow.Engine;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class ProjectContentVM : MotionVM
    {
        /// <summary>
        /// 激活配方
        /// </summary>
        protected Recipe activeRecipe;

        /// <summary>
        /// 对话框服务
        /// </summary>
        private IDialogService _dialogService;

        private IMotionController _motionController;

        public ProjectContentVM(IDialogService dialogService, ICommonBus commonBus,IMotionController motionController) : base(commonBus)
        {
            _dialogService = dialogService;
            _motionController = motionController;
            ProjectList = new ObservableCollection<ProjectInfo>();
            BuildProjects();
        }


        protected override void RegisterEvent(IEventAggregator bus)
        {
            //bus.GetEvent<ProjectOpenEvent>().Subscribe(model =>
            //{
            //    if (model != null)
            //    {
            //        OpenProjCommand.Execute(model.FullName);
            //    }
            //});

            bus.GetEvent<RecipeOpenEvent>().Subscribe(recipe =>
            {
                activeRecipe = recipe;
                ProjectCurrent = recipe.ProjInfo;
                commonBus.EventBus.GetEvent<ProjectNameEvent>().Publish(ProjectCurrent.ProjName);
            });
        }

        #region 属性
        /// <summary>
        /// 当前工程
        /// </summary>
        private ProjectInfo _projectCurrent;
        public ProjectInfo ProjectCurrent
        {
            get { return _projectCurrent; }
            set { SetProperty(ref _projectCurrent, value); }
        }

        /// <summary>
        /// 工程列表
        /// </summary>
        private ObservableCollection<ProjectInfo> _projectList;
        public ObservableCollection<ProjectInfo> ProjectList
        {
            get { return _projectList; }
            set { SetProperty(ref _projectList, value); }
        }

        /// <summary>
        /// 当前工程的工程名称
        /// </summary>
        private string _projTitle;
        public string ProjTitle
        {
            get { return _projTitle; }
            set { SetProperty(ref _projTitle, value); }
        }

        /// <summary>
        /// 当前工程的创建时间
        /// </summary>
        private string _projTime;
        public string ProjTime
        {
            get { return _projTime; }
            set { SetProperty(ref _projTime, value); }
        }

        /// <summary>
        /// 最大页码数
        /// </summary>
        private int _maxPageCount;
        public int MaxPageCount
        {
            get { return _maxPageCount; }
            set { SetProperty(ref _maxPageCount, value); }
        }

        /// <summary>
        /// 当前显示页码
        /// </summary>
        private int _pageIndex;
        public int PageIndex
        {
            get { return _pageIndex; }
            set { SetProperty(ref _pageIndex, value); }
        }

        /// <summary>
        /// 工程查询内容
        /// </summary>
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); }
        }
        /// <summary>
        /// 是否显示
        /// </summary>
        private Visibility _isVisible;
        public Visibility IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        #endregion

        /// <summary>
        /// 获取工程列表
        /// </summary>
        private void BuildProjects()
        {
            ProjectList.Clear();
            if (commonBus.ProjectList != null & commonBus.ProjectList.Count > 0)
            {

                List<ProjectInfo> ListOrder = commonBus.ProjectList.OrderByDescending(n => n.Time).ToList();
                MaxPageCount = Convert.ToInt32(Math.Ceiling((double)(ListOrder.Count) / 6));
                List<ProjectInfo> ListResult = ListOrder.Skip(0).Take(6).ToList();
                ProjectList.Clear();
                ListResult.ForEach(x => ProjectList.Add(x));
                BuildActiveProj();
                IsVisible = Visibility.Visible;
            }
            else
            {
                IsVisible = Visibility.Hidden;
            }
        }

        /// <summary>
        /// 构建激活工程
        /// </summary>
        private void BuildActiveProj()
        {
            if (ProjectList != null & ProjectList.Count > 0)
            {
                var proj = ProjectList.FirstOrDefault(u => u.IsActive == true);
                if (proj != null)
                {
                    ProjectCurrent = proj;
                }
                else
                {
                    ProjectCurrent = ProjectList[0];
                }
                commonBus.ProjInfo = ProjectCurrent;
                commonBus.EventBus.GetEvent<ProjectNameEvent>().Publish(ProjectCurrent.ProjName);
            }
            else
            {
                commonBus.EventBus.GetEvent<ProjectNameEvent>().Publish("");
            }
        }

        #region 新建 删除 激活 打开已有配方
        /// <summary>
        /// 新建配方
        /// </summary>
        private DelegateCommand<string> _addRecipeCommand;
        public DelegateCommand<string> AddRecipeCommand => _addRecipeCommand ?? (_addRecipeCommand = new DelegateCommand<string>((projname) =>
        {
            string slnPath = "";
            var proj = commonBus.ProjectList.FirstOrDefault(u => u.ProjName == projname);
            if (proj != null && proj.FullName != null)
            {
                slnPath = Path.GetDirectoryName(proj.FullName);
                _dialogService.ShowCreateProject(Luster.Motion.Assests.Langs.LangProvider.GetLang("NewRecipe"), projname, slnPath, (r) =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("Recipe", out string recipeText))
                        {
                            if (proj.RecipeList.Any(u => u.Name == recipeText))
                            {
                                throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("Recipe")}:{recipeText}{Luster.Motion.Assests.Langs.LangProvider.GetLang("AlreadyExists")}!");
                            }
                            else
                            {
                                proj.AddNewRecipe(recipeText);//添加配方
                                commonBus.IsNeedSave = true;
                            }
             
                        }
                    }
                });
            }
        }));

        /// <summary>
        /// 删除配方
        /// </summary>
        private DelegateCommand<Recipe> _deleteRecipeCommand;
        public DelegateCommand<Recipe> DeleteRecipeCommand => _deleteRecipeCommand ?? (_deleteRecipeCommand = new DelegateCommand<Recipe>((s) =>
        {
            s.ProjInfo.RemoveRecipe(s.Name);
            commonBus.IsNeedSave = true;
        }));


        /// <summary>
        /// 激活配方
        /// </summary>
        private DelegateCommand<Recipe> _activeRecipeCommand;
        public DelegateCommand<Recipe> ActiveRecipeCommand => _activeRecipeCommand ?? (_activeRecipeCommand = new DelegateCommand<Recipe>((s) =>
        {
            commonBus.OnActiveRecipe(s);
            commonBus.IsNeedSave = true;
        }));


        /// <summary>
        /// 添加已有配方
        /// </summary>
        private DelegateCommand<string> _addExistRecipeCommand;
        public DelegateCommand<string> AddExistRecipeCommand => _addExistRecipeCommand ?? (_addExistRecipeCommand = new DelegateCommand<string>((slnpath) =>
        {
            Luster.WindowsAPICodePack.Dialogs.CommonOpenFileDialog dialog = new WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            dialog.IsFolderPicker = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (ProjectCurrent != null & ProjectCurrent.ProjName != null)
                {
                    string recipePath = dialog.FileName;
                    if (Path.GetExtension(recipePath) == ".recipe")
                    {
                        string reciprname = Path.GetFileNameWithoutExtension(recipePath);

                        if (ProjectCurrent.RecipeList.Any(u => u.Name == reciprname))
                        {
                            throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("Recipe")}:{reciprname}{Luster.Motion.Assests.Langs.LangProvider.GetLang("AlreadyExists")}!");
                        }

                        var proj = commonBus.ProjectList.FirstOrDefault(t => t.ProjName == ProjectCurrent.ProjName);
                        proj.AddNewRecipe(reciprname);
                        commonBus.IsNeedSave = true;
                        ProjectCurrent = proj;

                        string addrecipePath = Path.GetDirectoryName(recipePath);
                        string desDir = Path.Combine(Path.GetDirectoryName(slnpath), reciprname);
                        string filename = Path.GetFileName(addrecipePath);
                        if (filename == reciprname)
                        {
                            if (addrecipePath != desDir)
                            {
                                FileHelper.CopyFileAndDir(addrecipePath, desDir);
                            }
               
                        }
                        else
                        {
                            FileInfo file = new FileInfo(recipePath);
                            string newName = file.Name;
                            file.CopyTo(recipePath, true);
                        }
                    }
                    else
                    {
                        throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("RecipeFormatWrong")}！");
                    }                 
                }
            }
        }));

        /// <summary>
        /// 配方另存为
        /// </summary>
        private DelegateCommand<Recipe> _receipeSaveAsCommand;
        public DelegateCommand<Recipe> ReceipeSaveAsCommand => _receipeSaveAsCommand ?? (_receipeSaveAsCommand = new DelegateCommand<Recipe>((o) =>
        {
            if (o.ProjInfo.FullName != null)
            {
                Luster.WindowsAPICodePack.Dialogs.CommonSaveFileDialog dialog = new WindowsAPICodePack.Dialogs.CommonSaveFileDialog();
                string SourceFilePath = Path.Combine(o.ProjInfo.ProjPath, o.Name);
                dialog.DefaultFileName = Path.GetFileName(SourceFilePath);
                if (Directory.Exists(SourceFilePath))
                {
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        string targetFilePath = dialog.FileName;
                        FileInfo filedb = new FileInfo(SourceFilePath);
                        CopyFolder(SourceFilePath, targetFilePath);

                    }
                }
            }
        }));

        #endregion

        #region 工程查询方法
        /// <summary>
        /// 工程查询方法
        /// </summary>
        private DelegateCommand _searchCommand;
        public DelegateCommand SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand(() =>
        {
            if (SearchText != "" & SearchText != null)
            {
                if (ProjectList != null)
                {
                    List<ProjectInfo> ListOrder = commonBus.ProjectList.OrderByDescending(n => n.Time).ToList();
                    List<ProjectInfo> ListResult = ListOrder.Where(t => t.ProjName.Contains(SearchText)).ToList();
                    ProjectList.Clear();
                    ListResult.ForEach(x => ProjectList.Add(x));
                    MaxPageCount = Convert.ToInt32(Math.Ceiling((double)(commonBus.ProjectList.Count) / 6));
                }
            }
            else
            {
                BuildProjects();
            }
        }));



        /// <summary>
        /// 页码改变命令
        /// </summary>
        private DelegateCommand<object> _pageUpdatedCommand;
        public DelegateCommand<object> PageUpdatedCommand => _pageUpdatedCommand ?? (_pageUpdatedCommand = new DelegateCommand<object>((s) =>
        {
            FunctionEventArgs<int> pageinfo = (FunctionEventArgs<int>)s;
            List<ProjectInfo> ListOrder = commonBus.ProjectList.OrderByDescending(n => n.Time).ToList();
            List<ProjectInfo> ListResult = ListOrder.Skip((pageinfo.Info - 1) * 6).Take(6).ToList();
            ProjectList.Clear();
            ListResult.ForEach(x => ProjectList.Add(x));
            PageIndex = pageinfo.Info;
        }));

        #endregion

        #region 新建 打开 删除 激活工程
        /// <summary>
        /// 新建工程
        /// </summary>
        private DelegateCommand _addProjCommand;
        public DelegateCommand AddProjCommand => _addProjCommand ?? (_addProjCommand = new DelegateCommand(() =>
        {
            _dialogService.ShowCreateProject(Luster.Motion.Assests.Langs.LangProvider.GetLang("CreateProject"), "Project", "D:\\Motion", (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    string ProjectName = r.Parameters.GetValue<string>("ProjectName");
                    string rootPath = r.Parameters.GetValue<string>("ProjectPath");
                    string Recipe = r.Parameters.GetValue<string>("Recipe");
                    if (ProjectName != "" & rootPath != "")
                    {
                        commonBus.AddProject(ProjectName, rootPath, Recipe);
                        var proj = commonBus.ProjectList.FirstOrDefault(u => u.ProjName == ProjectName);
                        if (proj != null & proj.RecipeList != null)
                        {
                            if (proj.RecipeList.Count == 1)
                            {
                                commonBus.OnActiveRecipe(proj.RecipeList[0]);
                            }
                        }
                        BuildProjects();
                    }
                }
            });
        }));

        /// <summary>
        /// 打开项目
        /// </summary>
        private DelegateCommand<string> _openProjCommand;
        public DelegateCommand<string> OpenProjCommand => _openProjCommand ?? (_openProjCommand = new DelegateCommand<string>((projpath) =>
        {
            if (projpath != null)
            {
                ActiveProj(projpath);
            }
            else
            {
                Luster.WindowsAPICodePack.Dialogs.CommonOpenFileDialog dialog = new WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
                dialog.IsFolderPicker = false;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string slnPath = dialog.FileName;
                    var extension =Path.GetExtension(slnPath);
                    if (extension == ".msln")
                    {
                        if (!IsProjExist(Path.GetFileNameWithoutExtension(slnPath)))
                        {
                            commonBus.OpenExistProj(Path.GetFileNameWithoutExtension(slnPath), slnPath);
                        }
                        else
                        {
                            throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("ProjectAlreadyExists")}！");
                        }
                        BuildProjects();
                    }
                    else
                    {
                        throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("ProjectFileError")}！");
                    }

                }
            }
        }));

        public bool IsProjExist(string ProjName)
        {
            return commonBus.ProjectList.Any(u => u.ProjName.ToLower() == ProjName.ToLower());
        }



        /// <summary>
        /// 删除项目
        /// </summary>
        private DelegateCommand<string> _deleteProjCommand;
        public DelegateCommand<string> DeleteProjCommand => _deleteProjCommand ?? (_deleteProjCommand = new DelegateCommand<string>((projpath) =>
        {
            _dialogService.ShowConfirm($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("IsDeleteCurrentProject")}！", (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (activeRecipe != null)
                    {
                        if (commonBus.CurrentRecipe.ProjInfo.FullName != projpath)
                        {
                            commonBus.RemoveProject(projpath);
                        }
                        else
                        {
                            throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("CannotDeleteProjWhithRecipeActive")}！");
                        }
                    }
                    else
                    {
                        commonBus.RemoveProject(projpath);
                    }
                    BuildProjects();
                }
            });
        }));

        /// <summary>
        /// 激活工程
        /// </summary>
        /// <param name="slnPath"></param>
        private void ActiveProj(string slnPath)
        {
            var proj = commonBus.ProjectList.FirstOrDefault(u => u.FullName == slnPath);
            if (proj.RecipeList == null || proj.RecipeList.Count == 0)
            {
                throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("NoRecipeInProject")}!");
            }

            var activeRecipe = proj.RecipeList.FirstOrDefault(u => u.IsActive);
            if (activeRecipe == null)
            {
                activeRecipe = proj.RecipeList[0];
            }

            commonBus.OnActiveRecipe(activeRecipe);
        }


        /// <summary>
        /// 导出工程
        /// </summary>
        private DelegateCommand<string> _exportProjCommand;
        public DelegateCommand<string> ExportProjCommand => _exportProjCommand ?? (_exportProjCommand = new DelegateCommand<string>((projpath) =>
        {
            Luster.WindowsAPICodePack.Dialogs.CommonOpenFileDialog dialog = new WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            string SourceFilePath = Path.GetDirectoryName(projpath);
            dialog.DefaultFileName= Path.GetFileNameWithoutExtension(projpath);
            if (Directory.Exists(SourceFilePath))
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string targetFilePath = dialog.FileName;
                    FileInfo filedb = new FileInfo(SourceFilePath);
                    CopyFolder(SourceFilePath,targetFilePath);
                    
                }
            }
        }));
        #endregion


        /// <summary>
        /// 打开配方路径
        /// </summary>
        private DelegateCommand<Recipe> _openRecipePathCommand;
        public DelegateCommand<Recipe> OpenRecipePathCommand => _openRecipePathCommand ?? (_openRecipePathCommand = new DelegateCommand<Recipe>((o) =>
        {
            if (Directory.Exists(o.GetRecipePath()))
            {
                System.Diagnostics.Process.Start("explorer.exe", o.GetRecipePath());
            }
        }));

        /// <summary>
        /// 打开工程路径
        /// </summary>
        private DelegateCommand<Recipe> _openSlnPathCommand;
        public DelegateCommand<Recipe> OpenSlnPathCommand => _openSlnPathCommand ?? (_openSlnPathCommand = new DelegateCommand<Recipe>((o) =>
        {
            if (o.ProjInfo.FullName != null)
            {
                if (Directory.Exists(Path.GetDirectoryName(o.ProjInfo.FullName)))
                {
                    System.Diagnostics.Process.Start("explorer.exe", Path.GetDirectoryName(o.ProjInfo.FullName));
                }
            }
        }));


        #region 
        private DelegateCommand _backUpCommand;
        public DelegateCommand BackUpCommand => _backUpCommand ?? (_backUpCommand = new DelegateCommand(() =>
        {
            if (commonBus.ProjInfo == null) return;
            string projPath=commonBus.ProjInfo.ProjPath;
            if (!string.IsNullOrEmpty(projPath))
            {
                _motionController.ProjBackUp(projPath);
            }
            else
            {
                throw new FriendlyException($"{"未获取到当前激活工程的路径，请假查"}！");
            }
            
        }));
        #endregion



        /// <summary>
        /// 复制文件夹及内容
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="destFolder"></param>
        /// <returns></returns>
        public int CopyFolder(string sourceFolder, string destFolder)
        {
            try
            {
                //如果目标路径不存在,则创建目标路径
                if (!System.IO.Directory.Exists(destFolder))
                {
                    System.IO.Directory.CreateDirectory(destFolder);
                }
                //得到原文件根目录下的所有文件
                string[] files = System.IO.Directory.GetFiles(sourceFolder);
                foreach (string file in files)
                {
                    string name = System.IO.Path.GetFileName(file);
                    string dest = System.IO.Path.Combine(destFolder, name);
                    System.IO.File.Copy(file, dest);//复制文件
                }
                //得到原文件根目录下的所有文件夹
                string[] folders = System.IO.Directory.GetDirectories(sourceFolder);
                foreach (string folder in folders)
                {
                    string name = System.IO.Path.GetFileName(folder);
                    string dest = System.IO.Path.Combine(destFolder, name);
                    CopyFolder(folder, dest);//构建目标路径,递归复制文件
                }
                return 1;
            }
            catch (Exception e)
            {
                return -1;
            }
        }
    }
}
