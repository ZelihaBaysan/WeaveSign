using System;
using System.Collections;
using UnityEngine;

public class MockRecognitionService : RecognitionServiceBase
{
    private readonly string[] mockLetterLabels =
    {
        "A", "B", "C", "Ç", "D", "E", "F",
        "G", "Ğ", "H", "I", "İ", "J", "K",
        "L", "M", "N", "O", "Ö", "P", "R",
        "S", "Ş", "T", "U", "Ü", "V", "Y", "Z",
        "nothing", "del", "space"
    };

    public override IEnumerator Recognize(
        RecognitionRequest request,
        Action<RecognitionResult> onResult
    )
    {
        yield return new WaitForSeconds(0.2f);

        float randomConfidence =
            UnityEngine.Random.Range(0.55f, 1f);

        string predictedLabel;

        // Harf/Kelime öğrenme ve düelloda
        // beklenen cevap var.
        if (!string.IsNullOrEmpty(request.expectedLabel))
        {
            bool correctPrediction =
                UnityEngine.Random.value <= 0.80f;

            if (correctPrediction)
            {
                predictedLabel =
                    request.expectedLabel;
            }
            else
            {
                predictedLabel =
                    request.modelType == RecognitionModelType.Letter
                    ? "yanlis_harf"
                    : "yanlis_isaret";
            }
        }

        // İsmini Yaz Atölyesi:
        // beklenen cevap yok, model ne gördüyse döndürüyor.
        else if (
            request.modelType ==
            RecognitionModelType.Letter
        )
        {
            predictedLabel =
                mockLetterLabels[
                    UnityEngine.Random.Range(
                        0,
                        mockLetterLabels.Length
                    )
                ];
        }
        else
        {
            predictedLabel =
                "yanlis_isaret";
        }

        RecognitionResult result =
            new RecognitionResult(
                predictedLabel,
                randomConfidence
            );

        Debug.Log(
            "MOCK MODEL → Model: " +
            request.modelType +
            " | Tahmin: " +
            result.predictedLabel +
            " | Skor: %" +
            Mathf.RoundToInt(
                result.confidence * 100
            )
        );

        onResult?.Invoke(result);
    }
}