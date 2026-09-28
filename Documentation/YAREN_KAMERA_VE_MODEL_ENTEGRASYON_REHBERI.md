# WeaveSign — Yaren İçin Kamera ve Yapay Zeka Model Entegrasyon Rehberi

Bu rehber; **Yaren'in** işaret dili tanıma modelini ve canlı kamera (webcam / el izleme) görüntüsünü WeaveSign sahnelerine entegre ederken kullanacağı arayüz çerçevelerini, GameObject hiyerarşisini ve kod bağlantı noktalarını adım adım açıklar.

Bu rehberdeki temel amaç şudur:

> Yaren yalnızca kamera, el takibi, preprocessing ve gerçek model inference tarafını geliştirecek.  
> Oyun kuralları, hasar sistemi, heal sistemi, öğrenme akışları ve UI davranışları değiştirilmemelidir.

---

## 1. Genel Mimari ve Tasarım Vizyonu

Eğitim ve atölye ekranlarında kullanıcı deneyimi **3 ana görsel unsur** üzerine kurulmuştur:

1. **Hedef İşaret Kartı (Sol Çerçeve):** Yapılması gereken Türk İşaret Dili hareketinin görselini fantezi/kart çerçevesi içinde gösterir (`SignImage` + `SignFrame`).

2. **Kullanıcının Kendi Hareketi (Sağ Çerçeve):** Kullanıcının web kamerasından canlı görüntüsünü veya modelin el eklem noktalarıyla (landmark/skeletal tracking) zenginleştirilmiş akışını fantezi çerçeve içinde gösterir (`CameraPreviewPanel` → `CameraPreview`).

3. **Doğruluk ve Değerlendirme Metni (Alt Orta):** Modelin tahmin ettiği etiket, başarı skoru ve `"BAŞARILI!"` / `"TEKRAR DENE"` geri bildirimini sunar (`FeedbackText`).

> ⚠️ **DÜELLO (BATTLE) MODU KURALI:**  
> `Battle.unity` sahnesinde kamera önizleme çerçevesi **YOKTUR**. Oyuncular düello sırasında kartlarına, can barlarına ve rakibe odaklanırlar. Model arka planda çalışarak işareti değerlendirir.

---

## 2. Hazırlanan Çerçeveler ve Sahne Hiyerarşisi

Aşağıdaki üç sahnede kamera ve hedef kart çerçeveleri fantezi temasına uygun olarak hazırlanmıştır:

- `Assets/Scenes/Learning/LetterLearning.unity` — Harf Öğren
- `Assets/Scenes/Learning/WordLearning.unity` — Kelime Öğren
- `Assets/Scenes/NameWorkshop/NameWorkshop.unity` — İsmini Yaz Atölyesi

### Ortak Hiyerarşi Şeması

Örnek:

```text
Canvas
└── ContentLayer
    └── LearningContentPanel / WorkshopContentPanel
        ├── SignBackdrop
        ├── SignImage
        ├── SignFrame
        ├── SignImageLabel
        │
        ├── FeedbackFrame
        ├── FeedbackText
        │
        ├── CameraPreviewPanel
        │   ├── CameraBackdrop
        │   ├── CameraPreview
        │   ├── PreviewBorder
        │   └── PreviewLabel
        │
        └── Butonlar & İlerleme
```

### Elemanların Görevleri

```text
SignBackdrop
→ Hedef işaret alanının arka planı.

SignImage
→ Kullanıcının öğrenmesi gereken gerçek işaret görseli.

SignFrame
→ Hedef işaret görselinin fantezi çerçevesi.

FeedbackFrame
→ Başarı / başarısızlık metninin görsel plakası.

FeedbackText
→ Model sonucu sonrası oyun tarafından gösterilen geri bildirim.

CameraPreviewPanel
→ Kullanıcının kamera görüntüsünün bulunduğu alan.

CameraPreview
→ Yaren'in WebCamTexture / RenderTexture / işlenmiş model görüntüsünü bağlayacağı RawImage.

PreviewBorder
→ Kamera ekranının dekoratif çerçevesi.

PreviewLabel
→ "SENİN HAREKETİN" başlığı.
```

---

## 3. Sahnelere Göre Kamera ve UI Alanları

