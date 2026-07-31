// Sera (greenhouse) domain entity'si — akıllı tarım sistemindeki fiziksel sera birimini temsil eder.
namespace AgriYield.Domain.Entities;

public class Greenhouse
{
    public int Id { get; set; }                                    // Birincil anahtar
    public string Name { get; set; } = string.Empty;             // Sera adı
    public string Location { get; set; } = string.Empty;           // Coğrafi konum
    public string CropType { get; set; } = string.Empty;         // Yetiştirilen ürün türü
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   // Kayıt oluşturulma zamanı

    // Navigation property'ler — ilişkili sensör logları ve ML tahmin kayıtları.
    public ICollection<SensorData> SensorLogs { get; set; } = new List<SensorData>();
    public ICollection<YieldPrediction> Predictions { get; set; } = new List<YieldPrediction>();
}
