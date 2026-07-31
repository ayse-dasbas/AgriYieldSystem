// AgriYield API uygulamasının giriş noktası — servis kayıtları, ML modeli yükleme ve HTTP pipeline yapılandırması burada yapılır.
using AgriYield.Infrastructure.Persistence;
using AgriYield.Infrastructure.ML;
using AgriYield.Api.Hubs;
using AgriYield.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;

// ASP.NET Core web uygulaması oluşturucusu; appsettings.json ve ortam değişkenlerini otomatik okur.
var builder = WebApplication.CreateBuilder(args);

// SQL Server veritabanı bağlantısı — AppDbContext DI konteynerine kaydedilir.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// REST API controller'ları, Swagger dokümantasyonu ve SignalR gerçek zamanlı iletişim servisleri.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

// Eğitim veri seti (CSV) ve eğitilmiş model (.zip) dosya yolları — çalışma dizinine göre göreceli konumlandırılır.
string csvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "MLModel", "agri_yield_dataset.csv");
string modelPath = Path.Combine(Directory.GetCurrentDirectory(), "AgriModel.zip");

// CSV mevcut ama model dosyası yoksa ilk çalıştırmada otomatik model eğitimi yapılır.
if (File.Exists(csvPath) && !File.Exists(modelPath))
{
    ModelTrainer.TrainAndSaveModel(csvPath, modelPath);
}

// ML.NET tahmin motoru havuzu — kayıtlı model dosyasından yüklenir; dosya değişikliklerini izler (hot-reload).
builder.Services.AddPredictionEnginePool<ModelInput, ModelOutput>()
    .FromFile(modelName: "AgriModel", filePath: modelPath, watchForChanges: true);

// Arka planda çalışan IoT telemetri simülatörü — demo/test amaçlı sensör verisi üretir.
builder.Services.AddHostedService<IotTelemetrySimulator>();

var app = builder.Build();

// Statik Dosyaları (HTML, JS) Sunma Desteği
app.UseDefaultFiles();
app.UseStaticFiles();

// Geliştirme ortamında Swagger UI ile API dokümantasyonu sunulur.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// SignalR hub endpoint'i — istemciler /agriHub adresine WebSocket bağlantısı kurar.
app.MapHub<AgriHub>("/agriHub");

// Controller tabanlı REST API route'ları etkinleştirilir.
app.MapControllers();

app.Run();