| Sahne | Bileşen | GameObject | Konum | Boyut | Açıklama |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **LetterLearning** | Kamera Paneli | `CameraPreviewPanel` | `(200, -10)` | `640 x 460` | Kullanıcının kendisini gördüğü alan |
| **LetterLearning** | Kamera Görüntüsü | `CameraPreview` | `(0, 0)` | `560 x 400` | `RawImage`, kamera dokusu buraya bağlanır |
| **LetterLearning** | Hedef Çerçevesi | `SignFrame` | `(-380, 130)` | `260 x 260` | Referans işaret kartı |
| **LetterLearning** | Hedef Görsel | `SignImage` | `(-380, 130)` | `185 x 185` | `preserveAspect = true` |
| **LetterLearning** | Feedback Çerçevesi | `FeedbackFrame` | `(-380, -35)` | `320 x 135` | Sonuç plakası |
| **LetterLearning** | Feedback Yazısı | `FeedbackText` | `(-380, -35)` | `260 x 80` | Başarı / hata metni |
| **LetterLearning** | Dene Butonu | `PracticeButton` | `(-380, -145)` | `220 x 58` | Recognition başlatır |
| **WordLearning** | Tüm ana alanlar | LetterLearning ile aynı | Aynı | Aynı | Kelime öğrenme düzeni |
| **NameWorkshop** | Kamera Paneli | `CameraPreviewPanel` | `(200, -10)` | `640 x 460` | Kullanıcının kamera alanı |
| **NameWorkshop** | Kamera Görüntüsü | `CameraPreview` | `(0, 0)` | `560 x 400` | Ezber turunda da açık kalır |
| **NameWorkshop** | Hedef Çerçevesi | `SignFrame` | `(-380, 130)` | `260 x 260` | Ezber turunda hedef görsel gizlenir |
| **NameWorkshop** | Feedback Çerçevesi | `FeedbackFrame` | `(-380, -35)` | `320 x 135` | Model sonucu alanı |
| **NameWorkshop** | Feedback Yazısı | `FeedbackText` | `(-380, -35)` | `260 x 80` | Harf değerlendirmesi |
| **NameWorkshop** | İsim Girişi | `NameInputField` | `(-380, 50)` | `330 x 74` | Kullanıcı adını yazar |
| **NameWorkshop** | Başla Butonu | `StartButton` | `(-380, -40)` | `220 x 58` | Atölyeyi başlatır |

---

## 4. Kamerayı UI'a Bağlama

`CameraPreview` GameObject'i üzerinde bir:

```text
UnityEngine.UI.RawImage
```

bileşeni bulunmaktadır.

Yaren'in kamera görüntüsünü bağlayacağı ana alan burasıdır.

### 4.1. Basit WebCamTexture Örneği

Aşağıdaki örnek yalnızca kamera görüntüsünü UI'da göstermek için temel bir şablondur:

```csharp
using UnityEngine;
using UnityEngine.UI;

public class CameraFeedController : MonoBehaviour
{
    [Header("UI Referansı")]
    public RawImage cameraPreviewRawImage;

    private WebCamTexture webCamTexture;

    void Start()
    {
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogWarning("Kamera bulunamadı!");
            return;
        }

        string deviceName =
            WebCamTexture.devices[0].name;

        webCamTexture =
            new WebCamTexture(
                deviceName,
                640,
                480,
                30
            );

        if (cameraPreviewRawImage != null)
        {
            cameraPreviewRawImage.texture =
                webCamTexture;
        }

        webCamTexture.Play();
    }

    void OnDestroy()
    {
        if (
            webCamTexture != null &&
            webCamTexture.isPlaying
        )
        {
            webCamTexture.Stop();
        }
    }
}
```

Bu örnek production kamera sistemi değildir.

Gerçek sistem:

```text
Camera
↓
Frame Capture
↓
MediaPipe / Hand Tracking
↓
Preprocessing
↓
Model
```

akışıyla çalışabilir.

---

## 5. Landmark / İşlenmiş Kamera Görüntüsü

Eğer MediaPipe veya başka bir el takip sistemi kamera görüntüsü üzerine landmark çiziyorsa, işlenmiş görüntü:

- `Texture2D`
- `RenderTexture`

veya Unity'nin desteklediği başka bir texture türü olarak `CameraPreview` alanına bağlanabilir.

Örnek:

```csharp
cameraPreviewRawImage.texture =
    processedFrameTexture;
```

Bu durumda:

```text
CameraPreview
```

sadece görsel önizleme alanıdır.

Model inference sonucu ise doğrudan UI'dan değil:

```text
RecognitionServiceBase
```

üzerinden oyuna gönderilmelidir.

---

# 6. Recognition Service Mimarisi

WeaveSign'da model sistemi oyun mantığından ayrılmıştır.

