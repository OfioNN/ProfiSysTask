using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProfiSysTask.Core.Interfaces;
using ProfiSysTask.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;

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

                MessageBox.Show("Dane zostały pomyślnie zaimportowane i zapisane w bazie danych.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadDataAsync();
            }
            catch (Exception ex) {
                MessageBox.Show($"Wystąpił błąd podczas importu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task LoadDataAsync() {
            try {
                var data = await _repository.GetAllDocumentsAsync();

                Documents = new ObservableCollection<Document>(data);

                MessageBox.Show($"Pomyślnie załadowano {Documents.Count} dokumentów z bazy.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) {
                MessageBox.Show($"Błąd podczas ładowania danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
