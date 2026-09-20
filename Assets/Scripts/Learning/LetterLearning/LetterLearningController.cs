using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LetterLearningController : MonoBehaviour
{
    [Header("Ana Ekran")]
    public TMP_Text letterText;
    public TMP_Text progressText;
    public TMP_Text feedbackText;
    public Image signImage;

    [Header("Harf Seçim Paneli")]
    public GameObject letterSelectionPanel;
    public Transform letterGrid;
    public GameObject letterButtonPrefab;

    [Header("Recognition")]
    public RecognitionServiceBase recognitionService;

    [Range(0, 100)]
    public int minimumSuccessScore = 70;

    private LearningItemData[] letterItems;
    private int currentIndex = 0;

    private bool recognitionInProgress = false;

    void Start()
    {
        LoadLetterData();

        if (letterItems == null || letterItems.Length == 0)
        {
            Debug.LogError(
                "Harf LearningItemData bulunamadı!"
            );

            return;
        }

        GenerateLetterButtons();
        UpdateLetterDisplay();

        Image panelImage =
            letterSelectionPanel.GetComponent<Image>();

        if (panelImage != null)
        {
            Color color = panelImage.color;
            color.a = 1f;
            panelImage.color = color;
        }

        letterSelectionPanel.SetActive(false);

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
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
            "Yüklenen harf sayısı: " +
            letterItems.Length
        );
    }

    void GenerateLetterButtons()
    {
        for (int i = 0; i < letterItems.Length; i++)
        {
            int index = i;

            GameObject newButton =
                Instantiate(
                    letterButtonPrefab,
                    letterGrid
                );

            newButton.name =
                "LetterButton_" +
                letterItems[i].displayName;

            TMP_Text buttonText =
                newButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text =
                    letterItems[i].displayName;
            }

            Button button =
                newButton.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(
                    () => SelectLetter(index)
                );
            }
        }
    }

    void UpdateLetterDisplay()
    {
        LearningItemData currentItem =
            letterItems[currentIndex];

        letterText.text =
            currentItem.displayName;

        progressText.text =
            (currentIndex + 1) +
            " / " +
            letterItems.Length;

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

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    public void NextLetter()
    {
        if (recognitionInProgress)
            return;

        currentIndex++;

        if (currentIndex >= letterItems.Length)
        {
            currentIndex = 0;
        }

        UpdateLetterDisplay();
    }

    public void PreviousLetter()
    {
        if (recognitionInProgress)
            return;

        currentIndex--;

        if (currentIndex < 0)
        {
            currentIndex =
                letterItems.Length - 1;
        }

        UpdateLetterDisplay();
    }

    public void OpenLetterSelection()
    {
        if (recognitionInProgress)
            return;

        letterSelectionPanel.SetActive(true);

        letterSelectionPanel.transform
            .SetAsLastSibling();
    }

    public void CloseLetterSelection()
    {
        letterSelectionPanel.SetActive(false);
    }

    public void SelectLetter(int index)
    {
        if (
            recognitionInProgress ||
            index < 0 ||
            index >= letterItems.Length
        )
        {
            return;
        }

        currentIndex = index;

        UpdateLetterDisplay();

        CloseLetterSelection();
    }

    public void PracticeCurrentLetter()
    {
        if (recognitionInProgress)
            return;

        StartCoroutine(
            PracticeLetterRoutine()
        );
    }

    IEnumerator PracticeLetterRoutine()
    {
        if (recognitionService == null)
        {
            Debug.LogError(
                "Recognition Service bağlı değil!"
            );

            yield break;
        }

        LearningItemData currentItem =
            letterItems[currentIndex];

        recognitionInProgress = true;

        feedbackText.text =
            "DEĞERLENDİRİLİYOR...";

        RecognitionRequest request =
            new RecognitionRequest(
                currentItem.modelType,
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

        bool success =
            correctLabel &&
            score >= minimumSuccessScore;

        if (success)
        {
            feedbackText.text =
                "BAŞARILI!" +
                "\nSKOR: %" +
                score;
        }
        else
        {
            feedbackText.text =
                "TEKRAR DENE" +
                "\nTAHMİN: " +
                result.predictedLabel +
                " | SKOR: %" +
                score;
        }

        recognitionInProgress = false;
    }

    public LearningItemData GetCurrentItem()
    {
        if (
            letterItems == null ||
            letterItems.Length == 0
        )
        {
            return null;
        }

        return letterItems[currentIndex];
    }
}