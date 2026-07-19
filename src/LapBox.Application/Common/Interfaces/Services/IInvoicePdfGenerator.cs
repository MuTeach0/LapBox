using LapBox.Domain.Billing;

namespace LapBox.Application.Common.Interfaces.Services;

public interface IInvoicePdfGenerator
{
    byte[] Generate(Invoice invoice);
}