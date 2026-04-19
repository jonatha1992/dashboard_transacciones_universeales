namespace Corvux.Services;

/// <summary>
/// Diccionario estático de 80+ países/ciudades a coordenadas (Lat, Lng).
/// Búsqueda exacta y parcial por nombre normalizado.
/// Sin inyección de dependencias — datos inmutables.
/// </summary>
public static class CountryCoordinateMap
{
    private static readonly Dictionary<string, (double Lat, double Lng)> Coordinates = new(StringComparer.OrdinalIgnoreCase)
    {
        // América del Norte
        ["Estados Unidos"] = (39.8283, -98.5795),
        ["United States"] = (39.8283, -98.5795),
        ["USA"] = (39.8283, -98.5795),
        ["US"] = (39.8283, -98.5795),
        ["Canadá"] = (56.1304, -106.3468),
        ["Canada"] = (56.1304, -106.3468),
        ["México"] = (23.6345, -102.5528),
        ["Mexico"] = (23.6345, -102.5528),

        // América del Sur
        ["Argentina"] = (-38.4161, -63.6167),
        ["Brasil"] = (-14.2350, -51.9253),
        ["Brazil"] = (-14.2350, -51.9253),
        ["Chile"] = (-35.6751, -71.5430),
        ["Colombia"] = (4.5709, -74.2973),
        ["Perú"] = (-9.1900, -75.0152),
        ["Peru"] = (-9.1900, -75.0152),
        ["Uruguay"] = (-32.5228, -55.7658),
        ["Venezuela"] = (6.4238, -66.5897),
        ["Ecuador"] = (-1.8312, -78.1834),
        ["Bolivia"] = (-16.2902, -63.5887),
        ["Paraguay"] = (-23.4425, -58.4438),

        // América Central y Caribe
        ["Panamá"] = (8.5380, -80.7821),
        ["Panama"] = (8.5380, -80.7821),
        ["Costa Rica"] = (9.7489, -83.7534),
        ["Cuba"] = (21.5218, -77.7812),

        // Europa
        ["España"] = (40.4637, -3.7492),
        ["Spain"] = (40.4637, -3.7492),
        ["Francia"] = (46.2276, 2.2137),
        ["France"] = (46.2276, 2.2137),
        ["Alemania"] = (51.1657, 10.4515),
        ["Germany"] = (51.1657, 10.4515),
        ["DEU"] = (51.1657, 10.4515),
        ["Italia"] = (41.8719, 12.5674),
        ["Italy"] = (41.8719, 12.5674),
        ["Reino Unido"] = (55.3781, -3.4360),
        ["United Kingdom"] = (55.3781, -3.4360),
        ["UK"] = (55.3781, -3.4360),
        ["GBR"] = (55.3781, -3.4360),
        ["Portugal"] = (39.3999, -8.2245),
        ["Países Bajos"] = (52.1326, 5.2913),
        ["Netherlands"] = (52.1326, 5.2913),
        ["Bélgica"] = (50.5039, 4.4699),
        ["Belgium"] = (50.5039, 4.4699),
        ["Suiza"] = (46.8182, 8.2275),
        ["Switzerland"] = (46.8182, 8.2275),
        ["Austria"] = (47.5162, 14.5501),
        ["Suecia"] = (60.1282, 18.6435),
        ["Sweden"] = (60.1282, 18.6435),
        ["Noruega"] = (60.4720, 8.4689),
        ["Norway"] = (60.4720, 8.4689),
        ["Dinamarca"] = (56.2639, 9.5018),
        ["Denmark"] = (56.2639, 9.5018),
        ["Finlandia"] = (61.9241, 25.7482),
        ["Finland"] = (61.9241, 25.7482),
        ["Irlanda"] = (53.1424, -7.6921),
        ["Ireland"] = (53.1424, -7.6921),
        ["Polonia"] = (51.9194, 19.1451),
        ["Poland"] = (51.9194, 19.1451),
        ["Grecia"] = (39.0742, 21.8243),
        ["Greece"] = (39.0742, 21.8243),
        ["Rusia"] = (61.5240, 105.3188),
        ["Russia"] = (61.5240, 105.3188),
        ["Ucrania"] = (48.3794, 31.1656),
        ["Ukraine"] = (48.3794, 31.1656),
        ["Rumania"] = (45.9432, 24.9668),
        ["Romania"] = (45.9432, 24.9668),
        ["República Checa"] = (49.8175, 15.4730),
        ["Czech Republic"] = (49.8175, 15.4730),
        ["Hungría"] = (47.1625, 19.5033),
        ["Hungary"] = (47.1625, 19.5033),

        // Asia
        ["China"] = (35.8617, 104.1954),
        ["Japón"] = (36.2048, 138.2529),
        ["Japan"] = (36.2048, 138.2529),
        ["JPN"] = (36.2048, 138.2529),
        ["Corea del Sur"] = (35.9078, 127.7669),
        ["South Korea"] = (35.9078, 127.7669),
        ["India"] = (20.5937, 78.9629),
        ["Tailandia"] = (15.8700, 100.9925),
        ["Thailand"] = (15.8700, 100.9925),
        ["Vietnam"] = (14.0583, 108.2772),
        ["Singapur"] = (1.3521, 103.8198),
        ["Singapore"] = (1.3521, 103.8198),
        ["Indonesia"] = (-0.7893, 113.9213),
        ["Malasia"] = (4.2105, 101.9758),
        ["Malaysia"] = (4.2105, 101.9758),
        ["Filipinas"] = (12.8797, 121.7740),
        ["Philippines"] = (12.8797, 121.7740),
        ["Taiwán"] = (23.6978, 120.9605),
        ["Taiwan"] = (23.6978, 120.9605),
        ["Hong Kong"] = (22.3193, 114.1694),
        ["Israel"] = (31.0461, 34.8516),
        ["Turquía"] = (38.9637, 35.2433),
        ["Turkey"] = (38.9637, 35.2433),
        ["Arabia Saudita"] = (23.8859, 45.0792),
        ["Saudi Arabia"] = (23.8859, 45.0792),
        ["Emiratos Árabes Unidos"] = (23.4241, 53.8478),
        ["UAE"] = (23.4241, 53.8478),
        ["United Arab Emirates"] = (23.4241, 53.8478),

        // Países bloqueados AML
        ["Irán"] = (32.4279, 53.6880),
        ["Iran"] = (32.4279, 53.6880),
        ["IRN"] = (32.4279, 53.6880),
        ["Corea del Norte"] = (40.3399, 127.5101),
        ["North Korea"] = (40.3399, 127.5101),
        ["DPRK"] = (40.3399, 127.5101),
        ["PRK"] = (40.3399, 127.5101),

        // África
        ["Sudáfrica"] = (-30.5595, 22.9375),
        ["South Africa"] = (-30.5595, 22.9375),
        ["Nigeria"] = (9.0820, 8.6753),
        ["Egipto"] = (26.8206, 30.8025),
        ["Egypt"] = (26.8206, 30.8025),
        ["Marruecos"] = (31.7917, -7.0926),
        ["Morocco"] = (31.7917, -7.0926),
        ["Kenia"] = (-0.0236, 37.9062),
        ["Kenya"] = (-0.0236, 37.9062),

        // Oceanía
        ["Australia"] = (-25.2744, 133.7751),
        ["Nueva Zelanda"] = (-40.9006, 174.8860),
        ["New Zealand"] = (-40.9006, 174.8860),
    };

    /// <summary>
    /// Intenta resolver coordenadas para un país.
    /// Primero intenta match exacto, luego parcial.
    /// </summary>
    public static (double Lat, double Lng)? GetCoordinates(string country)
    {
        if (string.IsNullOrWhiteSpace(country)) return null;

        var trimmed = country.Trim();

        // Match exacto
        if (Coordinates.TryGetValue(trimmed, out var coords))
            return coords;

        // Match parcial — para formatos como "Berlín, DEU"
        var normalized = trimmed.ToLowerInvariant();
        foreach (var kvp in Coordinates)
        {
            if (normalized.Contains(kvp.Key.ToLowerInvariant()))
                return kvp.Value;
        }

        return null;
    }
}
