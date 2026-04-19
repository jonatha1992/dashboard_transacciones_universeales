namespace Corvux.Models;

/// <summary>
/// Resultado de operaciones CRUD sobre transacciones.
/// </summary>
public record TransactionCommandResult(bool Success, string? Error = null);
