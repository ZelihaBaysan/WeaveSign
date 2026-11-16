using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WordLearningController : MonoBehaviour
{
    [Header("Ana Ekran")]
    public TMP_Text wordText;
    public TMP_Text progressText;
    public TMP_Text feedbackText;
    public TMP_Text signDescriptionText;
    public Image signImage;

    [Header("Kelime Seçim Paneli")]
    public GameObject wordSelectionPanel;
    public Transform wordGrid;
    public GameObject wordButtonPrefab;

    [Header("Recognition")]
    public RecognitionServiceBase recognitionService;

    [Range(0, 100)]
    public int minimumSuccessScore = 70;

    private LearningItemData[] wordItems;
    private int currentIndex = 0;
    private bool recognitionInProgress = false;


    void Start()
    {
        LoadWordData();

        if (wordItems == null || wordItems.Length == 0)
        {
            Debug.LogError(
                "Kelime LearningItemData bulunamadı!"
            );

            return;
        }

        GenerateWordButtons();
        UpdateWordDisplay();

        Image panelImage =
            wordSelectionPanel.GetComponent<Image>();

        if (panelImage != null)
        {
            Color color = panelImage.color;
            color.a = 1f;
            panelImage.color = color;
        }

        wordSelectionPanel.SetActive(false);

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }


    void LoadWordData()
    {
        wordItems =
            Resources.LoadAll<LearningItemData>(
                "Learning/Words"
            )
            .OrderBy(item => item.name)
            .ToArray();

        Debug.Log(
            "Yüklenen kelime sayısı: " +
            wordItems.Length
        );
    }


    void GenerateWordButtons()
    {
        for (int i = 0; i < wordItems.Length; i++)
        {
            int index = i;

            GameObject newButton =
                Instantiate(
                    wordButtonPrefab,
                    wordGrid
                );

            newButton.name =
                "WordButton_" +
                wordItems[i].displayName;

            TMP_Text buttonText =
                newButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text =
                    wordItems[i].displayName;
            }

            Button button =
                newButton.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(
                    () => SelectWord(index)
                );
            }
        }
    }


    void UpdateWordDisplay()
    {
        LearningItemData currentItem =
            wordItems[currentIndex];

        wordText.text =
            currentItem.displayName.ToUpper();

        progressText.text =
            (currentIndex + 1) +
            " / " +
            wordItems.Length;


        // ==============================
        // AÇIKLAMA
        // ==============================

        if (signDescriptionText != null)
        {
            signDescriptionText.text =
                GetWordDescription(
                    currentItem.displayName
                );
        }


        // ==============================
        // GÖRSEL
        // ==============================

        Sprite loadedSprite =
            GetWordSignSprite(currentItem);

        if (loadedSprite != null)
        {
            signImage.enabled = true;
            signImage.sprite = loadedSprite;
            signImage.preserveAspect = true;
        }
        else
        {
            signImage.enabled = false;

            Debug.LogWarning(
                "Kelime için görsel bulunamadı: " +
                currentItem.displayName
            );
        }


        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }


    // =====================================================
    // KELİME AÇIKLAMALARI
    // =====================================================

    string GetWordDescription(
        string word
    )
    {
        string key =
            NormalizeFileName(word);

        switch (key)
        {
            case "anne":
                return
                    "Parmak uçları önce bir göğüs hizasına, ardından diğer göğüs hizasına getirilir.";

            case "arkadas":
                return
                    "İki el karın hizasında tutulur ve eller iki kez birbirine vurulur.";

            case "baba":
                return
                    "İki el göğüs hizasında tutulur; başparmak ve işaret parmakları birleşik şekilde yan yana getirilir.";

            case "dur":
                return
                    "Avuç içi karşıya bakacak şekilde el önde ve dik tutulur.";

            case "ev":
                return
                    "İki elin parmak uçları birleştirilerek çatı şekli oluşturulur.";

            case "evet":
                return
                    "İki el göğüs hizasında, avuç içleri yukarı bakacak şekilde tutulur ve hafifçe aşağı indirilir.";

            case "hayir":
                return
                    "İşaret parmağı açık tutulur ve el yatay yönde bir taraftan diğer tarafa hareket ettirilir.";

            case "kardes":
                return
                    "İki el yumruk yapılır, serçe parmaklar açık tutulur ve aşağı-yukarı birbirine sürtülür.";

            case "merhaba":
                return
                    "El açık ve düz şekilde baş hizasına getirilerek işaret yapılır.";

            case "nasil":
                return
                    "İki el göğüs hizasında tutulur ve avuç içleri aşağıdan yukarı doğru çevrilir.";

            case "nerede":
                return
                    "İki elin işaret parmakları aşağı bakacak şekilde tutulur ve eller aşağı yönde hareket ettirilir.";

            case "ozur_dilemek":
            case "ozur-dilemek":
                return
                    "İki el göğüs hizasında birleştirilerek görseldeki pozisyon uygulanır.";

            case "tamam":
                return
                    "İki el göğüs hizasında açık tutulur ve eller iki yana doğru açılır.";

            case "telefon":
                return
                    "Başparmak ve serçe parmak açık tutulur, el telefon tutar gibi yüz hizasına getirilir.";

            case "tesekkurler":
                return
                    "Bir elin işaret parmağı dik tutulur, diğer elin işaret parmağı üzerine yatay şekilde yerleştirilir.";

            case "tuvalet":
                return
                    "Başparmak ve işaret parmağı halka oluşturacak şekilde birleştirilir, diğer parmaklar açık tutulur.";

            case "yemek":
                return
                    "Parmak uçları birleştirilir ve el ağız hizasına getirilir.";

            case "icmek":
                return
                    "Başparmak açık tutulur ve el ağız hizasına getirilerek içme hareketi yapılır.";

            case "iyi":
                return
                    "Parmak uçları birleşik ve yukarı bakacak şekilde el göğüs hizasında tutulur, bilekten aşağı-yukarı hareket ettirilir.";

            case "kotu":
                return
                    "Serçe parmak açık tutulur ve el bilekten sağa-sola hareket ettirilir.";

            default:
                return
                    "Görseldeki el şeklini ve hareket yönünü takip edin.";
        }
    }


    // =====================================================
    // KELİME GÖRSELİ
    // =====================================================

    Sprite GetWordSignSprite(
        LearningItemData item
    )
    {
        string displayNamePath =
            "WordSigns/" +
            NormalizeFileName(
                item.displayName
            );

        Sprite sprite =
            Resources.Load<Sprite>(
                displayNamePath
            );

        if (sprite != null)
        {
            Debug.Log(
                "Kelime görseli bulundu: " +
                displayNamePath
            );

            return sprite;
        }


        string labelPath =
            "WordSigns/" +
            NormalizeFileName(
                item.modelLabel
            );

        sprite =
            Resources.Load<Sprite>(
                labelPath
            );

        if (sprite != null)
        {
            return sprite;
        }


        if (item.signImage != null)
        {
            return item.signImage;
        }

        return null;
    }


    // =====================================================
    // DOSYA ADI NORMALİZASYONU
    // =====================================================

    string NormalizeFileName(
        string text
    )
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        text =
            text.ToLowerInvariant();

        text = text.Replace("ç", "c");
        text = text.Replace("ğ", "g");
        text = text.Replace("ı", "i");
        text = text.Replace("ö", "o");
        text = text.Replace("ş", "s");
        text = text.Replace("ü", "u");

        text = text.Replace(" ", "_");

        return text;
    }


    // =====================================================
    // SONRAKİ KELİME
    // =====================================================

    public void NextWord()
    {
        if (recognitionInProgress)
        {
            return;
        }

        currentIndex++;

        if (currentIndex >= wordItems.Length)
        {
            currentIndex = 0;
        }

        UpdateWordDisplay();
    }


    // =====================================================
    // ÖNCEKİ KELİME
    // =====================================================

    public void PreviousWord()
    {
        if (recognitionInProgress)
        {
            return;
        }

        currentIndex--;

        if (currentIndex < 0)
        {
            currentIndex =
                wordItems.Length - 1;
        }

        UpdateWordDisplay();
    }


    // =====================================================
    // KELİME SEÇİM PANELİ
    // =====================================================

    public void OpenWordSelection()
    {
        if (recognitionInProgress)
        {
            return;
        }

        wordSelectionPanel.SetActive(true);

        wordSelectionPanel.transform
            .SetAsLastSibling();
    }


    public void CloseWordSelection()
    {
        wordSelectionPanel.SetActive(false);
    }


    public void SelectWord(
        int index
    )
    {
        if (
            recognitionInProgress ||
            index < 0 ||
            index >= wordItems.Length
        )
        {
            return;
        }

        currentIndex = index;

        UpdateWordDisplay();

        CloseWordSelection();
    }


    // =====================================================
    // PRATİK
    // =====================================================

    public void PracticeCurrentWord()
    {
        if (recognitionInProgress)
        {
            return;
        }

        StartCoroutine(
            PracticeWordRoutine()
        );
    }


    IEnumerator PracticeWordRoutine()
    {
        if (recognitionService == null)
        {
            Debug.LogError(
                "Recognition Service bağlı değil!"
            );

            yield break;
        }


        LearningItemData currentItem =
            wordItems[currentIndex];

        recognitionInProgress = true;


        if (feedbackText != null)
        {
            feedbackText.text =
                "DEĞERLENDİRİLİYOR...";
        }


        RecognitionRequest request =
            new RecognitionRequest(
                currentItem.modelType,
                currentItem.modelLabel
            );


        RecognitionResult result =
            default;

        bool resultReceived =
            false;


        yield return StartCoroutine(
            recognitionService.Recognize(
                request,
                recognitionResult =>
                {
                    result =
                        recognitionResult;

                    resultReceived =
                        true;
                }
            )
        );


        if (!resultReceived)
        {
            if (
                AudioManager.Instance !=
                null
            )
            {
                AudioManager.Instance
                    .PlayMistake();
            }

            if (feedbackText != null)
            {
                feedbackText.text =
                    "SONUÇ ALINAMADI";
            }

            recognitionInProgress =
                false;

            yield break;
        }


        int score =
            Mathf.RoundToInt(
                result.confidence * 100
            );


        bool correctLabel =
            string.Equals(
                result.predictedLabel,
                currentItem.modelLabel,
                StringComparison
                    .OrdinalIgnoreCase
            );


        bool success =
            correctLabel &&
            score >=
            minimumSuccessScore;


        if (success)
        {
            if (
                AudioManager.Instance !=
                null
            )
            {
                AudioManager.Instance
                    .PlaySuccess();
            }

            if (feedbackText != null)
            {
                feedbackText.text =
                    "BAŞARILI!" +
                    "\nSKOR: %" +
                    score;
            }
        }
        else
        {
            if (
                AudioManager.Instance !=
                null
            )
            {
                AudioManager.Instance
                    .PlayMistake();
            }

            if (feedbackText != null)
            {
                feedbackText.text =
                    "TEKRAR DENE" +
                    "\nTAHMİN: " +
                    result.predictedLabel +
                    " | SKOR: %" +
                    score;
            }
        }


        recognitionInProgress =
            false;
    }
}