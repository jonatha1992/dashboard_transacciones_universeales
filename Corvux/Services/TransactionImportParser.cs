using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Corvux.Models;

namespace Corvux.Services;

/// <summary>
/// Parsea archivos CSV y XLSX sin dependencias externas.
/// Soporta aliases de encabezados en español e inglés.
/// </summary>
public class TransactionImportParser
{
    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TransactionId"] = "TransactionId",
        ["ID"] = "TransactionId",
        ["transaction_id"] = "TransactionId",
        ["AmountUsd"] = "AmountUsd",
        ["Amount"] = "AmountUsd",
        ["amount_usd"] = "AmountUsd",
        ["Monto"] = "AmountUsd",
        ["OriginCountry"] = "OriginCountry",
        ["Origin"] = "OriginCountry",
        ["origin_country"] = "OriginCountry",
        ["País Origen"] = "OriginCountry",
        ["PaisOrigen"] = "OriginCountry",
        ["DestinationCountry"] = "DestinationCountry",
        ["Destination"] = "DestinationCountry",
        ["destination_country"] = "DestinationCountry",
        ["País Destino"] = "DestinationCountry",
        ["PaisDestino"] = "DestinationCountry",
    };

    private static readonly string[] RequiredColumns = ["TransactionId", "AmountUsd", "OriginCountry", "DestinationCountry"];

    public async Task<TransactionImportResult> ParseAsync(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" => await ParseCsvAsync(stream),
            ".xlsx" => ParseXlsx(stream),
            _ => new TransactionImportResult
            {
                Errors = { new ImportError(0, "File", $"Formato de archivo no soportado: {extension}. Use .csv o .xlsx") }
            }
        };
    }

    private async Task<TransactionImportResult> ParseCsvAsync(Stream stream)
    {
        var result = new TransactionImportResult();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            result.Errors.Add(new ImportError(1, "Header", "El archivo está vacío o no tiene encabezados"));
            return result;
        }

        var headers = ParseCsvLine(headerLine);
        var columnMap = MapColumns(headers, result);
        if (result.HasErrors) return result;

        int rowNumber = 1;
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            rowNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = ParseCsvLine(line);
            ParseRow(fields, columnMap, rowNumber, result);
        }

        return result;
    }

    private TransactionImportResult ParseXlsx(Stream stream)
    {
        var result = new TransactionImportResult();

        try
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            // Leer shared strings
            var sharedStrings = new List<string>();
            var ssEntry = archive.GetEntry("xl/sharedStrings.xml");
            if (ssEntry != null)
            {
                using var ssStream = ssEntry.Open();
                var ssDoc = XDocument.Load(ssStream);
                var ns = ssDoc.Root?.Name.Namespace ?? XNamespace.None;
                foreach (var si in ssDoc.Descendants(ns + "si"))
                {
                    var text = string.Join("", si.Descendants(ns + "t").Select(t => t.Value));
                    sharedStrings.Add(text);
                }
            }

            // Leer primera hoja
            var sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");
            if (sheetEntry == null)
            {
                result.Errors.Add(new ImportError(0, "File", "No se encontró la hoja sheet1.xml en el archivo XLSX"));
                return result;
            }

            using var sheetStream = sheetEntry.Open();
            var sheetDoc = XDocument.Load(sheetStream);
            var sheetNs = sheetDoc.Root?.Name.Namespace ?? XNamespace.None;

            var rows = sheetDoc.Descendants(sheetNs + "row").ToList();
            if (rows.Count == 0)
            {
                result.Errors.Add(new ImportError(0, "File", "La hoja está vacía"));
                return result;
            }

            // Primera fila = encabezados
            var headerCells = GetCellValues(rows[0], sheetNs, sharedStrings);
            var columnMap = MapColumns(headerCells, result);
            if (result.HasErrors) return result;

            // Filas de datos
            for (int i = 1; i < rows.Count; i++)
            {
                var cellValues = GetCellValues(rows[i], sheetNs, sharedStrings);
                if (cellValues.All(string.IsNullOrWhiteSpace)) continue;
                ParseRow(cellValues, columnMap, i + 1, result);
            }
        }
        catch (InvalidDataException)
        {
            result.Errors.Add(new ImportError(0, "File", "El archivo no es un XLSX válido"));
        }

        return result;
    }

    private static List<string> GetCellValues(XElement row, XNamespace ns, List<string> sharedStrings)
    {
        var cells = row.Elements(ns + "c").ToList();
        var values = new List<string>();

        foreach (var cell in cells)
        {
            var type = cell.Attribute("t")?.Value;
            var valueElement = cell.Element(ns + "v");
            var value = valueElement?.Value ?? "";

            if (type == "s" && int.TryParse(value, out var ssIndex) && ssIndex < sharedStrings.Count)
            {
                value = sharedStrings[ssIndex];
            }

            values.Add(value);
        }

        return values;
    }

    private Dictionary<string, int> MapColumns(List<string> headers, TransactionImportResult result)
    {
        var columnMap = new Dictionary<string, int>();

        for (int i = 0; i < headers.Count; i++)
        {
            var header = headers[i].Trim();
            if (HeaderAliases.TryGetValue(header, out var canonical))
            {
                columnMap[canonical] = i;
            }
        }

        foreach (var required in RequiredColumns)
        {
            if (!columnMap.ContainsKey(required))
            {
                result.Errors.Add(new ImportError(1, "Header", $"Columna obligatoria no encontrada: {required}"));
            }
        }

        return columnMap;
    }

    private static void ParseRow(List<string> fields, Dictionary<string, int> columnMap, int rowNumber, TransactionImportResult result)
    {
        string GetField(string name) =>
            columnMap.TryGetValue(name, out var idx) && idx < fields.Count ? fields[idx].Trim() : "";

        var transactionId = GetField("TransactionId");
        var amountStr = GetField("AmountUsd");
        var origin = GetField("OriginCountry");
        var destination = GetField("DestinationCountry");

        if (string.IsNullOrWhiteSpace(transactionId))
        {
            result.Errors.Add(new ImportError(rowNumber, "TransactionId", "El ID de transacción está vacío"));
            return;
        }

        if (!decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            result.Errors.Add(new ImportError(rowNumber, "AmountUsd", $"Monto inválido: '{amountStr}'"));
            return;
        }

        if (string.IsNullOrWhiteSpace(origin))
        {
            result.Errors.Add(new ImportError(rowNumber, "OriginCountry", "El país de origen está vacío"));
            return;
        }

        if (string.IsNullOrWhiteSpace(destination))
        {
            result.Errors.Add(new ImportError(rowNumber, "DestinationCountry", "El país de destino está vacío"));
            return;
        }

        result.Rows.Add(new TransactionImportRow(transactionId, amount, origin, destination, rowNumber));
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // skip escaped quote
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
}