Ana mimari:

```text
BattleManager
LetterLearningController
WordLearningController
NameWorkshopController
            │
            ▼
    RecognitionServiceBase
            │
      ┌─────┴─────┐
      ▼           ▼
MockRecognition   RealRecognition
Service           Service
                      │
                      ▼
                    Camera
                      │
                      ▼
             MediaPipe / Landmarks
                      │
                      ▼
                Preprocessing
                      │
                      ▼
                Sentis / Model
                      │
                      ▼
          predictedLabel + confidence
```

Controller'lar gerçek modelin nasıl çalıştığını bilmez.

Controller'ların bildiği tek şey:

```text
RecognitionRequest gönder
↓
RecognitionResult al
```

mantığıdır.

---

## 6.1. Ortak Recognition Dosyaları

Projede ortak recognition API şu dosyalardan oluşur:

```text
Assets/Scripts/Recognition/RecognitionRequest.cs
Assets/Scripts/Recognition/RecognitionResult.cs
Assets/Scripts/Recognition/RecognitionServiceBase.cs
Assets/Scripts/Recognition/MockRecognitionService.cs
```

Yaren bu ortak kontratı bozacak şekilde controller kodlarını değiştirmemelidir.

---

## 6.2. RecognitionServiceBase

Gerçek model servisi:

```csharp
RecognitionServiceBase
```

sınıfından türemelidir.

Temel metod:

```csharp
public abstract IEnumerator Recognize(
    RecognitionRequest request,
    System.Action<RecognitionResult> onResult
);
```

Örnek gerçek servis iskeleti:

```csharp
using System.Collections;
using UnityEngine;

public class YarenRecognitionService :
    RecognitionServiceBase
{
    public override IEnumerator Recognize(
        RecognitionRequest request,
        System.Action<RecognitionResult> onResult
    )
    {
        // 1. Kamera / landmark verisini al.
        // 2. Preprocessing yap.
        // 3. request.modelType değerine göre
        //    uygun modeli çalıştır.
        // 4. Model prediction sonucunu al.

        yield return null;

        string predictedLabel = "A";
        float confidence = 0.94f;

        RecognitionResult result =
            new RecognitionResult(
                predictedLabel,
                confidence
            );

        onResult?.Invoke(result);
    }
}
```

Yukarıdaki `"A"` ve `0.94f` yalnızca örnektir.

Gerçek serviste bu değerler model inference sonucundan gelmelidir.

---

## 6.3. RecognitionResult — KRİTİK

Projede mevcut `RecognitionResult` yapısı:

```csharp
public struct RecognitionResult
{
    public string predictedLabel;
    public float confidence;

    public RecognitionResult(
        string predictedLabel,
        float confidence
    )
    {
        this.predictedLabel = predictedLabel;
        this.confidence = confidence;
    }
}
```

Yani `RecognitionResult` yalnızca:

```text
predictedLabel
confidence
```

taşır.

Doğru kullanım:

```csharp
RecognitionResult result =
    new RecognitionResult(
        predictedLabel: "A",
        confidence: 0.94f
    );

onResult?.Invoke(result);
```

### RecognitionResult İçinde Olmayan Alanlar

Aşağıdaki alanlar mevcut değildir:

```text
isSuccessful
isCorrect
damage
heal
score
```

Örneğin şu kullanım YANLIŞTIR:

```csharp
var result = new RecognitionResult(
    predictedLabel: "A",
    confidence: 0.94f,
    isSuccessful: true
);
```

Çünkü:

```text
isSuccessful
```

isimli bir constructor parametresi yoktur.

Model başarı / başarısızlık kararı vermez.

Model yalnızca:

```text
predictedLabel
confidence
```

üretir.

Başarı kararını ilgili oyun controller'ı verir.

---

## 6.4. RecognitionRequest

Controller model servisine bir:

```text
RecognitionRequest
```

gönderir.

Request içinde:

```text
modelType
expectedLabel
```

bilgileri bulunur.

Örnek:

```csharp
RecognitionRequest request =
    new RecognitionRequest(
        RecognitionModelType.Letter,
        "A"
    );
```

`modelType`, hangi modelin kullanılacağını belirtir.

Mevcut model türleri:

```text
Letter
Word
```

---

## 6.5. expectedLabel Hakkında Çok Önemli Kural

`expectedLabel` oyunun o anda beklediği etikettir.

Örneğin:

```text
Hedef Harf = A
expectedLabel = A
```

