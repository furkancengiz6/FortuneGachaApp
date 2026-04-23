# 🔮 Fortune Gacha App

Fortune Gacha; günlük fal, Türk kahvesi falı, oyunlaştırma ve yapay zekayı birleştiren mistik, modern ve sosyal bir mobil uygulamadır. Kullanıcılar günlük fallarını "Gacha" slot makinesi mekaniğiyle çeker, fincan fotoğraflarıyla kahve falı baktırır ve bu falları koleksiyonlar, arkadaşlarıyla paylaşır.

---

## 🌟 Özellikler

### 🎰 Gacha Slot Makinesi
- **Slot Çarkı**: Makaralı slot makinesi animasyonuyla günlük fal çekimi.
- **Nadir Kart Sistemi**: Common, Uncommon, Rare ve Legendary kart koleksiyonu.
- **Luck Boost**: 100 GP karşılığında Legendary kart şansını %15'e çıkar.
- **Haptic Feedback**: Çekimlerde titreşim geri bildirimi ile gerçekçi deneyim.

### ☕ Türk Kahvesi Falı
- **Kamera Entegrasyonu**: Fincanın fotoğrafını çek veya galeriden seç.
- **GPT-4 Vision Analizi**: Yapay zeka fincandaki desenleri, figürleri ve şekilleri analiz eder.
- **Kişisel Yorum**: Fincandaki desenlere dayalı 3-5 cümlelik mistik ve samimi yorum.
- **DALL-E Kart Oluşturma**: AI yoruma göre sanatsal, dikey bir kart görseli üretir.
- **Günlük Limit**: 1 ücretsiz okuma, ek okumalar 50 GP.
- **Geçmiş Fallar**: Tüm kahve falı geçmişin kaydedilir ve geri görüntülenebilir.

### 🎭 Kişiselleştirilmiş AI Falları
- **GPT-4o Desteği**: Burcun, ilgi alanların ve biyografine göre sana özel üretilen mistik mesajlar.
- **DALL-E 3 Görselleri**: "Rare" ve "Legendary" kartlar için o ana özel üretilen eşsiz sanat eserleri.
- **Mock Test Modu**: OpenAI API olmadan rastgele fal ve yorum üretimi ile test imkânı.

### 🏪 Pazar Yeri & Mağaza
- **Dinamik Market**: Diğer kullanıcıların paylaştığı falları nadirlik filtreleriyle incele.
- **Kişiselleştirme Dükkanı**: Kazandığın GP'lerle profilin için şık **Avatar Çerçeveleri** satın al.

### 🤝 Sosyal & Hediyeleşme
- **Arkadaşlık Sistemi**: Kullanıcı ara, arkadaş ekle ve birbirinin vitrinini gez.
- **GP Hediyeleşme**: Arkadaşlarına GP göndererek onlara destek ol.
- **Liderlik Tablosu**: En şanslı ve en zengin falcılar arasında yerini al, prestijli çerçevelerini sergile.

### 🗺️ Günlük Görevler & Başarımlar
- **Görev Sistemi**: "Günün Falı", "Mistik Destekçi" gibi görevleri tamamla, ekstra GP kazan.
- **Madalya Kutusu**: Kazandığın başarımları profilinde sergile.

### 🧠 Haftalık Kader Analizi
- **AI Sentezi**: Son 7 gündeki tüm fallarını analiz eden ve haftalık hayat rehberini sunan yapay zeka raporu.

### 🎂 Doğum Günü Sürprizi
- Doğum gününde giriş yapanlara **+500 GP** hediyesi ve o günkü çekimde yüksek şans oranı!

---

## 📱 Ekranlar

| Ekran | Açıklama |
|-------|----------|
| 🎰 **Fal Çek** | Slot makinesiyle günlük fal çekimi |
| ☕ **Kahve Falı** | Fincan fotoğrafıyla AI kahve falı |
| 🏆 **Liderlik** | En şanslı falcılar sıralaması |
| 🖼️ **Koleksiyon** | Çektiğin tüm falları sergile |
| 🏪 **Pazar** | Fallarını satışa çıkar veya satın al |
| 👥 **Sosyal** | Arkadaş ekle, GP hediyele |
| 👤 **Profil** | Başarımlar, haftalık analiz, doğum günü |
| 🛒 **Mağaza** | Avatar çerçeveleri ve dekorasyonlar |

---

## 🚀 Teknolojik Altyapı

| Katman | Teknoloji |
|--------|-----------|
| **Frontend** | React Native (Expo SDK 53), Expo Router, NativeWind v4 (Tailwind CSS), Reanimated |
| **Backend** | .NET 8 Web API, Entity Framework Core, SQLite |
| **İletişim** | SignalR (Real-time), Expo Push Notifications |
| **Yapay Zeka** | OpenAI API — GPT-4o (metin), GPT-4 Vision (kahve falı analizi), DALL-E 3 (görsel üretimi) |
| **Medya** | expo-image-picker (kamera/galeri), multipart form data upload |

---

