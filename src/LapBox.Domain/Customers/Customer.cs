using System.Text.RegularExpressions;
using LapBox.Domain.Common;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Customers.Events;
using LapBox.Domain.Customers.ValueObjects;

namespace LapBox.Domain.Customers;

public sealed class Customer : AggregateRoot
{
    // الربط مع طبقة الـ Infrastructure (جدول الـ AppUser)
    public Guid IdentityId { get; private set; }

    public string? Name { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public int TotalOrdersCount { get; private set; }
    public DateTimeOffset? FirstPurchaseDate { get; private set; }

    // البزنس رول: هل هو Customer (مشترى) أم مجرد User (مسجل فقط)؟
    public bool IsActiveCustomer => TotalOrdersCount > 0;

    private readonly List<Address> _addresses = [];
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();
    private static readonly Regex PhoneRegex = new(@"^\+?[1-9]\d{1,14}$");
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    private Customer() { }

   private Customer(Guid id, Guid identityId, string name, string email, string? phoneNumber) : base(id)
    {
        IdentityId = identityId;
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        TotalOrdersCount = 0; // يبدأ كـ User عادي
    }

    public static Result<Customer> Create(Guid identityId, string name, string email, string? phoneNumber = null)
    {
        if (identityId == Guid.Empty)
            return CustomerErrors.IdentityRequired;

        if (string.IsNullOrWhiteSpace(name))
            return CustomerErrors.NameRequired;

        if (string.IsNullOrWhiteSpace(email))
            return CustomerErrors.EmailRequired;

        if (!EmailRegex.IsMatch(email))
            return CustomerErrors.EmailInvalid;

        if (!string.IsNullOrWhiteSpace(phoneNumber) && !PhoneRegex.IsMatch(phoneNumber))
            return CustomerErrors.InvalidPhoneNumber;

        var customer = new Customer(Guid.NewGuid(), identityId, name, email, phoneNumber);

        customer.AddDomainEvent(new CustomerCreatedEvent(customer.Id, customer.IdentityId, customer.Email!));

        return customer;
    }

    public Result<Success> UpdateProfile(string name, string email, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            return CustomerErrors.NameRequired;

        if (string.IsNullOrWhiteSpace(email))
            return CustomerErrors.EmailRequired;

        if (!EmailRegex.IsMatch(email))
            return CustomerErrors.EmailInvalid;

        if (!string.IsNullOrWhiteSpace(phoneNumber) && !PhoneRegex.IsMatch(phoneNumber))
            return CustomerErrors.InvalidPhoneNumber;

        // التحديث الفعلي بعد تخطي شروط الأمان
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;

        return Result.Success;
    }

    public Result<Success> AddAddress(Address address)
    {
        if (address is null)
            return CustomerErrors.AddressRequired;

        if (_addresses.Any(a => a == address))
            return CustomerErrors.DuplicateAddress;

        _addresses.Add(address);
        return Result.Success;
    }

    public Result<Success> RemoveAddress(Address address)
    {
        if (address is null)
            return CustomerErrors.AddressRequired;

        var existingAddress = _addresses.FirstOrDefault(a => a == address);
        if (existingAddress is null)
            return CustomerErrors.AddressNotFound;

        _addresses.Remove(existingAddress);
        return Result.Success;
    }

    public Result<Success> UpdateAddress(Address oldAddress, Address newAddress)
    {
        if (oldAddress is null || newAddress is null)
            return CustomerErrors.AddressRequired;

        var existingAddress = _addresses.FirstOrDefault(a => a == oldAddress);
        if (existingAddress is null)
            return CustomerErrors.AddressNotFound;

        if (_addresses.Any(a => a != existingAddress && a == newAddress))
            return CustomerErrors.DuplicateAddress;

        _addresses.Remove(existingAddress);
        _addresses.Add(newAddress);

        return Result.Success;
    }

    // هذه الدالة سيتم استدعاؤها عبر Domain Event عندما يكتمل إنشاء طلب جديد
    public Result<Success> RegisterSuccessfulPurchase()
    {
        TotalOrdersCount++;

        FirstPurchaseDate ??= DateTimeOffset.UtcNow; // هنا تحول رسمياً لـ Customer

        return Result.Success;
    }

    public Result<Success> RemoveCancelledPurchase()
    {
        if (TotalOrdersCount > 0)
        {
            TotalOrdersCount--;
        }

        // بزنس رول: لو عداد طلباته رجع صفر، مبقاش customer نشط وبقى user عادي
        if (TotalOrdersCount == 0)
        {
            FirstPurchaseDate = null;
        }

        return Result.Success;
    }
}