using System;
using UnityEngine;
using UnityEngine.UI;

public class WebcamCapture : MonoBehaviour
{
    [SerializeField]
    private RawImage preview;

    // Kamera sahne deðiþse bile yaþamaya devam edecek.
    private static WebCamTexture sharedWebcamTexture;


    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetStaticCamera()
    {
        if (sharedWebcamTexture != null)
        {
            if (sharedWebcamTexture.isPlaying)
            {
                sharedWebcamTexture.Stop();
            }

            sharedWebcamTexture = null;
        }
    }


    private void Start()
    {
        StartCamera();
    }


    public void StartCamera()
    {
        // Daha önce kamera oluþturulmadýysa oluþtur.
        if (sharedWebcamTexture == null)
        {
            WebCamDevice[] devices =
                WebCamTexture.devices;

            if (devices.Length == 0)
            {
                Debug.LogError(
                    "Kamera bulunamadi."
                );

                return;
            }

            sharedWebcamTexture =
                new WebCamTexture(
                    devices[0].name,
                    640,
                    480,
                    30
                );

            sharedWebcamTexture.Play();

            Debug.Log(
                "Kamera baslatildi: " +
                devices[0].name
            );
        }
        else
        {
            // Sahne deðiþiminden sonra kamera durmuþsa
            // tekrar baþlat.
            if (!sharedWebcamTexture.isPlaying)
            {
                sharedWebcamTexture.Play();

                Debug.Log(
                    "Mevcut kamera tekrar baslatildi."
                );
            }
        }


        // Her yeni sahnede o sahnenin CameraPreview'una
        // kamerayý yeniden baðla.
        BindPreview();
    }


    private void BindPreview()
    {
        if (preview == null)
        {
            Debug.LogWarning(
                "CameraPreview baglanmamis."
            );

            return;
        }

        preview.texture =
            sharedWebcamTexture;

        preview.enabled = true;

        Debug.Log(
            "Kamera yeni CameraPreview nesnesine baglandi."
        );
    }


    public bool IsCameraReady()
    {
        return
            sharedWebcamTexture != null &&
            sharedWebcamTexture.isPlaying &&
            sharedWebcamTexture.width > 16 &&
            sharedWebcamTexture.height > 16;
    }


    public string GetFrameBase64()
    {
        if (
            sharedWebcamTexture == null ||
            !sharedWebcamTexture.isPlaying
        )
        {
            StartCamera();
        }


        if (!IsCameraReady())
        {
            Debug.LogWarning(
                "Kamera henuz hazir degil."
            );

            return null;
        }


        Texture2D frame =
            new Texture2D(
                sharedWebcamTexture.width,
                sharedWebcamTexture.height,
                TextureFormat.RGB24,
                false
            );


        frame.SetPixels(
            sharedWebcamTexture.GetPixels()
        );

        frame.Apply();


        byte[] jpgBytes =
            frame.EncodeToJPG(75);


        Destroy(frame);


        return Convert.ToBase64String(
            jpgBytes
        );
    }


    public WebCamTexture GetTexture()
    {
        return sharedWebcamTexture;
    }


    // DÝKKAT:
    // Artýk OnDestroy içinde kamerayý durdurmuyoruz.
    // Çünkü sahne deðiþirken bu obje yok oluyor,
    // ama kamerayý sonraki sahnede kullanmaya devam edeceðiz.


    private void OnApplicationQuit()
    {
        if (
            sharedWebcamTexture != null &&
            sharedWebcamTexture.isPlaying
        )
        {
            sharedWebcamTexture.Stop();
        }

        sharedWebcamTexture = null;
    }
}