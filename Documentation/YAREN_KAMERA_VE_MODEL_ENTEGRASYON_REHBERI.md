# WeaveSign — Yaren İçin Kamera ve Yapay Zeka Model Entegrasyon Rehberi

Bu rehber; **Yaren'in** işaret dili tanıma modelini ve canlı kamera (webcam / el izleme) görüntüsünü WeaveSign sahnelerine entegre ederken kullanacağı arayüz çerçevelerini, GameObject hiyerarşisini ve kod bağlantı noktalarını adım adım açıklar.

---

## 1. Genel Mimari ve Tasarım Vizyonu

Eğitim ve atölye ekranlarında kullanıcı deneyimi **3 ana görsel unsur** üzerine kurulmuştur:

1. **Hedef İşaret Kartı (Sol Çerçeve):** Yapılması gereken Türk İşaret Dili hareketinin görselini fantezi/kart çerçevesi içinde gösterir (`SignImage` + `SignFrame`).
2. **Kullanıcının Kendi Hareketi (Sağ Çerçeve):** Kullanıcının web kamerasından canlı görüntüsünü veya modelin el eklem noktalarıyla (landmark/skeletal tracking) zenginleştirilmiş akışını fantezi çerçeve içinde gösterir (`CameraPreviewPanel` -> `CameraPreview`).
3. **Doğruluk ve Değerlendirme Metni (Alt Orta):** Modelin tahmin ettiği etiket, başarı skoru ve "BAŞARILI!" / "TEKRAR DENE" geri bildirimini sunar (`FeedbackText`).

> ⚠️ **DÜELLO (BATTLE) MODU KURALI:**
> `Battle.unity` sahnesinde kamera önizleme çerçevesi **YOKTUR**. Oyuncular düello sırasında kartlarına, can barlarına ve rakibe odaklanırlar. Model arka planda sessizce çalışarak işaretleri değerlendirir ve büyüleri tetikler.

---

## 2. Hazırlanan Çerçeveler ve Sahne Hiyerarşisi

Aşağıdaki üç sahnede kamera ve hedef kart çerçeveleri birebir simetrik ve fantezi temasına uygun olarak yerleştirilmiştir:
- `Assets/Scenes/Learning/LetterLearning.unity` (Harf Öğren)
- `Assets/Scenes/Learning/WordLearning.unity` (Kelime Öğren)
- `Assets/Scenes/NameWorkshop/NameWorkshop.unity` (İsmini Yaz Atölyesi)

### Ortak Hiyerarşi Şeması (Örnek: LetterLearning)

```text
Canvas
└── ContentLayer
    └── LearningContentPanel / WorkshopContentPanel
        ├── SignBackdrop          -> Siyah/kadife fon katmanı (Z-order: En altta)
        ├── SignImage             -> Hedef işaret görseli (UnityEngine.UI.Image)
        ├── SignFrame             -> Altın/işlemeli fantezi kart çerçevesi (FantasyDisplayFrame)
        ├── SignImageLabel        -> "HEDEF İŞARET" başlığı
        │
        ├── FeedbackFrame         -> ★ MODELİN BAŞARILI/BAŞARISIZ YAZACAĞI ÖZEL ÇERÇEVELİ PLAKA (FantasyFeedbackFrame)
        ├── FeedbackText          -> "BAŞARILI! %95" veya "TEKRAR DENE" geri bildirim metni
        │
        ├── CameraPreviewPanel    -> ★ DEV EĞİTİM AYNASI (Ekranın büyük kısmını kaplayan ana vitrin)
        │   ├── CameraBackdrop    -> Kamera açılmadan önceki koyu arka plan
        │   ├── CameraPreview     -> ★ YAREN'İN WEBCAM / MODEL DOKUSUNU KOYACAĞI YER (560x400 RawImage)
        │   ├── PreviewBorder     -> Altın/işlemeli fantezi ekran çerçevesi (640x460 FantasyDisplayFrame)
        │   └── PreviewLabel      -> "SENİN HAREKETİN" başlığı
        │
        └── Butonlar & İlerleme   -> "İŞARETİ DENE", "TÜMÜNÜ GÖR", Navigasyon butonları
```

### Sahnelere Göre Detaylı Koordinat ve Boyutlar

