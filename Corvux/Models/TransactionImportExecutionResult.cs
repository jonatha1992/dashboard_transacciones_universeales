namespace Corvux.Models;

/// <summary>
/// Resultado del proceso completo de importación (parseo + persistencia).
/// </summary>
public record TransactionImportExecutionResult(
    bool Success,
    int ImportedCount,
    List<ImportError> Errors
);
