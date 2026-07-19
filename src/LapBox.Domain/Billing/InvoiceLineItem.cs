using LapBox.Domain.Common;
using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Billing;

public sealed class InvoiceLineItem : Entity
{
    public Guid InvoiceId { get; private set; }
    public int LineNumber { get; private set; }
    public string Description { get; private set; } = string.Empty; // اسم اللابتوب ومواصفاته
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; } // الكمية × سعر الوحدة

    private InvoiceLineItem() { } // من أجل EF Core

    internal InvoiceLineItem(Guid id, Guid invoiceId, int lineNumber, string description, int quantity, decimal unitPrice) 
        : base(id)
    {
        InvoiceId = invoiceId;
        LineNumber = lineNumber;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = quantity * unitPrice; // حساب تلقائي ومحمي
    }

     public static Result<InvoiceLineItem> Create(
        Guid invoiceId,
        int lineNumber,
        string description,
        int quantity,
        decimal unitPrice)
    {
        if (invoiceId == Guid.Empty)
        {
            return InvoiceLineItemErrors.InvoiceIdRequired;
        }

        if (lineNumber <= 0)
        {
            return InvoiceLineItemErrors.LineNumberInvalid;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return InvoiceLineItemErrors.DescriptionRequired;
        }

        if (quantity <= 0)
        {
            return InvoiceLineItemErrors.QuantityInvalid;
        }

        if (unitPrice <= 0)
        {
            return InvoiceLineItemErrors.UnitPriceInvalid;
        }

        return new InvoiceLineItem(Guid.NewGuid(), invoiceId, lineNumber, description.Trim(), quantity, unitPrice);
    }
}