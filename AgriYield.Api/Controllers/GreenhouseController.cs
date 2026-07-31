// Sera (greenhouse) kayıtları ve ilgili telemetri/tahmin geçmişi sorgularını sunan REST API controller.
using AgriYield.Domain.Entities;
using AgriYield.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriYield.Api.Controllers;

[ApiController]
[Route("api/v1/greenhouses")]
public class GreenhouseController : ControllerBase
{
    private readonly AppDbContext _context;

    public GreenhouseController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Sistemde kayıtlı tüm seraları getirir.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllGreenhouses()
    {
        var greenhouses = await _context.Greenhouses.ToListAsync();
        return Ok(greenhouses);
    }

    /// <summary>
    /// Belirtilen seranın en son kaydedilen sensör telemetri verisini getirir.
    /// </summary>
    [HttpGet("{id}/telemetry/latest")]
    public async Task<IActionResult> GetLatestTelemetry(int id)
    {
        // Sera ID'sine göre filtrele, zaman damgasına göre azalan sırala ve en son kaydı al.
        var latestLog = await _context.SensorLogs
            .Where(s => s.GreenhouseId == id)
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefaultAsync();

        if (latestLog == null)
            return NotFound(new { Message = "Bu seraya ait henüz sensör verisi bulunamadı." });

        return Ok(latestLog);
    }

    /// <summary>
    /// Belirtilen seranın ML tarafından üretilmiş verim ve hastalık riski tahmin geçmişini getirir.
    /// </summary>
    [HttpGet("{id}/predictions/history")]
    public async Task<IActionResult> GetPredictionHistory(int id, [FromQuery] int limit = 20)
    {
        // Varsayılan olarak son 20 tahmin kaydı döndürülür; limit query parametresi ile değiştirilebilir.
        var predictions = await _context.Predictions
            .Where(p => p.GreenhouseId == id)
            .OrderByDescending(p => p.PredictedAt)
            .Take(limit)
            .ToListAsync();

        return Ok(predictions);
    }
}
