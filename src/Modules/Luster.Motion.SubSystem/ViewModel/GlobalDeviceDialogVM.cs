using Luster.Motion.EditorUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Luster.Motion.SubSystem.ViewModels
{
    public class DeviceDialogViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<GlobalVar> _boolVariables;
        private GlobalVar _selectedVariable;
        private string _searchText;

        public ObservableCollection<GlobalVar> BoolVariables
        {
            get => _boolVariables;
            set
            {
                _boolVariables = value;
                OnPropertyChanged(nameof(BoolVariables));
                OnPropertyChanged(nameof(FilteredVariables));
            }
        }

        public ObservableCollection<GlobalVar> FilteredVariables
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                    return BoolVariables;

                var filtered = BoolVariables.Where(v =>
                    v.Name.Contains(SearchText) ||
                    (v.Key?.Contains(SearchText) == true));

                return new ObservableCollection<GlobalVar>(filtered);
            }
        }

        public GlobalVar SelectedVariable
        {
            get => _selectedVariable;
            set
            {
                _selectedVariable = value;
                OnPropertyChanged(nameof(SelectedVariable));
                OnPropertyChanged(nameof(IsVariableSelected));
            }
        }
        public ObservableCollection<GlobalVar> SelectedVariables
        {
            get
            {
                var selected = BoolVariables?.Where(v => v.IsSelected).ToList() ?? new List<GlobalVar>();
                return new ObservableCollection<GlobalVar>(selected);
            }
        }
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                OnPropertyChanged(nameof(FilteredVariables));
            }
        }

        public bool IsVariableSelected => SelectedVariable != null;
        public bool HasSelectedVariables => SelectedVariables?.Any() == true;
        public ICommand SelectCommand { get; }
        public ICommand SearchCommand { get; }

        public DeviceDialogViewModel()
        {
            SelectCommand = new RelayCommand(OnSelect);
            SearchCommand = new RelayCommand(OnSearch);

            // 加载全局变量中的 bool 类型变量
            LoadBoolGlobalVariables();
        }

        private void LoadBoolGlobalVariables()
        {
            BoolVariables = new ObservableCollection<GlobalVar>();

            try
            {

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载全局变量时出错: {ex.Message}");
            }

            OnPropertyChanged(nameof(FilteredVariables));
        }

        // 添加一个方法来设置数据
        public void SetGlobalVariables(IEnumerable<GlobalVar> globalVars)
        {
            if (globalVars != null)
            {
                foreach (var var in globalVars)
                {
                    var.IsSelected = false;
                }

                BoolVariables = new ObservableCollection<GlobalVar>(globalVars);
                OnPropertyChanged(nameof(FilteredVariables));
                OnPropertyChanged(nameof(SelectedVariables));
                OnPropertyChanged(nameof(HasSelectedVariables));
            }
        }
        public void OnSelectionChanged()
        {
            OnPropertyChanged(nameof(SelectedVariables));
            OnPropertyChanged(nameof(HasSelectedVariables));
        }


        private void OnSelect(object parameter)
        {
            if (parameter is GlobalVar variable)
            {
                SelectedVariable = variable;
            }
        }

        private void OnSearch(object parameter)
        {
            OnPropertyChanged(nameof(FilteredVariables));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }
    }
}