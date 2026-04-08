using ProfiSysTask.Core.Models;

namespace ProfiSysTask.Core.Interfaces {
    public interface IDocumentRepository {

        Task<IEnumerable<Document>> GetAllDocumentsAsync();

        Task SaveDocumentsAsync(IEnumerable<Document> documents);

        Task ClearDatabaseAsync();

    }
}
