// Arka planda periyodik olarak sahte IoT sensör verisi üreten, ML tahmini yapan ve SignalR ile yayınlayan hosted servis.
using AgriYield.Domain.Entities;
using AgriYield.Infrastructure.ML;
using AgriYield.Infrastructure.Persistence;
using AgriYield.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.ML;

namespace AgriYield.Api.Services;

public class IotTelemetrySimulator : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEngine;
    private readonly IHubContext<AgriHub> _hubContext;
    private readonly ILogger<IotTelemetrySimulator> _logger;
    private readonly Random _random = new();
    
    // Sinüs dalgası için açı adımı — zamanla yumuşak iklim değişimleri simüle edilir.
    private double _angleStep = 0.0;

    public IotTelemetrySimulator(
        IServiceProvider serviceProvider,
        PredictionEnginePool<ModelInput, ModelOutput> predictionEngine,
        IHubContext<AgriHub> hubContext,
        ILogger<IotTelemetrySimulator> logger)
    {
        _serviceProvider = serviceProvider;
        _predictionEngine = predictionEngine;
        _hubContext = hubContext;
        _logger = logger;
    }

    // BackgroundService'in ana döngüsü — uygulama çalıştığı sürece telemetri simülasyonu devam eder.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("📡 Kararlı Biyolojik Mantık ve Tutarlı Eşikli Telemetri Servisi Başlatıldı.");

        // Veritabanında en az bir sera kaydı olduğundan emin olunur.
        await EnsureDefaultGreenhouseAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Scoped DbContext için her iterasyonda yeni bir DI scope oluşturulur.
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                int greenhouseId = 1;
                _angleStep += 0.2; // Yumuşak iklim dalgalanması

                // Sinüs fonksiyonu ile doğal iklim dalgalanması temel dalga olarak kullanılır.
                double baseWave = Math.Sin(_angleStep);
                
                // Sıcaklık: 18°C ile 32°C arası son derece yumuşak salınım (Aşırı gürültü kaldırıldı)
                double temp = Math.Round(25.0 + (baseWave * 7.0) + (_random.NextDouble() * 0.8 - 0.4), 2);
                
                // Nem: Sıcaklığa zıt olarak %40 ile %85 arası yumuşak dalgalanır
                double humidity = Math.Round(62.5 - (baseWave * 22.5) + (_random.NextDouble() * 1.6 - 0.8), 2);
                
                // Toprak nemi: ~50–60% aralığında hafif rastgele değişim.
                double soil = Math.Round(55.0 + (_random.NextDouble() * 10.0 - 5.0), 2);

                var sensorLog = new SensorData
                {
                    GreenhouseId = greenhouseId,
                    Temperature = temp,
                    Humidity = humidity,
                    SoilMoisture = soil,
                    Timestamp = DateTime.UtcNow
                };

                dbContext.SensorLogs.Add(sensorLog);

                // ML.NET Verim Tahmini
                var input = new ModelInput
                {
                    Temperature = (float)temp,
                    Humidity = (float)humidity,
                    SoilMoisture = (float)soil
                };

                var prediction = _predictionEngine.GetPredictionEngine("AgriModel").Predict(input);

                // GERÇEKÇİ VE TUTARLI HASTALIK RİSKİ (Tarımsal VPD / Buhar Basıncı Mantığı)
                // Yüksek nem VE yüksek sıcaklık aynı anda olduğunda risk fırlar.
                double rawRisk = ((humidity - 35.0) / 50.0 * 55.0) + ((temp - 15.0) / 18.0 * 45.0);
                float diseaseRisk = (float)Math.Clamp(rawRisk, 5.0, 98.0);
                diseaseRisk = (float)Math.Round(diseaseRisk, 2);

                // TUTARLI VE KARARLI EŞİKLER:
                // Low: <= %35.0 (Yeşil)
                // Medium: %35.1 - %69.9 (Turuncu)
                // High: >= %70.0 (Kırmızı - Gerçekten kritik durumlar)
                string riskLevel = diseaseRisk >= 70.0 ? "High" : (diseaseRisk <= 35.0 ? "Low" : "Medium");

                var predictionLog = new YieldPrediction
                {
                    GreenhouseId = greenhouseId,
                    PredictedYield = (float)Math.Round(prediction.PredictedYield, 2),
                    DiseaseRiskScore = diseaseRisk,
                    RiskLevel = riskLevel,
                    PredictedAt = DateTime.UtcNow
                };

                dbContext.Predictions.Add(predictionLog);
                await dbContext.SaveChangesAsync(stoppingToken);

                // SignalR istemcilerine gönderilecek anonim telemetri paketi oluşturulur.
                var telemetryPayload = new
                {
                    GreenhouseId = greenhouseId,
                    Temperature = temp,
                    Humidity = humidity,
                    SoilMoisture = soil,
                    PredictedYield = predictionLog.PredictedYield,
                    DiseaseRiskScore = predictionLog.DiseaseRiskScore,
                    RiskLevel = riskLevel,
                    Timestamp = sensorLog.Timestamp
                };

                await _hubContext.Clients.All.SendAsync("ReceiveTelemetryUpdate", telemetryPayload, cancellationToken: stoppingToken);
                _logger.LogInformation($"[IoT SignalR] Temp: {temp}°C | Hum: %{humidity} | Risk: %{diseaseRisk} ({riskLevel})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IoT Simülasyonu sırasında hata oluştu.");
            }

            // Her 5 saniyede bir yeni telemetri döngüsü çalıştırılır.
            await Task.Delay(5000, stoppingToken);
        }
    }

    // Veritabanında sera kaydı yoksa varsayılan demo serası eklenir.
    private async Task EnsureDefaultGreenhouseAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!dbContext.Greenhouses.Any())
        {
            dbContext.Greenhouses.Add(new Greenhouse
            {
                Name = "Antalya Akıllı Sera - 1",
                Location = "Teknokent / Antalya",
                CropType = "Salkım Domates"
            });
            await dbContext.SaveChangesAsync();
        }
    }
}
