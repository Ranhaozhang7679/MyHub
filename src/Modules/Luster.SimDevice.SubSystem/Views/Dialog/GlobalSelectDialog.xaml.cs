using System;
using System.Collections.Generic;
using System.Windows;
using Luster.SimDevice.SubSystem.ViewModel;

namespace Luster.SimDevice.SubSystem.Views.Dialog
{
    public partial class GlobalSelectDialog : Window
    {
        public GlobalSelectDialog(List<GlobalVarWrapper> availableVariables)
        {
            InitializeComponent();

            var viewModel = new ViewModel.Dialog.GlobalSelectDialogVM(availableVariables);
            viewModel.CloseAction = (result, selectedVars) =>
            {
                if (result)
                {
                    SelectedVariables = selectedVars;
                    DialogResult = true;
                }
                else
                {
                    DialogResult = false;
                }
                Close();
            };

            this.DataContext = viewModel;
        }

        /// <summary>
        /// 选择的变量列表
        /// </summary>
        public List<GlobalVarWrapper> SelectedVariables { get; private set; }
    }
}