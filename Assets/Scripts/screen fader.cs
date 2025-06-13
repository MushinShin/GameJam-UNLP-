using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class screenfader : MonoBehaviour
{
    
    [SerializeField] Image fadePanel; // Arrastra tu FadePanel aquí desde el Inspector
    [SerializeField] float fadeDuration = 1.5f; // Duración del fundido en segundos
    [SerializeField] float fadeOutDuration = 1.5f;

    public static screenfader Instance { get; private set; } // Patrón Singleton para acceso fácil

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        if (fadePanel == null)
        {
            Debug.LogError("ScreenFader: ¡Asigna el panel de fundido (FadePanel) en el Inspector!");
            // Busca el componente Image en los hijos si no está asignado
            fadePanel = GetComponentInChildren<Image>();
            if (fadePanel == null)
            {
                Debug.LogError("ScreenFader: No se encontró un componente Image en los hijos.");
            }
        }
    }

    void Start()
    {
        if (fadePanel != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    public IEnumerator FadeIn()
    {
        fadePanel.gameObject.SetActive(true);
        Color panelColor = fadePanel.color;
        panelColor.a = 1f; // Iniciar en opaco (negro)
        fadePanel.color = panelColor;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / fadeDuration; 
            panelColor.a = Mathf.Lerp(1f, 0f, normalizedTime); 
            fadePanel.color = panelColor;
            yield return null;
        }

        panelColor.a = 0f; 
        fadePanel.color = panelColor;
        fadePanel.gameObject.SetActive(false); 
    }


    public IEnumerator FadeOut()
    {
        fadePanel.gameObject.SetActive(true); // Asegúrate de que el panel esté activo
        Color panelColor = fadePanel.color;
        panelColor.a = 0f; 
        fadePanel.color = panelColor;

        float timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / fadeOutDuration; 
            panelColor.a = Mathf.Lerp(0f, 1f, normalizedTime); 
            fadePanel.color = panelColor;
            yield return null;
        }

        panelColor.a = 1f; 
        fadePanel.color = panelColor;
    }
}