| Sahne | Bileşen | GameObject Adı | Konum (X, Y) | Boyut (W x H) | Açıklama |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **LetterLearning** | Dev Kamera Çerçevesi | `CameraPreviewPanel` | `(200, -10)` | `640 x 460` | Ekranın büyük kısmını kaplayan ana ayna |
| **LetterLearning** | Canlı Kamera Görüntüsü | `CameraPreview` | `(0, 0)` | `560 x 400` | **RawImage** (Kamera dokusu buraya atanır, 4:3 oranında dev alan) |
| **LetterLearning** | Hedef Kart Çerçevesi | `SignFrame` | `(-380, 130)` | `260 x 260` | Sol taraftaki referans kart vitrini |
| **LetterLearning** | Hedef Kart Görseli | `SignImage` | `(-380, 130)` | `185 x 185` | İç pencere (`preserveAspect: true`) |
| **LetterLearning** | Değerlendirme Çerçevesi | `FeedbackFrame` | `(-380, -35)` | `320 x 135` | Rünlü ve taşlı altın değerlendirme plakası |
| **LetterLearning** | Değerlendirme Metni | `FeedbackText` | `(-380, -35)` | `260 x 80` | Plakanın içinde "BAŞARILI! %95" vb. |
| **LetterLearning** | İşareti Dene Butonu | `PracticeButton` | `(-380, -145)` | `220 x 58` | Değerlendirme plakasının hemen altında |
| **WordLearning** | *LetterLearning ile aynı* | *LetterLearning ile aynı* | *Birebir aynı* | *Birebir aynı* | Kelime öğrenme için birebir aynı hero düzeni |
| **NameWorkshop** | Dev Kamera Çerçevesi | `CameraPreviewPanel` | `(200, -10)` | `640 x 460` | Ekranın büyük kısmını kaplayan ana ayna |
| **NameWorkshop** | Canlı Kamera Görüntüsü | `CameraPreview` | `(0, 0)` | `560 x 400` | **RawImage** (Ezber turunda da açık kalır) |
| **NameWorkshop** | Hedef Kart Çerçevesi | `SignFrame` | `(-380, 130)` | `260 x 260` | Sol referans kartı (Ezber turunda gizlenir) |
| **NameWorkshop** | Değerlendirme Çerçevesi | `FeedbackFrame` | `(-380, -35)` | `320 x 135` | Değerlendirme plakası |
| **NameWorkshop** | Değerlendirme Metni | `FeedbackText` | `(-380, -35)` | `260 x 80` | İsim harfi doğruluk değerlendirmesi |
| **NameWorkshop** | İsim Giriş / Başla | `NameInputField` / `StartButton` | `(-380, 50)` / `(-380, -40)` | `330x74` / `220x58` | İsim yazma aşamasında sol sütunda |

---

## 3. Kamerayı / Modeli UI'a Bağlama (Yaren İçin Adım Adım)

`CameraPreview` GameObject'i üzerinde bir **`UnityEngine.UI.RawImage`** bileşeni bulunmaktadır.

### 3.1. Basit WebCamTexture Bağlantısı

Unity'de web kamerasından görüntü alıp UI'da göstermek için aşağıdaki deseni kullanabilirsiniz:

```csharp
using UnityEngine;
using UnityEngine.UI;

public class CameraFeedController : MonoBehaviour
{
    [Header("UI Referansı")]
    public RawImage cameraPreviewRawImage; // Inspector'dan 'CameraPreview' atanır

    private WebCamTexture webCamTexture;

    void Start()
    {
        if (WebCamTexture.devices.Length > 0)
        {
            // İlk uygun kamerayı başlat
            webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name, 640, 480, 30);
            
            if (cameraPreviewRawImage != null)
            {
                cameraPreviewRawImage.texture = webCamTexture;
                cameraPreviewRawImage.material.mainTexture = webCamTexture;
            }

            webCamTexture.Play();
        }
        else
        {
            Debug.LogWarning("Kamera bulunamadı!");
        }
    }

    void OnDestroy()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }
}
```

### 3.2. Model / El Takibi (Landmark Overlay) Bağlantısı

Eğer makine öğrenmesi modeli (MediaPipe, ONNX, OpenCV vb.) kameradan gelen kare üzerine el eklem çizgilerini (landmarks) çiziyorsa:
1. İşlenmiş kareyi bir **`Texture2D`** veya **`RenderTexture`** olarak elde edin.
2. `CameraPreview` GameObject'indeki `RawImage.texture` referansına bu işlenmiş dokuyu atayın:
   ```csharp
   cameraPreviewRawImage.texture = processedFrameTexture;
   ```
