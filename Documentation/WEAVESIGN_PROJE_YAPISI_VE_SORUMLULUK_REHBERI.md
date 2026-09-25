# WeaveSign — Proje Yapısı, Sorumluluk Alanları ve Dokunma Rehberi

Bu dosya, WeaveSign Unity projesinde hangi klasörün ve sahne katmanının ne amaçla kullanıldığını, hangi alanların **frontend**, hangi alanların **oyun mantığı/backend**, hangi alanların ise **Yaren'in model entegrasyonu** için ayrıldığını açıklar.

Amaç: Projeyi daha sonra başka bir geliştiriciye, başka bir AI aracına veya Yaren'e verdiğimizde kişinin **hangi dosyaya dokunabileceğini ve hangi dosyaya dokunmaması gerektiğini tek bakışta anlaması**.

---

## 1. Proje özeti

WeaveSign, Türk İşaret Dili odaklı bir eğitim + düello uygulamasıdır.

Ana modlar:

1. **Öğrenme Atölyesi**
   - Harf Öğren
   - Kelime / İşaret Öğren
2. **İsmini Yaz Atölyesi**
3. **Düello**
   - Sadece iki oyunculu
   - Single-player / AI rakip modu yok

Tüm modlarda işaret tanıma sistemi ortak bir `RecognitionServiceBase` üzerinden çalışır. Gerçek model bağlanmadan önce bütün oyun akışları `MockRecognitionService` ile çalışacak şekilde hazırlanmıştır.

---

## 2. En önemli mimari kural

### FRONTEND ile BACKEND birbirinden ayrı tutulacak.

Frontend değişikliği yaparken mümkün olduğunca şu oyun mantığı dosyalarına dokunulmaz:

```text
BattleManager.cs
LetterLearningController.cs
WordLearningController.cs
NameWorkshopController.cs
Recognition dosyaları
kart denge verileri
model entegrasyon kodları
```

Hedef:

> Bir ekranın görünüşünü tamamen değiştirebilmeliyim, ama oyun mantığı koduna dokunmak zorunda kalmamalıyım.

---

## 3. Sahne katman standardı

Frontend tarafında tüm sahnelerde `Canvas` altında görsel katmanlar kullanılır.

### MainMenu

```text
Canvas
├── BackgroundLayer
├── ContentLayer
│   ├── LearningButton
│   ├── NameWorkshopButton
│   └── BattleButton
└── OverlayLayer
    └── SettingsUI
```

### LearningMenu

```text
Canvas
├── BackgroundLayer
├── ContentLayer
│   ├── LetterLearningButton
│   └── WordLearningButton
└── OverlayLayer
    └── SettingsUI
```

### LetterLearning

```text
Canvas
├── BackgroundLayer
├── ContentLayer
│   ├── TitleText
│   ├── BackButton
│   └── LearningContentPanel
└── OverlayLayer
    ├── SettingsUI
    └── LetterSelectionPanel
```

### WordLearning

```text
Canvas
├── BackgroundLayer
├── ContentLayer
│   ├── TitleText
│   ├── BackButton
│   └── LearningContentPanel
└── OverlayLayer
    ├── SettingsUI
    └── WordSelectionPanel
```

### NameWorkshop

```text
Canvas
├── BackgroundLayer
├── ContentLayer
│   ├── TitleText
│   └── WorkshopContentPanel
└── OverlayLayer
    └── SettingsUI
```

### Battle

```text
Canvas
├── BackgroundLayer
├── ContentLayer
│   └── CardArea
├── HUDLayer
│   ├── TopHUD
│   ├── BattleStatusText
│   ├── Player1HPBar
│   ├── Player1HPText
│   ├── Player2HPBar
│   └── Player2HPText
└── OverlayLayer
    ├── ResultPanel
    └── SettingsUI
```

Katmanların anlamı:

- `BackgroundLayer`: arka plan görselleri, dekoratif illustration ve sahne öğeleri.
- `ContentLayer`: ana ekran içeriği, normal butonlar, kart alanı, eğitim paneli.
- `HUDLayer`: battle gibi sahnelerde oyuncu canı, sıra bilgisi ve durum yazısı.
- `OverlayLayer`: her şeyin üstünde görünmesi gereken tam ekran modal/overlay ekranları. Örnek: Settings, harf/kelime seçim paneli, ResultPanel.

---

## 4. SADECE FRONTEND TARAFI

Aşağıdaki alanlar görsel düzenleme için güvenli bölgelerdir.

