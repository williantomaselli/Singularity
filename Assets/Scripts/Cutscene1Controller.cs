using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Cutscene1Controller : MonoBehaviour
{
    [Header("Referências")]
    public Image imageToFade;         // Imagem do Canvas que sofrerá o fade
    public AudioSource audioSource;   // Fonte de áudio a ser utilizada
    public AudioClip audioClip;       // Áudio a ser tocado

    [Header("Configurações de Tempo")]
    [Tooltip("Tempo de espera antes de tocar o áudio.")]
    public float delayBeforeAudio = 2f;
    [Tooltip("Duração do fade out da imagem.")]
    public float fadeOutDuration = 1f;
    [Tooltip("Duração do fade in da imagem.")]
    public float fadeInDuration = 1f;
    [Tooltip("Tempo de espera após o áudio antes de iniciar o fade in (em segundos).")]
    public float delayAfterAudio = 15f;

    [Header("Cena de Destino")]
    [Tooltip("Nome da cena para a qual a transição será realizada.")]
    public string nextSceneName;

    void Start()
    {
        // Garante que a imagem comece com alpha 1 (totalmente visível)
        if (imageToFade != null)
        {
            Color color = imageToFade.color;
            imageToFade.color = new Color(color.r, color.g, color.b, 1f);
        }

        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        // Realiza o fade out da imagem (de 1 para 0)
        if (imageToFade != null)
        {
            yield return StartCoroutine(FadeImage(1f, 0f, fadeOutDuration));
        }

        // Aguarda o tempo definido no inspetor antes de tocar o áudio
        yield return new WaitForSeconds(delayBeforeAudio);

        // Toca o áudio, se definido
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }

        // Aguarda 15 segundos após o áudio (ou o tempo configurado)
        yield return new WaitForSeconds(delayAfterAudio);

        // Realiza o fade in da imagem (de 0 para 1)
        if (imageToFade != null)
        {
            yield return StartCoroutine(FadeImage(0f, 1f, fadeInDuration));
        }

        // Carrega a próxima cena
        SceneManager.LoadScene(nextSceneName);
    }

    // Coroutine para realizar o fade da imagem
    IEnumerator FadeImage(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color originalColor = imageToFade.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            imageToFade.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        // Garante o valor final do alpha
        imageToFade.color = new Color(originalColor.r, originalColor.g, originalColor.b, endAlpha);
    }
}
