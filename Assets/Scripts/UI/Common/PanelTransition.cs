using System;
using System.Collections;
using UnityEngine;

namespace WeaveSign.UI.Common
{
    /// <summary>
    /// Panellerin (Settings, Modallar, Popup'lar) yumuşakça açılıp kapanmasını sağlar.
    /// CanvasGroup alpha ve ölçek animasyonunu birlikte yönetir.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PanelTransition : MonoBehaviour
    {
        [Header("Animasyon Ayarları")]
        [SerializeField] private float duration = 0.22f;
        [SerializeField] private bool useScalePop = true;
        [SerializeField] private float startScale = 0.90f;
        [SerializeField] private bool playOnAwake = false;
        [SerializeField] private bool deactivateOnHide = true;

        private CanvasGroup canvasGroup;
        private Coroutine activeTransition;
        private Vector3 initialScale;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            initialScale = transform.localScale;

            if (playOnAwake)
            {
                Show();
            }
        }

        public void Show(Action onComplete = null)
        {
            gameObject.SetActive(true);
            if (activeTransition != null) StopCoroutine(activeTransition);
            activeTransition = StartCoroutine(TransitionRoutine(0f, 1f, startScale, 1f, true, onComplete));
        }

        public void Hide(Action onComplete = null)
        {
            if (!gameObject.activeInHierarchy)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                if (deactivateOnHide) gameObject.SetActive(false);
                onComplete?.Invoke();
                return;
            }

            if (activeTransition != null) StopCoroutine(activeTransition);
            activeTransition = StartCoroutine(TransitionRoutine(canvasGroup.alpha, 0f, 1f, startScale, false, () =>
            {
                if (deactivateOnHide) gameObject.SetActive(false);
                onComplete?.Invoke();
            }));
        }

        private IEnumerator TransitionRoutine(float fromAlpha, float toAlpha, float fromScaleMultiplier, float toScaleMultiplier, bool targetInteractive, Action onComplete)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = targetInteractive; // Açılırken hemen etkileşimi engelle veya izin ver

            float elapsed = 0f;
            Vector3 sStart = initialScale * fromScaleMultiplier;
            Vector3 sTarget = initialScale * toScaleMultiplier;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Yumuşak yaylanmalı Ease-Out
                float smoothT = Mathf.Sin(t * Mathf.PI * 0.5f);

                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, smoothT);
                if (useScalePop)
                {
                    transform.localScale = Vector3.LerpUnclamped(sStart, sTarget, smoothT);
                }

                yield return null;
            }

            canvasGroup.alpha = toAlpha;
            if (useScalePop) transform.localScale = sTarget;

            canvasGroup.interactable = targetInteractive;
            canvasGroup.blocksRaycasts = targetInteractive;

            activeTransition = null;
            onComplete?.Invoke();
        }
    }
}
