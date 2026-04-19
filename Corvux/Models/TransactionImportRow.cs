namespace Corvux.Models;

/// <summary>
/// Fila parseada de un archivo CSV/XLSX antes de persistir.
/// </summary>
public record TransactionImportRow(
    string TransactionId,
    decimal AmountUsd,
    string OriginCountry,
    string DestinationCountry,
    int RowNumber
);