## 🛠️ Kurulum & Geliştirme

### Ön Koşullar
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Expo CLI](https://docs.expo.dev/) (`npm install -g expo-cli`)
- [Expo Go](https://expo.dev/go) (telefona yükle)

### Backend Kurulumu
```bash
cd src/FortuneGacha.Api

# (Opsiyonel) OpenAI API Key'ini ayarla
# appsettings.json → "OpenAI:ApiKey" alanını doldur
# API Key yoksa otomatik olarak Mock modu çalışır

dotnet restore
dotnet run --urls="http://0.0.0.0:5132"
```

### Frontend Kurulumu
```bash
cd src/FortuneGacha.Client

npm install --legacy-peer-deps

# src/api/api.ts içindeki SERVER_URL'i kendi IP'nle güncelle:
# const SERVER_URL = 'http://SENIN_IP_ADRESIN:5132';

npx expo start -c
```

### 📱 Telefonda Test
1. Backend ve Frontend'i başlat
2. Telefondaki **Expo Go** uygulamasını aç
3. QR kodu tara veya IP adresini manuel gir
4. **Yeni hesap oluştur** (Kayıt Ol) ve uygulamayı keşfet!

---

## ⚙️ Yapılandırma

### Mock Mod (Varsayılan)
OpenAI API Key olmadan çalışır. Rastgele fal metni, placeholder görsel ve mock kahve falı yorumu üretir.

```csharp
// Program.cs — Mock Mod (varsayılan)
builder.Services.AddScoped<IGachaService, MockGachaService>();
```

### AI Mod (Canlı)
Gerçek AI deneyimi için OpenAI API Key gereklidir.

```csharp
// Program.cs — AI Mod
builder.Services.AddScoped<IGachaService, GachaServiceV2>();
```

```json
// appsettings.json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "TextModel": "gpt-4o",
    "ImageModel": "dall-e-3"
  }
}
```

> **Maliyet Tahmini (AI Mod)**
> - Günlük Fal Çekimi: ~$0.05/istek (metin + görsel)
> - Kahve Falı: ~$0.10/istek (vision analiz + görsel üretimi)

---

## 📂 Proje Yapısı

```
FortuneGachaApp/
├── src/
│   ├── FortuneGacha.Api/           # .NET 8 Backend
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs        # Giriş/Kayıt/Profil
│   │   │   ├── FortuneController.cs     # Gacha fal çekimi
│   │   │   ├── CoffeeFortuneController.cs # ☕ Kahve falı
│   │   │   ├── MarketplaceController.cs # Pazar yeri
│   │   │   ├── ShopController.cs        # Mağaza
│   │   │   ├── SocialController.cs      # Arkadaşlık & sosyal
│   │   │   └── QuestController.cs       # Görevler
│   │   ├── Models/
│   │   │   ├── GachaProfile.cs          # Kullanıcı modeli
│   │   │   ├── DailyFortune.cs          # Günlük fal
│   │   │   ├── CoffeeFortune.cs         # ☕ Kahve falı
│   │   │   ├── SocialAchievements.cs    # Başarımlar
│   │   │   ├── Quest.cs                 # Görevler
│   │   │   └── Decoration.cs            # Dekorasyonlar
│   │   ├── Services/
│   │   │   ├── GachaService.cs          # Mock AI servisi
│   │   │   ├── GachaServiceV2.cs        # Gerçek AI servisi (OpenAI)
│   │   │   └── QuestService.cs          # Görev takibi
│   │   └── Data/
│   │       └── GachaDbContext.cs        # EF Core veritabanı
│   │
│   └── FortuneGacha.Client/       # React Native Frontend
│       ├── app/
│       │   ├── index.tsx                # Giriş/Kayıt ekranı
│       │   ├── _layout.tsx              # Ana layout
│       │   └── (tabs)/
│       │       ├── gacha.tsx            # 🎰 Fal çekme
│       │       ├── coffee.tsx           # ☕ Kahve falı
│       │       ├── leaderboard.tsx      # 🏆 Liderlik
│       │       ├── showcase.tsx         # 🖼️ Koleksiyon
│       │       ├── market.tsx           # 🏪 Pazar
│       │       ├── friends.tsx          # 👥 Sosyal
│       │       ├── profile.tsx          # 👤 Profil
│       │       └── shop.tsx             # 🛒 Mağaza
│       └── src/
│           ├── api/api.ts               # API istemcisi
│           ├── components/              # Bileşenler
│           └── hooks/                   # Custom hooks
└── README.md
```

---

## 🔮 Gelecek Özellikler (Roadmap)

- [ ] Tarot Kartı Falı
- [ ] El Falı (Avuç İçi Analizi)
- [ ] Rüya Yorumlama
- [ ] Günlük Burç Yorumları
- [ ] Arkadaşlarla Canlı Fal Odası
- [ ] App Store / Google Play yayını

---

## 📜 Lisans
Bu proje kişisel kullanım ve test amaçlı hazırlanmıştır. Tüm hakları saklıdır.
