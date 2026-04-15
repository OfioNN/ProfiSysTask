using ProfiSysTask.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProfiSysTask.Core.Interfaces {
    public interface ICsvExporter {
        Task ExportAsync(string basePath, IEnumerable<Document> documents);
    }
}
