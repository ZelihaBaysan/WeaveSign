using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class CardView : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text difficultyText;

    [FormerlySerializedAs("damageText")]
    public TMP_Text powerText;

    [FormerlySerializedAs("durationText")]
    public TMP_Text typeText;

    public Button selectButton;

    private SpellCard cardData;
    private CardAreaController cardAreaController;

    private Image backgroundImage;

    public void Setup(
        SpellCard card,
        CardAreaController controller
    )
    {
        cardData = card;
        cardAreaController = controller;

        backgroundImage = GetComponent<Image>();

        titleText.text = card.displayName.ToUpper();

        difficultyText.text =
            "ZORLUK: " + card.difficulty + "/3";

        if (card.cardType == CardType.Attack)
        {
            typeText.text = "SALDIRI";

            powerText.text =
                "HASAR: " + card.basePower;

            // Saldırı kartı = kırmızı
            backgroundImage.color =
                new Color(
                    0.45f,
                    0.18f,
                    0.18f,
                    1f
                );
        }
        else
        {
            typeText.text = "İYİLEŞTİRME";

            powerText.text =
                "CAN: +" + card.basePower;

            // İyileştirme kartı = yeşil
            backgroundImage.color =
                new Color(
                    0.18f,
                    0.40f,
                    0.24f,
                    1f
                );
        }

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(SelectCard);
    }

    void SelectCard()
    {
        cardAreaController.OnCardSelected(
            cardData,
            this
        );

        selectButton.gameObject.SetActive(false);
    }
}