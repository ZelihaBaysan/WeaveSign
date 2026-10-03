using UnityEngine;
using UnityEngine.UI;

public class WebcamCapture : MonoBehaviour
{
    [SerializeField]
    private RawImage preview;

    private WebCamTexture webcamTexture;


    private void Start()
    {
        StartCamera();
    }


    public void StartCamera()
    {
        if (webcamTexture != null)
        {
            if (!webcamTexture.isPlaying)
            {
                webcamTexture.Play();
            }

            if (preview != null)
            {
                preview.texture = webcamTexture;
            }

            return;
        }

        webcamTexture = new WebCamTexture();

        webcamTexture.Play();

        if (preview != null)
        {
            preview.texture = webcamTexture;
        }
    }


    public void StopCamera()
    {
        if (
            webcamTexture != null &&
            webcamTexture.isPlaying
        )
        {
            webcamTexture.Stop();
        }
    }


    public string GetFrameBase64()
    {
        if (
            webcamTexture == null ||
            !webcamTexture.isPlaying ||
            webcamTexture.width <= 16
        )
        {
            return null;
        }

        Texture2D frame =
            new Texture2D(
                webcamTexture.width,
                webcamTexture.height,
                TextureFormat.RGB24,
                false
            );

        frame.SetPixels(
            webcamTexture.GetPixels()
        );

        frame.Apply();

        byte[] jpgBytes =
            frame.EncodeToJPG(75);

        Destroy(frame);

        return System.Convert.ToBase64String(
            jpgBytes
        );
    }


    public WebCamTexture GetTexture()
    {
        return webcamTexture;
    }


    private void OnDestroy()
    {
        StopCamera();
    }
}