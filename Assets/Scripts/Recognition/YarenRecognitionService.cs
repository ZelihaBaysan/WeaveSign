using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class YarenRecognitionService : RecognitionServiceBase
{
    [SerializeField]
    private string apiUrl =
        "http://127.0.0.1:8000/recognize";

    [SerializeField]
    private WebcamCapture webcamCapture;

    [Header("Countdown")]
    [SerializeField]
    private TMP_Text countdownText;


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
            Debug.LogError(
                "WebcamCapture baglanmamis."
            );

            onResult?.Invoke(
                new RecognitionResult("", 0f)
            );

            yield break;
        }


        webcamCapture.StartCamera();


        // Kamera hazır olana kadar bekle
        float timeout = 3f;
        float elapsed = 0f;

        while (
            !webcamCapture.IsCameraReady()
            && elapsed < timeout
        )
        {
            elapsed += Time.deltaTime;
            yield return null;
        }


        if (!webcamCapture.IsCameraReady())
        {
            Debug.LogError(
                "Kamera zamaninda hazir olmadi."
            );

            onResult?.Invoke(
                new RecognitionResult("", 0f)
            );

            yield break;
        }


        // ----------------------------
        // GERİ SAYIM
        // ----------------------------

        SetCountdownText("3");
        yield return new WaitForSeconds(1f);

        SetCountdownText("2");
        yield return new WaitForSeconds(1f);

        SetCountdownText("1");
        yield return new WaitForSeconds(1f);

        SetCountdownText("İŞARETİ YAP!");
        yield return new WaitForSeconds(1f);


        // Geri sayımdan sonra görüntüyü al
        string imageBase64 =
            webcamCapture.GetFrameBase64();


        if (string.IsNullOrEmpty(imageBase64))
        {
            SetCountdownText("");

            Debug.LogError(
                "Kameradan goruntu alinamadi."
            );

            onResult?.Invoke(
                new RecognitionResult("", 0f)
            );

            yield break;
        }


        SetCountdownText(
            "TAHMİN EDİLİYOR..."
        );


        ApiRequest apiRequest =
            new ApiRequest();

        apiRequest.modelType =
            request.modelType.ToString();

        apiRequest.expectedLabel =
            request.expectedLabel;

        apiRequest.imageBase64 =
            imageBase64;


        string json =
            JsonUtility.ToJson(apiRequest);


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
                new UploadHandlerRaw(bodyRaw);

            webRequest.downloadHandler =
                new DownloadHandlerBuffer();

            webRequest.SetRequestHeader(
                "Content-Type",
                "application/json"
            );


            yield return
                webRequest.SendWebRequest();


            if (
                webRequest.result !=
                UnityWebRequest.Result.Success
            )
            {
                SetCountdownText("");

                Debug.LogError(
                    "API hatasi: " +
                    webRequest.error
                );

                onResult?.Invoke(
                    new RecognitionResult("", 0f)
                );

                yield break;
            }


            ApiResponse response =
                JsonUtility.FromJson<ApiResponse>(
                    webRequest
                        .downloadHandler
                        .text
                );


            if (response == null)
            {
                SetCountdownText("");

                Debug.LogError(
                    "API cevabi okunamadi."
                );

                onResult?.Invoke(
                    new RecognitionResult("", 0f)
                );

                yield break;
            }


            RecognitionResult result =
                new RecognitionResult(
                    response.predictedLabel,
                    response.confidence
                );


            Debug.Log(
                "YAREN MODEL -> Model: " +
                request.modelType +
                " | Tahmin: " +
                result.predictedLabel +
                " | Skor: %" +
                Mathf.RoundToInt(
                    result.confidence * 100f
                )
            );


            // Controller kendi sonucu yazacağı için
            // geri sayım metnini temizliyoruz.
            SetCountdownText("");


            onResult?.Invoke(result);
        }
    }


    private void SetCountdownText(
        string message
    )
    {
        if (countdownText != null)
        {
            countdownText.text = message;
        }

        Debug.Log(message);
    }
}