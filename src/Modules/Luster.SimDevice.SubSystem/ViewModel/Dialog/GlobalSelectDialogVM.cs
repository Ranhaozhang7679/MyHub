using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class GlobalSelectDialogVM : BindableBase
    {
        private ObservableCollection<GlobalVarWrapper> _allVariables;
        private ObservableCollection<GlobalVarWrapper> _filteredVariables;
        private string _searchText;

        public GlobalSelectDialogVM(List<GlobalVarWrapper> availableVariables)
        {
            _allVariables = new ObservableCollection<GlobalVarWrapper>();

            foreach (var item in availableVariables)
            {
                var newItem = new GlobalVarWrapper
                {
                    Name = item.Name,
                    Key = item.Key,
                    Type = item.Type,
                    TypeName = item.TypeName,
                    Value = item.Value,
                    DefaultV = item.DefaultV,
                    IsSelected = false
                };
                _allVariables.Add(newItem);
            }

            FilteredVariables = new ObservableCollection<GlobalVarWrapper>(_allVariables);

            SearchCommand = new DelegateCommand(ExecuteSearch);
            ConfirmCommand = new DelegateCommand(ExecuteConfirm);
            CancelCommand = new DelegateCommand(ExecuteCancel);
        }

        public ObservableCollection<GlobalVarWrapper> FilteredVariables
        {
            get => _filteredVariables;
            set => SetProperty(ref _filteredVariables, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ExecuteSearch();
            }
        }

        public ICommand SearchCommand { get; private set; }
        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public Action<bool, List<GlobalVarWrapper>> CloseAction { get; set; }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredVariables = new ObservableCollection<GlobalVarWrapper>(_allVariables);
            }
            else
            {
                var filtered = _allVariables.Where(v =>
                    (v.Name != null && v.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (v.Key != null && v.Key.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0));

                FilteredVariables = new ObservableCollection<GlobalVarWrapper>(filtered);
            }
        }

        private void ExecuteConfirm()
        {
            var selectedVars = _allVariables.Where(v => v.IsSelected).ToList();
            CloseAction?.Invoke(true, selectedVars);
        }

        private void ExecuteCancel()
        {
            CloseAction?.Invoke(false, null);
        }
    }
}