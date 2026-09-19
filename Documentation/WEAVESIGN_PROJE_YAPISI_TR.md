# WeaveSign – Proje Yapısı ve Çalışma Rehberi

## 1. Projenin amacı

WeaveSign, Türk İşaret Dili öğrenimini destekleyen Unity tabanlı bir projedir.

Projede üç ana bölüm vardır:

1. Öğrenme Atölyesi
   - Harf Öğren
   - Kelime / İşaret Öğren

2. İsmini Yaz Atölyesi
   - Kullanıcı işaret dili harflerini kullanarak ismini oluşturur.
   - Harf modelindeki `nothing`, `del`, `space` sınıfları burada kontrol komutları olarak kullanılabilir.

3. Düello
   - İki oyunculu kart tabanlı moddur.
   - Single-player / AI modu YOKTUR.
   - Oyuncular kart seçer ve istenen işareti yapar.
   - Model tahmini ve confidence/skor sonucuna göre saldırı veya iyileştirme uygulanır.

---

# 2. Görev ayrımı

## A Kişisi – Unity / Oyun / UI

A kişisinin sorumlulukları:

- Sahne yönetimi
- Ana menü
- Öğrenme ekranları
- İsmini Yaz Atölyesi
- Düello sistemi
- Kart sistemi
- Can / hasar / iyileştirme mantığı
- UI
- Ayarlar sistemi
- Ses ve müzik sistemi
- Animasyon / VFX
- Eğitim görsellerinin Unity tarafında gösterilmesi
- Recognition sonucunun oyun mekaniklerine uygulanması
- Final görsel tasarım ve build

## B Kişisi – Kamera / MediaPipe / Sentis / ML

B kişisinin sorumlulukları:

- Kamera görüntüsünü almak
- El landmarklarını üretmek
- MediaPipe entegrasyonu
- Gerekliyse landmark ön işleme
- Sentis modelini yüklemek
- Harf modelini çalıştırmak
- Kelime/işaret modelini çalıştırmak
- Tahmin sonucunu Unity oyun katmanına iletmek
- Confidence / skor üretmek
- Model label eşleşmelerini korumak

B kişisi oyun kurallarını veya BattleManager içindeki hasar sistemini değiştirmemelidir.

---

# 3. Ana klasör yapısı

## Scenes

Assets/Scenes/

- MainMenu/
  - MainMenu.unity

- Learning/
  - LearningMenu.unity
  - LetterLearning.unity
  - WordLearning.unity

- NameWorkshop/
  - NameWorkshop.unity

- Battle/
  - Battle.unity

---

# 4. Script yapısı

Assets/Scripts/

## Core

Assets/Scripts/Core/

Ortak sistemler burada bulunur.

Örnek:

- SceneLoader.cs
- SettingsPanelController.cs

---

## Battle

Assets/Scripts/Battle/

Düello moduna özel oyun kodları burada bulunur.

Assets/Scripts/Battle/Cards/

- SpellCard.cs
- CardView.cs
- CardAreaController.cs

BattleManager.cs düello akışını yönetir.

B kişisi model kodunu BattleManager.cs içine yazmamalıdır.

---

## Learning

Assets/Scripts/Learning/

### Common

Assets/Scripts/Learning/Common/

Harf ve kelime öğrenme ekranlarının ortak kullanabileceği kodlar burada tutulabilir.

### LetterLearning

Assets/Scripts/Learning/LetterLearning/

Harf öğrenme ekranına özel kodlar.

Örnek:

- LetterLearningController.cs

### WordLearning

Assets/Scripts/Learning/WordLearning/

Kelime / işaret öğrenme ekranına özel kodlar.

---

## NameWorkshop

Assets/Scripts/NameWorkshop/

İsmini Yaz Atölyesi kodları burada bulunacaktır.

Bu mod harf recognition sistemini kullanacaktır.

`nothing`, `del` ve `space` gibi model sınıfları burada komut olarak değerlendirilebilir.

---

# 5. Recognition mimarisi

Recognition tarafının ana klasörü:

Assets/Scripts/Recognition/

Mevcut ortak dosyalar:

- RecognitionResult.cs
- RecognitionServiceBase.cs
- MockRecognitionService.cs

Alt klasörler:

## Camera

Assets/Scripts/Recognition/Camera/

B kişisi kamera ile ilgili scriptleri burada tutmalıdır.

Örnek:

