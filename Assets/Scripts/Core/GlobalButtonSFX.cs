using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalButtonSFX : MonoBehaviour
{
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCoroutine(
            BindButtonsAfterLoad()
        );
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        StartCoroutine(
            BindButtonsAfterLoad()
        );
    }

    IEnumerator BindButtonsAfterLoad()
    {
        // Dinamik butonların oluşmasını bekle.
        yield return null;
        yield return null;

        BindAllButtons();
    }

    void BindAllButtons()
    {
        Button[] buttons =
            Object.FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            // Düello kartlarında zaten
            // CardSelect sesi var.
            if (
                button.GetComponentInParent<CardView>() != null
            )
            {
                continue;
            }

            // Aynı listener tekrar eklenmesin.
            button.onClick.RemoveListener(
                PlayButtonClick
            );

            button.onClick.AddListener(
                PlayButtonClick
            );
        }

        Debug.Log(
            "Global button SFX bağlandı. Buton sayısı: " +
            buttons.Length
        );
    }

    void PlayButtonClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}