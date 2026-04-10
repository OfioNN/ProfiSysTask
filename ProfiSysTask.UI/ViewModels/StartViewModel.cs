using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProfiSysTask.UI.ViewModels {
    public partial class StartViewModel : ObservableObject {
        public Action? ImportRequested { get; set; }
        public Action? LoadRequested { get; set; }

        [RelayCommand]
        private void NavigateToImport() {
            ImportRequested?.Invoke();
        }

        [RelayCommand]
        private void NavigateToDatabase() {
            LoadRequested?.Invoke();
        }
    }
}