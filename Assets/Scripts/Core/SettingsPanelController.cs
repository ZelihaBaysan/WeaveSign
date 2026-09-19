using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    public GameObject settingsPanel;

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";

    void Start()
    {
        // Ayarlar ekranını tamamen opak yap.
        Image panelImage =
            settingsPanel.GetComponent<Image>();

        if (panelImage != null)
        {
            Color color = panelImage.color;
            color.a = 1f;
            panelImage.color = color;
        }

        float savedMasterVolume =
            PlayerPrefs.GetFloat(
                MasterVolumeKey,
                1f
            );

        float savedMusicVolume =
            PlayerPrefs.GetFloat(
                MusicVolumeKey,
                1f
            );

        masterVolumeSlider.value =
            savedMasterVolume;

        musicVolumeSlider.value =
            savedMusicVolume;

        AudioListener.volume =
            savedMasterVolume;

        masterVolumeSlider
            .onValueChanged
            .AddListener(SetMasterVolume);

        musicVolumeSlider
            .onValueChanged
            .AddListener(SetMusicVolume);

        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);

        // SettingsUI'yi diğer UI'ların önüne getir.
        transform.SetAsLastSibling();

        // Paneli de en öne getir.
        settingsPanel.transform.SetAsLastSibling();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;

        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            value
        );

        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            value
        );

        PlayerPrefs.Save();
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