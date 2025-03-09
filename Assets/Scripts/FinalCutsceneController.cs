using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinalCutsceneController : MonoBehaviour
{
    [Header("Year Display Settings")]
    [Tooltip("Tempo em segundos para aguardar antes de iniciar a exibição do ano (68s por padrão).")]
    public float initialWaitTime = 68f;
    
    [Tooltip("TMP que exibirá o 'Year' durante a cutscene.")]
    public TMP_Text yearTMP;
    
    [Tooltip("Duração do fade in para o yearTMP.")]
    public float yearFadeInDuration = 2f;
    
    [Tooltip("Tempo em segundos que o 'Year' será exibido (antes de iniciar o fade out).")]
    public float yearDisplayDuration = 30f;
    
    [Tooltip("Duração do fade out para o yearTMP.")]
    public float yearFadeOutDuration = 2f;
    
    [Tooltip("Velocidade inicial de incremento do ano (em unidades/segundo).")]
    public float initialIncrementSpeed = 1f;
    
    [Tooltip("Aceleração (não mais utilizada, pois a velocidade dobra a cada segundo).")]
    public float acceleration = 0.5f; // (Este valor não será utilizado, pois a velocidade dobra)
    
    [Header("Credits Settings")]
    [Tooltip("TMP que exibirá os créditos (que subirão lentamente).")]
    public TMP_Text creditsTMP;
    
    [Tooltip("Duração do fade in para os créditos.")]
    public float creditsFadeInDuration = 2f;
    
    [Tooltip("Tempo total para os créditos subirem (em segundos).")]
    public float creditsDuration = 30f;
    
    [Tooltip("Distância (em unidades) que os créditos se moverão para cima.")]
    public float creditsScrollDistance = 1000f;

    private int year = 0;

    void Start()
    {
        // Inicialmente, desativa o yearTMP para que não fique sempre ativo
        if(yearTMP != null)
            yearTMP.gameObject.SetActive(false);

        StartCoroutine(FinalCutsceneRoutine());
    }

    IEnumerator FinalCutsceneRoutine()
    {
        // Aguarda o tempo inicial (68s)
        yield return new WaitForSeconds(initialWaitTime);

        // Ativa o TMP do ano somente quando necessário
        if(yearTMP != null)
            yearTMP.gameObject.SetActive(true);

        // Fade in do TMP do ano
        yield return StartCoroutine(FadeTMP(yearTMP, 0f, 1f, yearFadeInDuration));

        // Exibe e incrementa o ano
        float elapsed = 0f;
        float currentSpeed = initialIncrementSpeed;
        float displayedYear = 2026f; // Ano inicial
        int lastWholeSecond = 0;
        while(elapsed < yearDisplayDuration)
        {
            float dt = Time.deltaTime;
            elapsed += dt;
            displayedYear += currentSpeed * dt;
            int wholeSeconds = Mathf.FloorToInt(elapsed);
            if(wholeSeconds > lastWholeSecond)
            {
                // Dobra a velocidade a cada segundo
                currentSpeed *= 2f;
                lastWholeSecond = wholeSeconds;
            }
            yearTMP.text = "Year \n " + ((int)displayedYear).ToString();
            yield return null;
        }

        // Fade out do TMP do ano
        yield return StartCoroutine(FadeTMP(yearTMP, 1f, 0f, yearFadeOutDuration));

        // Opcional: Desativa o yearTMP após o fade out (se desejar que ele não fique ativo)
        if(yearTMP != null)
            yearTMP.gameObject.SetActive(false);

        // Fade in dos créditos
        yield return StartCoroutine(FadeTMP(creditsTMP, 0f, 1f, creditsFadeInDuration));

        // Anima os créditos subindo lentamente
        float creditsElapsed = 0f;
        Vector2 startPos = creditsTMP.rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, creditsScrollDistance);
        while (creditsElapsed < creditsDuration)
        {
            creditsElapsed += Time.deltaTime;
            float t = creditsElapsed / creditsDuration;
            creditsTMP.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        // Opcional: Ao final, pode-se carregar uma nova cena ou finalizar a cutscene.
    }

    IEnumerator FadeTMP(TMP_Text tmp, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color c = tmp.color;
        c.a = startAlpha;
        tmp.color = c;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            tmp.color = c;
            yield return null;
        }
        c.a = endAlpha;
        tmp.color = c;
    }
}
