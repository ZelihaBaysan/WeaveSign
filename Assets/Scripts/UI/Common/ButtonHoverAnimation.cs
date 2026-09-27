using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WeaveSign.UI.Common
{
    /// <summary>
    /// Butonlara modern, pürüzsüz hover (üzerine gelme) ve click (basılma) animasyonu katar.
    /// Tamamen bağımsızdır; DOTween vb. gerektirmez.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Ölçek Ayarları")]
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float pressedScale = 0.94f;
        [SerializeField] private float animationDuration = 0.12f;

        [Header("İsteğe Bağlı Parlama / Renk")]
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private bool useColorTint = false;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(1.15f, 1.15f, 1.15f, 1f);

        private Vector3 originalScale;
        private Coroutine activeScaleRoutine;
        private Coroutine activeColorRoutine;
        private bool isPointerOver = false;

        private void Awake()
        {
            originalScale = transform.localScale;
            if (targetGraphic == null)
            {
                targetGraphic = GetComponent<Graphic>();
            }
        }

        private void OnEnable()
        {
            transform.localScale = originalScale;
            isPointerOver = false;
        }

        private void OnDisable()
        {
            if (activeScaleRoutine != null)
            {
                StopCoroutine(activeScaleRoutine);
                activeScaleRoutine = null;
            }
            if (activeColorRoutine != null)
            {
                StopCoroutine(activeColorRoutine);
                activeColorRoutine = null;
            }
            transform.localScale = originalScale;
            if (useColorTint && targetGraphic != null)
            {
                targetGraphic.color = normalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerOver = true;
            AnimateScale(originalScale * hoverScale);
            if (useColorTint && targetGraphic != null)
            {
                AnimateColor(hoverColor);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;
            AnimateScale(originalScale);
            if (useColorTint && targetGraphic != null)
            {
                AnimateColor(normalColor);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateScale(originalScale * pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Vector3 target = isPointerOver ? originalScale * hoverScale : originalScale;
            AnimateScale(target);
        }

        private void AnimateScale(Vector3 targetScale)
        {
            if (!gameObject.activeInHierarchy) return;
            if (activeScaleRoutine != null) StopCoroutine(activeScaleRoutine);
            activeScaleRoutine = StartCoroutine(ScaleRoutine(targetScale));
        }

        private void AnimateColor(Color targetColor)
        {
            if (!gameObject.activeInHierarchy || targetGraphic == null) return;
            if (activeColorRoutine != null) StopCoroutine(activeColorRoutine);
            activeColorRoutine = StartCoroutine(ColorRoutine(targetColor));
        }

        private IEnumerator ScaleRoutine(Vector3 target)
        {
            Vector3 start = transform.localScale;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                // Yumuşak SmoothStep eğrisi
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                transform.localScale = Vector3.LerpUnclamped(start, target, smoothT);
                yield return null;
            }

            transform.localScale = target;
            activeScaleRoutine = null;
        }

        private IEnumerator ColorRoutine(Color target)
        {
            Color start = targetGraphic.color;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                targetGraphic.color = Color.Lerp(start, target, t);
                yield return null;
            }

            targetGraphic.color = target;
            activeColorRoutine = null;
        }
    }
}
