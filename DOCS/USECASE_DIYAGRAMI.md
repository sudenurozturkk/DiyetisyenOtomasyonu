# USE CASE DİYAGRAMI
## Diyetisyen Hasta Takip Otomasyonu

**Proje Adı:** DiyetPro - Diyetisyen Hasta Otomasyonu  
**Tarih:** 17 Ocak 2026  
**Versiyon:** 2.0

---

## 1. AKTÖRLER (ACTORS)

### 1.1 Diyetisyen (Doctor)
- **Rol:** Sistem yöneticisi ve sağlık uzmanı
- **Yetkiler:** Tam erişim (hasta yönetimi, diyet planları, raporlar)
- **Sorumluluklar:** Hasta takibi, diyet planı oluşturma, ilerleme izleme

### 1.2 Hasta (Patient)
- **Rol:** Son kullanıcı
- **Yetkiler:** Kısıtlı erişim (kendi verileri)
- **Sorumluluklar:** Diyet planına uyum, ilerleme kaydı

### 1.3 Sistem (System)
- **Rol:** Otomatik işlemler
- **Sorumluluklar:** Hesaplamalar, bildirimler, veri yönetimi

---

## 2. USE CASE LİSTESİ

### 2.1 Diyetisyen Use Case'leri

#### UC-01: Giriş Yap
**Aktör:** Diyetisyen, Hasta  
**Önkoşul:** Kullanıcı sistemde kayıtlı olmalı  
**Akış:**
1. Kullanıcı adı ve şifre gir
2. Sistem kimlik doğrulaması yap
3. Rol bazlı yönlendirme (Diyetisyen/Hasta)

**Sonkoşul:** Kullanıcı ana ekrana yönlendirilir

---

#### UC-02: Hasta Kaydı Oluştur
**Aktör:** Diyetisyen  
**Önkoşul:** Diyetisyen giriş yapmış olmalı  
**Akış:**
1. "Yeni Hasta Ekle" formunu aç
2. Hasta bilgilerini gir (ad, yaş, boy, kilo, vb.)
3. Sistem otomatik olarak BMI ve TDEE hesapla
4. Kullanıcı adı ve şifre oluştur
5. Kaydı veritabanına kaydet

**Sonkoşul:** Yeni hasta sisteme eklenir

**Akıllı Algoritma:** BMI, TDEE, BMR otomatik hesaplama

---

#### UC-03: Hasta Listesini Görüntüle
**Aktör:** Diyetisyen  
**Önkoşul:** Diyetisyen giriş yapmış olmalı  
**Akış:**
1. Hasta listesi ekranını aç
2. Sistem tüm hastaları listele (avatar, cinsiyet badge)
3. Arama fonksiyonu ile filtrele (opsiyonel)
4. Hasta seç ve detayları görüntüle

**Sonkoşul:** Hasta bilgileri görüntülenir

---

#### UC-04: Hasta Bilgilerini Güncelle
**Aktör:** Diyetisyen  
**Önkoşul:** Hasta seçilmiş olmalı  
**Akış:**
1. Hasta seç
2. Bilgileri düzenle (kilo, boy, aktivite seviyesi)
3. Sistem yeni BMI ve TDEE hesapla
4. Güncellemeleri kaydet

**Sonkoşul:** Hasta bilgileri güncellenir

**Akıllı Algoritma:** Dinamik BMI/TDEE güncelleme

---

#### UC-05: Diyet Planı Oluştur
**Aktör:** Diyetisyen  
**Önkoşul:** Hasta seçilmiş olmalı  
**Akış:**
1. Hasta seç
2. Haftalık plan oluştur
3. Her gün için öğünler ekle (kahvaltı, ara öğün, öğle, akşam)
4. Besin değerlerini gir (kalori, protein, karbonhidrat, yağ)
5. Planı kaydet

**Sonkoşul:** Diyet planı hastaya atanır

---

#### UC-06: Hasta Notu Ekle
**Aktör:** Diyetisyen  
**Önkoşul:** Hasta seçilmiş olmalı  
**Akış:**
1. Not ekle formunu aç
2. Hasta seç
3. Not içeriğini yaz (klinik gözlemler, diyet uyumu)
4. Kategori seç (Beslenme, Tıbbi, İlerleme vb.)
5. Notu kaydet

**Sonkoşul:** Not hasta dosyasına eklenir

---

#### UC-07: Mesajlaşma (Diyetisyen → Hasta)
**Aktör:** Diyetisyen  
**Önkoşul:** Hasta seçilmiş olmalı  
**Akış:**
1. Mesajlaşma ekranını aç
2. Sol panelden hasta seç
3. Mesaj yaz
4. Gönder butonuna bas
5. Sistem mesajı kaydeder ve hastaya iletir