Fakat gerçek model inference sırasında bu bilgi tahmini yönlendirmek için kullanılmamalıdır.

YANLIŞ:

```text
expectedLabel = A
↓
Model A döndürsün
```

DOĞRU:

```text
Kamera görüntüsü
↓
Landmark / hareket verisi
↓
Model inference
↓
predictedLabel = modelin gerçek tahmini
confidence = modelin gerçek güven skoru
↓
Controller:
predictedLabel == expectedLabel ?
```

`expectedLabel`:

- mock testlerinde kullanılabilir,
- oyun tarafında karşılaştırma için kullanılabilir,

ama gerçek modelin sonucunu "doğru çıkarmak" için kullanılmamalıdır.

---

## 6.6. Letter ve Word Model Seçimi

`RecognitionRequest.modelType` kullanılarak hangi modelin çalışacağı belirlenebilir.

Örnek:

```csharp
if (
    request.modelType ==
    RecognitionModelType.Letter
)
{
    // Harf modeli
}
else if (
    request.modelType ==
    RecognitionModelType.Word
)
{
    // Kelime / işaret modeli
}
```

Bu sayede tek bir recognition service:

```text
Harf Öğren
Kelime Öğren
İsmini Yaz
Düello
```

tarafından kullanılabilir.

---

# 7. Model Label Kuralları

Yaren'in döndürdüğü:

```text
predictedLabel
```

değeri projedeki model label isimleriyle **birebir aynı** olmalıdır.

Büyük / küçük harf ve karakter farkları önemlidir.

---

## 7.1. Harf Modeli

Türk alfabesi:

```text
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
```

Toplam:

```text
29 harf
```

Harf modelindeki gerçek label isimleri dataset/model export sonucuyla birebir doğrulanmalıdır.

---

## 7.2. Kelime Modeli

Mevcut 20 kelime model label'ı:

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

Bu isimlerin spelling ve case değerleri değiştirilmemelidir.

Örneğin:

```text
UI Display Name:
Teşekkürler

Model Label:
Tesekkurler
```

şeklinde olabilir.

Model:

```text
Teşekkürler
```

değil,

```text
Tesekkurler
```

döndürmelidir.

---

# 8. Yaren'in Çalışacağı Klasörler

Yaren'in ana çalışma alanları:

```text
Assets/Scripts/Recognition/Camera/
Assets/Scripts/Recognition/HandTracking/
Assets/Scripts/Recognition/Model/

Assets/ML/Models/
Assets/ML/Config/
Assets/ML/TestData/
```

Gerçek recognition service tercihen:

```text
Assets/Scripts/Recognition/
```

altında tutulabilir.

Örneğin:

```text
Assets/Scripts/Recognition/YarenRecognitionService.cs
```

---

# 9. Yaren'in Dokunmaması Gereken Yerler

Model entegrasyonu için aşağıdaki oyun mantığı dosyalarının değiştirilmesi gerekmemelidir:

```text
Assets/Scripts/Battle/BattleManager.cs

Assets/Scripts/Battle/Cards/
Assets/Scripts/Learning/LetterLearning/
Assets/Scripts/Learning/WordLearning/
Assets/Scripts/NameWorkshop/

Assets/Scripts/UI/
```

Ayrıca model entegrasyonu sırasında:

- damage hesabı değiştirilmemeli,
- heal hesabı değiştirilmemeli,
- oyuncu HP'si model tarafından değiştirilmemeli,
- kart seçme mantığı değiştirilmemeli,
- öğrenme akışları değiştirilmemeli,
- frontend görsel düzeni değiştirilmemeli.

---

# 10. Model Servisinin Tek Görevi

Gerçek recognition servisinin görevi:

```text
Kamera girdisini al
↓
El / hareket verisini çıkar
↓
Preprocessing yap
↓
Doğru modeli çalıştır
↓
predictedLabel üret
↓
confidence üret
↓
RecognitionResult döndür
```

Özet çıktı:

```csharp
RecognitionResult result =
    new RecognitionResult(
        predictedLabel,
        confidence
    );

onResult?.Invoke(result);
```

---

# 11. Sahnelere Gerçek Servisi Bağlama

Gerçek model hazır olduğunda:

```text
MockRecognitionService
```

yerine:

```text
YarenRecognitionService
```

veya Yaren'in oluşturduğu gerçek servis kullanılmalıdır.

İlgili sahneler:

```text
LetterLearning
WordLearning
NameWorkshop
Battle
```

Genel işlem:

