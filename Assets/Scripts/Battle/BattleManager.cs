using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public enum BattleState
{
    CardSelection,     // Oyuncu kart seçiyor
    Preparing,         // 3 - 2 - 1
    Performing,        // İşaret yapılıyor
    Resolving,         // Model sonucu değerlendiriliyor
    TurnTransition,    // Yeni tura geçiliyor
    GameOver           // Oyun bitti
}

public class BattleManager : MonoBehaviour
{
    public int player1Health = 100;
    public int player2Health = 100;

    public TMP_Text player1HealthText;
    public TMP_Text player2HealthText;
    public TMP_Text turnText;
    public TMP_Text battleStatusText;

    public GameObject resultPanel;
    public TMP_Text winnerText;

    public RecognitionServiceBase recognitionService;
    public CardAreaController cardAreaController;

    // Bütün kartlar için ortak hareket süresi
    public int actionDuration = 5;

    private int currentPlayer = 1;
    private bool gameOver = false;

    private SpellCard selectedCard;

    private BattleState currentState;

    void Start()
    {
        currentPlayer = Random.Range(1, 3);

        battleStatusText.gameObject.SetActive(false);
        resultPanel.SetActive(false);

        SetState(BattleState.CardSelection);

        UpdateUI();
    }

    void SetState(BattleState newState)
    {
        currentState = newState;

        Debug.Log(
            "BATTLE STATE → " + currentState
        );
    }

    public void SelectCard(SpellCard card)
    {
        // Sadece kart seçim aşamasında kart seçilebilir
        if (gameOver)
            return;

        if (currentState != BattleState.CardSelection)
            return;

        selectedCard = card;

        Debug.Log(
            "Seçilen kart: " +
            selectedCard.displayName +
            " | Tür: " +
            selectedCard.cardType +
            " | Güç: " +
            selectedCard.basePower
        );

        SetState(BattleState.Preparing);

        StartCoroutine(
            PreparationCountdown()
        );
    }

    IEnumerator PreparationCountdown()
    {
        battleStatusText.gameObject.SetActive(true);

        battleStatusText.text =
            "HAREKETİ YAPMAYA HAZIRLAN";

        yield return new WaitForSeconds(1.5f);

        battleStatusText.text = "3";
        yield return new WaitForSeconds(1f);

        battleStatusText.text = "2";
        yield return new WaitForSeconds(1f);

        battleStatusText.text = "1";
        yield return new WaitForSeconds(1f);

        battleStatusText.text = "BAŞLA!";
        yield return new WaitForSeconds(0.7f);

        SetState(BattleState.Performing);

        // Ortak hareket süresi
        for (int i = actionDuration; i > 0; i--)
        {
            battleStatusText.text =
                "HAREKETİ YAP!\n" + i;

            yield return new WaitForSeconds(1f);
        }

        SetState(BattleState.Resolving);

        battleStatusText.text =
            "DEĞERLENDİRİLİYOR...";

        RecognitionResult result = default;
        bool resultReceived = false;

        yield return StartCoroutine(
            recognitionService.Recognize(
                selectedCard,
                recognitionResult =>
                {
                    result = recognitionResult;
                    resultReceived = true;
                }
            )
        );

        if (!resultReceived)
        {
            Debug.LogError(
                "Recognition sonucu alınamadı!"
            );

            yield break;
        }

        ApplyRecognitionResult(result);
    }

    void ApplyRecognitionResult(
        RecognitionResult result
    )
    {
        if (selectedCard == null)
        {
            Debug.LogError(
                "Seçili kart bulunamadı!"
            );

            return;
        }

        int score =
            Mathf.RoundToInt(
                result.confidence * 100
            );

        bool correctPrediction =
            result.predictedLabel ==
            selectedCard.modelLabel;

        bool failed =
            !correctPrediction ||
            score < 50;

        float powerMultiplier = 0f;

        if (!failed && score < 70)
        {
            powerMultiplier = 0.50f;
        }
        else if (!failed && score < 85)
        {
            powerMultiplier = 0.75f;
        }
        else if (!failed && score < 95)
        {
            powerMultiplier = 1.00f;
        }
        else if (!failed)
        {
            powerMultiplier = 1.10f;
        }

        if (failed)
        {
            ApplyFailurePenalty(
                result,
                score
            );
        }
        else
        {
            int finalPower =
                Mathf.RoundToInt(
                    selectedCard.basePower *
                    powerMultiplier
                );

            if (
                selectedCard.cardType ==
                CardType.Attack
            )
            {
                ApplyAttack(
                    finalPower,
                    score
                );
            }
            else
            {
                ApplyHeal(
                    finalPower,
                    score
                );
            }
        }

        CheckGameOver();

        if (gameOver)
        {
            SetState(
                BattleState.GameOver
            );

            UpdateUI();

            return;
        }

        ChangeTurn();

        SetState(
            BattleState.TurnTransition
        );

        UpdateUI();

        StartCoroutine(
            PrepareNextTurn()
        );
    }

