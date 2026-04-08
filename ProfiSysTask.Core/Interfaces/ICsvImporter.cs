using ProfiSysTask.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProfiSysTask.Core.Interfaces {
    public interface ICsvImporter {
        IEnumerable<Document> Import(string documentsFilePath, string itemsFilePath);
    }
}
