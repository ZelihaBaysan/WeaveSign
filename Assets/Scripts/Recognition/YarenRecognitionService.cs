using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


public class YarenRecognitionService : RecognitionServiceBase
{
    [SerializeField]
    private string apiUrl =
        "http://127.0.0.1:8000/recognize";
    [SerializeField]
    private WebcamCapture webcamCapture;


    [Serializable]
    private class ApiRequest
    {
        public string modelType;
        public string expectedLabel;
        public string imageBase64;
    }


    [Serializable]
    private class ApiResponse
    {
        public string predictedLabel;
        public float confidence;
    }


    public override IEnumerator Recognize(
        RecognitionRequest request,
        Action<RecognitionResult> onResult
    )
    {
        if (webcamCapture == null)
        {
            Debug.LogError("WebcamCapture baglanmamis.");

            onResult?.Invoke(
                new RecognitionResult(
                    "",
                    0f
                )
            );

            yield break;
        }

        webcamCapture.StartCamera();

        yield return new WaitForSeconds(0.5f);

        string imageBase64 =
            webcamCapture.GetFrameBase64();

        if (string.IsNullOrEmpty(imageBase64))
        {
            Debug.LogError(
                "Kameradan goruntu alinamadi."
            );

            onResult?.Invoke(
                new RecognitionResult(
                    "",
                    0f
                )
            );

            yield break;
        }

        ApiRequest apiRequest =
            new ApiRequest
            {
                modelType =
                    request.modelType ==
                    RecognitionModelType.Letter
                    ? "letter"
                    : "word",

                expectedLabel =
                    request.expectedLabel,

                imageBase64 =
                    imageBase64
            };


        string json =
            JsonUtility.ToJson(
                apiRequest
            );


        byte[] bodyRaw =
            System.Text.Encoding.UTF8
            .GetBytes(json);


        using (
            UnityWebRequest webRequest =
                new UnityWebRequest(
                    apiUrl,
                    "POST"
                )
        )
        {
            webRequest.uploadHandler =
                new UploadHandlerRaw(
                    bodyRaw
                );

            webRequest.downloadHandler =
                new DownloadHandlerBuffer();

            webRequest.SetRequestHeader(
                "Content-Type",
                "application/json"
            );


            yield return webRequest.SendWebRequest();


            if (
                webRequest.result !=
                UnityWebRequest.Result.Success
            )
            {
                Debug.LogError(
                    "YAREN MODEL API HATASI: " +
                    webRequest.error
                );

                onResult?.Invoke(
                    new RecognitionResult(
                        "",
                        0f
                    )
                );

                yield break;
            }


            string responseText =
                webRequest.downloadHandler.text;


            ApiResponse apiResponse =
                JsonUtility.FromJson<ApiResponse>(
                    responseText
                );


            RecognitionResult result =
                new RecognitionResult(
                    apiResponse.predictedLabel,
                    apiResponse.confidence
                );


            Debug.Log(
                "YAREN MODEL → Model: " +
                request.modelType +
                " | Tahmin: " +
                result.predictedLabel +
                " | Skor: %" +
                Mathf.RoundToInt(
                    result.confidence * 100
                )
            );


            onResult?.Invoke(
                result
            );
        }
    }
}