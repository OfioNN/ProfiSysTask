using ProfiSysTask.Core.Models;

namespace ProfiSysTask.Core.Interfaces {
    public interface IDocumentRepository {

        Task<IEnumerable<Document>> GetAllDocumentsAsync();

        Task<int> GetTotalDocumentsCountAsync(string searchText, string searchColumn, string dateOperator = "");

        Task<IEnumerable<Document>> GetPagedDocumentsAsync(int pageNumber, int pageSize, string searchText, string searchColumn, string dateOperator = "");

        Task SaveDocumentsAsync(IEnumerable<Document> documents);

        Task ClearDatabaseAsync();

        Task AddDocumentAsync(Document document);
        Task UpdateDocumentAsync(Document document);
        Task DeleteDocumentAsync(Document document);

        Task AddItemAsync(DocumentItem item);
        Task UpdateItemAsync(DocumentItem item);
        Task DeleteItemAsync(DocumentItem item);

    }
}
