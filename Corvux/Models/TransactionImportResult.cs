namespace Corvux.Models;

/// <summary>
/// Resultado del parser de archivos (CSV/XLSX).
/// Contiene las filas parseadas y los errores de validación encontrados.
/// </summary>
public class TransactionImportResult
{
    public List<TransactionImportRow> Rows = new();
    public List<ImportError> Errors = new();
    public bool HasErrors => Errors.Count > 0;
}