**Sonkoşul:** Mesaj hastaya gönderilir

---

#### UC-08: Raporları Görüntüle
**Aktör:** Diyetisyen  
**Önkoşul:** Hasta seçilmiş olmalı  
**Akış:**
1. Raporlar ekranını aç
2. Hasta seç
3. Kilo trendi grafiğini görüntüle
4. BMI değişim grafiğini görüntüle
5. Diyet uyum oranını kontrol et

**Sonkoşul:** Hasta ilerleme raporları görüntülenir

**Akıllı Algoritma:** İstatistiksel analiz, trend hesaplama

---

#### UC-09: Randevu Yönetimi
**Aktör:** Diyetisyen  
**Önkoşul:** Diyetisyen giriş yapmış olmalı  
**Akış:**
1. Randevu ekranını aç
2. Yeni randevu oluştur (hasta, tarih, saat)
3. Randevu tipini seç (ilk görüşme, kontrol, vb.)
4. Kaydet

**Sonkoşul:** Randevu sisteme eklenir

---

#### UC-10: Finansal Rapor
**Aktör:** Diyetisyen  
**Önkoşul:** Randevular kaydedilmiş olmalı  
**Akış:**
1. Finansal özet ekranını aç
2. Tarih aralığı seç
3. Sistem toplam gelir, randevu sayısı hesapla
4. Grafikleri görüntüle

**Sonkoşul:** Finansal rapor görüntülenir

**Akıllı Algoritma:** Gelir analizi, dönemsel karşılaştırma

---

### 2.2 Hasta Use Case'leri

#### UC-11: Haftalık Menüyü Görüntüle
**Aktör:** Hasta  
**Önkoşul:** Hasta giriş yapmış, diyet planı atanmış olmalı  
**Akış:**
1. Haftalık menü ekranını aç
2. Hafta seç (dropdown)
3. Sistem o haftanın planını göster
4. Her öğün için detayları görüntüle

**Sonkoşul:** Haftalık plan görüntülenir

---

#### UC-12: Öğün Tamamla (Yedim İşaretleme)
**Aktör:** Hasta  
**Önkoşul:** Diyet planı görüntülenmiş olmalı  
**Akış:**
1. Öğün yanındaki "Yedim ✓" checkbox'ını işaretle
2. Sistem tamamlama kaydı oluştur
3. İlerleme yüzdesini güncelle

**Sonkoşul:** Öğün tamamlandı olarak işaretlenir

**Akıllı Algoritma:** Uyum oranı hesaplama

---

#### UC-13: Hedeflerimi Görüntüle
**Aktör:** Hasta  
**Önkoşul:** Hedefler tanımlanmış olmalı  
**Akış:**
1. Hedeflerim ekranını aç
2. Su, kilo, adım, protein hedeflerini görüntüle
3. Güncel değerleri ve ilerlemeyi kontrol et
4. Progress bar'ları incele

**Sonkoşul:** Hedefler ve ilerleme görüntülenir

---

#### UC-14: Hedef Güncelle (Su, Adım)
**Aktör:** Hasta  
**Önkoşul:** Hedef tanımlı olmalı  
**Akış:**
1. Hedef kartında "Güncelle" butonuna bas
2. Yeni değer gir (örn: 2.5 litre su)
3. Kaydet
4. Sistem ilerleme yüzdesini yeniden hesapla

**Sonkoşul:** Hedef güncel değeri güncellenir

---

#### UC-15: İlerleme Grafiklerini Görüntüle
**Aktör:** Hasta  
**Önkoşul:** Hasta giriş yapmış olmalı  
**Akış:**
1. İlerleme ekranını aç
2. Kilo değişim grafiğini görüntüle
3. Haftalık tamamlama oranını kontrol et
4. BMI değişimini incele

**Sonkoşul:** İlerleme grafikleri görüntülenir

**Akıllı Algoritma:** Trend analizi, görselleştirme

---

#### UC-16: Mesajlaşma (Hasta → Diyetisyen)
**Aktör:** Hasta  
**Önkoşul:** Hasta giriş yapmış olmalı  
**Akış:**
1. Mesajlaşma ekranını aç
2. Mesaj yaz
3. Gönder
4. Sistem mesajı diyetisyene iletir

**Sonkoşul:** Mesaj diyetisyene gönderilir

---

#### UC-17: AI Asistan Kullan
**Aktör:** Hasta  
**Önkoşul:** Hasta giriş yapmış olmalı  
**Akış:**
1. AI Asistan ekranını aç
2. Soru sor (örn: "Kahvaltıda ne yiyebilirim?")
3. Sistem AI ile yanıt üret
4. Yanıtı görüntüle

