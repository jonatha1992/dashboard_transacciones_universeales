using Corvux.Models;

namespace Corvux.Services;

/// <summary>
/// Motor de reglas AML. Evalúa compliance sobre una transacción.
/// Sin estado, puro — no depende de I/O ni de DI.
/// </summary>
public class ComplianceRuleEngine
{
    private static readonly HashSet<string> BlockedCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "Iran", "Irán", "North Korea", "Corea del Norte", "DPRK"
    };

    /// <summary>
    /// Evalúa reglas AML sobre una transacción.
    /// - Monto > USD 10.000 → Requiere declaración de fondos
    /// - País bloqueado (Irán, Corea del Norte) → Operación bloqueada
    /// - Monto > USD 50.000 → Tasa administrativa 0.5%
    /// </summary>
    public ComplianceEvaluation Evaluate(decimal amountUsd, string originCountry, string destinationCountry)
    {
        bool requiresFundsDeclaration = amountUsd > 10_000m;

        string? blockReason = null;
        bool isBlocked = false;

        if (IsBlockedCountry(originCountry))
        {
            isBlocked = true;
            blockReason = $"País de origen bloqueado: {originCountry}";
        }
        else if (IsBlockedCountry(destinationCountry))
        {
            isBlocked = true;
            blockReason = $"País de destino bloqueado: {destinationCountry}";
        }

        decimal administrativeFee = amountUsd > 50_000m ? amountUsd * 0.005m : 0m;

        return new ComplianceEvaluation(requiresFundsDeclaration, isBlocked, administrativeFee, blockReason);
    }

    private static bool IsBlockedCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country)) return false;

        var trimmed = country.Trim();
        if (BlockedCountries.Contains(trimmed)) return true;

        // Match parcial para formatos como "Teherán, Iran"
        foreach (var blocked in BlockedCountries)
        {
            if (trimmed.Contains(blocked, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
