using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using VinhKhanhadmin.Models;
using QRCoder;

public class PoiController : Controller
{
    private readonly HttpClient _http;

    public PoiController(IHttpClientFactory factory)
    {
        _http = factory.CreateClient();
        _http.BaseAddress = new Uri("http://localhost:5137/");
    }

    public async Task<IActionResult> Detail(int id)
    {
        var poi = await _http.GetFromJsonAsync<Poi>($"api/pois/{id}");
        var trans = await _http.GetFromJsonAsync<List<PoiTranslation>>(
            $"api/pois/{id}/translations");

        ViewBag.Translations = trans;

        return View(poi);
    }
    public IActionResult Qr(int id)
    {
        var url = $"http://192.168.31.99:7170/Public/Poi/{id}";

        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);

        var bytes = qrCode.GetGraphic(20);

        return File(bytes, "image/png");
    }
}