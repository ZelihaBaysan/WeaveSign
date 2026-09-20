public enum RecognitionModelType
{
    Letter,
    Word
}

public class RecognitionRequest
{
    public RecognitionModelType modelType;
    public string expectedLabel;

    public RecognitionRequest(
        RecognitionModelType modelType,
        string expectedLabel = null
    )
    {
        this.modelType = modelType;
        this.expectedLabel = expectedLabel;
    }
}