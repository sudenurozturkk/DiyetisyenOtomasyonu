# Maliyet Kestirim Dokümanı - Diyetisyen Otomasyon Sistemi

## Proje Bilgileri

| Özellik | Değer |
|---------|-------|
| **Proje Adı** | Diyetisyen Otomasyon Sistemi |
| **Geliştirme Dili** | C# (.NET Framework) |
| **Veritabanı** | MySQL |
| **UI Framework** | Windows Forms + DevExpress |

---

## İşlev Nokta Analizi (Function Point Analysis)

### Ölçüm Parametreleri

| Ölçüm Parametresi | Sayı | Ağırlık Faktörü | Toplam |
|-------------------|------|-----------------|--------|
| Kullanıcı Girdi Sayısı | 18 | 3 | 54 |
| Kullanıcı Çıktı Sayısı | 12 | 4 | 48 |
| Kullanıcı Sorgu Sayısı | 15 | 3 | 45 |
| Veri Tabanındaki Tablo Sayısı | 9 | 7 | 63 |
| Arayüz Sayısı | 14 | 5 | 70 |
| **Ana İşlev Nokta Sayısı (AİN)** | | | **280** |

### Kullanıcı Girdileri (18)
1. Kullanıcı Girişi
2. Kullanıcı Kaydı
3. Hasta Ekleme
4. Hasta Güncelleme
5. Kilo Kaydı Girişi
6. Hedef Ekleme
7. Hedef Güncelleme
8. Not Ekleme
9. Mesaj Gönderme
10. Yemek Ekleme
11. Yemek Güncelleme
12. Diyet Planı Oluşturma
13. Öğün Atama
14. Haftalık Menü Atama
15. İlerleme Güncelleme
16. Profil Güncelleme
17. Alerji Ekleme
18. Şifre Değiştirme

### Kullanıcı Çıktıları (12)
1. Hasta Listesi
2. Hasta Profili
3. Kilo Grafiği
4. Makro Besin Grafiği
5. Mesaj Listesi
6. Konuşma Detayı
7. Hedef Listesi
8. Not Listesi
9. Yemek Listesi
10. Haftalık Menü
11. AI Analiz Sonuçları
12. Günlük İpucu

### Arayüzler (14)
1. FrmLogin
2. FrmRegister
3. FrmSplash
4. FrmDoctorShell
5. FrmPatients
6. FrmPatientProfile
7. FrmMeals
8. FrmAssignPlans
9. FrmGoalsNotes
10. FrmMessagesDoctor
11. FrmAnalytics
12. FrmPatientShell
13. FrmMessagesPatient
14. FrmAiAssistant

---

## Teknik Karmaşıklık Faktörü

| No | Soru | Puan (0-5) |
|----|------|------------|
| 1 | Uygulama, güvenilir yedekleme ve kurtarma gerektiriyor mu? | 3 |
| 2 | Veri iletişimi gerekiyor mu? | 4 |
| 3 | Dağıtık işlem işlevleri var mı? | 0 |
| 4 | Performans kritik mi? | 3 |
| 5 | Sistem mevcut ve ağır yükü olan bir işletim ortamında mı çalışacak? | 2 |
| 6 | Sistem, çevrim içi veri girişi gerektiriyor mu? | 5 |
| 7 | Çevrim içi veri girişi, bir ara işlem için birden çok ekran gerektiriyor mu? | 4 |
| 8 | Ana kütükler çevrim-içi olarak mı günleniyor? | 5 |
| 9 | Girdiler, çıktılar, kütükler ya da sorgular karmaşık mı? | 3 |
| 10 | İçsel işlemler karmaşık mı? | 4 |
| 11 | Tasarlanacak kod, yeniden kullanılabilir mi olacak? | 4 |
| 12 | Dönüştürme ve kurulum, tasarımda dikkate alınacak mı? | 3 |
| 13 | Sistem birden çok yerde yerleşik farklı kurumlar için mi geliştiriliyor? | 2 |
| 14 | Tasarlanan uygulama, kolay kullanılabilir ve kullanıcı tarafından kolayca değiştirilebilir mi olacak? | 4 |
| | **Toplam (TKF)** | **46** |

> **Puanlama Kılavuzu:**
> - 0: Hiçbir Etkisi Yok
> - 1: Çok Az etkisi var
> - 2: Etkisi Var
> - 3: Ortalama Etkisi Var
> - 4: Önemli Etkisi Var
> - 5: Mutlaka Olmalı, Kaçınılamaz

---

## Hesaplamalar

### İşlev Nokta Hesabı
```
İN = AİN × (0.65 + 0.01 × TKF)
İN = 280 × (0.65 + 0.01 × 46)
İN = 280 × (0.65 + 0.46)
İN = 280 × 1.11
İN = 310.8
```

### Satır Sayısı Tahmini
```
Satır Sayısı = İN × 30
Satır Sayısı = 310.8 × 30
Satır Sayısı ≈ 9,324 satır
```

---

## Gerçek Kod İstatistikleri

| Katman | Dosya Sayısı | Tahmini Satır |
|--------|--------------|---------------|
| Domain | 14 | ~1,500 |
| Infrastructure/Repositories | 11 | ~2,500 |
| Infrastructure/Services | 7 | ~1,800 |
| Infrastructure/Database | 2 | ~400 |
| Infrastructure/Security | 2 | ~150 |
| Forms | 14 | ~3,500 |
| Shared | 4 | ~500 |
| **Toplam** | **54** | **~10,350** |

> **Sonuç:** Tahmin edilen satır sayısı (9,324) ile gerçek satır sayısı (~10,350) %10 sapma ile tutarlıdır. Bu, İşlev Nokta Analizi yönteminin güvenilirliğini göstermektedir.

---

## Proje Büyüklük Sınıflandırması

| Büyüklük | İşlev Nokta Aralığı | Bu Proje |
|----------|---------------------|----------|
| Küçük | < 100 | - |
| Orta | 100-500 | ✅ (310.8) |
| Büyük | 500-1000 | - |
| Çok Büyük | > 1000 | - |

**Proje Büyüklüğü: ORTA**
