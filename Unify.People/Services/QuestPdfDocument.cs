using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Unify.People.Services
{
    public class QuestPdfDocument : IDocument
    {
        private readonly string _receiptDetails;

        public QuestPdfDocument(string receiptDetails)
        {
            _receiptDetails = receiptDetails;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(50);
                page.MarginLeft(125);
                page.DefaultTextStyle(x => x.FontFamily("Courier New").FontSize(12));

                page.Content().Column(col =>
                {
                    foreach (var line in _receiptDetails.Split('\n'))
                    {
                        col.Item().Text(line.TrimEnd());
                    }
                });
            });
        }
    }
}
