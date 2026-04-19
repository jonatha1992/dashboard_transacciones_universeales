using Corvux.Services;
using Corvux.Models;

namespace Corvux.Tests;

[TestClass]
public class ComplianceRuleEngineTests
{
    private readonly ComplianceRuleEngine _engine = new();

    // === Declaración de fondos ===

    [TestMethod]
    public void Amount_Below_10000_DoesNot_RequireDeclaration()
    {
        var result = _engine.Evaluate(5000m, "Argentina", "Brasil");
        Assert.IsFalse(result.RequiresFundsDeclaration);
    }

    [TestMethod]
    public void Amount_Equal_10000_DoesNot_RequireDeclaration()
    {
        var result = _engine.Evaluate(10000m, "Argentina", "Brasil");
        Assert.IsFalse(result.RequiresFundsDeclaration);
    }

    [TestMethod]
    public void Amount_Above_10000_RequiresDeclaration()
    {
        var result = _engine.Evaluate(10001m, "Argentina", "Brasil");
        Assert.IsTrue(result.RequiresFundsDeclaration);
    }

    [TestMethod]
    public void Amount_50001_RequiresDeclaration()
    {
        var result = _engine.Evaluate(50001m, "Argentina", "Brasil");
        Assert.IsTrue(result.RequiresFundsDeclaration);
    }

    // === Países bloqueados ===

    [TestMethod]
    public void Origin_Iran_IsBlocked()
    {
        var result = _engine.Evaluate(1000m, "Iran", "Argentina");
        Assert.IsTrue(result.IsBlocked);
        Assert.IsNotNull(result.BlockReason);
    }

    [TestMethod]
    public void Origin_Iran_Spanish_IsBlocked()
    {
        var result = _engine.Evaluate(1000m, "Irán", "Argentina");
        Assert.IsTrue(result.IsBlocked);
    }

    [TestMethod]
    public void Destination_NorthKorea_IsBlocked()
    {
        var result = _engine.Evaluate(1000m, "Argentina", "North Korea");
        Assert.IsTrue(result.IsBlocked);
    }

    [TestMethod]
    public void Destination_CoreaDelNorte_IsBlocked()
    {
        var result = _engine.Evaluate(1000m, "Argentina", "Corea del Norte");
        Assert.IsTrue(result.IsBlocked);
    }

    [TestMethod]
    public void Destination_DPRK_IsBlocked()
    {
        var result = _engine.Evaluate(1000m, "Argentina", "DPRK");
        Assert.IsTrue(result.IsBlocked);
    }

    [TestMethod]
    public void PartialMatch_Tehran_Iran_IsBlocked()
    {
        var result = _engine.Evaluate(1000m, "Teherán, Iran", "Argentina");
        Assert.IsTrue(result.IsBlocked);
    }

    [TestMethod]
    public void SafeCountries_NotBlocked()
    {
        var result = _engine.Evaluate(1000m, "Argentina", "Brasil");
        Assert.IsFalse(result.IsBlocked);
        Assert.IsNull(result.BlockReason);
    }

    // === Tasa administrativa ===

    [TestMethod]
    public void Amount_Below_50000_NoFee()
    {
        var result = _engine.Evaluate(49999m, "Argentina", "Brasil");
        Assert.AreEqual(0m, result.AdministrativeFee);
    }

    [TestMethod]
    public void Amount_Equal_50000_NoFee()
    {
        var result = _engine.Evaluate(50000m, "Argentina", "Brasil");
        Assert.AreEqual(0m, result.AdministrativeFee);
    }

    [TestMethod]
    public void Amount_Above_50000_HasFee_05Percent()
    {
        var result = _engine.Evaluate(100000m, "Argentina", "Brasil");
        Assert.AreEqual(500m, result.AdministrativeFee);
    }

    [TestMethod]
    public void Amount_60000_Fee_Is_300()
    {
        var result = _engine.Evaluate(60000m, "Argentina", "Brasil");
        Assert.AreEqual(300m, result.AdministrativeFee);
    }

    // === Combinaciones ===

    [TestMethod]
    public void Blocked_And_HighAmount_AllFlagsSet()
    {
        var result = _engine.Evaluate(100000m, "Iran", "Argentina");
        Assert.IsTrue(result.IsBlocked);
        Assert.IsTrue(result.RequiresFundsDeclaration);
        Assert.AreEqual(500m, result.AdministrativeFee);
    }

    [TestMethod]
    public void EmptyCountry_NotBlocked()
    {
        var result = _engine.Evaluate(1000m, "", "Brasil");
        Assert.IsFalse(result.IsBlocked);
    }
}
