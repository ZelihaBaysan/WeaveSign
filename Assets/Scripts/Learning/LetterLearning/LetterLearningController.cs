using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LetterLearningController : MonoBehaviour
{
    public TMP_Text letterText;
    public TMP_Text progressText;

    public GameObject letterSelectionPanel;
    public Transform letterGrid;
    public GameObject letterButtonPrefab;

    private string[] letters =
    {
        "A", "B", "C", "Ç", "D", "E", "F",
        "G", "Ğ", "H", "I", "İ", "J", "K",
        "L", "M", "N", "O", "Ö", "P", "R",
        "S", "Ş", "T", "U", "Ü", "V", "Y", "Z"
    };

    private int currentIndex = 0;

    void Start()
    {
        GenerateLetterButtons();

        UpdateLetterDisplay();

        // Harf seçim panelini tamamen opak yap.
        Image panelImage =
            letterSelectionPanel.GetComponent<Image>();

        if (panelImage != null)
        {
            Color color = panelImage.color;
            color.a = 1f;
            panelImage.color = color;
        }

        // Oyun başladığında seçim paneli kapalı olsun.
        letterSelectionPanel.SetActive(false);
    }

    void GenerateLetterButtons()
    {
        for (int i = 0; i < letters.Length; i++)
        {
            int index = i;

            GameObject newButton =
                Instantiate(
                    letterButtonPrefab,
                    letterGrid
                );

            newButton.name =
                "LetterButton_" + letters[i];

            TMP_Text buttonText =
                newButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text = letters[i];
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
        letterText.text =
            letters[currentIndex];

        progressText.text =
            (currentIndex + 1) +
            " / " +
            letters.Length;
    }

    public void NextLetter()
    {
        currentIndex++;

        if (currentIndex >= letters.Length)
        {
            currentIndex = 0;
        }

        UpdateLetterDisplay();
    }

    public void PreviousLetter()
    {
        currentIndex--;

        if (currentIndex < 0)
        {
            currentIndex =
                letters.Length - 1;
        }

        UpdateLetterDisplay();
    }

    public void OpenLetterSelection()
    {
        letterSelectionPanel.SetActive(true);

        // Paneli diğer UI elemanlarının önüne getir.
        letterSelectionPanel.transform.SetAsLastSibling();
    }

    public void CloseLetterSelection()
    {
        letterSelectionPanel.SetActive(false);
    }

    public void SelectLetter(int index)
    {
        currentIndex = index;

        UpdateLetterDisplay();

        CloseLetterSelection();
    }
}