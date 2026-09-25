using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameWorkshopController : MonoBehaviour
{
    [Header("Başlangıç Ekranı")]
    public TMP_InputField nameInputField;
    public GameObject startButton;

    [Header("Çalışma Ekranı")]
    public TMP_Text targetLetterText;
    public Image signImage;
    public TMP_Text progressText;
    public TMP_Text feedbackText;
    public GameObject practiceButton;
    public GameObject resetButton;

    [Header("Recognition")]
    public RecognitionServiceBase recognitionService;

    [Range(0, 100)]
    public int minimumSuccessScore = 70;

    private LearningItemData[] letterItems;

    private List<LearningItemData> nameLetters =
        new List<LearningItemData>();

    private int currentIndex = 0;

    // false = görselli öğrenme turu
    // true = görselsiz ezber turu
    private bool memoryRound = false;

    private bool recognitionInProgress = false;

    private string enteredName = "";

    void Start()
    {
        LoadLetterData();
        ShowInputScreen();
    }

    void LoadLetterData()
    {
        letterItems =
            Resources.LoadAll<LearningItemData>(
                "Learning/Letters"
            )
            .OrderBy(item => item.name)
            .ToArray();

        Debug.Log(
            "İsmini Yaz için yüklenen harf sayısı: " +
            letterItems.Length
        );
    }

    public void StartWorkshop()
    {
        if (recognitionInProgress)
            return;

        if (
            letterItems == null ||
            letterItems.Length == 0
        )
        {
            feedbackText.gameObject.SetActive(true);

            feedbackText.text =
                "HARF VERİLERİ BULUNAMADI";

            PlayMistakeSound();

            return;
        }

        string rawName =
            nameInputField.text.Trim();

        if (string.IsNullOrEmpty(rawName))
        {
            feedbackText.gameObject.SetActive(true);

            feedbackText.text =
                "ÖNCE İSMİNİ YAZ";

            PlayMistakeSound();

            return;
        }

        CultureInfo turkishCulture =
            new CultureInfo("tr-TR");

        enteredName =
            rawName.ToUpper(turkishCulture);

        nameLetters.Clear();

        for (
            int i = 0;
            i < enteredName.Length;
            i++
        )
        {
            char character =
                enteredName[i];

            // Boşlukları otomatik geç.
            if (char.IsWhiteSpace(character))
                continue;

            string letter =
                character.ToString();

            LearningItemData item =
                letterItems.FirstOrDefault(
                    data =>
                        data.displayName == letter
                );

            if (item == null)
            {
                feedbackText.gameObject.SetActive(true);

                feedbackText.text =
                    "DESTEKLENMEYEN KARAKTER: " +
                    letter;

                PlayMistakeSound();

                return;
            }

            nameLetters.Add(item);
        }

        if (nameLetters.Count == 0)
        {
            feedbackText.gameObject.SetActive(true);

            feedbackText.text =
                "GEÇERLİ BİR İSİM YAZ";

            PlayMistakeSound();

            return;
        }

        currentIndex = 0;
        memoryRound = false;

        ShowPracticeScreen();
        UpdateCurrentLetter();
    }

    void ShowInputScreen()
    {
        nameInputField.gameObject.SetActive(true);
        startButton.SetActive(true);

        targetLetterText.gameObject.SetActive(false);
        signImage.gameObject.SetActive(false);
        progressText.gameObject.SetActive(false);
        practiceButton.SetActive(false);
        resetButton.SetActive(false);

        feedbackText.gameObject.SetActive(true);
        feedbackText.text = "";
    }

    void ShowPracticeScreen()
    {
        nameInputField.gameObject.SetActive(false);
        startButton.SetActive(false);

        targetLetterText.gameObject.SetActive(true);
        signImage.gameObject.SetActive(true);
        progressText.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        practiceButton.SetActive(true);
        resetButton.SetActive(true);
    }

    void UpdateCurrentLetter()
    {
        if (currentIndex >= nameLetters.Count)
        {
            FinishCurrentRound();
            return;
        }

        LearningItemData currentItem =
            nameLetters[currentIndex];

        targetLetterText.text =
            currentItem.displayName;

        if (memoryRound)
        {
            progressText.text =
                "EZBER TURU   " +
                (currentIndex + 1) +
                " / " +
                nameLetters.Count;

            // Ezber turunda el işareti gösterilmez.
            signImage.gameObject.SetActive(false);
        }
        else
        {
            progressText.text =
                "ÖĞRENME TURU   " +
                (currentIndex + 1) +
                " / " +
                nameLetters.Count;

            signImage.gameObject.SetActive(true);

            if (currentItem.signImage != null)
            {
                signImage.enabled = true;

                signImage.sprite =
                    currentItem.signImage;

                signImage.preserveAspect = true;
            }
            else
            {
                signImage.enabled = false;
            }
        }
    }

    void FinishCurrentRound()
    {
        // İlk tur tamamlandı.
        if (!memoryRound)
        {
            memoryRound = true;
            currentIndex = 0;

            feedbackText.text =
                "ÖĞRENME TURU TAMAMLANDI!\n" +
                "ŞİMDİ GÖRSELE BAKMADAN TEKRAR YAP.";

            UpdateCurrentLetter();

            return;
        }

        // Ezber turu da tamamlandı.
        CompleteWorkshop();
    }

    void CompleteWorkshop()
    {
        // Final zafer sesi
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVictory();
        }

        targetLetterText.gameObject.SetActive(true);

        targetLetterText.text =
            "TEBRİKLER!";

        signImage.gameObject.SetActive(false);

        progressText.text =
            "TAMAMLANDI";

        practiceButton.SetActive(false);
        resetButton.SetActive(true);

        feedbackText.text =
            enteredName +
            "\nİSMİNİ İŞARET DİLİYLE TAMAMLADIN!";
    }

    public void PracticeCurrentLetter()
    {
        if (recognitionInProgress)
            return;

        if (currentIndex >= nameLetters.Count)
            return;

        StartCoroutine(
            PracticeRoutine()
        );
    }

    IEnumerator PracticeRoutine()
    {
        if (recognitionService == null)
        {
            Debug.LogError(
                "Recognition Service bağlı değil!"
            );

            feedbackText.text =
                "RECOGNITION SERVICE BAĞLI DEĞİL";

            yield break;
        }

        recognitionInProgress = true;

        LearningItemData currentItem =
            nameLetters[currentIndex];

        feedbackText.text =
            "DEĞERLENDİRİLİYOR...";

        RecognitionRequest request =
            new RecognitionRequest(
                RecognitionModelType.Letter,
                currentItem.modelLabel
            );

        RecognitionResult result = default;
        bool resultReceived = false;

        yield return StartCoroutine(
            recognitionService.Recognize(
                request,
                recognitionResult =>
                {
                    result = recognitionResult;
                    resultReceived = true;
                }
            )
        );

        if (!resultReceived)
        {
            feedbackText.text =
                "SONUÇ ALINAMADI";

            PlayMistakeSound();

            recognitionInProgress = false;

            yield break;
        }

        int score =
            Mathf.RoundToInt(
                result.confidence * 100
            );

        bool correctLabel =
            result.predictedLabel ==
            currentItem.modelLabel;

        bool enoughConfidence =
            score >= minimumSuccessScore;

        if (
            correctLabel &&
            enoughConfidence
        )
        {
            currentIndex++;

            bool completingMemoryRound =
                memoryRound &&
                currentIndex >= nameLetters.Count;

            // Final harfte Success ve Victory
            // aynı anda çalmasın.
            if (!completingMemoryRound)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySuccess();
                }
            }

            feedbackText.text =
                "DOĞRU! ✓\nSKOR: %" +
                score;

            UpdateCurrentLetter();
        }
        else
        {
            PlayMistakeSound();

            feedbackText.text =
                "TEKRAR DENE\n" +
                "TAHMİN: " +
                result.predictedLabel +
                " | SKOR: %" +
                score;
        }

        recognitionInProgress = false;
    }

    void PlayMistakeSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMistake();
        }
    }

    public void ResetWorkshop()
    {
        recognitionInProgress = false;
        currentIndex = 0;
        memoryRound = false;

        nameLetters.Clear();

        ShowInputScreen();
    }
}