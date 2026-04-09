using Plugin.Maui.Audio;
using VinhKhanhTour.Models;
using System.Linq;

namespace VinhKhanhTour.Services;

public class NarrationEngine
{
    private IAudioPlayer? _player;
    private static readonly HttpClient _http = new HttpClient();

    public async Task PlayPoiNarrationAsync(Poi poi, bool isManual = false)
    {
        try
        {
            var lang = Services.LocalizationResourceManager.Instance.CurrentLanguageCode;

            if (poi.Translations == null || poi.Translations.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Không có dữ liệu dịch", "OK");
                return;
            }
            var translation = poi.Translations
    .Where(t => t.AudioUrl.Contains("-")) // chỉ lấy audio thật (GUID)
    .FirstOrDefault(t => t.LanguageCode == lang)
    ?? poi.Translations.FirstOrDefault();
            var url = translation?.AudioUrl;


            if (string.IsNullOrEmpty(url))
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Không có audio", "OK");
                return;
            }

            _player?.Stop();

            // 🔥 tải audio background
            var memoryStream = await Task.Run(async () =>
            {
                var bytes = await _http.GetByteArrayAsync(url);
                return new MemoryStream(bytes);
            });

            // 🔥 play trên UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _player = AudioManager.Current.CreatePlayer(memoryStream);
                _player.Play();
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                Application.Current.MainPage.DisplayAlert("Lỗi", ex.Message, "OK"));
        }
    }

    public void CancelCurrentNarration()
    {
        _player?.Stop();
    }
}