using System.Collections;
using UnityEngine;

namespace WeaveSign.UI.Common
{
    /// <summary>
    /// UI elemanlarını (can barları, paneller, kartlar, uyarılar) hatalı hareket veya hasar durumunda titretir.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIShake : MonoBehaviour
    {
        [Header("Varsayılan Titreme Ayarları")]
        [SerializeField] private float defaultDuration = 0.35f;
        [SerializeField] private float defaultMagnitude = 12f;
        [SerializeField] private float frequency = 30f;

        private RectTransform rectTransform;
        private Vector2 originalPosition;
        private Coroutine activeShake;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalPosition = rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            rectTransform.anchoredPosition = originalPosition;
        }

        private void OnDisable()
        {
            if (activeShake != null)
            {
                StopCoroutine(activeShake);
                activeShake = null;
            }
            rectTransform.anchoredPosition = originalPosition;
        }

        public void TriggerShake()
        {
            TriggerShake(defaultDuration, defaultMagnitude);
        }

        public void TriggerShake(float duration, float magnitude)
        {
            if (!gameObject.activeInHierarchy) return;
            if (activeShake != null) StopCoroutine(activeShake);
            activeShake = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                // Zamanla azalan sönümleme (damping)
                float damper = 1f - Mathf.Clamp01(progress);

                float offsetX = (Mathf.PerlinNoise(Time.time * frequency, 0f) * 2f - 1f) * magnitude * damper;
                float offsetY = (Mathf.PerlinNoise(0f, Time.time * frequency) * 2f - 1f) * magnitude * damper;

                rectTransform.anchoredPosition = originalPosition + new Vector2(offsetX, offsetY);
                yield return null;
            }

            rectTransform.anchoredPosition = originalPosition;
            activeShake = null;
        }
    }
}
