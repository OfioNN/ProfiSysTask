using ProfiSysTask.Core.Models;

namespace ProfiSysTask.Core.Interfaces {
    public interface IReportGenerator {
        Task GenerateDocumentReportAsync(Document document, string outputPath);
    }
}
