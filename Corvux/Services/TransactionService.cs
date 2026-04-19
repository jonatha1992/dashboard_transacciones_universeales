using Microsoft.EntityFrameworkCore;
using Corvux.Data;
using Corvux.Models;

namespace Corvux.Services;

/// <summary>
/// Orquesta operaciones CRUD e importación masiva.
/// Usa IDbContextFactory para crear contextos cortos — correcto para Blazor Server.
/// </summary>
public class TransactionService
{
    private readonly IDbContextFactory<ComplianceDbContext> _factory;
    private readonly TransactionImportParser _parser;

    public TransactionService(IDbContextFactory<ComplianceDbContext> factory, TransactionImportParser parser)
    {
        _factory = factory;
        _parser = parser;
    }

    public async Task<List<TransactionRecord>> GetTransactionsAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Transactions.OrderByDescending(t => t.CreatedAtUtc).ToListAsync();
    }

    public async Task<TransactionCommandResult> SaveAsync(TransactionFormModel form, int? editingId = null)
    {
        await using var db = await _factory.CreateDbContextAsync();

        // Verificar TransactionId duplicado
        var existing = await db.Transactions.FirstOrDefaultAsync(t => t.TransactionId == form.TransactionId);
        if (existing != null && existing.Id != editingId)
        {
            return new TransactionCommandResult(false, $"Ya existe una transacción con ID '{form.TransactionId}'");
        }

        if (editingId.HasValue)
        {
            // Editar
            var record = await db.Transactions.FindAsync(editingId.Value);
            if (record == null)
                return new TransactionCommandResult(false, "Transacción no encontrada");

            record.TransactionId = form.TransactionId;
            record.AmountUsd = form.AmountUsd;
            record.OriginCountry = form.OriginCountry;
            record.DestinationCountry = form.DestinationCountry;
            record.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            // Crear
            db.Transactions.Add(new TransactionRecord
            {
                TransactionId = form.TransactionId,
                AmountUsd = form.AmountUsd,
                OriginCountry = form.OriginCountry,
                DestinationCountry = form.DestinationCountry,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
        return new TransactionCommandResult(true);
    }

    public async Task<TransactionCommandResult> DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var record = await db.Transactions.FindAsync(id);
        if (record == null)
            return new TransactionCommandResult(false, "Transacción no encontrada");

        db.Transactions.Remove(record);
        await db.SaveChangesAsync();
        return new TransactionCommandResult(true);
    }

    public async Task<TransactionImportExecutionResult> ImportAsync(Stream stream, string fileName)
    {
        var parseResult = await _parser.ParseAsync(stream, fileName);

        if (parseResult.HasErrors)
        {
            return new TransactionImportExecutionResult(false, 0, parseResult.Errors);
        }

        if (parseResult.Rows.Count == 0)
        {
            return new TransactionImportExecutionResult(false, 0,
                [new ImportError(0, "File", "El archivo no contiene filas de datos")]);
        }

        // Validar duplicados contra la base de datos
        await using var db = await _factory.CreateDbContextAsync();
        var existingIds = await db.Transactions.Select(t => t.TransactionId).ToListAsync();
        var existingSet = new HashSet<string>(existingIds, StringComparer.OrdinalIgnoreCase);

        var errors = new List<ImportError>();
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in parseResult.Rows)
        {
            if (existingSet.Contains(row.TransactionId))
            {
                errors.Add(new ImportError(row.RowNumber, "TransactionId",
                    $"ID '{row.TransactionId}' ya existe en la base de datos"));
            }
            else if (!seenIds.Add(row.TransactionId))
            {
                errors.Add(new ImportError(row.RowNumber, "TransactionId",
                    $"ID '{row.TransactionId}' está duplicado en el archivo"));
            }
        }

        if (errors.Count > 0)
        {
            return new TransactionImportExecutionResult(false, 0, errors);
        }

        // Persistir en lote
        var now = DateTime.UtcNow;
        var records = parseResult.Rows.Select(row => new TransactionRecord
        {
            TransactionId = row.TransactionId,
            AmountUsd = row.AmountUsd,
            OriginCountry = row.OriginCountry,
            DestinationCountry = row.DestinationCountry,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        db.Transactions.AddRange(records);
        await db.SaveChangesAsync();

        return new TransactionImportExecutionResult(true, parseResult.Rows.Count, []);
    }
}
