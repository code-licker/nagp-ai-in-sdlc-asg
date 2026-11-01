using InvoiceApp.Domain;

namespace InvoiceApp.Tests;

internal class FakeInvoiceRepository : IInvoiceRepository
{
    private readonly List<Invoice> _invoices = new();

    public FakeInvoiceRepository(params Invoice[] seed) => _invoices.AddRange(seed);

    public IReadOnlyList<Invoice> GetAll() => _invoices.ToList();
    public Invoice? GetById(string id) => _invoices.FirstOrDefault(i => i.Id == id);
    public IReadOnlyList<Invoice> GetByIds(IEnumerable<string> ids) => _invoices.Where(i => ids.Contains(i.Id)).ToList();
    public Invoice Add(Invoice invoice) { _invoices.Add(invoice); return invoice; }
    public void Update(Invoice invoice) { }
    public bool Remove(string id) => _invoices.RemoveAll(i => i.Id == id) > 0;
}
