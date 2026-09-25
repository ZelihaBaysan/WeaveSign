using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum BattleState
{
    CardSelection,
    Preparing,
    Performing,
    Resolving,
    TurnTransition,
    GameOver
}

public class BattleManager : MonoBehaviour
{
    [Header("Can Sistemi")]
    public int maxHealth = 100;
    public int player1Health = 100;
    public int player2Health = 100;

    [Header("Mevcut HUD Yazıları")]
    public TMP_Text player1HealthText;
    public TMP_Text player2HealthText;
    public TMP_Text turnText;
    public TMP_Text battleStatusText;

    [Header("HP Barları")]
    public Slider player1HPBar;
    public Slider player2HPBar;

    public TMP_Text player1HPText;
    public TMP_Text player2HPText;

    [Header("Sonuç Ekranı")]
    public GameObject resultPanel;
    public TMP_Text winnerText;

    [Header("Sistemler")]
    public RecognitionServiceBase recognitionService;
    public CardAreaController cardAreaController;

    [Header("Aksiyon")]
    public int actionDuration = 5;

    private int currentPlayer = 1;
    private bool gameOver = false;

    private SpellCard selectedCard;

    private BattleState currentState;

    void Start()
    {
        player1Health = maxHealth;
        player2Health = maxHealth;

        currentPlayer = Random.Range(1, 3);

        battleStatusText.gameObject.SetActive(false);
        resultPanel.SetActive(false);

        SetupHealthBars();

        SetState(BattleState.CardSelection);

        UpdateUI();
    }

    void SetupHealthBars()
    {
        if (player1HPBar != null)
        {
            player1HPBar.minValue = 0;
            player1HPBar.maxValue = maxHealth;
            player1HPBar.value = player1Health;
        }

        if (player2HPBar != null)
        {
            player2HPBar.minValue = 0;
            player2HPBar.maxValue = maxHealth;
            player2HPBar.value = player2Health;
        }
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

        for (int i = actionDuration; i > 0; i--)
        {
            battleStatusText.text =
                "HAREKETİ YAP!\n" + i;

            yield return new WaitForSeconds(1f);
        }

        SetState(BattleState.Resolving);

        battleStatusText.text =
            "DEĞERLENDİRİLİYOR...";

        if (recognitionService == null)
        {
            Debug.LogError(
                "Recognition Service bağlı değil!"
            );

            battleStatusText.text =
                "RECOGNITION SERVICE BAĞLI DEĞİL";

            yield break;
        }

        RecognitionRequest request =
            new RecognitionRequest(
                RecognitionModelType.Word,
                selectedCard.modelLabel
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
            Debug.LogError(
                "Recognition sonucu alınamadı!"
            );

            battleStatusText.text =
                "SONUÇ ALINAMADI";

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

            // Zafer sesi
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVictory();
            }

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
        // Yanlış / başarısız sesi
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMistake();
        }

        if (currentPlayer == 1)
        {
            player1Health =
                Mathf.Clamp(
                    player1Health - 5,
                    0,
                    maxHealth
                );
        }
        else
        {
            player2Health =
                Mathf.Clamp(
                    player2Health - 5,
                    0,
                    maxHealth
                );
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
        // Saldırı sesi
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAttack();
        }

        if (currentPlayer == 1)
        {
            player2Health =
                Mathf.Clamp(
                    player2Health - finalDamage,
                    0,
                    maxHealth
                );
        }
        else
        {
            player1Health =
                Mathf.Clamp(
                    player1Health - finalDamage,
                    0,
                    maxHealth
                );
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
        // Heal sesi
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHeal();
        }

        int actualHeal = 0;

        if (currentPlayer == 1)
        {
            int oldHealth =
                player1Health;

            player1Health =
                Mathf.Clamp(
                    player1Health + healAmount,
                    0,
                    maxHealth
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
                    maxHealth
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

    void UpdateUI()
    {
        if (player1HealthText != null)
        {
            player1HealthText.text =
                "OYUNCU 1 - CAN: " +
                player1Health;
        }

        if (player2HealthText != null)
        {
            player2HealthText.text =
                "OYUNCU 2 - CAN: " +
                player2Health;
        }

        if (player1HPBar != null)
        {
            player1HPBar.value =
                player1Health;
        }

        if (player2HPBar != null)
        {
            player2HPBar.value =
                player2Health;
        }

        if (player1HPText != null)
        {
            player1HPText.text =
                player1Health +
                " / " +
                maxHealth;
        }

        if (player2HPText != null)
        {
            player2HPText.text =
                player2Health +
                " / " +
                maxHealth;
        }

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