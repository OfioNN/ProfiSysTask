using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace ProfiSysTask.UI.ViewModels {
    public partial class MainViewModel : ObservableObject {

        [ObservableProperty]
        private ObservableObject _currentViewModel;

        private readonly StartViewModel _startViewModel;
        private readonly DataViewModel _dataViewModel;

        public MainViewModel(StartViewModel startViewModel, DataViewModel dataViewModel) {
            _startViewModel = startViewModel;
            _dataViewModel = dataViewModel;

            _startViewModel.ImportRequested = async () => {
                CurrentViewModel = _dataViewModel;

                ResizeAndCenterWindow(1200, 800);

                await _dataViewModel.ImportDataAsync();
            };

            _startViewModel.LoadRequested = async () => {
                CurrentViewModel = _dataViewModel;

                ResizeAndCenterWindow(1200, 800);

                await _dataViewModel.LoadDataAsync();
            };

            _dataViewModel.GoBackRequested = () => {
                CurrentViewModel = _startViewModel;
                ResizeAndCenterWindow(900, 600);
            };

            CurrentViewModel = _startViewModel;
        }

        private void ResizeAndCenterWindow(double width, double height) {
            var window = Application.Current.MainWindow;
            if (window != null) {

                window.Width = width;
                window.Height = height;

                window.Left = (SystemParameters.WorkArea.Width - width) / 2 + SystemParameters.WorkArea.Left;
                window.Top = (SystemParameters.WorkArea.Height - height) / 2 + SystemParameters.WorkArea.Top;
            }
        }
    }
}