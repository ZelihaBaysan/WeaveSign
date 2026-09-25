using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip battleMusic;

    [Header("SFX")]
    public AudioClip buttonClickSFX;
    public AudioClip cardSelectSFX;
    public AudioClip attackSFX;
    public AudioClip healSFX;
    public AudioClip successSFX;
    public AudioClip mistakeSFX;
    public AudioClip victorySFX;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        LoadVolumeSettings();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Start()
    {
        HandleSceneMusic(
            SceneManager.GetActiveScene().name
        );
    }

    void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        HandleSceneMusic(scene.name);
    }

    void HandleSceneMusic(string sceneName)
    {
        PlayMusic(battleMusic);
    }

    void LoadVolumeSettings()
    {
        float masterVolume =
            PlayerPrefs.GetFloat(
                MasterVolumeKey,
                1f
            );

        float musicVolume =
            PlayerPrefs.GetFloat(
                MusicVolumeKey,
                1f
            );

        AudioListener.volume =
            masterVolume;

        if (musicSource != null)
        {
            musicSource.volume =
                musicVolume;
        }
    }

    public void SetMasterVolume(float value)
    {
        AudioListener.volume =
            value;

        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            value
        );

        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        if (musicSource != null)
        {
            musicSource.volume =
                value;
        }

        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            value
        );

        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (
            musicSource == null ||
            clip == null
        )
        {
            return;
        }

        if (
            musicSource.clip == clip &&
            musicSource.isPlaying
        )
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (
            sfxSource == null ||
            clip == null
        )
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSFX);
    }

    public void PlayCardSelect()
    {
        PlaySFX(cardSelectSFX);
    }

    public void PlayAttack()
    {
        PlaySFX(attackSFX);
    }

    public void PlayHeal()
    {
        PlaySFX(healSFX);
    }

    public void PlaySuccess()
    {
        PlaySFX(successSFX);
    }

    public void PlayMistake()
    {
        PlaySFX(mistakeSFX);
    }

    public void PlayVictory()
    {
        PlaySFX(victorySFX);
    }

    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(
            MasterVolumeKey,
            1f
        );
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(
            MusicVolumeKey,
            1f
        );
    }
}