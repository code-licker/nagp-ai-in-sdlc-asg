namespace InvoiceApp.Domain;

/// <summary>Volume and loyalty discount rules used when pricing invoices.</summary>
public class DiscountCalculator
{
    public decimal QuantityDiscount(int quantity)
    {
        if (quantity >= 100) return 0.15m;
        if (quantity >= 50) return 0.10m;
        if (quantity >= 10) return 0.05m;
        return 0m;
    }

    public decimal LoyaltyDiscount(int yearsAsCustomer)
    {
        return Math.Min(yearsAsCustomer * 0.01m, 0.10m);
    }

    public decimal CombinedDiscount(int quantity, int yearsAsCustomer, string? customerType)
    {
        var discount = QuantityDiscount(quantity) + LoyaltyDiscount(yearsAsCustomer);

        if (customerType == "Nonprofit")
        {
            discount += 0.05m;
        }

        return discount;
    }

    public decimal ApplyDiscount(decimal amount, decimal discountRate)
    {
        return Math.Round(amount * (1 - discountRate), 2);
    }
}
