using UnityEngine;

public struct RecognitionResult
{
    public string predictedLabel;
    public float confidence;

    public RecognitionResult(string predictedLabel, float confidence)
    {
        this.predictedLabel = predictedLabel;
        this.confidence = confidence;
    }
}

public class MockRecognitionService : MonoBehaviour
{
    public RecognitionResult Recognize(SpellCard targetCard)
    {
        // %0 - %100 arasında rastgele skor
        float randomConfidence = Random.Range(0f, 1f);

        // Test için %80 ihtimalle doğru sınıf,
        // %20 ihtimalle yanlış sınıf döndür.
        bool correctPrediction = Random.value <= 0.80f;

        string predictedLabel;

        if (correctPrediction)
        {
            predictedLabel = targetCard.modelLabel;
        }
        else
        {
            predictedLabel = "yanlis_isaret";
        }

        RecognitionResult result = new RecognitionResult(
            predictedLabel,
            randomConfidence
        );

        Debug.Log(
            "MOCK MODEL → Tahmin: " +
            result.predictedLabel +
            " | Skor: %" +
            Mathf.RoundToInt(result.confidence * 100)
        );

        return result;
    }
}