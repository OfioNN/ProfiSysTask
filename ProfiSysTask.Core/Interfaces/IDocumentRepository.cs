using ProfiSysTask.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProfiSysTask.Core.Interfaces {
    public interface IDocumentRepository {

        Task<IEnumerable<Document>> GetAllDocumentsAsync();

        Task SaveDocumentsAsync(IEnumerable<Document> documents);

        Task ClearDatabaseAsync();

    }
}
