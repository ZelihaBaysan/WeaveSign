using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WeaveSign.UI.Common
{
    /// <summary>
    /// Ekran kararma (Fade In / Fade Out) efektini yönetir.
    /// Sahne geçişlerinde ve tur başlangıç/bitişlerinde pürüzsüz sinematik his verir.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ScreenFade : MonoBehaviour
    {
        [SerializeField] private float defaultDuration = 0.4f;
        [SerializeField] private bool fadeInOnStart = true;

        private Image fadeImage;
        private Coroutine activeFade;

        private void Awake()
        {
            fadeImage = GetComponent<Image>();
            // Dokunmaları geçirmesi veya engellemesi durumunu kontrol eder
            fadeImage.raycastTarget = false;
        }

        private void Start()
        {
            if (fadeInOnStart)
            {
                FadeIn();
            }
        }

        public void FadeIn(Action onComplete = null)
        {
            Fade(1f, 0f, defaultDuration, false, onComplete);
        }

        public void FadeOut(Action onComplete = null)
        {
            Fade(0f, 1f, defaultDuration, true, onComplete);
        }

        public void Fade(float fromAlpha, float toAlpha, float duration, bool blockRaycasts, Action onComplete = null)
        {
            if (!gameObject.activeInHierarchy) return;
            if (activeFade != null) StopCoroutine(activeFade);
            fadeImage.raycastTarget = blockRaycasts;
            activeFade = StartCoroutine(FadeRoutine(fromAlpha, toAlpha, duration, onComplete));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Color c = fadeImage.color;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                c.a = Mathf.Lerp(from, to, t);
                fadeImage.color = c;
                yield return null;
            }

            c.a = to;
            fadeImage.color = c;
            activeFade = null;
            onComplete?.Invoke();
        }
    }
}
