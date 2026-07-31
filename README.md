
```markdown
#  AgriYield / SmartGreen B2B

### Akıllı Sera IoT & ML Tabanlı Canlı Karar Destek Platformu

AgriYield, endüstriyel seralardaki iklim koşullarını anlık olarak izleyen, Makine Öğrenmesi (ML) desteğiyle metrekare başına rekolte tahmini üreten ve ürün hastalık risklerini henüz patlak vermeden tespit eden modern bir B2B Karar Destek Platformudur (Decision Support System).

---

##  Neden AgriYield? (İş Değeri & Çözüm)

Geleneksel seracılıkta yüksek nem ve dengesiz sıcaklık dalgalanmaları; Külleme ve Kurşuni Küf gibi mantar hastalıklarına yol açarak rekolte kayıplarına neden olur.

AgriYield, sahadaki IoT sensörlerinden aldığı telemetri verilerini milisaniyelik gecikmeyle işler:

* **Erken Teşhis:** Yüksek nem ve sıcaklık kombinasyonlarını analiz ederek hastalık riskini (%0 - %100) canlı hesaplar.
* **Rekolte Tahmini:** Sıcaklık, nem ve toprak nemi değerlerine göre metrekare başına ürün verimini ($\text{kg/m}^2$) tahmin eder.
* **Canlı Akış:** Verileri web paneline sayfa yenilemeye gerek kalmadan SignalR WebSockets ile basar.

---

##  Mimari Yapı (Clean Architecture)

Proje, bağımlılıkları minimuma indiren ve test edilebilirliği artıran Clean Architecture (Onion Architecture) prensiplerine uygun olarak 4 temel katmanda geliştirilmiştir:

```text
AgriYieldSystem/
├── AgriYield.Domain/         # Entity'ler, Domain Modelleri ve Sabitler
├── AgriYield.Application/    # DTO'lar, Interfaces ve İş Mantığı
├── AgriYield.Infrastructure/ # DbContext, ML.NET Modeli ve Veritabanı
└── AgriYield.Api/            # Controllers, SignalR Hubs, Background Services

```

###  Mimari ve Veri Akış Şeması

```plain
[ IoT Cihazları / Telemetri Simülatörü ]
                  │
                  ▼ (Anlık Sıcaklık, Nem, Toprak Nemi)
    [ AgriYield.Api Ingestion Layer ]
                  │
         ┌────────┴────────┐
         ▼                 ▼
  [ EF Core DbContext ]  [ ML.NET Tahmin Motoru ]
  (Veri Kaydı)           + (Biyolojik Risk Analiz Motoru)
         │                 │
         └────────┬────────┘
                  ▼
         [ ASP.NET Core SignalR ]
                  │ (Real-Time Push / WebSockets)
                  ▼
     [ Canlı Web Dashboard / UI ]

```

###  Öne Çıkan Teknik Detaylar

* **Thread-Safe ML.NET Integration:** Ölçeklenebilir performans için `PredictionEnginePool<ModelInput, ModelOutput>` yapısı kullanılmıştır.
* **IoT Telemetry Ingestion API:** Fiziksel IoT cihazlarının (`ESP32`, `Raspberry Pi` vb.) veri atabileceği `POST /api/v1/telemetry/ingest` REST endpoint'i mevcuttur.
* **Real-World Accelerated Simulator:** Cihaz entegrasyonu öncesi sistem tepkilerini ve risk geçişlerini doğrulamak için `BackgroundService` tabanlı, doğal sensör gürültüsü (`Sensor Noise/Jitter`) içeren hızlandırılmış bir iklim simülatörü kurgulanmıştır.

---

##  Kullanılan Teknolojiler

* **Backend Framework:** .NET 10 Web API
* **Language:** C# 13
* **Real-Time Network:** ASP.NET Core SignalR (WebSocket Protocol)
* **Machine Learning:** ML.NET (Regression Engine)
* **Database & ORM:** Entity Framework Core (In-Memory / SQL Server)
* **Background Tasks:** `IHostedService` / `BackgroundService`
* **Data Generation:** Python & Pandas (`generate_data.py` ile sentetik dataset üretimi)
* **Frontend:** HTML5, CSS3 (Dark Mode B2B UI), JavaScript (SignalR Client, FontAwesome)
* **Containerization:** Docker

---

##  Kurulum ve Çalıştırma

### Ön Koşullar

* .NET 10 SDK veya üzeri

### Adımlar

1. **Repoyu Klonlayın:**
```bash
git clone [https://github.com/ayse-dasbas/AgriYieldSystem.git](https://github.com/ayse-dasbas/AgriYieldSystem.git)
cd AgriYieldSystem

```


2. **Projeyi Derleyin ve Çalıştırın:**
```bash
dotnet run --project AgriYield.Api

```


3. **Canlı Paneli Açın:**
Tarayıcınızda `http://localhost:5289/index.html` adresine giderek canlı telemetri akışını izleyin.
4. **Swagger API Dokümantasyonu:**
`http://localhost:5289/swagger` adresinden REST API endpoint'lerini inceleyebilirsiniz.

---

## 🐳 Docker İle Çalıştırma (Production Ready)

Proje Dockerize edilmeye hazır şekilde kurgulanmıştır.

1. **Docker İmajını Oluşturun:**
```bash
docker build -t agriyield-api .

```


2. **Konteyneri Çalıştırın:**
```bash
docker run -d -p 8080:8080 --name agriyield_container agriyield-api

```



> Uygulamaya `http://localhost:8080/swagger` veya `http://localhost:8080/index.html` adresinden erişebilirsiniz.

---

##  Gerçek IoT Cihazı Bağlantısı (Plug-and-Play)

Sistem "Tak-Çalıştır" yapıda tasarlandığı için fiziki bir cihaz bağlamak son derece kolaydır:

1. `Program.cs` içindeki `IotTelemetrySimulator` satırını devre dışı bırakın.
2. Sahadaki ESP32 / Arduino kartından sensör verilerini şu endpoint'e `POST` yapın:

```http
POST /api/v1/telemetry/ingest
Content-Type: application/json

{
  "greenhouseId": 1,
  "temperature": 26.5,
  "humidity": 78.2,
  "soilMoisture": 52.0
}

```

---

##  Geliştirme Yaklaşımı & Yapay Zekâ Kullanımı

Bu proje geliştirilirken mimari kurgu, iş mantığı ve kodlama süreçlerinde Yapay Zekâ (AI) araçlarından kodlama asistanı ve teknik destek olarak yararlanılmıştır.

```

---