- CameraManager.cs
- CameraFrameProvider.cs

## HandTracking

Assets/Scripts/Recognition/HandTracking/

MediaPipe ve landmark işlemleri burada tutulmalıdır.

Örnek:

- HandLandmarkProvider.cs
- HandTrackingManager.cs

## Model

Assets/Scripts/Recognition/Model/

Sentis ve model inference kodları burada tutulmalıdır.

Örnek:

- LetterModelRunner.cs
- WordModelRunner.cs
- ModelPreprocessor.cs

---

# 6. B kişisinin ana entegrasyon noktası

B kişisinin oyun sistemine vermesi gereken temel sonuç:

- predictedLabel
- confidence

Örnek:

predictedLabel = "Merhaba"
confidence = 0.91

veya:

predictedLabel = "Z"
confidence = 0.87

Oyun katmanı bu sonucu kullanır.

B kişisi:

- hasar hesaplamamalı
- can değiştirmemeli
- kart seçmemeli
- sıra değiştirmemeli

B sadece recognition sonucunu üretmelidir.

---

# 7. RecognitionResult

RecognitionResult ortak veri tipidir.

Mantıksal olarak şu bilgileri taşır:

- predictedLabel
- confidence

Bu dosya A ve B tarafından ortak kullanılır.

Aynı struct/class başka dosyalarda tekrar tanımlanmamalıdır.

---

# 8. RecognitionServiceBase

RecognitionServiceBase, oyun kodu ile gerçek model arasında ara katmandır.

Amaç:

BattleManager
        |
RecognitionServiceBase
        |
----------------------------
|                          |
MockRecognitionService   Gerçek Recognition
                          |
                    MediaPipe + Sentis

MockRecognitionService geliştirme ve test sırasında kullanılır.

Gerçek sistem hazır olduğunda mock yerine gerçek recognition servisi bağlanacaktır.

NOT:

Recognition altyapısı yalnızca SpellCard'a bağımlı bırakılmamalıdır.

Aynı recognition sistemi şu alanlarda kullanılacaktır:

- Harf Öğren
- Kelime Öğren
- İsmini Yaz Atölyesi
- Düello

Bu nedenle ortak API ileride target label / model type gibi genel verilerle çalışacak şekilde düzenlenecektir.

---

# 9. ML klasörü

Assets/ML/

B kişisinin model ile ilgili dosyaları burada tutulur.

## Models

Assets/ML/Models/

Örnek:

- LetterModel.onnx
- WordModel.onnx

Modelin Unity Sentis tarafından farklı bir konumdan yüklenmesi gerekirse bu yapı entegrasyon sırasında değiştirilebilir.

## Config

Assets/ML/Config/

Model label listeleri ve model ayarları burada tutulabilir.

Örnek:

- letter_labels
- word_labels
- model config

## TestData

Assets/ML/TestData/

Model entegrasyonu sırasında kullanılacak test dosyaları burada tutulabilir.

---

# 10. Harf modeli

Harf Öğren ekranında Türk alfabesindeki 29 harf desteklenecektir:

A
B
C
Ç
D
E
F
G
Ğ
H
I
İ
J
K
L
M
N
O
Ö
P
R
S
Ş
T
U
Ü
V
Y
Z

B kişisinin güncel dataset/model label isimlerini A kişisine bildirmesi gerekir.

UI'da gösterilen harf ile modelLabel birbirinden ayrı tutulabilir.

Örnek:

displayName = "Ş"
modelLabel = modelde kullanılan gerçek sınıf adı

---

# 11. Kelime modeli

Düello ve Kelime Öğren için kullanılan mevcut kelime sınıfları:

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

Model label değerlerinin yazımı ve büyük/küçük harfleri korunmalıdır.

Örneğin:

displayName = "Özür Dilemek"
modelLabel = "Ozur-Dilemek"

---

# 12. Prefab yapısı

Assets/Prefabs/

## Battle

Düello prefabları.

Örnek:

- CardVisual.prefab

## Common

Bütün modlarda kullanılabilecek ortak UI.

Örnek:

- SettingsUI.prefab

## MainMenu

Ana menüye özel prefablar.

## Learning

### LetterLearning

Harf öğrenme prefabları.

Örnek:

- LetterButton.prefab

### WordLearning

Kelime öğrenme prefabları.

## NameWorkshop

İsmini Yaz Atölyesi prefabları.

---

# 13. Frontend / Görsel tasarım alanları

