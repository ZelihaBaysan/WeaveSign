using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public int player1Health = 100;
    public int player2Health = 100;

    public TMP_Text player1HealthText;
    public TMP_Text player2HealthText;
    public TMP_Text turnText;
    public TMP_Text battleStatusText;

    // Oyun sonu ekranı
    public GameObject resultPanel;
    public TMP_Text winnerText;

    public MockRecognitionService recognitionService;
    public CardAreaController cardAreaController;

    private int currentPlayer = 1;
    private bool gameOver = false;

    private SpellCard selectedCard;
    public void RestartBattle()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
    }

    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    void Start()
    {
        // İlk oyuncuyu rastgele seç
        currentPlayer = Random.Range(1, 3);

        // Başlangıçta gizli
        battleStatusText.gameObject.SetActive(false);
        resultPanel.SetActive(false);

        UpdateUI();
    }

    // Oyuncunun seçtiği kart buraya gelir
    public void SelectCard(SpellCard card)
    {
        if (gameOver)
        {
            return;
        }

        selectedCard = card;

        Debug.Log(
            "BattleManager kartı aldı: " +
            selectedCard.displayName +
            " | Hasar: " +
            selectedCard.baseDamage
        );

        StartCoroutine(PreparationCountdown());
    }

    // Hazırlan → 3 → 2 → 1 → Başla
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
        yield return new WaitForSeconds(1f);

        // Şimdilik gerçek model yerine Mock model
        RecognitionResult result =
            recognitionService.Recognize(selectedCard);

        ApplyRecognitionResult(result);
    }

    // Model sonucunu oyuna uygular
    void ApplyRecognitionResult(RecognitionResult result)
    {
        if (selectedCard == null)
        {
            Debug.LogError("Seçili kart bulunamadı!");
            return;
        }

        int score = Mathf.RoundToInt(
            result.confidence * 100
        );

        float damageMultiplier = 0f;

        // Model doğru kelimeyi tahmin etti mi?
        bool correctPrediction =
            result.predictedLabel == selectedCard.modelLabel;

        // Yanlış kelime VEYA skor %50 altıysa başarısız
        bool failed =
            !correctPrediction || score < 50;

        // Hasar katsayısı
        if (!failed && score < 70)
        {
            damageMultiplier = 0.50f;
        }
        else if (!failed && score < 85)
        {
            damageMultiplier = 0.75f;
        }
        else if (!failed && score < 95)
        {
            damageMultiplier = 1.00f;
        }
        else if (!failed)
        {
            damageMultiplier = 1.10f;
        }

        // =========================
        // BAŞARISIZ HAREKET
        // =========================
        if (failed)
        {
            // Saldıran oyuncu 5 can kaybeder
            if (currentPlayer == 1)
            {
                player1Health -= 5;

                if (player1Health < 0)
                {
                    player1Health = 0;
                }
            }
            else
            {
                player2Health -= 5;

                if (player2Health < 0)
                {
                    player2Health = 0;
                }
            }

            battleStatusText.text =
                "BAŞARISIZ!" +
                "\nTAHMİN: " + result.predictedLabel +
                "\nSKOR: %" + score +
                "\n-5 CAN";
        }

        // =========================
        // BAŞARILI HAREKET
        // =========================
        else
        {
            int finalDamage = Mathf.RoundToInt(
                selectedCard.baseDamage *
                damageMultiplier
            );

            // Oyuncu 1 saldırıyorsa Oyuncu 2 hasar alır
            if (currentPlayer == 1)
            {
                player2Health -= finalDamage;

                if (player2Health < 0)
                {
                    player2Health = 0;
                }
            }
            else
            {
                // Oyuncu 2 saldırıyorsa Oyuncu 1 hasar alır
                player1Health -= finalDamage;

                if (player1Health < 0)
                {
                    player1Health = 0;
                }
            }

            if (score >= 95)
            {
                battleStatusText.text =
                    "KRİTİK!" +
                    "\nSKOR: %" + score +
                    "\nHASAR: " + finalDamage;
            }
            else
            {
                battleStatusText.text =
                    "BAŞARILI!" +
                    "\nSKOR: %" + score +
                    "\nHASAR: " + finalDamage;
            }
        }

        CheckGameOver();

        // Oyun bitmediyse sıra değişsin
        if (!gameOver)
        {
            ChangeTurn();
        }

        UpdateUI();

        // Oyun bitmediyse yeni tur
        if (!gameOver)
        {
            StartCoroutine(PrepareNextTurn());
        }
    }

    // Sonucu biraz göster, sonra yeni 5 kart getir
    IEnumerator PrepareNextTurn()
    {
        yield return new WaitForSeconds(2.5f);

        battleStatusText.gameObject.SetActive(false);

        selectedCard = null;

        cardAreaController.StartNewTurn();
    }

    // Oyuncu sırasını değiştir
    void ChangeTurn()
    {
        if (currentPlayer == 1)
        {
            currentPlayer = 2;
        }
        else
        {
            currentPlayer = 1;
        }
    }

    // Oyunculardan biri öldü mü?
    void CheckGameOver()
    {
        if (player1Health <= 0 ||
            player2Health <= 0)
        {
            gameOver = true;
        }
    }

    // Geçici TEST HASAR butonu
    public void TestAttack()
    {
        if (gameOver)
        {
            return;
        }

        if (currentPlayer == 1)
        {
            player2Health -= 10;

            if (player2Health < 0)
            {
                player2Health = 0;
            }
        }
        else
        {
            player1Health -= 10;

            if (player1Health < 0)
            {
                player1Health = 0;
            }
        }

        CheckGameOver();

        if (!gameOver)
        {
            ChangeTurn();
        }

        UpdateUI();
    }

    // Can, sıra ve sonuç ekranını günceller
    void UpdateUI()
    {
        player1HealthText.text =
            "OYUNCU 1 - CAN: " +
            player1Health;

        player2HealthText.text =
            "OYUNCU 2 - CAN: " +
            player2Health;

        if (gameOver)
        {
            // Oyun bitti, sonuç panelini aç
            resultPanel.SetActive(true);

            // Hazırlık / skor yazısını gizle
            battleStatusText.gameObject.SetActive(false);

            if (player1Health <= 0)
            {
                turnText.text =
                    "OYUNCU 2 KAZANDI!";

                winnerText.text =
                    "OYUNCU 2 KAZANDI!";
            }
            else
            {
                turnText.text =
                    "OYUNCU 1 KAZANDI!";

                winnerText.text =
                    "OYUNCU 1 KAZANDI!";
            }
        }
        else
        {
            turnText.text =
                "SIRA: OYUNCU " +
                currentPlayer;
        }
    }
}