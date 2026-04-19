using System.ComponentModel.DataAnnotations;

namespace Corvux.Models;

/// <summary>
/// Form binding para Blazor EditForm + DataAnnotationsValidator.
/// Usa propiedades porque EditForm las requiere.
/// </summary>
public class TransactionFormModel
{
    [Required(ErrorMessage = "El ID de transacción es obligatorio")]
    [MaxLength(40, ErrorMessage = "El ID no puede superar 40 caracteres")]
    public string TransactionId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal AmountUsd { get; set; }

    [Required(ErrorMessage = "El país de origen es obligatorio")]
    [MaxLength(80, ErrorMessage = "El país de origen no puede superar 80 caracteres")]
    public string OriginCountry { get; set; } = string.Empty;

    [Required(ErrorMessage = "El país de destino es obligatorio")]
    [MaxLength(80, ErrorMessage = "El país de destino no puede superar 80 caracteres")]
    public string DestinationCountry { get; set; } = string.Empty;
}