```text
Assets/Art/
Assets/Animations/
Assets/Materials/
Assets/Audio/
Assets/Prefabs/UI/
Assets/Scripts/UI/
```

Mevcut önemli frontend klasörleri:

```text
Assets/Art/
├── Battle/
├── Cards/
│   └── Battle/
├── Icons/
├── Learning/
│   ├── Letters/
│   └── Words/
└── UI/
    ├── Battle/
    ├── Common/
    ├── Learning/
    ├── MainMenu/
    └── NameWorkshop/
```

Frontend scriptleri için kullanılacak alan:

```text
Assets/Scripts/UI/
├── Common/
├── MainMenu/
├── Learning/
├── NameWorkshop/
└── Battle/
```

Bu klasöre yalnızca görsel davranış scriptleri konmalıdır. Örnek:

```text
ButtonHoverAnimation.cs
PanelTransition.cs
ScreenFade.cs
CardHoverEffect.cs
HPBarAnimation.cs
UIShake.cs
```

Bu scriptler oyun kurallarını bilmemelidir.

---

## 5. Frontend'de güvenle yapılabilecek şeyler

- butonların yerini değiştirmek
- butonların boyutunu değiştirmek
- font değiştirmek
- renk değiştirmek
- background değiştirmek
- panel sprite değiştirmek
- kart frame / border eklemek
- ikon eklemek
- hover efekti
- click animasyonu
- fade animasyonu
- panel giriş / çıkış animasyonu
- HP bar görselini değiştirmek
- Settings görünümünü değiştirmek
- spacing / alignment düzeltmek
- resolution uyumluluğu
- RectTransform düzenlemek

Bunlar için backend kodu değiştirilmemelidir.

---

## 6. Frontend için önemli görsel kurallar

### Eğitim ekranları

`LetterLearning` ve `WordLearning` ekranlarında **gerçek eğitim amaçlı el işareti görselleri** gösterilir.

Bu görseller şu veri alanına bağlanır:

```text
LearningItemData.signImage
```

Harf görselleri:

```text
Assets/Art/Learning/Letters/
```

Kelime görselleri:

```text
Assets/Art/Learning/Words/
```

### Düello kartları

Düello kartlarında **öğretici el işareti görseli gösterilmeyecek**.

Düello kartları fantasy artwork / sembol / element / büyü temalı olabilir. Kullanıcı kartı seçerken doğru işareti görselden kopyalayamamalıdır.

---

## 7. BACKEND / oyun mantığı — frontend çalışırken dokunma

```text
Assets/Scripts/Battle/
Assets/Scripts/Learning/
Assets/Scripts/NameWorkshop/
Assets/Scripts/Core/
Assets/Scripts/Recognition/
Assets/Scripts/Editor/
```

Frontend çalışması sırasında bu klasörlerdeki dosyalar gereksiz yere değiştirilmemelidir.

---

## 8. Battle sistemi

Ana dosyalar:

```text
Assets/Scripts/Battle/BattleManager.cs
Assets/Scripts/Battle/Cards/SpellCard.cs
Assets/Scripts/Battle/Cards/CardView.cs
Assets/Scripts/Battle/Cards/CardAreaController.cs
```

### BattleManager.cs sorumlulukları

- sıra sistemi
- kart etkisini uygulama
- HP hesaplama
- attack / heal
- başarısız işaret cezası
- recognition sonucunu değerlendirme
- game over
- sonuç ekranı
- HP barlarını güncelleme

Frontend için bu dosyaya dokunulmamalıdır.

### Güncel kart sistemi

```text
Toplam: 20 kart
Attack: 15
Heal: 5
Her elde: 5 kart
Minimum Attack: 3
```

Güncel güç dengesi:

```text
Attack
Difficulty 1 = 20
Difficulty 2 = 30
Difficulty 3 = 40

Heal
Difficulty 1 = 10
Difficulty 2 = 15
```

Başarısız recognition:

```text
aktif oyuncu -5 HP
```

Recognition çarpanları:

```text
< 50       = fail
50–69      = x0.50
70–84      = x0.75
85–94      = x1.00
95–100     = x1.10
```

---

## 9. Harf Öğren sistemi

Ana controller:

```text
Assets/Scripts/Learning/LetterLearning/LetterLearningController.cs
```

Veri:

```text
Assets/Resources/Learning/Letters/
```

29 Türkçe harf:

