namespace Corvux.Models;

/// <summary>
/// Entidad EF Core persistida en la tabla Transactions.
/// Usa propiedades (no campos) porque EF Core las requiere para LINQ→SQL.
/// </summary>
public class TransactionRecord
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal AmountUsd { get; set; }
    public string OriginCountry { get; set; } = string.Empty;
    public string DestinationCountry { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
