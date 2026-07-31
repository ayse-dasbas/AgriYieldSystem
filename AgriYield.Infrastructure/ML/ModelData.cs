// ML.NET modeli için giriş/çıkış veri sınıfları — CSV sütun eşlemeleri ve tahmin sonucu tanımları.
using Microsoft.ML.Data;

namespace AgriYield.Infrastructure.ML;

// Model eğitimi ve tahmin aşamasında kullanılan giriş verisi; CSV'deki sütun sırasıyla eşleşir.
public class ModelInput
{
    [LoadColumn(0)] public float Temperature { get; set; }      // Sera sıcaklığı (°C)
    [LoadColumn(1)] public float Humidity { get; set; }         // Ortam nemi (%)
    [LoadColumn(2)] public float SoilMoisture { get; set; }     // Toprak nemi (%)
    [LoadColumn(3)] public float PredictedYield { get; set; }  // Eğitim sırasında hedef etiket (kg/m²)
    [LoadColumn(4)] public float DiseaseRiskScore { get; set; } // CSV'deki hastalık riski (eğitimde kullanılmaz)
}

// Model tahmin çıktısı — regresyon skoru verim tahmini olarak yorumlanır.
public class ModelOutput
{
    [ColumnName("Score")]
    public float PredictedYield { get; set; }
}
