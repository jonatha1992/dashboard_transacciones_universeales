namespace Corvux.Models;

/// <summary>
/// Error de validación durante la importación, con fila y campo afectado.
/// </summary>
public record ImportError(int Row, string Field, string Message);