3. `PreviewBorder` (`FantasyDisplayFrame.png`) yarı saydam altın yaldızlı bir çerçeve katmanıdır ve `RawImage`'ın kenarlarını pürüzsüzce maskeleyerek ekrana tam oturmasını sağlar.

---

## 4. Recognition Service Mimarisi ile Entegrasyon

Oyun mantığı kodlarında tanıma çağrıları soyut `RecognitionServiceBase` üzerinden yapılmaktadır:

```text
LetterLearningController / WordLearningController / NameWorkshopController
                            │
                            ▼
               [ RecognitionServiceBase ]
                            │
              ┌─────────────┴─────────────┐
              ▼                           ▼
   [ MockRecognitionService ]    [ YarenRealModelService ] (Yeni eklenecek)
```

### Yapılması Gerekenler:
1. `Assets/Scripts/Recognition/` altına `RecognitionServiceBase`'den türeyen model servisinizi yazın (örneğin `YarenRecognitionService.cs`).
2. Sahnelerdeki `RecognitionService` GameObject'ine bu bileşeni ekleyin.
3. Controller scriptlerindeki `public RecognitionServiceBase recognitionService;` alanına yeni servisinizi bağlayın.
4. Model sonucu geldiğinde `RecognitionResult` nesnesini döndürün:
   ```csharp
   var result = new RecognitionResult(
       predictedLabel: "A",
       confidence: 0.94f,
       isSuccessful: true
   );
   callback(result);
   ```

---

## 5. Sahne Bazlı Davranış Notları

### 5.1. Harf Öğren & Kelime Öğren (`LetterLearning` / `WordLearning`)
- Sol tarafta öğrenilmekte olan harf veya kelimenin el işareti kartı (`SignImage`) görüntülenir.
- Sağ tarafta oyuncunun canlı kamerası (`CameraPreview`) sürekli açık kalır, böylece oyuncu işaretini doğru yapıp yapmadığını kendi gözüyle kıyaslayabilir.
- "İŞARETİ DENE" butonuna tıklandığında `FeedbackText` üzerinde anlık değerlendirme yapılır.

### 5.2. İsmini Yaz Atölyesi (`NameWorkshop`)
- **İsim Yazma Ekranı (`ShowInputScreen`):** Oyuncu adını yazarken `NameInputField` ve `StartButton` merkezdedir.
- **Çalışma Ekranı (`ShowPracticeScreen`):**
  - Sol tarafta o anki harfin hedef işareti (`SignImage` ve `SignFrame`),
  - Sağ tarafta canlı kamera görüntüsü (`CameraPreviewPanel`),
  - Altta `FeedbackText`, `ProgressText` ve `PracticeButton` yer alır.
- **Ezber Turu (`memoryRound`):**
  - İsim bir kez öğrenildikten sonra ezber turuna geçilir.
  - Ezber turunda `NameWorkshopController`, kopya verilmemesi için `signImage`'ı kapatır.
  - Sağdaki canlı kamera (`CameraPreviewPanel`) ise açık kalır, böylece oyuncu ezberden yaparken kendisini görmeye devam eder.

### 5.3. Düello (`Battle`)
- Düelloda oyuncuların kendilerini görmesine gerek yoktur (`CameraPreviewPanel` sahnede bulunmaz).
- Model arka planda sırası gelen oyuncunun hareketini tanır ve düello büyüsünü icra eder.

---

## 6. Hızlı Kontrol Listesi (Yaren İçin Checklist)

- [ ] Sahnedeki `CameraPreview` GameObject'i bulundu mu? (`Canvas/ContentLayer/.../CameraPreviewPanel/CameraPreview`)
- [ ] `CameraPreview` bileşeninin `RawImage` olduğu teyit edildi mi?
- [ ] Kamera dokusu (`WebCamTexture` veya modelin işlenmiş dokusu) `RawImage.texture`'a atandı mı?
- [ ] Model servisi `RecognitionServiceBase` üzerinden türetilip sahneye bağlandı mı?
- [ ] Battle sahnesinde kamera görseli aranmadığı (arka planda çalıştığı) doğrulandı mı?
