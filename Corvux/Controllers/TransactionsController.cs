using Microsoft.AspNetCore.Mvc;
using Corvux.Models;
using Corvux.Services;

namespace Corvux.Controllers;

/// <summary>
/// REST API para transacciones. Expone CRUD + importación.
/// Documentado automáticamente en Swagger (/swagger).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _service;
    private readonly ComplianceRuleEngine _engine;

    public TransactionsController(TransactionService service, ComplianceRuleEngine engine)
    {
        _service = service;
        _engine = engine;
    }

    /// <summary>Lista todas las transacciones con su evaluación de compliance.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _service.GetTransactionsAsync();
        var result = transactions.Select(t => new
        {
            t.Id,
            t.TransactionId,
            t.AmountUsd,
            t.OriginCountry,
            t.DestinationCountry,
            t.CreatedAtUtc,
            t.UpdatedAtUtc,
            Compliance = _engine.Evaluate(t.AmountUsd, t.OriginCountry, t.DestinationCountry)
        });
        return Ok(result);
    }

    /// <summary>Crea una nueva transacción.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TransactionFormModel form)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.SaveAsync(form);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Edita una transacción existente.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TransactionFormModel form)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.SaveAsync(form, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Elimina una transacción.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Importa un archivo CSV/XLSX de transacciones.</summary>
    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Error = "No se envió ningún archivo" });

        using var stream = file.OpenReadStream();
        var result = await _service.ImportAsync(stream, file.FileName);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
