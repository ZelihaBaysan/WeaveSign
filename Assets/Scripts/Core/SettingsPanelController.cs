using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Settings UI")]
    public GameObject settingsPanel;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";

    void Start()
    {
        // Settings ekranı tamamen opak olsun.
        Image panelImage =
            settingsPanel.GetComponent<Image>();

        if (panelImage != null)
        {
            Color color = panelImage.color;
            color.a = 1f;
            panelImage.color = color;
        }

        float savedMasterVolume;
        float savedMusicVolume;

        if (AudioManager.Instance != null)
        {
            savedMasterVolume =
                AudioManager.Instance.GetMasterVolume();

            savedMusicVolume =
                AudioManager.Instance.GetMusicVolume();
        }
        else
        {
            savedMasterVolume =
                PlayerPrefs.GetFloat(
                    MasterVolumeKey,
                    1f
                );

            savedMusicVolume =
                PlayerPrefs.GetFloat(
                    MusicVolumeKey,
                    1f
                );
        }

        masterVolumeSlider.value =
            savedMasterVolume;

        musicVolumeSlider.value =
            savedMusicVolume;

        masterVolumeSlider.onValueChanged.AddListener(
            SetMasterVolume
        );

        musicVolumeSlider.onValueChanged.AddListener(
            SetMusicVolume
        );

        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);

        transform.SetAsLastSibling();
        settingsPanel.transform.SetAsLastSibling();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void SetMasterVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(
                value
            );
        }
        else
        {
            AudioListener.volume =
                value;

            PlayerPrefs.SetFloat(
                MasterVolumeKey,
                value
            );

            PlayerPrefs.Save();
        }
    }

    public void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(
                value
            );
        }
        else
        {
            PlayerPrefs.SetFloat(
                MusicVolumeKey,
                value
            );

            PlayerPrefs.Save();
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(
            "MainMenu"
        );
    }

    public void QuitGame()
    {
        Debug.Log(
            "Oyundan çıkılıyor..."
        );

        Application.Quit();
    }
}