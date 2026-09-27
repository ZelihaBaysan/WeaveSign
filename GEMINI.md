# WeaveSign — Frontend Only & Strict Protection Rules

Bu projede çalışırken asistan SADECE FRONTEND geliştirmesi yapacaktır.
Backend oyun mantığına ve Yaren'in yapay zeka/model entegrasyonu alanlarına DOKUNMAK KESİNLİKLE YASAKTIR.
Tüm kurallar ve mimari ayrıntılar için referans: `Documentation/WEAVESIGN_PROJE_YAPISI_VE_SORUMLULUK_REHBERI.md`

---

## ⛔ ASLA DOKUNULMAYACAK ALANLAR (KIRMIZI ÇİZGİ)
Yapay zeka asistanı aşağıdaki dizinlerdeki ve dosyalardaki hiçbir şeyi DEĞİŞTİREMEZ, SİLEMEZ veya YENİ DOSYA OLUŞTURAMAZ:

1. **Backend / Oyun Mantığı Kodları:**
   - `Assets/Scripts/Battle/` (özellikle `BattleManager.cs`, `SpellCard.cs`, `CardView.cs`, `CardAreaController.cs`)
   - `Assets/Scripts/Learning/` (özellikle `LetterLearningController.cs`, `WordLearningController.cs`)
   - `Assets/Scripts/NameWorkshop/` (özellikle `NameWorkshopController.cs`)
   - `Assets/Scripts/Core/` (özellikle `AudioManager.cs`, `SettingsPanelController.cs`)
   - `Assets/Scripts/Editor/`

2. **Yaren / ML & Model Entegrasyonu:**
   - `Assets/Scripts/Recognition/` (`Camera/`, `HandTracking/`, `Model/`, `RecognitionServiceBase.cs`, `MockRecognitionService.cs`, `RecognitionRequest.cs`, `RecognitionResult.cs`)
   - `Assets/ML/` (`Models/`, `Config/`, `TestData/`)

3. **Veri Assetleri:**
   - `Assets/Resources/` (`Cards/`, `Learning/Letters/`, `Learning/Words/` altındaki tüm `.asset` dosyaları)

---

## ✅ İZİN VERİLEN ALANLAR (SADECE FRONTEND)
Yapay zeka asistanı görsel düzenleme, arayüz zenginleştirme ve UI animasyonları için yalnızca şu alanlarda çalışabilir:
- `Assets/Art/` (Görseller, sprite'lar, ikonlar, arka planlar)
- `Assets/Animations/` (UI animasyonları ve animatörler)
- `Assets/Materials/` (UI materyalleri ve shader'lar)
- `Assets/Audio/` (UI ses efektleri ve müzikler)
- `Assets/Prefabs/UI/` (UI prefabları)
- `Assets/Scripts/UI/` (Yalnızca görsel davranış scriptleri: `ButtonHoverAnimation.cs`, `PanelTransition.cs`, `ScreenFade.cs`, `UIShake.cs` vb.)
- Sahnelerdeki Canvas hiyerarşisi (`Canvas/BackgroundLayer`, `Canvas/ContentLayer`, `Canvas/HUDLayer`, `Canvas/OverlayLayer`)

---

## 📌 KRİTİK TASARIM VE MİMARİ KURALLAR
1. **İki Oyunculu Kuralı:** Battle modu yalnızca iki kişiliktir; single-player veya yapay zeka rakip modu eklenemez.
2. **Kopya İpucu Yasağı:** Düello kartlarında eğitici el işareti gösterilmez; kartlar büyü/element/fantasy tarzı sembolik görsellere sahip olmalıdır.
3. **Model Etiketleri Sabittir:** `modelLabel` değerleri değiştirilemez.
4. **Modallar Opaktır:** Settings ve diğer overlay ekranları tam ekran ve opak kalmalıdır.
5. **Referans Koruması:** Görsel düzenleme yapılırken mevcut `Button`, script veya Inspector bağlantıları bozulamaz veya silinemez.
