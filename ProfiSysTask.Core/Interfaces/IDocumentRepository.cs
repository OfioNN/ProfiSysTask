using ProfiSysTask.Core.Models;

namespace ProfiSysTask.Core.Interfaces {
    public interface IDocumentRepository {

        Task<IEnumerable<Document>> GetAllDocumentsAsync();

        Task SaveDocumentsAsync(IEnumerable<Document> documents);

        Task ClearDatabaseAsync();

        Task<int> GetTotalDocumentsCountAsync(string searchText, string searchColumn);

        Task<IEnumerable<Document>> GetPagedDocumentsAsync(int pageNumber, int pageSize, string searchText, string searchColumn);

    }
}
