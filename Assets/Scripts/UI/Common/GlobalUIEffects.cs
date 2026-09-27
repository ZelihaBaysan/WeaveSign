using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WeaveSign.UI.Battle;

namespace WeaveSign.UI.Common
{
    /// <summary>
    /// Tüm sahnelerdeki butonlara otomatik olarak ButtonHoverAnimation ve
    /// düello sahnesindeki kartlara CardHoverEffect ekleyerek oyunun tamamına
    /// tek dokunuşla canlı, modern bir kart oyunu hissi kazandırır.
    /// </summary>
    public class GlobalUIEffects : MonoBehaviour
    {
        private static GlobalUIEffects instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (instance == null)
            {
                GameObject host = new GameObject("[GlobalUIEffects]");
                DontDestroyOnLoad(host);
                instance = host.AddComponent<GlobalUIEffects>();
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(BindEffectsDelayed());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(BindEffectsDelayed());
        }

        private IEnumerator BindEffectsDelayed()
        {
            // Dinamik instantiate edilen elemanların (ör. eldeki kartlar) yerleşmesini bekle
            yield return null;
            yield return null;
            ApplyEffects();
        }

        public void ApplyEffects()
        {
            // 1. Tüm butonlara ButtonHoverAnimation ekle (eğer yoksa)
            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
            foreach (Button btn in buttons)
            {
                if (btn == null) continue;

                // Kartın kendisinde zaten CardHoverEffect olacağı için kart içi butonları hariç tutabiliriz
                if (btn.GetComponentInParent<CardHoverEffect>() != null) continue;

                if (btn.GetComponent<ButtonHoverAnimation>() == null)
                {
                    btn.gameObject.AddComponent<ButtonHoverAnimation>();
                }
            }

            // 2. Düello kartlarına CardHoverEffect ekle (CardArea veya konteynerleri ASLA dahil etme!)
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            foreach (GameObject go in allObjects)
            {
                if (go == null) continue;

                // CardArea, CardContainer veya benzeri taşıyıcı panelleri kesinlikle atla!
                if (go.name.Contains("Area") || go.name.Contains("Container") || go.name.Contains("Layout") || go.name.Contains("Panel"))
                {
                    continue;
                }

                // Yalnızca gerçek CardVisual nesnelerine uygula
                if (go.name.Contains("CardVisual") && go.GetComponent<CardHoverEffect>() == null)
                {
                    if (go.GetComponent<RectTransform>() != null && go.GetComponent<Image>() != null)
                    {
                        go.AddComponent<CardHoverEffect>();
                    }
                }
            }

            // 3. Can barlarına (Slider) pürüzsüz HPBarAnimation ve UIShake ekle
            Slider[] sliders = Object.FindObjectsByType<Slider>(FindObjectsInactive.Include);
            foreach (Slider sld in sliders)
            {
                if (sld == null) continue;
                string sName = sld.name.ToLower();
                if (sName.Contains("hp") || sName.Contains("health"))
                {
                    if (sld.GetComponent<UIShake>() == null)
                    {
                        sld.gameObject.AddComponent<UIShake>();
                    }
                    if (sld.GetComponent<HPBarAnimation>() == null)
                    {
                        sld.gameObject.AddComponent<HPBarAnimation>();
                    }
                }
            }
        }
    }
}
