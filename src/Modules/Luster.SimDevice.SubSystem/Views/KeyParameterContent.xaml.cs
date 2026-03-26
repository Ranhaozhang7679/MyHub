using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct;
using Luster.Motion.TaskFlow.Engine;
using Luster.SimDevice.SubSystem.ViewModel;
using Luster.TaskFlow.Motion.Interfaces;
using Microsoft.Win32;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Luster.SimDevice.SubSystem.Views
{
    /// <summary>
    /// KeyParameterContent.xaml 的交互逻辑
    /// </summary>
    public partial class KeyParameterContent : UserControl
    {
        private KeyParameterContentVM _viewModel;
        private KeyParameterSFCVM _sfcViewModel;
        private KeyParameterGlobalVM _globalViewModel;
        private IDeviceEngine _deviceEngine;
        private IEventAggregator _ea;
        private IMotionController _mController;
        private IMotionEngine _engine; 

        public KeyParameterContent()
        {
            InitializeComponent();

            try
            {
                _deviceEngine = ContainerLocator.Container.Resolve<IDeviceEngine>();
                _ea = ContainerLocator.Container.Resolve<IEventAggregator>();
                _mController = ContainerLocator.Container.Resolve<IMotionController>();
                _engine = ContainerLocator.Container.Resolve<IMotionEngine>(); 
            }
            catch (Exception ex)
            {
                
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel = DataContext as KeyParameterContentVM;
                if (_viewModel != null)
                {
                    _viewModel.OnViewLoaded();
                    StationComboBox.ItemsSource = _viewModel.StationList;
                    StationComboBox.SetBinding(ComboBox.SelectedItemProperty, new System.Windows.Data.Binding("SelectedStation")
                    {
                        Source = _viewModel
                    });
                }

                if (SFCContent.DataContext is KeyParameterSFCVM existingSfcVm)
                {
                    _sfcViewModel = existingSfcVm;
                }
                else
                {
                    _sfcViewModel = new KeyParameterSFCVM(_deviceEngine, _ea);
                    SFCContent.DataContext = _sfcViewModel;
                }

                _sfcViewModel.OnViewLoaded();

                SFCStationComboBox.ItemsSource = _sfcViewModel.StationList;
                SFCStationComboBox.SetBinding(ComboBox.SelectedItemProperty, new System.Windows.Data.Binding("SelectedStation")
                {
                    Source = _sfcViewModel
                });

                if (GlobalContent.DataContext is KeyParameterGlobalVM existingGlobalVm)
                {
                    _globalViewModel = existingGlobalVm;
                }
                else
                {
                    _globalViewModel = new KeyParameterGlobalVM(_engine, _ea, _mController);
                    GlobalContent.DataContext = _globalViewModel;
                }
                _globalViewModel.OnViewLoaded();

                UpdateButtonStyle(AELimitButton, SFCButton, GlobalButton);
                AELimitContent.Visibility = Visibility.Visible;
                SFCContent.Visibility = Visibility.Collapsed;
                GlobalContent.Visibility = Visibility.Collapsed;
                PDCAStationPanel.Visibility = Visibility.Visible;
                SFCStationPanel.Visibility = Visibility.Collapsed;
                PDCAExportButton.Visibility = Visibility.Visible;
                SFCExportButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StationComboBox.ItemsSource = null;
                StationComboBox.ClearValue(ComboBox.SelectedItemProperty);
                SFCStationComboBox.ItemsSource = null;
                SFCStationComboBox.ClearValue(ComboBox.SelectedItemProperty);
            }
            catch (Exception ex)
            {
                
            }
        }

        private void AELimitButton_Click(object sender, RoutedEventArgs e)
        {
            AELimitContent.Visibility = Visibility.Visible;
            SFCContent.Visibility = Visibility.Collapsed;
            GlobalContent.Visibility = Visibility.Collapsed;
            PDCAStationPanel.Visibility = Visibility.Visible;
            SFCStationPanel.Visibility = Visibility.Collapsed;
            PDCAExportButton.Visibility = Visibility.Visible;
            SFCExportButton.Visibility = Visibility.Collapsed;
            GlobalAddButton.Visibility = Visibility.Collapsed;

            if (_viewModel != null)
            {
                _viewModel.RefreshCommand?.Execute(null);
            }

            UpdateButtonStyle(AELimitButton, SFCButton, GlobalButton);
        }

        private void SFCButton_Click(object sender, RoutedEventArgs e)
        {
            AELimitContent.Visibility = Visibility.Collapsed;
            SFCContent.Visibility = Visibility.Visible;
            GlobalContent.Visibility = Visibility.Collapsed;
            PDCAStationPanel.Visibility = Visibility.Collapsed;
            SFCStationPanel.Visibility = Visibility.Visible;
            PDCAExportButton.Visibility = Visibility.Collapsed;
            SFCExportButton.Visibility = Visibility.Visible;
            GlobalAddButton.Visibility = Visibility.Collapsed;

            if (_sfcViewModel != null)
            {
                _sfcViewModel.RefreshCommand?.Execute(null);
            }

            UpdateButtonStyle(SFCButton, AELimitButton, GlobalButton);
        }

        private void GlobalButton_Click(object sender, RoutedEventArgs e)
        {
            AELimitContent.Visibility = Visibility.Collapsed;
            SFCContent.Visibility = Visibility.Collapsed;
            GlobalContent.Visibility = Visibility.Visible;

            // 切换工具栏显示
            PDCAStationPanel.Visibility = Visibility.Collapsed;
            SFCStationPanel.Visibility = Visibility.Collapsed;

            // 切换按钮显示
            PDCAExportButton.Visibility = Visibility.Collapsed;
            SFCExportButton.Visibility = Visibility.Collapsed;
            GlobalAddButton.Visibility = Visibility.Visible;  

            if (_globalViewModel != null)
            {
                _globalViewModel.RefreshCommand?.Execute(null);
            }

            UpdateButtonStyle(GlobalButton, AELimitButton, SFCButton);
        }

        private void UpdateButtonStyle(Button activeButton, Button button1, Button button2)
        {
            // 设置激活按钮样式
            activeButton.Background = new SolidColorBrush(Color.FromRgb(240, 248, 255));
            activeButton.BorderBrush = new SolidColorBrush(Color.FromRgb(24, 144, 255));
            activeButton.Foreground = new SolidColorBrush(Color.FromRgb(24, 144, 255));

            // 设置其他按钮样式
            SetInactiveButtonStyle(button1, activeButton);
            SetInactiveButtonStyle(button2, activeButton);
        }

        private void SetInactiveButtonStyle(Button button, Button activeButton)
        {
            if (button != activeButton)
            {
                button.Background = new SolidColorBrush(Colors.White);
                button.BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221));
                button.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            }
        }

        private void ExpandProdDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PDCAKeyParameterRow row)
            {
                row.IsProdDataExpanded = !row.IsProdDataExpanded;

                if (button.Template.FindName("ExpandIcon", button) is Path path)
                {
                    path.Data = row.IsProdDataExpanded ?
                        Geometry.Parse("M0,6 L6,0 12,6") :
                        Geometry.Parse("M0,0 L6,6 12,0");
                }

                if (VisualTreeHelper.GetParent(button) is Grid grid)
                {
                    var textBlock = grid.Children.OfType<TextBlock>().FirstOrDefault();
                    if (textBlock != null)
                    {
                        if (row.IsProdDataExpanded)
                        {
                            textBlock.MaxHeight = 1000;
                            textBlock.TextTrimming = TextTrimming.None;
                        }
                        else
                        {
                            textBlock.MaxHeight = 60;
                            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                        }
                    }
                }
            }
        }

        public bool HasData()
        {
            if (AELimitContent.Visibility == Visibility.Visible)
                return _viewModel?.ParameterRows?.Count > 0;
            else if (SFCContent.Visibility == Visibility.Visible)
                return _sfcViewModel?.ParameterRows?.Count > 0;
            else
                return _globalViewModel?.GlobalVariables?.Count > 0;
        }

        public int GetModuleCount()
        {
            if (AELimitContent.Visibility == Visibility.Visible)
                return _viewModel?.TotalModules ?? 0;
            else if (SFCContent.Visibility == Visibility.Visible)
                return _sfcViewModel?.TotalModules ?? 0;
            else
                return _globalViewModel?.GlobalVariables?.Count ?? 0;
        }

        public bool IsLoading()
        {
            if (AELimitContent.Visibility == Visibility.Visible)
                return _viewModel?.IsLoading ?? false;
            else if (SFCContent.Visibility == Visibility.Visible)
                return _sfcViewModel?.IsLoading ?? false;
            else
                return false;
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}