using System;
using System.Collections;
using UnityEngine;

public class MockRecognitionService : RecognitionServiceBase
{
    public override IEnumerator Recognize(
        SpellCard targetCard,
        Action<RecognitionResult> onResult
    )
    {
        // Gerçek modelin küçük işlem süresini taklit eder.
        yield return new WaitForSeconds(0.2f);

        float randomConfidence =
            UnityEngine.Random.Range(0f, 1f);

        bool correctPrediction =
            UnityEngine.Random.value <= 0.80f;

        string predictedLabel;

        if (correctPrediction)
        {
            predictedLabel = targetCard.modelLabel;
        }
        else
        {
            predictedLabel = "yanlis_isaret";
        }

        RecognitionResult result =
            new RecognitionResult(
                predictedLabel,
                randomConfidence
            );

        Debug.Log(
            "MOCK MODEL → Tahmin: " +
            result.predictedLabel +
            " | Skor: %" +
            Mathf.RoundToInt(
                result.confidence * 100
            )
        );

        onResult?.Invoke(result);
    }
}