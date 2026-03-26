using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Views;
using Luster.Authorization.Client.Helper;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DC.Authorization.WPF.ViewModels
{
    public class CreateRoleViewModel : BindableBase, IDialogAware
    {
        private IRoleRepository _roleRepository;

        public CreateRoleViewModel(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public event Action<IDialogResult> RequestClose;

        public string Title { get; set; } = "新建角色";
        private DelegateCommand _createCommand;
        public ICommand CreateCommand => _createCommand ??= new DelegateCommand(Create);

        private void Create()
        {
            Role role = new Role();
            role.Level = SelectedLevel;
            role.Name = Name;
            role.IsAdmin = false;
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
        public void OnDialogOpened(IDialogParameters parameters) { }

        private ObservableCollection<int> _levels;
        public ObservableCollection<int> Levels { get => _levels; set => SetProperty(ref _levels, value); }

        private int _selectedLevel;
        public int SelectedLevel { get => _selectedLevel; set => SetProperty(ref _selectedLevel, value); }

        private string _name;
        //private DialogCloseListener _requestClose = new DialogCloseListener();

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        //public DialogCloseListener RequestClose => _requestClose;
    }
}
