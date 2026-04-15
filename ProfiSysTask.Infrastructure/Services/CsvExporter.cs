using ProfiSysTask.Core.Interfaces;
using ProfiSysTask.Core.Models;
using System.IO;
using System.Text;

namespace ProfiSysTask.Infrastructure.Services {
    public class CsvExporter : ICsvExporter {
        public async Task ExportAsync(string basePath, IEnumerable<Document> documents) {
            string docsPath = $"{basePath}_Documents.csv";
            string itemsPath = $"{basePath}_DocumentItems.csv";

            using (var writer = new StreamWriter(docsPath, false, Encoding.UTF8)) {
                await writer.WriteLineAsync("Id;Type;Date;FirstName;LastName;City");
                foreach (var doc in documents) {
                    await writer.WriteLineAsync($"{doc.Id};{doc.Type};{doc.Date:yyyy-MM-dd};{doc.FirstName};{doc.LastName};{doc.City}");
                }
            }

            using (var writer = new StreamWriter(itemsPath, false, Encoding.UTF8)) {
                await writer.WriteLineAsync("Ordinal;DocumentId;Product;Quantity;Price;TaxRate");
                foreach (var doc in documents) {
                    if (doc.Items == null) continue;

                    foreach (var item in doc.Items) {
                        await writer.WriteLineAsync($"{item.Ordinal};{doc.Id};{item.Product};{item.Quantity};{item.Price};{item.TaxRate}");
                    }
                }
            }
        }
    }
}