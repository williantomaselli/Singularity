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
}

public class DialogueController : MonoBehaviour
{
    [Header("Configurações de Diálogo")]
    [Tooltip("Array de linhas de diálogo (defina texto, áudio e duração para cada linha)")]
    public DialogueLine[] dialogueLines;

    [Header("Referências UI e Áudio")]
    [Tooltip("Elemento de TextMeshPro para exibir as legendas")]
    public TMP_Text subtitleText;
    [Tooltip("Fonte de áudio para reproduzir os clipes de dublagem")]
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
        if (audioSource == null)
        {
            yield break;
        }

        // Percorre cada linha do diálogo
        for (int i = 0; i < dialogueLines.Length; i++)
        {
            DialogueLine line = dialogueLines[i];
            // Exibe o texto na tela
            subtitleText.text = line.subtitle;

            // Reproduz o áudio, se houver
            if (line.voiceClip != null)
            {
                audioSource.clip = line.voiceClip;
                audioSource.Play();
            }
            // Aguarda o tempo definido para a linha
            yield return new WaitForSeconds(line.displayDuration);
        }

        // Ao finalizar o diálogo, limpa a legenda
        subtitleText.text = "";
    }
}
