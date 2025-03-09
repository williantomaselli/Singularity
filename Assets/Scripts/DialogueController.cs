using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    [Tooltip("Texto da legenda para esta linha")]
    public string subtitle;
    [Tooltip("Áudio da dublagem para esta linha")]
    public AudioClip voiceClip;
    [Tooltip("Tempo (em segundos) que a legenda ficará na tela")]
    public float displayDuration = 3f;
    [Tooltip("AudioSource específico para esta linha (opcional). Se não atribuído, o AudioSource padrão do DialogueController será usado.")]
    public AudioSource customAudioSource;
}

public class DialogueController : MonoBehaviour
{
    [Header("Configurações de Diálogo")]
    [Tooltip("Array de linhas de diálogo (defina texto, áudio e duração para cada linha)")]
    public DialogueLine[] dialogueLines;

    [Header("Referências UI e Áudio")]
    [Tooltip("Elemento de TextMeshPro para exibir as legendas")]
    public TMP_Text subtitleText;
    [Tooltip("AudioSource padrão para reproduzir os clipes de dublagem caso o diálogo não especifique um customAudioSource")]
    public AudioSource audioSource;

    // Este método pode ser chamado de outro script para iniciar o diálogo
    public void PlayDialogue()
    {
        StartCoroutine(PlayDialogueCoroutine());
    }

    // Coroutine que percorre as linhas de diálogo
    private IEnumerator PlayDialogueCoroutine()
    {
        subtitleText.gameObject.SetActive(true);

        // Verifica se os componentes estão atribuídos
        if (subtitleText == null)
        {
            yield break;
        }
        if (audioSource == null && dialogueLines.Length > 0)
        {
            Debug.LogWarning("DialogueController: AudioSource padrão não está atribuído.");
        }

        // Percorre cada linha do diálogo
        for (int i = 0; i < dialogueLines.Length; i++)
        {
            DialogueLine line = dialogueLines[i];
            // Exibe o texto na tela
            subtitleText.text = line.subtitle;

            // Determina qual AudioSource usar: o custom ou o padrão
            AudioSource currentAudioSource = line.customAudioSource != null ? line.customAudioSource : audioSource;

            // Reproduz o áudio, se houver
            if (line.voiceClip != null && currentAudioSource != null)
            {
                currentAudioSource.clip = line.voiceClip;
                currentAudioSource.Play();
            }
            // Aguarda o tempo definido para a linha
            yield return new WaitForSeconds(line.displayDuration);
        }

        // Ao finalizar o diálogo, limpa a legenda
        subtitleText.text = "";
    }
}
