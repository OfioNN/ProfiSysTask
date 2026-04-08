using ProfiSysTask.Core.Models;

namespace ProfiSysTask.Core.Interfaces {
    public interface ICsvImporter {
        IEnumerable<Document> Import(string documentsFilePath, string itemsFilePath);
    }
}
