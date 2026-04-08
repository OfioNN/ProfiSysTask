using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProfiSysTask.Core.Interfaces;
using ProfiSysTask.Core.Models;
using System.Collections.ObjectModel;

namespace ProfiSysTask.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDocumentRepository _repository;
        private readonly ICsvImporter _csvImporter;

        [ObservableProperty]
        private ObservableCollection<Document> _documents = new();

        [ObservableProperty]
        private Document? _selectedDocument;

        public MainViewModel(IDocumentRepository repository, ICsvImporter csvImporter) {
            _repository = repository;
            _csvImporter = csvImporter;
        }

        [RelayCommand]
        private async Task ImportDataAsync() {

        }

        [RelayCommand]
        private async Task LoadDataAsync() {

        }
    }
}
