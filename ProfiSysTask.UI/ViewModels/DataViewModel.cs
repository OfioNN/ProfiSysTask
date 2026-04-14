using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProfiSysTask.Core.Interfaces;
using ProfiSysTask.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace ProfiSysTask.UI.ViewModels
{
    public partial class DataViewModel : ObservableObject
    {
        private readonly IDocumentRepository _repository;
        private readonly ICsvImporter _csvImporter;

        [ObservableProperty]
        private ObservableCollection<Document> _documents = new();

        [ObservableProperty]
        private Document? _selectedDocument;

        public Action? GoBackRequested { get; set; }

        public List<string> FilterColumns { get; } = new List<string> { "Wszystko", "Typ", "Imię", "Nazwisko", "Miasto"};

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
        private int _currentPage = 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
        private int _totalPages = 1;

        private readonly int _pageSize = 50;

        private bool CanGoPrevious() => CurrentPage > 1;
        private bool CanGoNext() => CurrentPage < TotalPages;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedFilterColumn = "Wszystko";


        public DataViewModel(IDocumentRepository repository, ICsvImporter csvImporter) {
            _repository = repository;
            _csvImporter = csvImporter;
        }

        [RelayCommand]
        private void GoBack() {
            GoBackRequested?.Invoke();
        }

        [RelayCommand(CanExecute = nameof(CanGoPrevious))]
        private async Task PreviousPageAsync() {
            CurrentPage--;
            await LoadDataAsync();
        }

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private async Task NextPageAsync() {
            CurrentPage++;
            await LoadDataAsync();
        }

        partial void OnSearchTextChanged(string value) {
            CurrentPage = 1;
            _ = LoadDataAsync();
        }

        partial void OnSelectedFilterColumnChanged(string value) {
            CurrentPage = 1;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        public async Task ImportDataAsync() {
            try {
                var openFileDialog = new OpenFileDialog {
                    Title = "Wybierz plik Documents.csv",
                    Filter = "Pliki CSV (*.csv)|*.csv"
                };

                if (openFileDialog.ShowDialog() != true) return;
                string documentsPath = openFileDialog.FileName;

                openFileDialog.Title = "Wybierz plik DocumentItems.csv";
                if (openFileDialog.ShowDialog() != true) return;
                string itemsPath = openFileDialog.FileName;

                var importedDocuments = _csvImporter.Import(documentsPath, itemsPath);

                await _repository.ClearDatabaseAsync();
                await _repository.SaveDocumentsAsync(importedDocuments);

                await LoadDataAsync();
            }
            catch (Exception ex) {
                MessageBox.Show($"Wystąpił błąd podczas importu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task LoadDataAsync() {
            try {
                int totalItems = await _repository.GetTotalDocumentsCountAsync(SearchText, SelectedFilterColumn);
                TotalPages = (int)Math.Ceiling((double)totalItems / _pageSize);
                if (TotalPages == 0) TotalPages = 1;

                var pagedData = await _repository.GetPagedDocumentsAsync(CurrentPage, _pageSize, SearchText, SelectedFilterColumn);

                Documents = new ObservableCollection<Document>(pagedData);
            }
            catch (Exception ex) {
                MessageBox.Show($"Błąd podczas ładowania danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
