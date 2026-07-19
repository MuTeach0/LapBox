using LapBox.Domain.Billing.Enums;
using LapBox.Domain.Common;
using LapBox.Domain.Common.Constants;
using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Billing;

public sealed class Invoice : AggregateRoot
{
    public Guid OrderId { get; private set; } 
    public Guid CustomerId { get; private set; } 

    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; } // ⚡ الخاصية الجديدة للخصومات
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTimeOffset? PaidDateUtc { get; private set; }
    public DateTimeOffset IssuedAtUtc { get; private set; }
    private readonly List<InvoiceLineItem> _lineItems = [];
    public IReadOnlyCollection<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    private Invoice() { } // من أجل EF Core

    // كونسركتور داخلي نظيف يتم استدعاؤه من الـ Factory Methods فقط
    private Invoice(Guid id, Guid orderId, Guid customerId) : base(id)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Status = InvoiceStatus.Unpaid;
        DiscountAmount = 0;
        IssuedAtUtc = DateTimeOffset.UtcNow; // القيمة الافتراضية بدون خصم
    }

    // 🔥 1. الـ Factory Method الذكية (Create) بناءً على طلبك
    // بتستقبل تفاصيل الأجهزة (الاسم، الكمية، سعر القطعة) وبتبني الفاتورة أوتوماتيك
    public static Result<Invoice> Create(Guid orderId, Guid customerId, List<(string Description, int Quantity, decimal UnitPrice)> items)
    {
        if (items is null || items.Count == 0)
            return InvoiceErrors.LineItemsRequired;

        var invoice = new Invoice(Guid.NewGuid(), orderId, customerId);
        int lineNumber = 1;

        foreach (var item in items)
        {
            if (item.Quantity <= 0) return InvoiceErrors.QuantityInvalid(lineNumber);
            if (item.UnitPrice <= 0) return InvoiceErrors.UnitPriceInvalid(lineNumber);

            // إنشاء سطر الفاتورة (اللابتوب أو القطعة)
            var lineItem = new InvoiceLineItem(
                Guid.NewGuid(), 
                invoice.Id, 
                lineNumber++, 
                item.Description, 
                item.Quantity, 
                item.UnitPrice);

            invoice._lineItems.Add(lineItem);
        }

        // الحساب المبدئي للمبالغ
        invoice.RecalculateAmounts();

        return invoice;
    }

    // 🚀 2. ميثود الـ ApplyDiscount (تطبيق خصم/كوبون على الفاتورة)
    public Result<Success> ApplyDiscount(decimal discountAmount)
    {
        // 🛑 قيد بزنس 1: الخصم لازم يطبق على فاتورة لسه مدفعتش أو متلغتش
        if (Status != InvoiceStatus.Unpaid)
            return InvoiceErrors.InvalidStatusForDiscount;

        // 🛑 قيد بزنس 2: قيمة الخصم لازم تكون أكبر من صفر
        if (discountAmount <= 0)
            return InvoiceErrors.DiscountAmountInvalid;

        // 🛑 قيد بزنس 3: مش منطقي الخصم يكون أكبر من إجمالي سعر اللابتوبات نفسه!
        if (discountAmount > SubTotal)
            return InvoiceErrors.DiscountExceedsSubTotal;

        // تطبيق الخصم وإعادة احتساب الضرائب والإجمالي فوراً
        DiscountAmount = discountAmount;
        RecalculateAmounts();

        return Result.Success;
    }

    public Result<Success> MarkAsPaid()
    {
        if (Status == InvoiceStatus.Paid)
            return InvoiceErrors.AlreadyPaid;

        Status = InvoiceStatus.Paid;
        PaidDateUtc = DateTimeOffset.UtcNow;

        return Result.Success;
    }

    // 🧠 ميثود داخلية مركزية لإعادة الحسابات المالية بدقة ومنع التضارب
    private void RecalculateAmounts()
    {
        // 1. حساب المجموع قبل الخصم والضريبة
        SubTotal = _lineItems.Sum(x => x.LineTotal);

        // 2. المبلغ الخاضع للضريبة (السعر بعد الخصم)
        var taxableAmount = Math.Max(0, SubTotal - DiscountAmount);

        // 3. حساب الضريبة بناءً على الثابت بتاعك LapBoxConstants.TaxRate
        TaxAmount = taxableAmount * LapBoxConstants.TaxRate;

        // 4. الإجمالي النهائي الصافي المطلوب دفعه
        TotalAmount = taxableAmount + TaxAmount;
    }
}