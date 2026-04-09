using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanhCMS.Data;
using VinhKhanhCMS.Models;

namespace VinhKhanhCMS.Controllers;

[ApiController]
[Route("api/pois")]
public class PoiController : ControllerBase
{
    private readonly AppDbContext _context;

    public PoiController(AppDbContext context)
    {
        _context = context;
    }

    // 🔍 Lấy tất cả POI
    [HttpGet]
    public IActionResult GetAll()
    {
        var data = _context.Pois
            .Include(p => p.Translations)
            .ToList();

        // 🔥 FIX AUDIO URL
        var baseUrl = "http://10.0.2.2:5137"; // Android Emulator

        foreach (var poi in data)
        {
            // ✅ FIX IMAGE
            if (!string.IsNullOrEmpty(poi.ImageUrl) && !poi.ImageUrl.StartsWith("http"))
            {
                poi.ImageUrl = baseUrl + "/images/" + poi.ImageUrl;
            }

            // FIX AUDIO
            if (poi.Translations != null)
            {
                foreach (var t in poi.Translations)
                {
                    if (!string.IsNullOrEmpty(t.AudioUrl) && !t.AudioUrl.StartsWith("http"))
                    {
                        t.AudioUrl = baseUrl + t.AudioUrl;
                    }
                }
            }
        }

        return Ok(data);
    }
    [HttpPut("{id}/toggle")]
    public IActionResult Toggle(int id)
    {
        var poi = _context.Pois.Find(id);
        if (poi == null) return NotFound();

        poi.IsActive = !poi.IsActive;
        _context.SaveChanges();

        return Ok(poi);
    }

    // 🔍 Lấy 1 POI theo id
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var poi = _context.Pois
            .Include(p => p.Translations)
            .FirstOrDefault(p => p.Id == id);

        if (poi == null) return NotFound();


        // 🔥 FIX AUDIO URL
        var baseUrl = "http://10.0.2.2:5137";

        if (poi.Translations != null)
        {
            foreach (var t in poi.Translations)
            {
                if (!string.IsNullOrEmpty(t.AudioUrl) && !t.AudioUrl.StartsWith("http"))
                {
                    t.AudioUrl = baseUrl + t.AudioUrl;
                }
            }
        }

        return Ok(poi);
    }
    // ➕ Tạo POI
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Poi poi)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Pois.Add(poi);
        await _context.SaveChangesAsync();

        return Ok(poi);
    }

    // ✏️ Update POI
    [HttpPut("{id}")]
    public IActionResult Update(int id, Poi updated)
    {
        var poi = _context.Pois.Find(id);
        if (poi == null) return NotFound();

        poi.Name = updated.Name;
        poi.Description = updated.Description;
        poi.Address = updated.Address;
        poi.Latitude = updated.Latitude;
        poi.Longitude = updated.Longitude;
        poi.ImageUrl = updated.ImageUrl;
        poi.IsActive = updated.IsActive;
        _context.SaveChanges();

        return Ok(poi);
    }

    // ❌ Xóa POI
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var poi = _context.Pois.Find(id);
        if (poi == null) return NotFound();

        _context.Pois.Remove(poi);
        _context.SaveChanges();

        return Ok();
    }
}