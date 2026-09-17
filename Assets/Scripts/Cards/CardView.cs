using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text difficultyText;
    public TMP_Text damageText;
    public TMP_Text durationText;
    public Button selectButton;

    private SpellCard cardData;
    private CardAreaController cardAreaController;

    public void Setup(SpellCard card, CardAreaController controller)
    {
        cardData = card;
        cardAreaController = controller;

        titleText.text = card.displayName.ToUpper();
        difficultyText.text = "ZORLUK: " + card.difficulty + "/3";
        damageText.text = "HASAR: " + card.baseDamage;
        durationText.text = "SÜRE: " + card.duration + " sn";

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(SelectCard);
    }

    void SelectCard()
    {
        cardAreaController.OnCardSelected(cardData, this);

        selectButton.gameObject.SetActive(false);
    }
}