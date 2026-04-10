using CsvHelper;
using CsvHelper.Configuration;
using ProfiSysTask.Core.Interfaces;
using ProfiSysTask.Core.Models;
using System.Globalization;

namespace ProfiSysTask.Infrastructure.Services {
    public class CsvImporter : ICsvImporter {
        public IEnumerable<Document> Import(string documentsFilePath, string itemsFilePath) {

            var csvConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("pl-PL")) {
                HasHeaderRecord = true,
                Delimiter = ";",
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using var docReader = new StreamReader(documentsFilePath);
            using var docCsv = new CsvReader(docReader, csvConfig);
            var documents = docCsv.GetRecords<Document>().ToList();

            using var itemReader = new StreamReader(itemsFilePath);
            using var itemCsv = new CsvReader(itemReader, csvConfig);
            var items = itemCsv.GetRecords<DocumentItem>().ToList();

            foreach (var document in documents) {
                document.Items = items.Where(i => i.DocumentId == document.Id).ToList();
            }

            return documents;
        }
    }
}