1. Sahnedeki `RecognitionService` GameObject'ini bul.
2. Gerçek recognition service componentini ekle.
3. Gerekli camera / model referanslarını Inspector'dan bağla.
4. Controller'daki:

```csharp
public RecognitionServiceBase recognitionService;
```

alanına gerçek servisi bağla.
5. Mock servis kaldırılacaksa yalnızca gerçek sistem doğrulandıktan sonra kaldır.

---

# 12. Harf Öğren Davranışı

`LetterLearning` sahnesinde:

```text
Hedef harf
↓
SignImage
↓
Kullanıcı hareketi
↓
CameraPreview
↓
DENE
↓
RecognitionRequest
↓
Letter Model
↓
RecognitionResult
↓
LetterLearningController
↓
BAŞARILI / TEKRAR DENE
```

Model doğrudan `FeedbackText` değiştirmemelidir.

Feedback'i oyun controller'ı yönetir.

---

# 13. Kelime Öğren Davranışı

`WordLearning` sahnesinde:

```text
Hedef kelime / işaret
↓
SignImage
↓
Kullanıcı hareketi
↓
CameraPreview
↓
DENE
↓
RecognitionRequest
↓
Word Model
↓
RecognitionResult
↓
WordLearningController
↓
BAŞARILI / TEKRAR DENE
```

Model yalnızca prediction üretir.

---

# 14. İsmini Yaz Atölyesi Davranışı

İsmini Yaz sisteminin doğru akışı:

```text
Kullanıcı ismini klavyeyle yazar
↓
BAŞLA
↓
ÖĞRENME TURU
↓
Harf sırayla gösterilir
↓
Kullanıcı hareketi yapar
↓
Letter Model kontrol eder
↓
İsim tamamlanır
↓
EZBER TURU
↓
Hedef işaret görseli kapatılır
↓
Kullanıcı görsele bakmadan tekrar yapar
↓
TAMAMLANDI
```

Örnek:

```text
İsim:
AYŞE

Öğrenme turu:
A
Y
Ş
E

Ezber turu:
A
Y
Ş
E
```

Ezber turunda:

```text
SignImage
```

gizlenir.

Fakat:

```text
CameraPreview
```

açık kalabilir.

Model her adımda o an beklenen harfin tahminini üretir.

---

# 15. Düello Davranışı

Battle sahnesinde:

```text
CameraPreviewPanel
```

bulunmaz.

Kamera / recognition sistemi arka planda çalışır.

Akış:

```text
Oyuncu kart seçer
↓
Hazırlık geri sayımı
↓
Oyuncu işareti yapar
↓
Word Model inference
↓
RecognitionResult
↓
BattleManager sonucu değerlendirir
↓
Attack / Heal / Failure
```

Model:

```text
damage
heal
HP
critical
```

hesaplamaz.

Bunların tamamı `BattleManager` sorumluluğundadır.

---

# 16. Confidence Formatı

`RecognitionResult.confidence`:

```text
0.0 - 1.0
```

aralığında olmalıdır.

Örnek:

```text
0.42
0.70
0.94
0.99
```

Controller tarafında gerektiğinde:

```csharp
Mathf.RoundToInt(
    result.confidence * 100
);
```

ile yüzdeye çevrilir.

Dolayısıyla model:

```text
94
```

değil,

```text
0.94f
```

göndermelidir.

---

# 17. Error / Sonuç Alınamaması

Recognition servisinin callback'i mümkün olan her normal inference çağrısında sonuç üretmelidir.

Model sonucu oluştuğunda:

```csharp
onResult?.Invoke(
    new RecognitionResult(
        predictedLabel,
        confidence
    )
);
```

çağrılmalıdır.

Kamera bulunamaması, model yüklenememesi veya inference hatası gibi durumlar:

```text
Debug.LogError
Debug.LogWarning
```

ile loglanabilir.

Ancak bu hata yönetimi oyun controller'larının temel akışını gereksiz yere değiştirmemelidir.

---

# 18. Camera Preview ve Model Aynı Şey Değildir

Önemli ayrım:

```text
CameraPreview
```

UI içindeki görsel alandır.

```text
RecognitionService
```

ise inference sistemidir.

İkisi birbirinden bağımsız tutulmalıdır.

Örneğin kamera önizlemesi kapalı olsa bile teknik olarak model inference çalışabilir.

Aynı şekilde kamera görüntüsü UI'da gösterilirken model henüz çalışmıyor olabilir.

---

# 19. Entegrasyon Sonrası Testler

