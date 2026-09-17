using System.Collections.Generic;
using UnityEngine;

public class CardAreaController : MonoBehaviour
{
    public GameObject cardPrefab;
    public BattleManager battleManager;

    private List<SpellCard> allCards = new List<SpellCard>();

    void Start()
    {
        LoadCards();
        ShowRandomCards(5);
    }

    void LoadCards()
    {
        SpellCard[] loadedCards = Resources.LoadAll<SpellCard>("Cards");

        allCards.Clear();
        allCards.AddRange(loadedCards);

        Debug.Log("Yüklenen kart sayısı: " + allCards.Count);
    }

    void ShowRandomCards(int count)
    {
        if (allCards.Count < count)
        {
            Debug.LogError("Yeterli kart bulunamadı!");
            return;
        }

        List<SpellCard> availableCards = new List<SpellCard>(allCards);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableCards.Count);

            SpellCard selectedCard = availableCards[randomIndex];
            availableCards.RemoveAt(randomIndex);

            GameObject newCard = Instantiate(cardPrefab, transform);

            CardView cardView = newCard.GetComponent<CardView>();

            if (cardView != null)
            {
                cardView.Setup(selectedCard, this);
            }
        }
    }

    public void OnCardSelected(SpellCard selectedCard, CardView selectedCardView)
    {
        Debug.Log("Seçilen kart: " + selectedCard.displayName);

        battleManager.SelectCard(selectedCard);

        foreach (Transform child in transform)
        {
            if (child != selectedCardView.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}