**Sonkoşul:** AI yanıtı görüntülenir

**Akıllı Algoritma:** Gemini AI entegrasyonu (stub)

---

#### UC-18: Vücut Ölçülerini Kaydet
**Aktör:** Hasta  
**Önkoşul:** Hasta giriş yapmış olmalı  
**Akış:**
1. Vücut ölçüleri ekranını aç
2. Göğüs, bel, kalça, kol, bacak ölçülerini gir
3. Tarihi seç
4. Kaydet

**Sonkoşul:** Vücut ölçüleri kaydedilir

---

#### UC-19: Egzersiz Görevlerini Görüntüle
**Aktör:** Hasta  
**Önkoşul:** Egzersiz görevleri atanmış olmalı  
**Akış:**
1. Egzersiz görevleri ekranını aç
2. Bekleyen görevleri listele
3. Görev detaylarını görüntüle (süre, zorluk)
4. Tamamlanan görevleri kontrol et

**Sonkoşul:** Egzersiz görevleri görüntülenir

---

#### UC-20: Egzersiz Görevini Tamamla
**Aktör:** Hasta  
**Önkoşul:** Egzersiz görevi atanmış olmalı  
**Akış:**
1. Görev seç
2. "Tamamlandı" işaretle
3. Geri bildirim yaz (opsiyonel)
4. Kaydet

**Sonkoşul:** Egzersiz tamamlandı olarak işaretlenir

---

### 2.3 Sistem Use Case'leri (Otomatik)

#### UC-21: BMI Hesapla
**Aktör:** Sistem  
**Tetikleyici:** Hasta kaydı veya kilo güncelleme  
**Akış:**
1. Boy (cm) ve kilo (kg) değerlerini al
2. Formül: BMI = Kilo / (Boy²)
3. BMI kategorisini belirle (Zayıf, Normal, Fazla Kilolu, Obez)
4. Sonucu göster

**Sonkoşul:** BMI hesaplanır ve görüntülenir

---

#### UC-22: TDEE Hesapla (Günlük Kalori İhtiyacı)
**Aktör:** Sistem  
**Tetikleyici:** Hasta kaydı veya güncelleme  
**Akış:**
1. BMR hesapla (Mifflin-St Jeor denklemi)
   - Erkek: BMR = (10 × kilo) + (6.25 × boy) - (5 × yaş) + 5
   - Kadın: BMR = (10 × kilo) + (6.25 × boy) - (5 × yaş) - 161
2. Aktivite seviyesi çarpanını uygula (1.2 - 1.9)
3. TDEE = BMR × Aktivite Çarpanı
4. Sonucu göster

**Sonkoşul:** Günlük kalori ihtiyacı hesaplanır

---

#### UC-23: İlerleme Yüzdesi Hesapla
**Aktör:** Sistem  
**Tetikleyici:** Öğün tamamlama, hedef güncelleme  
**Akış:**
1. Hedef değerini al
2. Güncel değeri al
3. Yüzde = (Güncel / Hedef) × 100
4. Progress bar'ı güncelle

**Sonkoşul:** İlerleme yüzdesi gösterilir

---

#### UC-24: Otomatik Bildirim Gönder
**Aktör:** Sistem  
**Tetikleyici:** Randevu tarihi yaklaştığında  
**Akış:**
1. Bugünden 1 gün sonraki randevuları kontrol et
2. Her randevu için bildirim oluştur
3. Hastaya mesaj gönder

**Sonkoşul:** Hatırlatma bildirimi gönderilir

---

#### UC-25: Risk Analizi Yap
**Aktör:** Sistem  
**Tetikleyici:** Haftalık otomatik analiz  
**Akış:**
1. Son 30 günlük kilo verisini analiz et
2. Hızlı kilo kaybı/alımı tespit et (>3 kg/ay)
3. Kilo platosu tespit et (değişim <%0.5)
4. Düşük uyum oranı tespit et (<%60)
5. Risk raporu oluştur

**Sonkoşul:** Risk analizi tamamlanır

**Akıllı Algoritma:** İstatistiksel analiz, anomali tespiti

---

## 3. USE CASE DİYAGRAMI (GÖRSEL)

