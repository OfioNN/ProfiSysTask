using ProfiSysTask.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = ProfiSysTask.Core.Models.Document;

namespace ProfiSysTask.Infrastructure.Services {
    public class QuestPdfReportGenerator : IReportGenerator {
        public QuestPdfReportGenerator() {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task GenerateDocumentReportAsync(Document document, string outputPath) {
            return Task.Run(() => {
                var pdfDocument = QuestPDF.Fluent.Document.Create(container => {
                    container.Page(page => {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Element(compose => ComposeHeader(compose, document));
                        page.Content().Element(compose => ComposeContent(compose, document));

                        page.Footer().AlignCenter().Text(x => {
                            x.Span("Strona ");
                            x.CurrentPageNumber();
                            x.Span(" z ");
                            x.TotalPages();
                        });
                    });
                });

                pdfDocument.GeneratePdf(outputPath);
            });
        }

        private void ComposeHeader(IContainer container, Document document) {
            container.Row(row => {
                row.RelativeItem().Column(column => {
                    column.Item().Text($"Dokument #{document.Id}").FontSize(22).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(5).Text($"Typ: {document.Type}").FontSize(14);
                    column.Item().Text($"Data: {document.Date:yyyy-MM-dd}");
                });

                row.ConstantItem(250).Background(Colors.Grey.Lighten4).Padding(15).Column(column => {
                    column.Item().Text("NABYWCA").FontSize(10).SemiBold().FontColor(Colors.Grey.Medium);
                    column.Item().PaddingTop(5).Text($"{document.FirstName} {document.LastName}").FontSize(14).SemiBold().FontColor(Colors.Black);
                    column.Item().PaddingTop(2).Text($"Miasto: {document.City}").FontSize(11).FontColor(Colors.Grey.Darken3);
                });
            });
        }

        private void ComposeContent(IContainer container, Document document) {
            container.PaddingVertical(1, Unit.Centimetre).Column(column => {
                column.Spacing(20);

                column.Item().Table(table => {
                    table.ColumnsDefinition(columns => {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn();
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(90);
                    });

                    table.Header(header => {
                        header.Cell().Text("Lp.").SemiBold();
                        header.Cell().Text("Produkt").SemiBold();
                        header.Cell().AlignRight().Text("Ilość").SemiBold();
                        header.Cell().AlignRight().Text("Cena").SemiBold();
                        header.Cell().AlignRight().Text("VAT").SemiBold();
                        header.Cell().AlignRight().Text("Wartość").SemiBold();

                        header.Cell().ColumnSpan(6).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
                    });

                    decimal totalSum = 0;

                    if (document.Items != null) {
                        foreach (var item in document.Items) {
                            decimal itemTotal = item.Quantity * item.Price;
                            totalSum += itemTotal;

                            table.Cell().PaddingVertical(5).Text(item.Ordinal.ToString());
                            table.Cell().PaddingVertical(5).Text(item.Product);
                            table.Cell().PaddingVertical(5).AlignRight().Text(item.Quantity.ToString());
                            table.Cell().PaddingVertical(5).AlignRight().Text($"{item.Price:C}");

                            table.Cell().PaddingVertical(5).AlignRight().Text($"{item.TaxRate:0}%");

                            table.Cell().PaddingVertical(5).AlignRight().Text($"{itemTotal:C}");
                        }
                    }

                    table.Cell().ColumnSpan(6).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

                    table.Cell().ColumnSpan(5).PaddingTop(15).AlignRight().Text("Suma całkowita: ").SemiBold().FontSize(14);
                    table.Cell().PaddingTop(15).PaddingLeft(5).AlignRight().Text($"{totalSum:C}").SemiBold().FontSize(14).FontColor(Colors.Blue.Darken2);
                });
            });
        }
    }
}