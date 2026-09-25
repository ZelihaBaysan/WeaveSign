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

    public void NextWord()
    {
        if (recognitionInProgress)
            return;

        currentIndex++;

        if (currentIndex >= wordItems.Length)
        {
            currentIndex = 0;
        }

        UpdateWordDisplay();
    }

    public void PreviousWord()
    {
        if (recognitionInProgress)
            return;

        currentIndex--;

        if (currentIndex < 0)
        {
            currentIndex =
                wordItems.Length - 1;
        }

        UpdateWordDisplay();
    }

    public void OpenWordSelection()
    {
        if (recognitionInProgress)
            return;

        wordSelectionPanel.SetActive(true);

        wordSelectionPanel.transform
            .SetAsLastSibling();
    }

    public void CloseWordSelection()
    {
        wordSelectionPanel.SetActive(false);
    }

    public void SelectWord(int index)
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

    public void PracticeCurrentWord()
    {
        if (recognitionInProgress)
            return;

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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMistake();
            }

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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySuccess();
            }

            feedbackText.text =
                "BAŞARILI!" +
                "\nSKOR: %" +
                score;
        }
        else
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMistake();
            }

            feedbackText.text =
                "TEKRAR DENE" +
                "\nTAHMİN: " +
                result.predictedLabel +
                " | SKOR: %" +
                score;
        }

        recognitionInProgress = false;
    }
}