A kişisinin görsel tasarım dosyaları:

Assets/Art/

## UI

Assets/Art/UI/

- Common/
- MainMenu/
- Learning/
- NameWorkshop/
- Battle/

Buton, panel, pencere, arka plan ve genel UI görselleri burada tutulur.

## Cards

Assets/Art/Cards/Battle/

Düello kartlarının sanatsal artwork ve çerçeve görselleri burada tutulur.

Kart sistemi hibrit olacaktır:

- Çerçeve / artwork = görsel asset
- Kart adı = Unity TMP
- Tür = Unity TMP
- Zorluk = Unity TMP
- Güç = Unity TMP
- Buton = Unity UI

Kart bilgileri tek parça görselin içine yazılmayacaktır.

## Learning

Assets/Art/Learning/

### Letters

Harflerin öğretici el işareti görselleri.

### Words

Kelimelerin öğretici işaret görselleri.

Düello kartlarındaki artwork ile eğitim görselleri farklıdır.

Düelloda cevabı gösterecek el işareti görseli kullanılmaz.

---

# 14. Resources

Assets/Resources/

## Cards

Düellodaki 20 SpellCard asseti burada tutulur.

Bu path mevcut kod tarafından kullanılmaktadır.

## Learning

Assets/Resources/Learning/

- Letters/
- Words/

Eğitim verileri gerektiğinde burada tutulabilir.

---

# 15. Ses

Assets/Audio/

- Music/
- SFX/
- UI/

Music:
arka plan müzikleri.

SFX:
oyun efektleri.

UI:
buton / menü sesleri.

Settings ekranında:

- genel ses seviyesi
- müzik seviyesi

ayarları bulunmaktadır.

---

# 16. Animasyon

Assets/Animations/

- UI/
- Battle/
- Learning/

UI geçişleri, kart efektleri ve eğitim geri bildirim animasyonları burada tutulur.

---

# 17. Materials

Assets/Materials/

- UI/
- Battle/

UI veya Battle görsel efektlerinde kullanılan material dosyaları burada tutulur.

---

# 18. Ayarlar sistemi

SettingsUI ortak prefab olarak kullanılır.

Her ana ekranda erişilebilir olacaktır.

Ayarlar ekranı:

- SES
- MÜZİK
- ANA MENÜYE DÖN
- OYUNDAN ÇIK
- KAPAT

özelliklerini içerir.

Modal ekranlar tam ekran ve opak olmalıdır.

Arkadaki UI görünmemeli ve tıklanmamalıdır.

Bu kural Harf Seç gibi diğer modal ekranlar için de geçerlidir.

---

# 19. Düello sistemi

Düello iki oyunculudur.

Single-player / AI modu yoktur.

Kart türleri:

- Attack
- Heal

Saldırı kartı rakibin canını azaltır.

İyileştirme kartı aktif oyuncunun canını artırır.

Maksimum can:

100

Recognition sonucu kart etkisini belirler.

---

# 20. Git çalışma düzeni

main:
kararlı sürüm.

zeliha:
A kişisinin çalışma branch'i.

yaren:
B kişisinin çalışma branch'i.

B kişisi mümkün olduğunca recognition ve ML klasörlerinde çalışmalıdır.

B kişisi işini bitirdiğinde:

yaren -> main

Pull Request açmalıdır.

A kişisi de:

zeliha -> main

Pull Request üzerinden değişikliklerini birleştirir.

---

# 21. B kişisi için kısa özet

Çalışılacak ana yerler:

Assets/Scripts/Recognition/Camera/
Assets/Scripts/Recognition/HandTracking/
Assets/Scripts/Recognition/Model/
Assets/ML/Models/
Assets/ML/Config/
Assets/ML/TestData/

Ortak kullanılacak:

Assets/Scripts/Recognition/RecognitionResult.cs
Assets/Scripts/Recognition/RecognitionServiceBase.cs

Dokunulmaması tercih edilen oyun kodları:

Assets/Scripts/Battle/BattleManager.cs
Assets/Scripts/Battle/Cards/
Assets/Scripts/Learning/
Assets/Scripts/NameWorkshop/

Gerekli bir API değişikliği varsa A kişisiyle konuşularak yapılmalıdır.

Ana hedef:

Kamera -> Landmark -> Model -> predictedLabel + confidence

çıktısını güvenilir biçimde Unity oyun katmanına vermektir.
