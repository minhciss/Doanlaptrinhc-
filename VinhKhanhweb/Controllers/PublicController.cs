using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using VinhKhanhadmin.Models;

public class PublicController : Controller
{
    private readonly HttpClient _http;

    public PublicController(IHttpClientFactory factory)
    {
        _http = factory.CreateClient();
        _http.BaseAddress = new Uri("http://192.168.31.99:5137/");
    }

    public async Task<IActionResult> Poi(int id)
    {
        var poi = await _http.GetFromJsonAsync<Poi>($"api/pois/{id}");
        var translations = await _http.GetFromJsonAsync<List<PoiTranslation>>(
            $"api/pois/{id}/translations");

        ViewBag.Translations = translations;
        return View(poi);
    }
}