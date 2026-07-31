# -*- coding: utf-8 -*-
import pandas as pd
import numpy as np

# Random seed
np.random.seed(42)
num_samples = 1000

# Sensor verileri uretimi
temperature = np.random.uniform(15.0, 38.0, num_samples) # C
humidity = np.random.uniform(40.0, 95.0, num_samples)    # %
soil_moisture = np.random.uniform(20.0, 80.0, num_samples) # %

# Algoritma / Kural Temelli Verim (Yield) Formulu (kg/m2)
# Ideal kosullar: Sicaklik 22-28 C, Nem %60-75, Toprak Nemi %50-70
yield_val = (
    8.0 
    + (30 - np.abs(temperature - 25)) * 0.25 
    + (50 - np.abs(humidity - 68)) * 0.15 
    + (40 - np.abs(soil_moisture - 60)) * 0.10 
    + np.random.normal(0, 0.5, num_samples)
)
yield_val = np.clip(yield_val, 2.0, 18.0) # 2.0 - 18.0 kg/m2 arasi sinirla

# Hastalik Riski Formulu (DiseaseRiskScore: %0 - %100)
# Yuksek Nem (> %80) ve Yuksek Sicaklik (> 28 C) mantar/hastalik riskini artirir
disease_risk = (
    ((humidity - 40) / 55.0) * 50.0 
    + ((temperature - 15) / 23.0) * 30.0 
    + np.random.normal(0, 5.0, num_samples)
)
disease_risk = np.clip(disease_risk, 5.0, 98.0)

# DataFrame Olusturma
df = pd.DataFrame({
    'Temperature': np.round(temperature, 2),
    'Humidity': np.round(humidity, 2),
    'SoilMoisture': np.round(soil_moisture, 2),
    'PredictedYield': np.round(yield_val, 2),
    'DiseaseRiskScore': np.round(disease_risk, 2)
})

# CSV olarak kaydet
df.to_csv('MLModel/agri_yield_dataset.csv', index=False)
print("ML Dataset 'MLModel/agri_yield_dataset.csv' konumuna basariyla olusturuldu!")
print("\n--- ILK 5 SATIR ---")
print(df.head())
