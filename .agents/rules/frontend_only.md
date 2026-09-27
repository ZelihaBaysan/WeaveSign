# WeaveSign — Frontend Only & Protection Rules

Bu projede çalışırken asistan SADECE FRONTEND geliştirmesi yapacaktır.
Backend oyun mantığına ve Yaren'in yapay zeka/model entegrasyonu alanlarına DOKUNMAK KESİNLİKLE YASAKTIR.
Tüm kurallar ve mimari ayrıntılar için referans: `Documentation/WEAVESIGN_PROJE_YAPISI_VE_SORUMLULUK_REHBERI.md`

## ⛔ ASLA DOKUNULMAYACAK ALANLAR (KIRMIZI ÇİZGİ)
1. **Backend / Oyun Mantığı Kodları:**
   - `Assets/Scripts/Battle/`
   - `Assets/Scripts/Learning/`
   - `Assets/Scripts/NameWorkshop/`
   - `Assets/Scripts/Core/`
   - `Assets/Scripts/Editor/`

2. **Yaren / ML & Model Entegrasyonu:**
   - `Assets/Scripts/Recognition/`
   - `Assets/ML/`

3. **Veri Assetleri:**
   - `Assets/Resources/`

## ✅ İZİN VERİLEN ALANLAR (SADECE FRONTEND)
- `Assets/Art/`
- `Assets/Animations/`
- `Assets/Materials/`
- `Assets/Audio/`
- `Assets/Prefabs/UI/`
- `Assets/Scripts/UI/`
- Sahnelerdeki Canvas katmanları (`BackgroundLayer`, `ContentLayer`, `HUDLayer`, `OverlayLayer`)
