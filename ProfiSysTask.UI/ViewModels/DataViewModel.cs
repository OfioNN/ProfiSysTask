using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProfiSysTask.Core.Interfaces;
using ProfiSysTask.Core.Models;
using ProfiSysTask.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace ProfiSysTask.UI.ViewModels
{
    public partial class DataViewModel : ObservableObject
    {
        private readonly IDocumentRepository _repository;
        private readonly ICsvImporter _csvImporter;
        private readonly ICsvExporter _csvExporter;
        private readonly IReportGenerator _reportGenerator;

        [ObservableProperty]
        private ObservableCollection<Document> _documents = new();

        [ObservableProperty]
        private Document? _selectedDocument;

        [ObservableProperty]
        private DocumentItem? _selectedItem;

        public Action? GoBackRequested { get; set; }


        public List<string> FilterColumns { get; } = new List<string> { "Wszystko", "Typ", "Data", "Imię", "Nazwisko", "Miasto"};
        public List<string> ItemFilterColumns { get; } = new List<string> { "Wszystko", "Produkt", "Cena", "VAT" };
        public List<string> DocumentTypes { get; } = new List<string> { "Invoice", "Order", "Receipt" };
        public List<string> ProductsList { get; } = new List<string> { "Graphics Card", "Hard drive", "Headphones", "Keyboard", "Monitor", "Mouse", "Printer", "Processor", "RAM" };
        public List<string> VatRatesStringList { get; } = new List<string> { "8", "23" };
        public List<int> VatRatesList { get; } = new List<int> { 8, 23 };
        public List<string> PriceOperators { get; } = new List<string> { "Od", "Do", "Równa" };
        public List<string> DateOperators { get; } = new List<string> { "Dnia", "Powyżej", "Poniżej" };

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private DateTime? _searchDate = DateTime.Now;

        [ObservableProperty]
        private string _selectedDateOperator = "Dnia";

        [ObservableProperty]
        private string _selectedPriceOperator = "Od";

        [ObservableProperty]
        private string _searchText = string.Empty;
        [ObservableProperty]
        private string _selectedFilterColumn = "Wszystko";

        [ObservableProperty]
        private string _itemSearchText = string.Empty;
        [ObservableProperty]
        private string _selectedItemFilterColumn = "Wszystko";
        public ICollectionView? ItemsView { get; private set; }

        [ObservableProperty]
        private bool _isConfirmDialogOpen;

        [ObservableProperty]
        private string _confirmDialogTitle = string.Empty;

        [ObservableProperty]
        private string _confirmDialogMessage = string.Empty;

        [ObservableProperty]
        private bool _isInfoOnlyMode;

        [ObservableProperty]
        private bool _isErrorMode;

        private TaskCompletionSource<bool>? _dialogTaskCompletionSource;

        [ObservableProperty]
        private bool _isDocumentFormOpen;

        [ObservableProperty]
        private string _documentFormTitle = string.Empty;

        [ObservableProperty]
        private Document _editingDocument = new();

        private bool _isEditMode;

        [ObservableProperty]
        private bool _isItemFormOpen;

        [ObservableProperty]
        private string _itemFormTitle = string.Empty;

        [ObservableProperty]
        private DocumentItem _editingItem = new();

        private bool _isEditItemMode;

        [ObservableProperty]
        private bool _isStatisticsModalOpen;

        [ObservableProperty] private decimal _statTotalRevenue;
        [ObservableProperty] private string _statTopProduct = "";
        [ObservableProperty] private int _statTotalItemsCount;

        [ObservableProperty]
        private string _editingItemPriceText = string.Empty;

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

        partial void OnItemSearchTextChanged(string value) => ItemsView?.Refresh();


        public DataViewModel(IDocumentRepository repository, ICsvImporter csvImporter, ICsvExporter csvExporter, IReportGenerator reportGenerator) {
            _repository = repository;
            _csvImporter = csvImporter;
            _csvExporter = csvExporter;
            _reportGenerator = reportGenerator;
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

        partial void OnSelectedDateOperatorChanged(string value) => _ = LoadDataAsync();

        partial void OnSelectedPriceOperatorChanged(string value) => ItemsView?.Refresh();

        partial void OnSelectedFilterColumnChanged(string value) {
            if (value == "Data") {
                SearchDate = DateTime.Now;
                SelectedDateOperator = "Dnia";
            }
            else if (value == "Typ") {
                SearchText = DocumentTypes.FirstOrDefault() ?? string.Empty;
            }
            else {
                SearchText = string.Empty;
            }
            CurrentPage = 1;
            _ = LoadDataAsync();
        }



        partial void OnSelectedItemFilterColumnChanged(string value) {
            if (value == "Produkt") {
                ItemSearchText = ProductsList.FirstOrDefault() ?? string.Empty;
            }
            else if (value == "VAT") {
                ItemSearchText = VatRatesStringList.FirstOrDefault() ?? string.Empty;
            }
            else {
                ItemSearchText = string.Empty;
            }

            ItemsView?.Refresh();
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

                IsLoading = true;

                var importedDocuments = await Task.Run(() => _csvImporter.Import(documentsPath, itemsPath));

                await _repository.ClearDatabaseAsync();
                await _repository.SaveDocumentsAsync(importedDocuments);

                await LoadDataAsync();
            }
            catch (Exception ex) {
                MessageBox.Show($"Wystąpił błąd podczas importu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ExportDataAsync() {
            try {
                string exportFolder = GetAndEnsureFolder("exports");

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                    InitialDirectory = exportFolder,
                    Title = "Zapisz eksport plików CSV",
                    Filter = "Plik CSV (*.csv)|*.csv",
                    FileName = $"{DateTime.Now:yyyyMMdd}_Export"
                };

                if (saveFileDialog.ShowDialog() != true) return;

                string basePath = saveFileDialog.FileName.Replace(".csv", "");

                var allDocuments = await _repository.GetAllDocumentsAsync();

                await _csvExporter.ExportAsync(basePath, allDocuments);

                await ShowConfirmDialogAsync(
                    "Eksport zakończony",
                    $"Baza została pomyślnie wyeksportowana do plików:\n\n• {Path.GetFileName(basePath)}_Documents.csv\n• {Path.GetFileName(basePath)}_DocumentItems.csv",
                    true);
            }
            catch (Exception ex) {
                MessageBox.Show($"Wystąpił błąd podczas eksportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task LoadDataAsync() {
            IsLoading = true;
            try {
                int? savedDocumentId = SelectedDocument?.Id;

                int totalItems = await _repository.GetTotalDocumentsCountAsync(SearchText, SelectedFilterColumn, SelectedDateOperator);
                TotalPages = (int)Math.Ceiling((double)totalItems / _pageSize);
                if (TotalPages == 0) TotalPages = 1;

                if (CurrentPage > TotalPages) {
                    CurrentPage = TotalPages;
                }

                var pagedData = await _repository.GetPagedDocumentsAsync(CurrentPage, _pageSize, SearchText, SelectedFilterColumn, SelectedDateOperator);
                Documents = new ObservableCollection<Document>(pagedData);

                if (savedDocumentId.HasValue) {
                    SelectedDocument = Documents.FirstOrDefault(d => d.Id == savedDocumentId.Value);
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"Błąd podczas ładowania danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GenerateReportAsync() {
            if (SelectedDocument == null) {
                await ShowConfirmDialogAsync("Informacja", "Najpierw zaznacz na liście dokument, dla którego chcesz wygenerować raport.", true);
                return;
            }

            try {
                string reportsFolder = GetAndEnsureFolder("reports");

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                    InitialDirectory = reportsFolder,
                    Title = "Zapisz raport PDF",
                    Filter = "Plik PDF (*.pdf)|*.pdf",
                    FileName = $"Raport_Document_{SelectedDocument.Id}_{DateTime.Now:yyyyMMdd}"
                };

                if (saveFileDialog.ShowDialog() != true) return;

                string filePath = saveFileDialog.FileName;

                await _reportGenerator.GenerateDocumentReportAsync(SelectedDocument, filePath);

                var processInfo = new System.Diagnostics.ProcessStartInfo {
                    FileName = filePath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processInfo);
            }
            catch (Exception ex) {
                MessageBox.Show($"Wystąpił błąd podczas generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ClearDatabaseAsync() {
            int count = await _repository.GetTotalDocumentsCountAsync(string.Empty, "Wszystko");
            if (count == 0) {
                await ShowConfirmDialogAsync("Informacja", "Baza danych jest już pusta. Nie ma nic do usunięcia.", true);
                return;
            }

            bool confirmed = await ShowConfirmDialogAsync(
                "Ostrzeżenie - Czyszczenie bazy",
                "Czy na pewno chcesz bezpowrotnie usunąć wszystkie dokumenty i pozycje z bazy danych?\n\nTej operacji nie można cofnąć!",
                false);

            if (confirmed) {
                try {
                    await _repository.ClearDatabaseAsync();
                    CurrentPage = 1;
                    SearchText = string.Empty;
                    SelectedDocument = null;
                    await LoadDataAsync();
                }
                catch (Exception ex) {
                    MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void AddDocument() {
            EditingDocument = new Document {
                Type = DocumentTypes.FirstOrDefault() ?? "Invoice",
                Date = DateTime.Now,
                FirstName = string.Empty,
                LastName = string.Empty,
                City = string.Empty
            };

            DocumentFormTitle = "Dodaj nowy dokument";
            _isEditMode = false;
            IsDocumentFormOpen = true;
        }

        [RelayCommand]
        private void EditDocument(Document doc) {
            if (doc == null) return;

            EditingDocument = new Document {
                Id = doc.Id,
                Type = doc.Type,
                Date = doc.Date,
                FirstName = doc.FirstName,
                LastName = doc.LastName,
                City = doc.City,
                Items = doc.Items
            };

            DocumentFormTitle = $"Edytuj dokument #{doc.Id}";
            _isEditMode = true;
            IsDocumentFormOpen = true;
        }

        [RelayCommand]
        private void CancelDocumentForm() {
            IsDocumentFormOpen = false;
        }

        [RelayCommand]
        private async Task SaveDocumentAsync() {

            if (string.IsNullOrWhiteSpace(EditingDocument.Type) ||
                            string.IsNullOrWhiteSpace(EditingDocument.FirstName) ||
                            string.IsNullOrWhiteSpace(EditingDocument.LastName) ||
                            string.IsNullOrWhiteSpace(EditingDocument.City)) {
                await ShowErrorDialogAsync("Brakujące dane", "Wszystkie pola formularza są wymagane! Uzupełnij brakujące dane przed zapisem.");
                return;
            }

            try {
                if (_isEditMode) {
                    await _repository.UpdateDocumentAsync(EditingDocument);
                }
                else {
                    await _repository.AddDocumentAsync(EditingDocument);

                    CurrentPage = int.MaxValue;
                }

                IsDocumentFormOpen = false;
                await LoadDataAsync();
            }
            catch (Exception ex) {
                MessageBox.Show($"Błąd zapisu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteDocumentAsync(Document doc) {
            if (doc == null) return;

            bool confirmed = await ShowConfirmDialogAsync(
                "Usuwanie dokumentu",
                $"Czy na pewno chcesz usunąć dokument #{doc.Id} wystawiony dla {doc.FirstName} {doc.LastName}?\n\nWszystkie jego pozycje również zostaną usunięte. Tej operacji nie można cofnąć.");

            if (confirmed) {
                try {
                    await _repository.DeleteDocumentAsync(doc);

                    if (SelectedDocument?.Id == doc.Id) {
                        SelectedDocument = null;
                    }

                    await LoadDataAsync();
                }
                catch (Exception ex) {
                    MessageBox.Show($"Błąd podczas usuwania: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task DeleteItemAsync(DocumentItem item) {
            if (item == null) return;

            bool confirmed = await ShowConfirmDialogAsync(
                "Usuwanie pozycji",
                $"Czy na pewno chcesz usunąć pozycję: '{item.Product}'?");

            if (confirmed) {
                try {
                    await _repository.DeleteItemAsync(item);
                    await LoadDataAsync();
                }
                catch (Exception ex) {
                    MessageBox.Show($"Błąd podczas usuwania pozycji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void AddItem() {
            if (SelectedDocument == null) return;

            EditingItem = new DocumentItem {
                DocumentId = SelectedDocument.Id,
                Product = ProductsList.FirstOrDefault() ?? "Graphics Card",
                Quantity = 1,
                Price = 0,
                TaxRate = 8
            };

            ItemFormTitle = "Dodaj nową pozycję";
            _isEditItemMode = false;

            EditingItemPriceText = string.Empty;

            OnPropertyChanged(nameof(IsVat8));
            OnPropertyChanged(nameof(IsVat23));

            IsItemFormOpen = true;
        }

        [RelayCommand]
        private void EditItem(DocumentItem item) {
            if (item == null) return;

            EditingItem = new DocumentItem {
                Id = item.Id,
                Ordinal = item.Ordinal,
                DocumentId = item.DocumentId,
                Product = item.Product,
                Quantity = item.Quantity,
                Price = item.Price,
                TaxRate = item.TaxRate
            };

            EditingItemPriceText = item.Price.ToString("N2");

            ItemFormTitle = "Edytuj pozycję";
            _isEditItemMode = true;

            OnPropertyChanged(nameof(IsVat8));
            OnPropertyChanged(nameof(IsVat23));

            IsItemFormOpen = true;
        }

        [RelayCommand]
        private void CancelItemForm() => IsItemFormOpen = false;

        [RelayCommand]
        private async Task SaveItemAsync() {
            if (string.IsNullOrWhiteSpace(EditingItem.Product) || EditingItem.Quantity <= 0) {
                await ShowErrorDialogAsync("Brakujące dane", "Nazwa produktu i ilość (większa od zera) są wymagane!");
                return;
            }

            string normalizedPrice = EditingItemPriceText.Replace(".", ",");

            if (!decimal.TryParse(normalizedPrice, out decimal parsedPrice) || parsedPrice < 0) {
                await ShowErrorDialogAsync("Błędna cena", "Wprowadzona cena jest nieprawidłowa. Użyj formatu liczbowego (np. 15,50). Upewnij się, że nie ma tam liter.");
                return;
            }

            EditingItem.Price = parsedPrice;

            try {
                if (_isEditItemMode) {
                    await _repository.UpdateItemAsync(EditingItem);
                }
                else {
                    await _repository.AddItemAsync(EditingItem);
                }

                IsItemFormOpen = false;
                await LoadDataAsync();
            }
            catch (Exception ex) {
                MessageBox.Show($"Błąd zapisu pozycji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void IncreaseQuantity() {
            EditingItem.Quantity++;
            OnPropertyChanged(nameof(EditingItem));
        }

        [RelayCommand]
        private void DecreaseQuantity() {
            if (EditingItem.Quantity > 1) {
                EditingItem.Quantity--;
                OnPropertyChanged(nameof(EditingItem));
            }
        }

        partial void OnSelectedDocumentChanged(Document? value) {
            if (value != null && value.Items != null) {
                ItemsView = CollectionViewSource.GetDefaultView(value.Items);
                ItemsView.Filter = FilterItems;
            }
            else {
                ItemsView = null;
            }
            OnPropertyChanged(nameof(ItemsView));
        }

        private bool FilterItems(object obj) {
            if (obj is not DocumentItem item) return false;
            if (string.IsNullOrWhiteSpace(ItemSearchText)) return true;

            string search = ItemSearchText.ToLower();

            switch (SelectedItemFilterColumn) {
                case "Produkt":
                    return item.Product != null && item.Product.ToLower().Contains(search);

                case "Cena":
                    string normalizedSearch = search.Replace(".", ",");

                    if (decimal.TryParse(normalizedSearch, out decimal searchPrice)) {
                        if (SelectedPriceOperator == "Od") return item.Price >= searchPrice;
                        if (SelectedPriceOperator == "Do") return item.Price <= searchPrice;
                        if (SelectedPriceOperator == "Równa") return item.Price == searchPrice;
                    }
                    return true;

                case "VAT":
                    return item.TaxRate.ToString().Contains(search);

                case "Wszystko":
                default:
                    return (item.Product != null && item.Product.ToLower().Contains(search)) ||
                           item.Price.ToString().Contains(search) ||
                           item.TaxRate.ToString().Contains(search);
            }
        }

        private string GetAndEnsureFolder(params string[] subFolders) {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Path.Combine(subFolders));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        private Task<bool> ShowConfirmDialogAsync(string title, string message, bool isInfoOnly = false) {
            ConfirmDialogTitle = title;
            ConfirmDialogMessage = message;
            IsInfoOnlyMode = isInfoOnly;
            IsErrorMode = false;
            IsConfirmDialogOpen = true;

            _dialogTaskCompletionSource = new TaskCompletionSource<bool>();
            return _dialogTaskCompletionSource.Task;
        }

        private Task ShowErrorDialogAsync(string title, string message) {
            ConfirmDialogTitle = title;
            ConfirmDialogMessage = message;
            IsInfoOnlyMode = false;
            IsErrorMode = true;
            IsConfirmDialogOpen = true;

            _dialogTaskCompletionSource = new TaskCompletionSource<bool>();
            return _dialogTaskCompletionSource.Task;
        }

        [RelayCommand]
        private void ConfirmDialogYes() {
            IsConfirmDialogOpen = false;
            _dialogTaskCompletionSource?.TrySetResult(true);
        }

        [RelayCommand]
        private void ConfirmDialogNo() {
            IsConfirmDialogOpen = false;
            _dialogTaskCompletionSource?.TrySetResult(false);
        }

        [RelayCommand]
        private async Task ShowStatistics() {
            IsLoading = true;
            try {
                var allDocs = await _repository.GetAllDocumentsAsync();
                var allItems = allDocs.SelectMany(d => d.Items).ToList();

                if (!allItems.Any()) {
                    StatTotalRevenue = 0;
                    StatTopProduct = "Brak danych";
                    StatTotalItemsCount = 0;
                }
                else {
                    StatTotalRevenue = allItems.Sum(i => i.Price * i.Quantity);

                    StatTotalItemsCount = allItems.Sum(i => i.Quantity);

                    StatTopProduct = allItems.GroupBy(i => i.Product)
                                         .OrderByDescending(g => g.Sum(x => x.Quantity))
                                         .First().Key;
                }

                IsStatisticsModalOpen = true;
            }
            finally {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void CloseStatistics() => IsStatisticsModalOpen = false;

        public bool IsVat8 {
            get => EditingItem.TaxRate == 8;
            set {
                if (value) {
                    EditingItem.TaxRate = 8;
                    OnPropertyChanged(nameof(IsVat8));
                    OnPropertyChanged(nameof(IsVat23));
                }
            }
        }

        public bool IsVat23 {
            get => EditingItem.TaxRate == 23;
            set {
                if (value) {
                    EditingItem.TaxRate = 23;
                    OnPropertyChanged(nameof(IsVat8));
                    OnPropertyChanged(nameof(IsVat23));
                }
            }
        }
        partial void OnSearchDateChanged(DateTime? value) {
            if (SelectedFilterColumn == "Data") {
                SearchText = value.HasValue ? value.Value.ToString("yyyy-MM-dd") : string.Empty;
            }
        }
    }
}