```text
A B C Ç D E F G Ğ H I İ J K L M N O Ö P R S Ş T U Ü V Y Z
```

Fonksiyonlar:

- önceki / sonraki harf
- harf seçimi
- eğitim görseli
- DENE
- mock recognition
- başarı / tekrar dene
- Success / Mistake SFX

Frontend çalışmasında controller değiştirilmemelidir.

---

## 10. Kelime Öğren sistemi

Ana controller:

```text
Assets/Scripts/Learning/WordLearning/WordLearningController.cs
```

Veri:

```text
Assets/Resources/Learning/Words/
```

Model label'ları tam olarak:

```text
Anne
Arkadas
Baba
Dur
Ev
Evet
Hayir
Kardes
Merhaba
Nasil
Nerede
Ozur-Dilemek
Tamam
Telefon
Tesekkurler
Tuvalet
Yemek
icmek
iyi
kotu
```

UI'da görünen Türkçe adlar model label'lardan farklı olabilir. Örnek:

```text
Model Label: Tesekkurler
Display Name: Teşekkürler
```

**Model label isimleri keyfi şekilde değiştirilmemelidir.**

---

## 11. İsmini Yaz Atölyesi

Ana controller:

```text
Assets/Scripts/NameWorkshop/NameWorkshopController.cs
```

Doğru akış:

```text
Kullanıcı adını klavyeyle yazar
↓
BAŞLA
↓
ÖĞRENME TURU
A → görsel göster → kullanıcı A işaretini yapar
Y → görsel göster → kullanıcı Y işaretini yapar
...
↓
EZBER TURU
A → görsel YOK
Y → görsel YOK
...
↓
TAMAMLANDI
```

Bu mod modelin rastgele harflerini isme ekleyen bir sistem değildir. Model her adımda hedef harfi kontrol eder.

```text
Beklenen: A
Tahmin: A
→ doğru

Beklenen: Y
Tahmin: B
→ yanlış
```

---

## 12. Recognition katmanı

Ortak dosyalar:

```text
Assets/Scripts/Recognition/RecognitionResult.cs
Assets/Scripts/Recognition/RecognitionRequest.cs
Assets/Scripts/Recognition/RecognitionServiceBase.cs
Assets/Scripts/Recognition/MockRecognitionService.cs
```

Ana kontrat:

```text
RecognitionRequest
→ modelType
→ expectedLabel

RecognitionResult
→ predictedLabel
→ confidence
```

Oyun modları gerçek modelin nasıl çalıştığını bilmez. Bu ayrım özellikle korunmalıdır.

---

## 13. YAREN'İN ÇALIŞACAĞI ALANLAR

Yaren'in ana sorumluluk alanları:

```text
Assets/Scripts/Recognition/Camera/
Assets/Scripts/Recognition/HandTracking/
Assets/Scripts/Recognition/Model/

Assets/ML/Models/
Assets/ML/Config/
Assets/ML/TestData/
```

Yaren'in işi:

```text
Camera
↓
MediaPipe / landmark extraction
↓
preprocessing
↓
Sentis / model inference
↓
predictedLabel + confidence
```

Final çıktı oyuna `RecognitionResult` üzerinden verilmelidir.

---

## 14. Yaren'in dokunmaması gereken yerler

Yaren model entegrasyonu sırasında mümkün olduğunca şuraları değiştirmemelidir:

```text
Assets/Scripts/Battle/BattleManager.cs
Assets/Scripts/Battle/Cards/
Assets/Scripts/Learning/LetterLearning/
Assets/Scripts/Learning/WordLearning/
Assets/Scripts/NameWorkshop/
Assets/Scripts/UI/
Assets/Art/
```

Özellikle:

- damage hesaplaması Yaren'in işi değildir
- heal hesaplaması Yaren'in işi değildir
- oyuncu HP'si Yaren'in işi değildir
- UI feedback Yaren'in işi değildir
- kart seçimi Yaren'in işi değildir
- sahne geçişi Yaren'in işi değildir

Model yalnızca tahmin üretmelidir.

---

## 15. Yaren için entegrasyon kuralı

Gerçek model geldiğinde hedef:

```text
MockRecognitionService
↓
RealRecognitionService
```

değişimidir.

Oyun modlarının controller kodları değiştirilmemelidir.

İdeal yapı:

```text
BattleManager
LetterLearningController
WordLearningController
NameWorkshopController
        ↓
RecognitionServiceBase
        ↓
RealRecognitionService
        ↓
Camera / MediaPipe / Sentis / Model
```

