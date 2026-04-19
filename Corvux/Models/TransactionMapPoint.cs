namespace Corvux.Models;

/// <summary>
/// Punto geográfico para el mapa de Leaflet.
/// Record liviano — no se justifican DTOs nuevos.
/// </summary>
public record TransactionMapPoint(
    double Lat,
    double Lng,
    string Country,
    string Label,
    string Color
);
