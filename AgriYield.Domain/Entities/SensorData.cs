// IoT sensörlerinden gelen telemetri verisini temsil eden domain entity'si.
namespace AgriYield.Domain.Entities;

public class SensorData
{
    public long Id { get; set; }                          // Birincil anahtar
    public int GreenhouseId { get; set; }                 // İlişkili sera ID'si (Foreign Key)
    public double Temperature { get; set; }               // Sera içi sıcaklık (°C)
    public double Humidity { get; set; }                  // Ortam nemi (%)
    public double SoilMoisture { get; set; }            // Toprak nemi (%)
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Ölçüm zaman damgası (UTC)

    public Greenhouse? Greenhouse { get; set; }           // Navigation property — bağlı sera kaydı
}
