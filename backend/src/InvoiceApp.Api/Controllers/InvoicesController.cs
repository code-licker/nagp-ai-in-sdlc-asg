using InvoiceApp.Api.Auth;
using InvoiceApp.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ApiControllerBase
{
    private readonly IInvoiceRepository _repository;
    private readonly ApprovalService _approvals;

    public InvoicesController(IInvoiceRepository repository, ApprovalService approvals)
    {
        _repository = repository;
        _approvals = approvals;
    }

    public record CreateInvoiceRequest(string CustomerName, decimal Amount);

    [HttpGet]
    public ActionResult<IReadOnlyList<Invoice>> GetAll([FromQuery] string? status)
    {
        var invoices = _repository.GetAll();

        if (!string.IsNullOrEmpty(status))
        {
            if (!Enum.TryParse<InvoiceStatus>(status, ignoreCase: true, out var parsed))
            {
                return BadRequest(new { error = $"Unknown status '{status}'." });
            }

            invoices = invoices.Where(i => i.Status == parsed).ToList();
        }

        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public ActionResult<Invoice> GetById(string id)
    {
        var invoice = _repository.GetById(id);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    public ActionResult<Invoice> Create([FromBody] CreateInvoiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return BadRequest(new { error = "Customer name is required." });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new { error = "Amount must be greater than zero." });
        }

        var invoice = _repository.Add(new Invoice
        {
            CustomerName = request.CustomerName.Trim(),
            Amount = request.Amount
        });

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    [HttpPost("{id}/submit")]
    public ActionResult<Invoice> Submit(string id) => Handle(() => _approvals.Submit(id));

    [HttpDelete("{id}")]
    [RequireRole("Admin")]
    public IActionResult Delete(string id) =>
        _repository.Remove(id) ? NoContent() : NotFound();
}
