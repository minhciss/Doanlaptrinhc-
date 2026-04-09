using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using VinhKhanhTour.Models;
namespace VinhKhanhTour.Models
{
    public partial class Poi : ObservableObject
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameEs { get; set; } = string.Empty;
        public string NameFr { get; set; } = string.Empty;
        public string NameDe { get; set; } = string.Empty;
        public string NameZh { get; set; } = string.Empty;
        public string NameJa { get; set; } = string.Empty;
        public string NameKo { get; set; } = string.Empty;
        public string NameRu { get; set; } = string.Empty;
        public string NameIt { get; set; } = string.Empty;
        public string NamePt { get; set; } = string.Empty;
        public string NameHi { get; set; } = string.Empty;

        public string? AudioUrl { get; set; }
        public string AudioFile { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; } = 30;
        public int Priority { get; set; }

        // ================= TRANSLATIONS =================
        [Ignore]
        public List<PoiTranslation> Translations { get; set; } = new();

        public class PoiTranslation
        {
            public int Id { get; set; }
            public int PoiId { get; set; }
            public string LanguageCode { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string AudioUrl { get; set; } = string.Empty;
        }

        // ================= DISTANCE =================
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayDistanceText))]
        [NotifyPropertyChangedFor(nameof(ListDisplayDistanceText))]
        private double? _distanceToUser;
        [Ignore]
        public ImageSource DisplayImageSource
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ImageUrl))
                    return "poi_placeholder.png";

                // Nếu là link online
                if (ImageUrl.StartsWith("http"))
                    return ImageSource.FromUri(new Uri(ImageUrl));

                // Nếu là file local
                return ImageSource.FromFile(ImageUrl);
            }
        }

        [Ignore]
        public string DisplayDistanceText =>
            DistanceToUser.HasValue
                ? $"📍 {GetText("Cách bạn")}: {DistanceToUser.Value:F0}m"
                : $"📍 {GetText("Đang định vị...")}";

        [Ignore]
        public string ListDisplayDistanceText =>
            DistanceToUser.HasValue ? $"📍 {DistanceToUser.Value:F0}m" : "📍 ---";

        [Ignore]
        public string DisplayRadiusText =>
            $"📏 {GetText("Bán kính")}: {Radius}m";

        [Ignore]
        public string ListDisplayRadiusText =>
            $"📏 {Radius}m";

        // ================= DESCRIPTION =================
        public string Description { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionEs { get; set; } = string.Empty;
        public string DescriptionFr { get; set; } = string.Empty;
        public string DescriptionDe { get; set; } = string.Empty;
        public string DescriptionZh { get; set; } = string.Empty;
        public string DescriptionJa { get; set; } = string.Empty;
        public string DescriptionKo { get; set; } = string.Empty;
        public string DescriptionRu { get; set; } = string.Empty;
        public string DescriptionIt { get; set; } = string.Empty;
        public string DescriptionPt { get; set; } = string.Empty;
        public string DescriptionHi { get; set; } = string.Empty;

        // ================= TTS =================
        public string TtsScript { get; set; } = string.Empty;
        public string TtsScriptEn { get; set; } = string.Empty;
        public string TtsScriptEs { get; set; } = string.Empty;
        public string TtsScriptFr { get; set; } = string.Empty;
        public string TtsScriptDe { get; set; } = string.Empty;
        public string TtsScriptZh { get; set; } = string.Empty;
        public string TtsScriptJa { get; set; } = string.Empty;
        public string TtsScriptKo { get; set; } = string.Empty;
        public string TtsScriptRu { get; set; } = string.Empty;
        public string TtsScriptIt { get; set; } = string.Empty;
        public string TtsScriptPt { get; set; } = string.Empty;
        public string TtsScriptHi { get; set; } = string.Empty;

        // ================= DISPLAY =================
        [Ignore]
        public string DisplayImage =>
            string.IsNullOrWhiteSpace(ImageUrl)
                ? "poi_placeholder.png"
                : ImageUrl;

        [Ignore]
        public string DisplayName => GetLocalized(Name, NameEn, NameEs, NameFr, NameDe,
                                                  NameZh, NameJa, NameKo, NameRu,
                                                  NameIt, NamePt, NameHi);

        [Ignore]
        public string DisplayDescription => GetLocalized(Description, DescriptionEn, DescriptionEs,
                                                         DescriptionFr, DescriptionDe, DescriptionZh,
                                                         DescriptionJa, DescriptionKo, DescriptionRu,
                                                         DescriptionIt, DescriptionPt, DescriptionHi);

        [Ignore]
        public string DisplayTtsScript => GetLocalized(TtsScript, TtsScriptEn, TtsScriptEs,
                                                      TtsScriptFr, TtsScriptDe, TtsScriptZh,
                                                      TtsScriptJa, TtsScriptKo, TtsScriptRu,
                                                      TtsScriptIt, TtsScriptPt, TtsScriptHi);

        // ================= HELPER =================
        private string GetText(string key)
        {
            return Services.LocalizationResourceManager.Instance?[key] ?? key;
        }

        private string GetLocalized(string vi, string en, string es, string fr, string de,
                                    string zh, string ja, string ko, string ru,
                                    string it, string pt, string hi)
        {
            var lang = Services.LocalizationResourceManager.Instance?.CurrentLanguageCode ?? "vi";

            return lang switch
            {
                "en" => !string.IsNullOrWhiteSpace(en) ? en : vi,
                "es" => !string.IsNullOrWhiteSpace(es) ? es : vi,
                "fr" => !string.IsNullOrWhiteSpace(fr) ? fr : vi,
                "de" => !string.IsNullOrWhiteSpace(de) ? de : vi,
                "zh" => !string.IsNullOrWhiteSpace(zh) ? zh : vi,
                "ja" => !string.IsNullOrWhiteSpace(ja) ? ja : vi,
                "ko" => !string.IsNullOrWhiteSpace(ko) ? ko : vi,
                "ru" => !string.IsNullOrWhiteSpace(ru) ? ru : vi,
                "it" => !string.IsNullOrWhiteSpace(it) ? it : vi,
                "pt" => !string.IsNullOrWhiteSpace(pt) ? pt : vi,
                "hi" => !string.IsNullOrWhiteSpace(hi) ? hi : vi,
                _ => vi
            };
        }
        public static List<Poi> GetSampleData()
        {
            return new List<Poi>
    {
        new Poi
        {
            Name = "Chợ Bến Thành",
            NameEn = "Ben Thanh Market",
            Description = "Khu chợ nổi tiếng tại TP.HCM",
            DescriptionEn = "Famous market in Ho Chi Minh City",
            Latitude = 10.772,
            Longitude = 106.698,
            ImageUrl = "ben_thanh.jpg",
            Radius = 50,
            Priority = 1
        },
        new Poi
        {
            Name = "Nhà thờ Đức Bà",
            NameEn = "Notre-Dame Cathedral",
            Description = "Nhà thờ cổ nổi tiếng",
            DescriptionEn = "Historic cathedral",
            Latitude = 10.779,
            Longitude = 106.699,
            ImageUrl = "duc_ba.jpg",
            Radius = 50,
            Priority = 2
        }
    };
        }
    }
   



    }