---

## 16. expectedLabel hakkında önemli not

`expectedLabel`, mock sistemde doğru / yanlış senaryolarını test etmek için kullanılabilir.

Gerçek model inference sırasında model hedef etiketi bilerek tahmin yapmamalıdır.

Doğru akış:

```text
kamera girdisi
↓
model bağımsız tahmin üretir
↓
oyun tarafı predictedLabel ile expectedLabel'i karşılaştırır
```

---

## 17. ML dosyaları

```text
Assets/ML/
├── Config/
├── Models/
└── TestData/
```

Unity Sentis import davranışına göre gerçek model assetlerinin fiziksel konumu daha sonra değişebilir. Bu durumda değişiklik yalnızca ML / Recognition tarafında yapılmalı, oyun controller'larına yayılmamalıdır.

---

## 18. Audio sistemi

Ana sistem:

```text
Assets/Scripts/Core/AudioManager.cs
Assets/Scripts/Core/GlobalButtonSFX.cs
```

Dosyalar:

```text
Assets/Audio/Music/
Assets/Audio/SFX/
```

Mevcut SFX rolleri:

```text
ButtonClick
CardSelect
Attack
Heal
Success
Mistake
Victory
```

Müzik tüm sahnelerde devam eder. `AudioManager` `DontDestroyOnLoad` kullanır.

---

## 19. Settings sistemi

Ana dosya:

```text
Assets/Scripts/Core/SettingsPanelController.cs
```

Settings:

- tam ekran
- opak
- SES slider
- MÜZİK slider
- ANA MENÜ
- OYUNDAN ÇIK
- KAPAT

Ses değerleri `PlayerPrefs` ile saklanır.

`Application.Quit()` Unity Editor içinde oyunu kapatmaz; Windows build içinde çalışır.

---

## 20. Prefab alanları

Mevcut önemli prefablar:

```text
Assets/Prefabs/Common/SettingsUI.prefab
Assets/Prefabs/Battle/CardVisual.prefab
Assets/Prefabs/Learning/LetterButton.prefab
Assets/Prefabs/Learning/WordLearning/WordButton.prefab
```

Frontend çalışmasında prefabın görsel tarafı düzenlenebilir. Ancak gerekli `Button` componentleri, script componentleri ve controller bağlantıları silinmemelidir.

---

## 21. Resources alanı

Runtime'da yüklenen veriler:

```text
Assets/Resources/Cards/
Assets/Resources/Learning/Letters/
Assets/Resources/Learning/Words/
```

Bunlar görsel prefab klasörleri değildir. Buradaki `.asset` dosyaları veri taşır.

Özellikle şu alanlar oyun verisidir:

```text
modelLabel
displayName
difficulty
cardType
basePower
signImage
```

Frontend amacıyla oyun mantığı değerleri keyfi değiştirilmemelidir.

---

## 22. Editor scriptleri

```text
Assets/Scripts/Editor/CardAssetGenerator.cs
Assets/Scripts/Editor/LetterLearningDataGenerator.cs
Assets/Scripts/Editor/WordLearningDataGenerator.cs
```

Bunlar Unity Editor içinde veri assetlerini üretmek / güncellemek için kullanılır. Frontend çalışırken bu dosyalara dokunmaya gerek yoktur.

---

## 23. Git branch düzeni

Ana branchler:

```text
main
zeliha
yaren
```

- `zeliha` → A kişi / oyun / UI / frontend / genel entegrasyon
- `yaren` → model / camera / MediaPipe / Sentis / ML
- `main` → birleşmiş stabil sürüm

Yaren mümkün olduğunca:

```text
yaren → Pull Request → main
```

akışıyla çalışmalıdır.

---

## 24. Frontend tasarım kuralı

Görseller Unity içinde sıfırdan çizilmeyecek.

Tercih edilen dış tasarım araçları:

```text
Figma
Canva
Photoshop
Photopea
```

Çıktılar PNG / Sprite olarak Unity'ye import edilir.

Unity'nin görevi:

- layout
- dynamic text
- data
- button logic
- scene flow
- animation
- input
- runtime logic

olacaktır.

---

## 25. Frontend'e müdahale ederken kırmızı çizgiler

