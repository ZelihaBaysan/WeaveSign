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


    // GRU kelime modeli için
    private const int WORD_SEQUENCE_LENGTH = 30;


    [Serializable]
    private class ApiRequest
    {
        public string modelType;
        public string expectedLabel;

        // Harf modeli için
        public string imageBase64;

        // GRU kelime modeli için
        public string[] framesBase64;
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
        // =================================================
        // KAMERA KONTROLÜ
        // =================================================

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


        // =================================================
        // GERİ SAYIM
        // =================================================

        SetCountdownText("3");
        yield return new WaitForSeconds(1f);

        SetCountdownText("2");
        yield return new WaitForSeconds(1f);

        SetCountdownText("1");
        yield return new WaitForSeconds(1f);

        SetCountdownText("İŞARETİ YAP!");


        // =================================================
        // API REQUEST
        // =================================================

        ApiRequest apiRequest =
            new ApiRequest();

        apiRequest.modelType =
            request.modelType.ToString();

        apiRequest.expectedLabel =
            request.expectedLabel;


        // =================================================
        // KELİME -> 30 KARE
        // =================================================

        if (
            request.modelType ==
            RecognitionModelType.Word
        )
        {
            Debug.Log(
                "GRU kelime modeli icin "
                + WORD_SEQUENCE_LENGTH
                + " kare toplanacak."
            );


            string[] frames =
                new string[
                    WORD_SEQUENCE_LENGTH
                ];


            int collectedFrames = 0;
            int attempts = 0;

            // Kamera bazen bir kareyi boş verebilir.
            // Bu yüzden 30 kare için en fazla 60 deneme.
            int maxAttempts =
                WORD_SEQUENCE_LENGTH * 2;


            while (
                collectedFrames <
                WORD_SEQUENCE_LENGTH
                &&
                attempts < maxAttempts
            )
            {
                attempts++;


                string frameBase64 =
                    webcamCapture
                        .GetFrameBase64();


                if (
                    !string.IsNullOrEmpty(
                        frameBase64
                    )
                )
                {
                    frames[
                        collectedFrames
                    ] = frameBase64;

                    collectedFrames++;
                }


                // Yaklaşık 30 FPS
                yield return new WaitForSeconds(
                    0.033f
                );
            }


            if (
                collectedFrames !=
                WORD_SEQUENCE_LENGTH
            )
            {
                SetCountdownText("");

                Debug.LogError(
                    "30 kare toplanamadi. "
                    + "Toplanan: "
                    + collectedFrames
                );

                onResult?.Invoke(
                    new RecognitionResult(
                        "",
                        0f
                    )
                );

                yield break;
            }


            apiRequest.framesBase64 =
                frames;

            apiRequest.imageBase64 =
                null;


            Debug.Log(
                "30 kare basariyla toplandi."
            );
        }


        // =================================================
        // HARF -> TEK KARE
        // =================================================

        else
        {
            // İşareti yapması için kısa süre
            yield return new WaitForSeconds(
                1.5f
            );


            string imageBase64 =
                webcamCapture
                    .GetFrameBase64();


            if (
                string.IsNullOrEmpty(
                    imageBase64
                )
            )
            {
                SetCountdownText("");

                Debug.LogError(
                    "Kameradan goruntu "
                    + "alinamadi."
                );

                onResult?.Invoke(
                    new RecognitionResult(
                        "",
                        0f
                    )
                );

                yield break;
            }


            apiRequest.imageBase64 =
                imageBase64;

            apiRequest.framesBase64 =
                null;
        }


        // =================================================
        // API'YE GÖNDER
        // =================================================

        SetCountdownText(
            "TAHMİN EDİLİYOR..."
        );


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


            yield return
                webRequest.SendWebRequest();


            // =================================================
            // API HATA KONTROLÜ
            // =================================================

            if (
                webRequest.result !=
                UnityWebRequest.Result.Success
            )
            {
                SetCountdownText("");

                Debug.LogError(
                    "API hatasi: "
                    + webRequest.error
                );

                Debug.LogError(
                    "API cevabi: "
                    + webRequest
                        .downloadHandler
                        .text
                );

                onResult?.Invoke(
                    new RecognitionResult(
                        "",
                        0f
                    )
                );

                yield break;
            }


            // =================================================
            // API CEVABI
            // =================================================

            ApiResponse response =
                JsonUtility
                    .FromJson<ApiResponse>(
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
                    new RecognitionResult(
                        "",
                        0f
                    )
                );

                yield break;
            }


            RecognitionResult result =
                new RecognitionResult(
                    response.predictedLabel,
                    response.confidence
                );


            Debug.Log(
                "YAREN MODEL -> Model: "
                + request.modelType
                + " | Tahmin: "
                + result.predictedLabel
                + " | Skor: %"
                + Mathf.RoundToInt(
                    result.confidence
                    * 100f
                )
            );


            SetCountdownText("");


            onResult?.Invoke(
                result
            );
        }
    }


    private void SetCountdownText(
        string message
    )
    {
        if (countdownText != null)
        {
            countdownText.text =
                message;
        }

        Debug.Log(
            message
        );
    }
}