using System.Collections.Generic;
using UnityEngine;

public class CardAreaController : MonoBehaviour
{
    [Header("Kart Sistemi")]
    public GameObject cardPrefab;
    public BattleManager battleManager;

    [Header("El Ayarları")]
    public int cardsPerHand = 5;

    [Range(0, 5)]
    public int minimumAttackCards = 3;

    private List<SpellCard> allCards =
        new List<SpellCard>();

    private List<SpellCard> usedCards =
        new List<SpellCard>();

    void Start()
    {
        LoadCards();
        ShowRandomCards(cardsPerHand);
    }

    void LoadCards()
    {
        SpellCard[] loadedCards =
            Resources.LoadAll<SpellCard>(
                "Cards"
            );

        allCards.Clear();
        allCards.AddRange(loadedCards);

        int attackCount = 0;
        int healCount = 0;

        foreach (SpellCard card in allCards)
        {
            if (card.cardType == CardType.Attack)
            {
                attackCount++;
            }
            else if (card.cardType == CardType.Heal)
            {
                healCount++;
            }
        }

        Debug.Log(
            "Yüklenen kart sayısı: " +
            allCards.Count +
            " | Attack: " +
            attackCount +
            " | Heal: " +
            healCount
        );
    }

    void ShowRandomCards(int count)
    {
        if (allCards.Count == 0)
        {
            Debug.LogError(
                "Gösterilecek kart bulunamadı!"
            );

            return;
        }

        int requiredAttackCount =
            Mathf.Min(
                minimumAttackCards,
                count
            );

        List<SpellCard> availableCards =
            GetAvailableCards();

        List<SpellCard> availableAttackCards =
            GetAttackCards(
                availableCards
            );

        // Yeterli toplam kart veya saldırı kartı
        // kalmadıysa kart geçmişini sıfırla.
        if (
            availableCards.Count < count ||
            availableAttackCards.Count <
            requiredAttackCount
        )
        {
            Debug.Log(
                "Kart havuzu yenileniyor."
            );

            usedCards.Clear();

            availableCards =
                new List<SpellCard>(
                    allCards
                );

            availableAttackCards =
                GetAttackCards(
                    availableCards
                );
        }

        List<SpellCard> selectedCards =
            new List<SpellCard>();

        // Önce minimum saldırı kartlarını seç.
        for (
            int i = 0;
            i < requiredAttackCount;
            i++
        )
        {
            if (
                availableAttackCards.Count == 0
            )
            {
                break;
            }

            int randomIndex =
                Random.Range(
                    0,
                    availableAttackCards.Count
                );

            SpellCard selectedAttack =
                availableAttackCards[
                    randomIndex
                ];

            selectedCards.Add(
                selectedAttack
            );

            availableAttackCards.Remove(
                selectedAttack
            );

            availableCards.Remove(
                selectedAttack
            );
        }

        // Kalan yerleri tüm uygun kartlardan
        // rastgele doldur.
        while (
            selectedCards.Count < count &&
            availableCards.Count > 0
        )
        {
            int randomIndex =
                Random.Range(
                    0,
                    availableCards.Count
                );

            SpellCard selectedCard =
                availableCards[
                    randomIndex
                ];

            selectedCards.Add(
                selectedCard
            );

            availableCards.RemoveAt(
                randomIndex
            );
        }

        // Kartların ekrandaki sırası
        // hep Attack -> Attack -> Attack olmasın.
        ShuffleCards(selectedCards);

        foreach (
            SpellCard selectedCard
            in selectedCards
        )
        {
            GameObject newCard =
                Instantiate(
                    cardPrefab,
                    transform
                );

            CardView cardView =
                newCard.GetComponent<CardView>();

            if (cardView != null)
            {
                cardView.Setup(
                    selectedCard,
                    this
                );
            }
        }

        int shownAttackCount = 0;
        int shownHealCount = 0;

        foreach (
            SpellCard card
            in selectedCards
        )
        {
            if (
                card.cardType ==
                CardType.Attack
            )
            {
                shownAttackCount++;
            }
            else
            {
                shownHealCount++;
            }
        }

        Debug.Log(
            "Yeni el → Attack: " +
            shownAttackCount +
            " | Heal: " +
            shownHealCount
        );
    }

    List<SpellCard> GetAvailableCards()
    {
        List<SpellCard> availableCards =
            new List<SpellCard>();

        foreach (SpellCard card in allCards)
        {
            if (!usedCards.Contains(card))
            {
                availableCards.Add(card);
            }
        }

        return availableCards;
    }

    List<SpellCard> GetAttackCards(
        List<SpellCard> source
    )
    {
        List<SpellCard> attackCards =
            new List<SpellCard>();

        foreach (SpellCard card in source)
        {
            if (
                card.cardType ==
                CardType.Attack
            )
            {
                attackCards.Add(card);
            }
        }

        return attackCards;
    }

    void ShuffleCards(
        List<SpellCard> cards
    )
    {
        for (
            int i = cards.Count - 1;
            i > 0;
            i--
        )
        {
            int randomIndex =
                Random.Range(
                    0,
                    i + 1
                );

            SpellCard temp =
                cards[i];

            cards[i] =
                cards[randomIndex];

            cards[randomIndex] =
                temp;
        }
    }

    public void OnCardSelected(
        SpellCard selectedCard,
        CardView selectedCardView
    )
    {
        Debug.Log(
            "Seçilen kart: " +
            selectedCard.displayName
        );

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardSelect();
        }

        // Sadece gerçekten kullanılan kart
        // tekrar gelmesin.
        if (!usedCards.Contains(selectedCard))
        {
            usedCards.Add(
                selectedCard
            );
        }

        battleManager.SelectCard(
            selectedCard
        );

        // Diğer 4 kartı kaldır.
        foreach (Transform child in transform)
        {
            if (
                child !=
                selectedCardView.transform
            )
            {
                Destroy(
                    child.gameObject
                );
            }
        }
    }

    public void StartNewTurn()
    {
        // Önce eski kartları temizle.
        foreach (Transform child in transform)
        {
            Destroy(
                child.gameObject
            );
        }

        // Yeni el oluştur.
        ShowRandomCards(
            cardsPerHand
        );
    }
}