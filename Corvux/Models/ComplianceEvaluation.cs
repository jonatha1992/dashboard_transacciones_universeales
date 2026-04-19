namespace Corvux.Models;

/// <summary>
/// Resultado de la evaluación de reglas de compliance sobre una transacción.
/// </summary>
public record ComplianceEvaluation(
    bool RequiresFundsDeclaration,
    bool IsBlocked,
    decimal AdministrativeFee,
    string? BlockReason = null
);
