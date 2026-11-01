namespace InvoiceApp.Domain;

public interface IInvoiceRepository
{
    IReadOnlyList<Invoice> GetAll();
    Invoice? GetById(string id);
    IReadOnlyList<Invoice> GetByIds(IEnumerable<string> ids);
    Invoice Add(Invoice invoice);
    void Update(Invoice invoice);
    bool Remove(string id);
}
