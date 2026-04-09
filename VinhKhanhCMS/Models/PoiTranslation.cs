using System.Text.Json.Serialization;
using VinhKhanhCMS.Models;

public class PoiTranslation
{
    public int Id { get; set; }
    public int PoiId { get; set; }

    public string LanguageCode { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string AudioUrl { get; set; }

    [JsonIgnore] // 🔥 QUAN TRỌNG
    public Poi? Poi { get; set; } // 🔥 cho phép null
}
public class GenerateTranslationRequest
{
    public int PoiId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}
