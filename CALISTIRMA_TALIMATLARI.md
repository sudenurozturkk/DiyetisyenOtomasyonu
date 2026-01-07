# 🚀 Projeyi Çalıştırma Talimatları

## Yöntem 1: Visual Studio'dan (Önerilen)

1. **Visual Studio 2022** veya **2019** açın
2. **File > Open > Project/Solution** ile `DiyetisyenOtomasyonu.sln` dosyasını açın
3. **F5** tuşuna basın veya **Debug > Start Debugging** menüsünden çalıştırın

## Yöntem 2: Doğrudan EXE Çalıştırma

1. Windows Explorer'da şu klasöre gidin:
   ```
   C:\Users\Administrator\Desktop\Projelerim\DiyetisyenOtomasyonu\bin\Debug
   ```
2. `takip.exe` dosyasına çift tıklayın

## Yöntem 3: PowerShell/CMD'den Çalıştırma

```powershell
cd "C:\Users\Administrator\Desktop\Projelerim\DiyetisyenOtomasyonu\bin\Debug"
.\takip.exe
```

## Yöntem 4: MSBuild ile Derleyip Çalıştırma

```powershell
# Proje klasörüne gidin
cd "C:\Users\Administrator\Desktop\Projelerim\DiyetisyenOtomasyonu"

# Projeyi derleyin
msbuild DiyetisyenOtomasyonu.sln /p:Configuration=Debug /p:Platform=x86

# Çalıştırın
.\bin\Debug\takip.exe
```

---

## 🔐 Giriş Bilgileri

### Doktor Hesabı
- **Kullanıcı Adı:** `whodenur`
- **Şifre:** `12345678`

### Hasta Hesapları
- **Kullanıcı Adı:** `vesudenur` / **Şifre:** `12345678`
- **Kullanıcı Adı:** `hasta1` / **Şifre:** `12345678`
- **Kullanıcı Adı:** `hasta2` / **Şifre:** `12345678`
- **Kullanıcı Adı:** `hasta3` / **Şifre:** `12345678`
- **Kullanıcı Adı:** `hasta4` / **Şifre:** `12345678`

---

## ⚠️ Sorun Giderme

### Veritabanı Bağlantısı (MySQL)
Proje varsayılan olarak yerel MySQL sunucusuna bağlanır:
- **Server:** localhost
- **Database:** dietpro_db
- **User:** root
- **Password:** (boş)

Eğer bağlantı hatası alırsanız:
1. XAMPP veya MySQL servisinin çalıştığından emin olun.
2. `App.config` veya `Infrastructure\Database\DatabaseConfig.cs` dosyasındaki bağlantı bilgilerini kontrol edin.
3. Veritabanı otomatik oluşturulamazsa, manuel olarak `dietpro_db` adında bir veritabanı oluşturun.

### DevExpress Lisans Uyarısı
- Trial sürüm kullanıyorsanız başlangıçta uyarı çıkabilir
- "OK" deyip devam edebilirsiniz
- Lisanslı sürümde sorun olmaz

---

## 📋 Sistem Gereksinimleri

- **Windows 10/11** (64-bit veya 32-bit)
- **.NET Framework 4.8**
- **MySQL Server** (XAMPP önerilir)
- **Visual Studio 2022** (geliştirme için)

---

## 🎯 İlk Çalıştırma

1. MySQL servisini başlatın.
2. Uygulamayı çalıştırın.
3. İlk açılışta veritabanı tabloları otomatik oluşturulacaktır.
4. **Login** ekranı görünecek.
5. Doktor hesabı ile giriş yapın: `whodenur` / `12345678`

---

## 📝 Notlar

- Veriler MySQL veritabanında (`dietpro_db`) saklanır.
- Demo veriler otomatik olarak oluşturulur.

