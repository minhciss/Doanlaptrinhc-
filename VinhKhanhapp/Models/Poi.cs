using System.Collections.ObjectModel;

namespace VinhKhanhapp.Models;

public class Poi
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Code { get; set; } = string.Empty;

    public double Lat { get; set; }

    public double Lng { get; set; }

    public double RadiusMeters { get; set; } = 40;

    public int Priority { get; set; } = 1;

    public string ImageUrl { get; set; } = string.Empty;

    public string MapLink { get; set; } = string.Empty;

    public ObservableCollection<PoiTranslation> Translations { get; set; } = new();
}

public class PoiActivationResult
{
    public Poi Poi { get; set; } = null!;
    public double DistanceMeters { get; set; }
}