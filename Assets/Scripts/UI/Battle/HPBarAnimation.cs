using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using WeaveSign.UI.Common;

namespace WeaveSign.UI.Battle
{
    /// <summary>
    /// Can barlarının (Player1HPBar, Player2HPBar) anlık zıplamak yerine
    /// RPG ve kart oyunlarındaki gibi pürüzsüzce akmasını (smooth lerp) sağlar.
    /// Hasar alındığında isteğe bağlı olarak can barını sarsar.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class HPBarAnimation : MonoBehaviour
    {
        [Header("Animasyon Ayarları")]
        [SerializeField] private float smoothSpeed = 3.5f;
        [SerializeField] private bool triggerShakeOnDamage = true;
        [SerializeField] private float shakeMagnitude = 8f;

        private Slider slider;
        private float displayedValue;
        private float targetValue;
        private UIShake uiShake;

        private bool isInitialized = false;

        private void Awake()
        {
            slider = GetComponent<Slider>();
            uiShake = GetComponent<UIShake>();
            InitializeBar();
        }

        private void OnEnable()
        {
            InitializeBar();
        }

        private void InitializeBar()
        {
            if (slider == null) return;
            
            // Eğer değer 0 veya tanımsızsa doğrudan max can değerine ayarla
            float initialVal = slider.value > 0 ? slider.value : slider.maxValue;
            if (initialVal <= 0) initialVal = 100f;

            displayedValue = initialVal;
            targetValue = initialVal;
            slider.SetValueWithoutNotify(initialVal);
            isInitialized = true;
        }

        private void Update()
        {
            if (slider == null) return;

            if (!isInitialized)
            {
                InitializeBar();
                return;
            }

            // BattleManager veya oyun kodu slider.value değerini doğrudan güncellediğinde
            if (!Mathf.Approximately(slider.value, displayedValue) && !Mathf.Approximately(slider.value, targetValue))
            {
                float newValue = slider.value;

                // Hasar alındıysa (can azaldıysa) sarsıntı tetikle
                if (newValue < targetValue && triggerShakeOnDamage && uiShake != null)
                {
                    uiShake.TriggerShake(0.25f, shakeMagnitude);
                }

                targetValue = newValue;
                // Slider'ı anlık olarak eski gösterilen değerde tut, Lerp ile indireceğiz
                slider.SetValueWithoutNotify(displayedValue);
            }

            // Gösterilen değeri hedefe doğru pürüzsüzce kaydır
            if (!Mathf.Approximately(displayedValue, targetValue))
            {
                displayedValue = Mathf.MoveTowards(displayedValue, targetValue, Time.unscaledDeltaTime * (Mathf.Abs(targetValue - displayedValue) * smoothSpeed + 20f));
                slider.SetValueWithoutNotify(displayedValue);
            }
        }
    }
}
