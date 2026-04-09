namespace VinhKhanhCMS.Models;

using System.Text.Json.Serialization;

public class Poi
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Address { get; set; } = "";

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string ImageUrl { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public List<PoiTranslation>? Translations { get; set; }
}