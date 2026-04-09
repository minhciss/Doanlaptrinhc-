using Google.Cloud.TextToSpeech.V1;

namespace VinhKhanhCMS.Services;

public class TtsService
{
    private readonly string _apiKey = "AIzaSyAj8iCjTbnluKYvTaOvOpZHWGOPAxW-HAc";

    public async Task<byte[]> GenerateAudio(string text)
    {
        var client = new TextToSpeechClientBuilder
        {
            ApiKey = _apiKey
        }.Build();
        var input = new SynthesisInput
        {
            Text = text
        };

        var voice = new VoiceSelectionParams
        {
            LanguageCode = "vi-VN",
            SsmlGender = SsmlVoiceGender.Female
        };

        var config = new AudioConfig
        {
            AudioEncoding = AudioEncoding.Mp3
        };

        var response = await client.SynthesizeSpeechAsync(input, voice, config);

        return response.AudioContent.ToByteArray();
    }
}