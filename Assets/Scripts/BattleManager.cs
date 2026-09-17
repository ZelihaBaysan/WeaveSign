using System.Collections;
using UnityEngine;
using TMPro;

public class BattleManager : MonoBehaviour
{
    public int player1Health = 100;
    public int player2Health = 100;

    public TMP_Text player1HealthText;
    public TMP_Text player2HealthText;
    public TMP_Text turnText;

    // Hazırlan / 3 / 2 / 1 / BAŞLA yazısı
    public TMP_Text battleStatusText;

    private int currentPlayer = 1;
    private bool gameOver = false;

    // Oyuncunun seçtiği kart
    private SpellCard selectedCard;

    void Start()
    {
        currentPlayer = Random.Range(1, 3);

        // Oyun başlarken status yazısı görünmesin
        battleStatusText.gameObject.SetActive(false);

        UpdateUI();
    }

    // CardAreaController seçilen kartı buraya gönderir
    public void SelectCard(SpellCard card)
    {
        selectedCard = card;

        Debug.Log(
            "BattleManager kartı aldı: " +
            selectedCard.displayName +
            " | Hasar: " +
            selectedCard.baseDamage
        );

        StartCoroutine(PreparationCountdown());
    }

    IEnumerator PreparationCountdown()
    {
        battleStatusText.gameObject.SetActive(true);

        battleStatusText.text = "HAREKETİ YAPMAYA HAZIRLAN";
        yield return new WaitForSeconds(1.5f);

        battleStatusText.text = "3";
        yield return new WaitForSeconds(1f);

        battleStatusText.text = "2";
        yield return new WaitForSeconds(1f);

        battleStatusText.text = "1";
        yield return new WaitForSeconds(1f);

        battleStatusText.text = "BAŞLA!";

        Debug.Log(
            "Tanıma başlayacak kart: " +
            selectedCard.displayName
        );

        // Daha sonra B'nin kamera/model sistemi TAM BURADA başlayacak.
    }

    public void TestAttack()
    {
        if (gameOver)
        {
            return;
        }

        if (currentPlayer == 1)
        {
            player2Health -= 10;

            if (player2Health <= 0)
            {
                player2Health = 0;
                gameOver = true;
            }
            else
            {
                currentPlayer = 2;
            }
        }
        else
        {
            player1Health -= 10;

            if (player1Health <= 0)
            {
                player1Health = 0;
                gameOver = true;
            }
            else
            {
                currentPlayer = 1;
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        player1HealthText.text = "OYUNCU 1 - CAN: " + player1Health;
        player2HealthText.text = "OYUNCU 2 - CAN: " + player2Health;

        if (gameOver)
        {
            if (player1Health <= 0)
            {
                turnText.text = "OYUNCU 2 KAZANDI!";
            }
            else
            {
                turnText.text = "OYUNCU 1 KAZANDI!";
            }
        }
        else
        {
            turnText.text = "SIRA: OYUNCU " + currentPlayer;
        }
    }
}