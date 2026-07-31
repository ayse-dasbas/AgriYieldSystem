// ML modeli tarafından üretilen verim ve hastalık riski tahmin kaydını temsil eden domain entity'si.
namespace AgriYield.Domain.Entities;

public class YieldPrediction
{
    public long Id { get; set; }                              // Birincil anahtar
    public int GreenhouseId { get; set; }                     // İlişkili sera ID'si (Foreign Key)
    public float PredictedYield { get; set; }                 // Tahmin edilen verim (kg/m²)
    public float DiseaseRiskScore { get; set; }               // Hastalık riski skoru (0–100)
    public string RiskLevel { get; set; } = "Low";            // Risk kategorisi: Low / Medium / High
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow; // Tahmin oluşturulma zamanı (UTC)

    public Greenhouse? Greenhouse { get; set; }               // Navigation property — bağlı sera kaydı
}