    void ApplyFailurePenalty(
        RecognitionResult result,
        int score
    )
    {
        if (currentPlayer == 1)
        {
            player1Health -= 5;

            if (player1Health < 0)
                player1Health = 0;
        }
        else
        {
            player2Health -= 5;

            if (player2Health < 0)
                player2Health = 0;
        }

        battleStatusText.text =
            "BAŞARISIZ!" +
            "\nTAHMİN: " +
            result.predictedLabel +
            "\nSKOR: %" +
            score +
            "\n-5 CAN";
    }

    void ApplyAttack(
        int finalDamage,
        int score
    )
    {
        if (currentPlayer == 1)
        {
            player2Health -= finalDamage;

            if (player2Health < 0)
                player2Health = 0;
        }
        else
        {
            player1Health -= finalDamage;

            if (player1Health < 0)
                player1Health = 0;
        }

        if (score >= 95)
        {
            battleStatusText.text =
                "KRİTİK SALDIRI!" +
                "\nSKOR: %" +
                score +
                "\nHASAR: " +
                finalDamage;
        }
        else
        {
            battleStatusText.text =
                "BAŞARILI!" +
                "\nSKOR: %" +
                score +
                "\nHASAR: " +
                finalDamage;
        }
    }

    void ApplyHeal(
        int healAmount,
        int score
    )
    {
        int actualHeal = 0;

        if (currentPlayer == 1)
        {
            int oldHealth =
                player1Health;

            player1Health =
                Mathf.Clamp(
                    player1Health + healAmount,
                    0,
                    100
                );

            actualHeal =
                player1Health -
                oldHealth;
        }
        else
        {
            int oldHealth =
                player2Health;

            player2Health =
                Mathf.Clamp(
                    player2Health + healAmount,
                    0,
                    100
                );

            actualHeal =
                player2Health -
                oldHealth;
        }

        if (score >= 95)
        {
            battleStatusText.text =
                "KRİTİK İYİLEŞTİRME!" +
                "\nSKOR: %" +
                score +
                "\nCAN: +" +
                actualHeal;
        }
        else
        {
            battleStatusText.text =
                "İYİLEŞTİRME BAŞARILI!" +
                "\nSKOR: %" +
                score +
                "\nCAN: +" +
                actualHeal;
        }
    }

    IEnumerator PrepareNextTurn()
    {
        yield return new WaitForSeconds(2.5f);

        battleStatusText.gameObject.SetActive(false);

        selectedCard = null;

        cardAreaController.StartNewTurn();

        SetState(
            BattleState.CardSelection
        );
    }

    void ChangeTurn()
    {
        if (currentPlayer == 1)
            currentPlayer = 2;
        else
            currentPlayer = 1;
    }

    void CheckGameOver()
    {
        if (
            player1Health <= 0 ||
            player2Health <= 0
        )
        {
            gameOver = true;
        }
    }

    public void TestAttack()
    {
        if (gameOver)
            return;

        if (currentPlayer == 1)
        {
            player2Health -= 10;

            if (player2Health < 0)
                player2Health = 0;
        }
        else
        {
            player1Health -= 10;

            if (player1Health < 0)
                player1Health = 0;
        }

        CheckGameOver();

        if (gameOver)
        {
            SetState(
                BattleState.GameOver
            );
        }
        else
        {
            ChangeTurn();
        }

        UpdateUI();
    }

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
            resultPanel.SetActive(true);

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

    public void RestartBattle()
    {
        SceneManager.LoadScene("Battle");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}