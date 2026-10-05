using System.Collections;
using UnityEngine;

public class RecognitionTest : MonoBehaviour
{
    [SerializeField]
    private YarenRecognitionService recognitionService;

    private IEnumerator Start()
    {
        Debug.Log("3 saniye sonra test yapilacak.");

        yield return new WaitForSeconds(8f);

        yield return recognitionService.Recognize(
            new RecognitionRequest(
                RecognitionModelType.Letter,
                "A"
            ),
            result =>
            {
                Debug.Log(
                    "TEST SONUCU -> " +
                    result.predictedLabel +
                    " | " +
                    result.confidence
                );
            }
        );
    }
}