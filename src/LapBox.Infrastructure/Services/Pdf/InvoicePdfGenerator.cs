using LapBox.Application.Common.Interfaces.Services;
using LapBox.Domain.Billing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LapBox.Infrastructure.Services.Pdf;

public sealed class InvoicePdfGenerator : IInvoicePdfGenerator
{
    private readonly Color _primaryColor = Colors.Blue.Darken3;

    public byte[] Generate(Invoice invoice)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                // استخدام lambda مباشرة لتمرير الكود
                page.Header().Element(c => ComposeHeader(c, invoice));
                page.Content().Element(c => ComposeContent(c, invoice));
                page.Footer().Element(ComposeFooter);
            });
        })
        .GeneratePdf();
    }

    private void ComposeHeader(IContainer container, Invoice invoice)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("LapBox").FontSize(28).Bold().FontColor(_primaryColor);
                    c.Item().Text("Premium Tech & Laptops").FontSize(11).FontColor(Colors.Grey.Medium);
                });

                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text($"INVOICE #{invoice.Id.ToString()[..8].ToUpper()}").FontSize(20).SemiBold().FontColor(_primaryColor);
                    c.Item().Text($"Date: {invoice.IssuedAtUtc:dd MMM yyyy}").FontSize(10);
                    c.Item().DefaultTextStyle(x => x.FontSize(10)).Text(t => {
                        t.Span("Status: ");
                        t.Span(invoice.Status.ToString()).FontColor(GetStatusColor(invoice.Status.ToString())).Bold();
                    });
                });
            });

            col.Item().PaddingTop(10).LineHorizontal(1).LineColor(_primaryColor);
        });
    }

    private void ComposeContent(IContainer container, Invoice invoice)
    {
        container.Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Product Description");
                    header.Cell().Element(CellStyle).AlignCenter().Text("Qty");
                    header.Cell().Element(CellStyle).AlignCenter().Text("Unit Price");
                    header.Cell().Element(CellStyle).AlignRight().Text("Total");

                    static IContainer CellStyle(IContainer c) => c.DefaultTextStyle(x => x.SemiBold())
                        .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Blue.Darken3).Background(Colors.Grey.Lighten4).Padding(5);
                });

                foreach (var item in invoice.LineItems)
                {
                    table.Cell().Element(RowStyle).Text(item.Description);
                    table.Cell().Element(RowStyle).AlignCenter().Text(item.Quantity.ToString());
                    table.Cell().Element(RowStyle).AlignCenter().Text($"{item.UnitPrice:C}");
                    table.Cell().Element(RowStyle).AlignRight().Text($"{item.LineTotal:C}");

                    static IContainer RowStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5);
                }
            });

            col.Item().PaddingTop(20).AlignRight().Column(totals =>
            {
                totals.Item().Row(r => { r.RelativeItem().Text("Subtotal:"); r.RelativeItem().AlignRight().Text($"{invoice.SubTotal:C}"); });
                totals.Item().Row(r => { r.RelativeItem().Text("Tax (VAT):"); r.RelativeItem().AlignRight().Text($"{invoice.TaxAmount:C}"); });
                totals.Item().PaddingTop(5).Row(r => { 
                    r.RelativeItem().Text("GRAND TOTAL:").Bold().FontSize(14); 
                    r.RelativeItem().AlignRight().Text($"{invoice.TotalAmount:C}").Bold().FontSize(14).FontColor(_primaryColor); 
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("Thank you for choosing LapBox. All devices include a 1-year warranty.").FontSize(9).FontColor(Colors.Grey.Medium);
                row.RelativeItem().AlignRight().Text(x => {
                    x.Span("Generated: ").FontSize(8);
                    x.Span($"{DateTime.UtcNow:yyyy-MM-dd HH:mm}").FontSize(8);
                });
            });
        });
    }

    private static Color GetStatusColor(string status) => status.ToLower() switch
    {
        "paid" => Colors.Green.Medium,
        "shipped" => Colors.Blue.Medium,
        "pending" => Colors.Orange.Medium,
        _ => Colors.Grey.Darken1
    };
}