Gerçek model bağlandıktan sonra sırayla şu testler yapılmalıdır.

### Harf Öğren

```text
A hedefi
→ doğru A hareketi
→ A prediction
→ yeterli confidence
→ BAŞARILI
```

Yanlış:

```text
A hedefi
→ B hareketi
→ B prediction
→ TEKRAR DENE
```

---

### Kelime Öğren

```text
Merhaba hedefi
→ Merhaba hareketi
→ predictedLabel = Merhaba
→ BAŞARILI
```

Yanlış işaret:

```text
Merhaba hedefi
→ Dur hareketi
→ predictedLabel = Dur
→ TEKRAR DENE
```

---

### İsmini Yaz

Örnek:

```text
AYŞE
```

sırasıyla:

```text
A
Y
Ş
E
```

kontrol edilmeli.

Öğrenme turu bittikten sonra ezber turuna geçilmeli.

---

### Battle

Kontrol edilmesi gerekenler:

```text
Attack kartı
→ doğru hareket
→ rakip HP düşer

Heal kartı
→ doğru hareket
→ aktif oyuncu HP artar

Yanlış hareket
→ aktif oyuncu ceza alır

HP = 0
→ ResultPanel açılır
```

Model yalnızca tahmin üretir.

Battle sonucu oyun kodu hesaplar.

---

# 20. Yaren İçin Final Checklist

## Kamera

- [ ] Kamera cihazı bulunuyor.
- [ ] Kamera düzgün açılıyor.
- [ ] Kamera sahne değişimlerinde gereksiz şekilde çoğalmıyor.
- [ ] Kamera kapatılması gerektiğinde düzgün durduruluyor.
- [ ] `CameraPreview` üzerine görüntü doğru bağlanıyor.

## Hand Tracking

- [ ] Eller doğru algılanıyor.
- [ ] Tek / çift el senaryoları destekleniyor.
- [ ] Landmark koordinatları modele uygun formata çevriliyor.

## Model

- [ ] Letter modeli yükleniyor.
- [ ] Word modeli yükleniyor.
- [ ] `request.modelType` doğru modeli seçiyor.
- [ ] Model label eşleşmeleri birebir doğru.
- [ ] Confidence `0-1` aralığında.
- [ ] `expectedLabel` prediction sonucunu yönlendirmiyor.

## Recognition API

- [ ] Servis `RecognitionServiceBase`'den türemiş.
- [ ] `Recognize(...)` override edilmiş.
- [ ] Sonuç `RecognitionResult` olarak dönüyor.
- [ ] `RecognitionResult` yalnızca `predictedLabel` ve `confidence` kullanıyor.
- [ ] Callback `onResult?.Invoke(result)` ile çağrılıyor.

## Harf Öğren

- [ ] Letter model kullanılıyor.
- [ ] Doğru harf doğru sonuç üretiyor.
- [ ] Yanlış harf başarısız sonuç üretiyor.

## Kelime Öğren

- [ ] Word model kullanılıyor.
- [ ] 20 model label birebir doğru.

## İsmini Yaz

- [ ] Letter model kullanılıyor.
- [ ] İsim harfleri sırayla kontrol ediliyor.
- [ ] Öğrenme turu çalışıyor.
- [ ] Ezber turu çalışıyor.
- [ ] Ezber turunda hedef işaret gizleniyor.

## Battle

- [ ] Word model kullanılıyor.
- [ ] Kamera preview Battle sahnesine eklenmiyor.
- [ ] Model yalnızca prediction üretiyor.
- [ ] Damage / heal hesabına model kodu karışmıyor.
- [ ] Game Over akışı bozulmuyor.

---

# 21. Yaren İçin Kısa Özet

Yaren'in yapacağı iş:

```text
Camera
↓
MediaPipe / Hand Tracking
↓
Preprocessing
↓
Letter veya Word Model
↓
predictedLabel
confidence
↓
RecognitionResult
```

Yaren'in yapmayacağı işler:

```text
Damage hesaplama
Heal hesaplama
HP değiştirme
Kart sistemi değiştirme
Learning akışı değiştirme
NameWorkshop akışı değiştirme
UI feedback yönetme
Frontend değiştirme
```

Gerçek model entegrasyonunun oyuna verdiği final çıktı yalnızca:

```csharp
new RecognitionResult(
    predictedLabel,
    confidence
);
```

olmalıdır.

Bu kontrat korunduğu sürece model sistemi, oyun mantığından ve frontend'den bağımsız şekilde geliştirilebilir.
