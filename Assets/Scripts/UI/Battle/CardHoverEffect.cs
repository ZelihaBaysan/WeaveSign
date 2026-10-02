using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WeaveSign.UI.Battle
{
    /// <summary>
    /// Düello modundaki kartların üzerine gelindiğinde (Hover) kartı pürüzsüzce büyütür
    /// ve diğer kartların önüne çıkarır.
    /// LayoutGroup yapısını ASLA bozmaz (SetAsLastSibling çağırmaz, X pozisyonunu asla değiştirmez, kaçma/titreme yapmaz).
    /// </summary>
    public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hover Animasyonu")]
        [SerializeField] private float hoverScale = 1.10f;
        [SerializeField] private float liftDistance = 24f;
        [SerializeField] private float duration = 0.12f;

        private RectTransform rectTransform;
        private Vector3 baseScale = Vector3.one;
        private float baseLocalY = 0f;
        private Coroutine activeRoutine;
        private Canvas cardCanvas;
        private GraphicRaycaster raycaster;
        private bool isHovered = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            baseScale = rectTransform.localScale;

            // Kartın diğer kartların önüne geçebilmesi için Canvas ekle/kullan
            cardCanvas = GetComponent<Canvas>();
            if (cardCanvas == null)
            {
                cardCanvas = gameObject.AddComponent<Canvas>();
            }
            raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
            cardCanvas.overrideSorting = false;
        }

        private void OnEnable()
        {
            rectTransform.localScale = baseScale;
            if (cardCanvas != null)
            {
                cardCanvas.overrideSorting = false;
            }
            isHovered = false;
        }

        private void OnDisable()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }
            rectTransform.localScale = baseScale;
            if (cardCanvas != null)
            {
                cardCanvas.overrideSorting = false;
            }
            isHovered = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isHovered) return;
            if (IsModalOrOverlayActive()) return;
            isHovered = true;

            // Sibling değiştirmek yerine Canvas sortingOrder ile öne çıkar (Overlay/Settings 200'ün altında kalacak şekilde 10)
            if (cardCanvas != null)
            {
                cardCanvas.overrideSorting = true;
                cardCanvas.sortingOrder = 10;
            }

            // O anki Y pozisyonunu baz al (asla X'e dokunma!)
            baseLocalY = rectTransform.localPosition.y;

            StartAnimation(1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isHovered) return;
            isHovered = false;

            StartAnimation(0f, () =>
            {
                if (!isHovered && cardCanvas != null)
                {
                    cardCanvas.overrideSorting = false;
                }
            });
        }

        private void Update()
        {
            if (isHovered && IsModalOrOverlayActive())
            {
                ForceReset();
            }
        }

        public void ForceReset()
        {
            if (!isHovered) return;
            isHovered = false;

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            rectTransform.localScale = baseScale;
            Vector3 currentPos = rectTransform.localPosition;
            currentPos.y = baseLocalY;
            rectTransform.localPosition = currentPos;

            if (cardCanvas != null)
            {
                cardCanvas.overrideSorting = false;
            }
        }

        private bool IsModalOrOverlayActive()
        {
            SettingsPanelController settings = FindAnyObjectByType<SettingsPanelController>();
            if (settings != null && settings.settingsPanel != null && settings.settingsPanel.activeInHierarchy)
            {
                return true;
            }
            return false;
        }

        private void StartAnimation(float targetProgress, System.Action onComplete = null)
        {
            if (!gameObject.activeInHierarchy) return;
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(AnimateRoutine(targetProgress, onComplete));
        }

        private IEnumerator AnimateRoutine(float targetProgress, System.Action onComplete)
        {
            Vector3 startScale = rectTransform.localScale;
            Vector3 targetScale = Vector3.Lerp(baseScale, baseScale * hoverScale, targetProgress);

            float startY = rectTransform.localPosition.y;
            float targetY = baseLocalY + (liftDistance * targetProgress);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                rectTransform.localScale = Vector3.LerpUnclamped(startScale, targetScale, smoothT);

                // SADECE Y eksenini güncelle, X ve Z'ye ASLA DOKUNMA!
                Vector3 currentPos = rectTransform.localPosition;
                currentPos.y = Mathf.LerpUnclamped(startY, targetY, smoothT);
                rectTransform.localPosition = currentPos;

                yield return null;
            }

            rectTransform.localScale = targetScale;
            Vector3 finalPos = rectTransform.localPosition;
            finalPos.y = targetY;
            rectTransform.localPosition = finalPos;

            activeRoutine = null;
            onComplete?.Invoke();
        }
    }
}
