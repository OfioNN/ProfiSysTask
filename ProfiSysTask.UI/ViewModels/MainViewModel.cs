using CommunityToolkit.Mvvm.ComponentModel;

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
                await _dataViewModel.ImportDataAsync();
            };

            _startViewModel.LoadRequested = async () => {
                CurrentViewModel = _dataViewModel;
                await _dataViewModel.LoadDataAsync();
            };

            CurrentViewModel = _startViewModel;
        }
    }
}