```
┌─────────────────────────────────────────────────────────────────┐
│                  DİYETİSYEN HASTA TAKİP SİSTEMİ                 │
└─────────────────────────────────────────────────────────────────┘

        ┌──────────┐
        │Diyetisyen│
        └────┬─────┘
             │
             ├──────► UC-01: Giriş Yap
             │
             ├──────► UC-02: Hasta Kaydı Oluştur ◄──[include]── UC-21: BMI Hesapla
             │                                    ◄──[include]── UC-22: TDEE Hesapla
             │
             ├──────► UC-03: Hasta Listesini Görüntüle
             │
             ├──────► UC-04: Hasta Bilgilerini Güncelle ◄──[include]── UC-21: BMI Hesapla
             │
             ├──────► UC-05: Diyet Planı Oluştur
             │
             ├──────► UC-06: Hasta Notu Ekle
             │
             ├──────► UC-07: Mesajlaşma (D→H)
             │
             ├──────► UC-08: Raporları Görüntüle ◄──[include]── UC-25: Risk Analizi
             │
             ├──────► UC-09: Randevu Yönetimi ◄──[extend]── UC-24: Otomatik Bildirim
             │
             └──────► UC-10: Finansal Rapor


        ┌──────┐
        │Hasta │
        └───┬──┘
            │
            ├──────► UC-01: Giriş Yap
            │
            ├──────► UC-11: Haftalık Menüyü Görüntüle
            │
            ├──────► UC-12: Öğün Tamamla ◄──[include]── UC-23: İlerleme Yüzdesi Hesapla
            │
            ├──────► UC-13: Hedeflerimi Görüntüle
            │
            ├──────► UC-14: Hedef Güncelle ◄──[include]── UC-23: İlerleme Yüzdesi Hesapla
            │
            ├──────► UC-15: İlerleme Grafiklerini Görüntüle
            │
            ├──────► UC-16: Mesajlaşma (H→D)
            │
            ├──────► UC-17: AI Asistan Kullan
            │
            ├──────► UC-18: Vücut Ölçülerini Kaydet
            │
            ├──────► UC-19: Egzersiz Görevlerini Görüntüle
            │
            └──────► UC-20: Egzersiz Görevini Tamamla


        ┌──────┐
        │Sistem│ (Otomatik İşlemler)
        └───┬──┘
            │
            ├──────► UC-21: BMI Hesapla
            │
            ├──────► UC-22: TDEE Hesapla
            │
            ├──────► UC-23: İlerleme Yüzdesi Hesapla
            │
            ├──────► UC-24: Otomatik Bildirim Gönder
            │
            └──────► UC-25: Risk Analizi Yap
```

---

## 4. USE CASE İLİŞKİLERİ

### 4.1 Include İlişkileri
- **UC-02 (Hasta Kaydı) ──[include]──► UC-21 (BMI Hesapla)**
- **UC-02 (Hasta Kaydı) ──[include]──► UC-22 (TDEE Hesapla)**
- **UC-04 (Hasta Güncelle) ──[include]──► UC-21 (BMI Hesapla)**
- **UC-12 (Öğün Tamamla) ──[include]──► UC-23 (İlerleme Hesapla)**
- **UC-14 (Hedef Güncelle) ──[include]──► UC-23 (İlerleme Hesapla)**

### 4.2 Extend İlişkileri
- **UC-09 (Randevu Yönetimi) ──[extend]──► UC-24 (Otomatik Bildirim)**

---

## 5. AKILLI ALGORİTMALAR

### 5.1 Hesaplama Algoritmaları
1. **BMI Hesaplama** (UC-21)
2. **TDEE Hesaplama** (UC-22) - Mifflin-St Jeor denklemi
3. **İlerleme Yüzdesi** (UC-23)
4. **Finansal Analiz** (UC-10)

### 5.2 Karar Verme Algoritmaları
1. **Risk Analizi** (UC-25) - Hızlı kilo kaybı/alımı tespiti
2. **BMI Kategorizasyonu** (UC-21) - Zayıf/Normal/Obez
3. **Otomatik Bildirim** (UC-24) - Randevu hatırlatma

### 5.3 Öğrenme Algoritmaları (Gelecek)
1. **AI Asistan** (UC-17) - Gemini AI entegrasyonu
2. **Diyet Öneri Sistemi** - Geçmiş verilere dayalı öneriler

---

## 6. SONUÇ

Bu Use Case diyagramı, DiyetPro sisteminin tüm fonksiyonel gereksinimlerini kapsar. Sistem, **25 adet use case** içermekte ve **3 farklı aktör** (Diyetisyen, Hasta, Sistem) tarafından kullanılmaktadır.

**Akıllı Algoritma Sayısı:** 9 adet  
**Otomatik İşlem Sayısı:** 5 adet  
**Kullanıcı İşlemi:** 20 adet

---

**Hazırlayan:** Proje Ekibi  
**Onaylayan:** Danışman Öğretim Üyesi  
**Tarih:** 17 Ocak 2026
