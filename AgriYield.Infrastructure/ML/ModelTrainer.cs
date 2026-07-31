// ML.NET regresyon modeli eğitim ve kaydetme işlemlerini gerçekleştiren statik yardımcı sınıf.
using Microsoft.ML;

namespace AgriYield.Infrastructure.ML;

public static class ModelTrainer
{
    // CSV veri setinden SDCA regresyon modeli eğitir ve .zip dosyası olarak diske kaydeder.
    public static void TrainAndSaveModel(string csvPath, string outputPath)
    {
        // Sabit seed (42) ile tekrarlanabilir model eğitimi sağlanır.
        var mlContext = new MLContext(seed: 42);

        // 1. Veriyi Oku — CSV dosyası ModelInput şemasına göre yüklenir.
        IDataView dataView = mlContext.Data.LoadFromTextFile<ModelInput>(
            path: csvPath,
            hasHeader: true,
            separatorChar: ',');

        // 2. Data Pipeline (Sicaklik, Nem, Toprak Nemi -> Feature Vector)
        // Sıcaklık, nem ve toprak nemi birleştirilerek özellik vektörü oluşturulur; SDCA regresyon eğitilir.
        var pipeline = mlContext.Transforms.Concatenate("Features", nameof(ModelInput.Temperature), nameof(ModelInput.Humidity), nameof(ModelInput.SoilMoisture))
            .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: nameof(ModelInput.PredictedYield)));

        // 3. Modeli Egit
        Console.WriteLine("ML.NET Modeli egitiliyor...");
        var model = pipeline.Fit(dataView);

        // 4. .zip Dosyasi Olarak Kaydet — PredictionEnginePool bu dosyayı runtime'da yükler.
        mlContext.Model.Save(model, dataView.Schema, outputPath);
        Console.WriteLine($"Model basariyla kaydedildi: {outputPath}");
    }
}
