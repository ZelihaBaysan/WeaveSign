public struct RecognitionResult
{
    public string predictedLabel;
    public float confidence;

    public RecognitionResult(
        string predictedLabel,
        float confidence
    )
    {
        this.predictedLabel = predictedLabel;
        this.confidence = confidence;
    }
}