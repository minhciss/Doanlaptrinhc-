namespace VinhKhanhapp.Models;

public class PoiTranslation
{
    public string Language { get; set; } = "vi";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? TtsScript { get; set; }
    public string? AudioFile { get; set; }
}