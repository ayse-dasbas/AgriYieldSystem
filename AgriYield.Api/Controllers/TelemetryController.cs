// Dış IoT cihazlarından gelen telemetri verilerini alan, ML ile analiz eden ve SignalR ile yayınlayan API controller.
using AgriYield.Domain.Entities;
using AgriYield.Infrastructure.ML;
using AgriYield.Infrastructure.Persistence;
using AgriYield.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.ML;

namespace AgriYield.Api.Controllers;

[ApiController]
[Route("api/v1/telemetry")]
public class TelemetryController : ControllerBase
{
    // Veritabanı erişimi, ML tahmin motoru ve SignalR hub bağlamı constructor injection ile alınır.
    private readonly AppDbContext _context;
    private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEngine;
    private readonly IHubContext<AgriHub> _hubContext;

    public TelemetryController(
        AppDbContext context,
        PredictionEnginePool<ModelInput, ModelOutput> predictionEngine,
        IHubContext<AgriHub> hubContext)
    {
        _context = context;
        _predictionEngine = predictionEngine;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Dış IoT Gateway cihazlarından doğrudan sensör verisi alır, ML modeliyle işler ve SignalR yayını yapar.
    /// </summary>
    [HttpPost("ingest")]
    public async Task<IActionResult> IngestTelemetry([FromBody] SensorData dto)
    {
        // Gelen telemetri kaydına UTC zaman damgası eklenir ve veritabanına yazılır.
        dto.Timestamp = DateTime.UtcNow;
        _context.SensorLogs.Add(dto);

        // ML Tahmini — sensör değerleri ModelInput formatına dönüştürülerek verim tahmini yapılır.
        var input = new ModelInput
        {
            Temperature = (float)dto.Temperature,
            Humidity = (float)dto.Humidity,
            SoilMoisture = (float)dto.SoilMoisture
        };

        var prediction = _predictionEngine.GetPredictionEngine("AgriModel").Predict(input);

        // Hastalık riski skoru: nem ve sıcaklık değerlerine dayalı heuristik formül (5–95 aralığına sıkıştırılır).
        float diseaseRisk = (float)Math.Clamp(((dto.Humidity - 40) / 50.0 * 60) + ((dto.Temperature - 15) / 20.0 * 40), 5.0, 95.0);
        // Risk seviyesi eşik değerlerine göre kategorize edilir: Low / Medium / High.
        string riskLevel = diseaseRisk > 70 ? "High" : (diseaseRisk > 40 ? "Medium" : "Low");

        // Tahmin sonuçları YieldPrediction entity'si olarak veritabanına kaydedilir.
        var predictionLog = new YieldPrediction
        {
            GreenhouseId = dto.GreenhouseId,
            PredictedYield = (float)Math.Round(prediction.PredictedYield, 2),
            DiseaseRiskScore = (float)Math.Round(diseaseRisk, 2),
            RiskLevel = riskLevel,
            PredictedAt = DateTime.UtcNow
        };

        _context.Predictions.Add(predictionLog);
        await _context.SaveChangesAsync();

        // Broadcast via SignalR — tüm bağlı istemcilere güncel telemetri ve tahmin verisi gönderilir.
        await _hubContext.Clients.All.SendAsync("ReceiveTelemetryUpdate", new
        {
            dto.GreenhouseId,
            dto.Temperature,
            dto.Humidity,
            dto.SoilMoisture,
            predictionLog.PredictedYield,
            predictionLog.DiseaseRiskScore,
            predictionLog.RiskLevel,
            dto.Timestamp
        });

        return Ok(new
        {
            Message = "Telemetri başarıyla işlendi ve ML analizi gerçekleştirildi.",
            Prediction = predictionLog
        });
    }
}