1. Backend controller scriptlerini sadece görsel değiştirmek için düzenleme.
2. `modelLabel` değerlerini görsel isimlere uydurmak için değiştirme.
3. Recognition logic'i UI scriptine taşıma.
4. Battle damage hesabını `CardView` içine taşıma.
5. LearningItemData yerine UI objesinin içine veri hard-code etme.
6. Settings'i saydam yapma.
7. Modal ekranların altındaki UI'yı tıklanabilir bırakma.
8. Düello kartlarında öğretici el işareti gösterme.
9. Single-player sistemi ekleme.
10. Yaren'in model kodunu `BattleManager` içine gömme.

---

## 26. Başka bir AI'ye verilecek kısa talimat

```text
1. Frontend değişiklikleri yalnızca UI katmanlarında ve frontend klasörlerinde yap.
2. BattleManager, Learning controller'ları ve NameWorkshop controller'ı oyun mantığıdır.
3. Recognition katmanı model entegrasyonu için soyutlanmıştır.
4. Yaren yalnızca Camera / HandTracking / Model / ML alanlarında çalışmalıdır.
5. Gerçek model RecognitionResult(predictedLabel, confidence) üretmelidir.
6. Model label isimlerini değiştirme.
7. Modal ekranlar tam ekran ve opak kalmalıdır.
8. Battle iki oyunculudur; single-player ekleme.
9. Düello kartlarında eğitim amaçlı el işareti gösterme.
10. Görsel değişiklik yaparken mevcut Button / script / Inspector bağlantılarını bozma.
```

---

## 27. Frontend için kısa harita

### Dokunabilirsin

```text
Canvas/BackgroundLayer
Canvas/ContentLayer
Canvas/HUDLayer
Canvas/OverlayLayer

Assets/Art/
Assets/Animations/
Assets/Materials/
Assets/Audio/
Assets/Prefabs/UI/
Assets/Scripts/UI/
```

### Dikkatli dokun

```text
Assets/Prefabs/Battle/
Assets/Prefabs/Learning/
Assets/Prefabs/Common/
```

Görsel kısmını değiştirebilirsin fakat component ve referansları silme.

### Frontend için dokunma

```text
Assets/Scripts/Battle/
Assets/Scripts/Learning/
Assets/Scripts/NameWorkshop/
Assets/Scripts/Recognition/
Assets/Scripts/Editor/
Assets/Resources/
Assets/ML/
```

---

## 28. Yaren için kısa harita

### Yaren'in ana alanı

```text
Assets/Scripts/Recognition/Camera/
Assets/Scripts/Recognition/HandTracking/
Assets/Scripts/Recognition/Model/
Assets/ML/Models/
Assets/ML/Config/
Assets/ML/TestData/
```

### Ortak API — dikkatli değiştir

```text
RecognitionRequest.cs
RecognitionResult.cs
RecognitionServiceBase.cs
```

Bu üç dosyada yapılacak değişiklik bütün modları etkileyebilir.

### Yaren'in dokunmaması tercih edilen alanlar

```text
BattleManager.cs
CardAreaController.cs
LetterLearningController.cs
WordLearningController.cs
NameWorkshopController.cs
Assets/Scripts/UI/
Assets/Art/
Scenes içindeki frontend düzeni
```

---

## 29. Son mimari hedef

```text
               FRONTEND
                   │
        Canvas / UI / Animation
                   │
                   ▼
          Controller Referansları
                   │
                   ▼
              GAME LOGIC
                   │
        ┌──────────┴──────────┐
        │                     │
     Battle                Learning
        │                     │
        └──────────┬──────────┘
                   │
                   ▼
         RecognitionServiceBase
                   │
                   ▼
          RealRecognitionService
                   │
                   ▼
      Camera / MediaPipe / Sentis
                   │
                   ▼
                 Model
```

Frontend değişebilir. Model değişebilir. Ama aradaki sözleşme sabit kaldığı sürece proje parçaları birbirini bozmadan geliştirilebilir.

---

## 30. Kısa sorumluluk özeti

### Zeliha / A tarafı

- oyun akışı
- frontend
- UI
- ses
- animasyon
- sahneler
- kart sistemi
- learning sistemleri
- name workshop
- final build / entegrasyon

### Yaren / B tarafı

- kamera
- landmark
- MediaPipe
- preprocessing
- Sentis
- gerçek model
- prediction + confidence

### Frontend

- Canvas katmanları
- Art
- UI prefabları
- UI animasyonları
- görsel düzen

### Model ile oyun arasındaki tek ortak kapı

```text
RecognitionServiceBase
RecognitionRequest
RecognitionResult
```

Bu ayrım korunmalıdır.
