using System.Text;
using Corvux.Services;

namespace Corvux.Tests;

[TestClass]
public class TransactionImportParserTests
{
    private readonly TransactionImportParser _parser = new();

    // === CSV válido ===

    [TestMethod]
    public async Task ParseCsv_ValidFile_ReturnsRows()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry,DestinationCountry\nTX-001,5000,Argentina,Brasil";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(1, result.Rows.Count);
        Assert.AreEqual("TX-001", result.Rows[0].TransactionId);
    }

    [TestMethod]
    public async Task ParseCsv_MultipleRows_ReturnsAll()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry,DestinationCountry\nTX-001,5000,Argentina,Brasil\nTX-002,12000,USA,Mexico";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(2, result.Rows.Count);
    }

    [TestMethod]
    public async Task ParseCsv_WithQuotes_ParsesCorrectly()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry,DestinationCountry\n\"TX-001\",5000,\"Buenos Aires, Argentina\",Brasil";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual("Buenos Aires, Argentina", result.Rows[0].OriginCountry);
    }

    // === Aliases de encabezados ===

    [TestMethod]
    public async Task ParseCsv_SpanishHeaders_Works()
    {
        var csv = "ID,Monto,País Origen,País Destino\nTX-001,5000,Argentina,Brasil";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(1, result.Rows.Count);
    }

    [TestMethod]
    public async Task ParseCsv_SnakeCaseHeaders_Works()
    {
        var csv = "transaction_id,amount_usd,origin_country,destination_country\nTX-001,5000,Argentina,Brasil";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsFalse(result.HasErrors);
    }

    // === Errores de formato ===

    [TestMethod]
    public async Task ParseCsv_EmptyFile_ReturnsError()
    {
        var stream = ToStream("");
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsTrue(result.HasErrors);
    }

    [TestMethod]
    public async Task ParseCsv_MissingColumn_ReturnsError()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry\nTX-001,5000,Argentina";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any(e => e.Field == "Header"));
    }

    [TestMethod]
    public async Task ParseCsv_InvalidAmount_ReturnsError()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry,DestinationCountry\nTX-001,INVALID,Argentina,Brasil";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any(e => e.Field == "AmountUsd"));
    }

    [TestMethod]
    public async Task ParseCsv_EmptyTransactionId_ReturnsError()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry,DestinationCountry\n,5000,Argentina,Brasil";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any(e => e.Field == "TransactionId"));
    }

    [TestMethod]
    public async Task ParseCsv_EmptyOrigin_ReturnsError()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry,DestinationCountry\nTX-001,5000,,Brasil";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsTrue(result.HasErrors);
    }

    [TestMethod]
    public async Task ParseCsv_NegativeAmount_ReturnsError()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry,DestinationCountry\nTX-001,-500,Argentina,Brasil";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsTrue(result.HasErrors);
    }

    // === Formato de archivo ===

    [TestMethod]
    public async Task Parse_UnsupportedFormat_ReturnsError()
    {
        var stream = ToStream("data");
        var result = await _parser.ParseAsync(stream, "test.txt");
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("no soportado")));
    }

    // === Filas vacías ===

    [TestMethod]
    public async Task ParseCsv_SkipsEmptyRows()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry,DestinationCountry\nTX-001,5000,Argentina,Brasil\n\nTX-002,3000,Chile,Peru";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(2, result.Rows.Count);
    }

    // === Decimal con punto ===

    [TestMethod]
    public async Task ParseCsv_DecimalAmount_ParsesCorrectly()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry,DestinationCountry\nTX-001,12500.50,Argentina,Brasil";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(12500.50m, result.Rows[0].AmountUsd);
    }

    // === Row numbers ===

    [TestMethod]
    public async Task ParseCsv_ErrorIncludesRowNumber()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry,DestinationCountry\nTX-001,5000,Argentina,Brasil\nTX-002,INVALID,Chile,Peru";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsTrue(result.HasErrors);
        Assert.AreEqual(3, result.Errors[0].Row);
    }

    [TestMethod]
    public async Task ParseCsv_HeaderOnly_ReturnsNoRows()
    {
        var csv = "TransactionId,AmountUsd,OriginCountry,DestinationCountry";
        var stream = ToStream(csv);
        var result = await _parser.ParseAsync(stream, "test.csv");
        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(0, result.Rows.Count);
    }

    // === CountryCoordinateMap Tests ===

    [TestMethod]
    public void CountryMap_ExactMatch_ReturnsCoordinates()
    {
        var coords = CountryCoordinateMap.GetCoordinates("Argentina");
        Assert.IsNotNull(coords);
    }

    [TestMethod]
    public void CountryMap_PartialMatch_ReturnsCoordinates()
    {
        var coords = CountryCoordinateMap.GetCoordinates("Berlín, DEU");
        Assert.IsNotNull(coords);
    }

    [TestMethod]
    public void CountryMap_Unknown_ReturnsNull()
    {
        var coords = CountryCoordinateMap.GetCoordinates("PaisInventado");
        Assert.IsNull(coords);
    }

    [TestMethod]
    public void CountryMap_Empty_ReturnsNull()
    {
        var coords = CountryCoordinateMap.GetCoordinates("");
        Assert.IsNull(coords);
    }

    private static MemoryStream ToStream(string content)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }
}
