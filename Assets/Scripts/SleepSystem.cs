using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SleepSystem : MonoBehaviour
{
    [Header("Fade Settings")]
    public Image sceneFadeImage;          // Imagem full-screen para fade (ex.: tela preta)
    public float sceneFadeDuration = 2f;    // Duração do fade in/out
    public float waitTimeDuringSleep = 1f;  // Tempo a aguardar com a tela preta (usado antes do diálogo)

    [Header("Sleep Dialogue Objects")]
    // Diálogos que serão reproduzidos enquanto o jogador está dormindo (tela preta)
    public GameObject[] sleepDialogueObjects;

    [Header("Wake-Up Dialogue Objects")]
    // Diálogos que serão reproduzidos quando o jogador acordar (tela já esclarecida)
    public GameObject[] wakeUpDialogueObjects;

    [Header("Not Ready Dialogue Object")]
    // Objeto com DialogueController para o caso de não estar pronto para dormir
    public GameObject notReadyDialogueObject;

    [Header("Dialogue Duration")]
    [Tooltip("Duração padrão para cada diálogo, se não houver duração específica.")]
    public float defaultDialogueDuration = 3f;

    [Header("Player Status")]
    public int day = 1;
    public float hunger = 0f;  // Status de fome (sincronizado com o SceneController)
    public float thirst = 0f;  // Status de sede

    [Header("Player Movement")]
    public FirstPersonController playerMovement; // Referência ao script de movimentação

    // Evento para notificar que o ciclo de sono terminou
    public event Action<int, float, float> OnSleepFinished;

    private bool isSleeping = false;
    public float sleepDuration = 5f;

    void Start()
    {
        // Opcional: se desejar que, ao iniciar a cena (dia 1), haja um diálogo de acordar,
        // descomente o trecho abaixo.
        if (day == 1 && wakeUpDialogueObjects != null && wakeUpDialogueObjects.Length >= 1)
        {
            StartCoroutine(ActivateAndPlayDialogue(wakeUpDialogueObjects[0], defaultDialogueDuration));
        }
    }

    /// <summary>
    /// Tenta dormir. Se canSleep for true, executa o ciclo de sono; se false, ativa o diálogo “não pronto”.
    /// </summary>
    public void TrySleep(bool canSleep)
    {
        if (isSleeping) return;
        if (canSleep)
        {
            StartCoroutine(HandleSleep());
        }
        else
        {
            if (notReadyDialogueObject != null)
            {
                StartCoroutine(ActivateAndPlayDialogue(notReadyDialogueObject, defaultDialogueDuration));
            }
        }
    }

    IEnumerator HandleSleep()
    {
        isSleeping = true;
        if (playerMovement != null)
            playerMovement.enabled = false;

        // 1. Fade in: escurece a tela (indo para o sono)
        if (sceneFadeImage != null)
            yield return StartCoroutine(FadeImage(sceneFadeImage, 0f, 1f, sceneFadeDuration));

        // 2. Aguarda 1 segundo com a tela preta
        yield return new WaitForSeconds(waitTimeDuringSleep);

        // 3. Executa o diálogo de sono (enquanto a tela está preta)
        if (sleepDialogueObjects != null && sleepDialogueObjects.Length >= day)
        {
            yield return StartCoroutine(ActivateAndPlayDialogue(sleepDialogueObjects[day - 1], defaultDialogueDuration));
        }

        // 4. Aguarda o tempo de sono com a tela preta
        yield return new WaitForSeconds(sleepDuration);

        // 5. Fade out: esclarece a tela (momento do despertar)
        if (sceneFadeImage != null)
            yield return StartCoroutine(FadeImage(sceneFadeImage, 1f, 0f, sceneFadeDuration));

        // 6. Incrementa o dia e reseta os status de fome e sede
        day++;
        hunger = 0f;
        thirst = 0f;
        Debug.Log("New Day: " + day);

        // 7. Executa o diálogo de acordar de forma não bloqueante, permitindo que o jogador se mova
        if (wakeUpDialogueObjects != null && wakeUpDialogueObjects.Length >= day)
        {
            StartCoroutine(ActivateAndPlayDialogue(wakeUpDialogueObjects[day - 1], defaultDialogueDuration));
        }

        if (playerMovement != null)
            playerMovement.enabled = true;

        isSleeping = false;
        OnSleepFinished?.Invoke(day, hunger, thirst);
    }

    IEnumerator ActivateAndPlayDialogue(GameObject dialogueObj, float displayDuration)
    {
        dialogueObj.SetActive(true);
        DialogueController dc = dialogueObj.GetComponent<DialogueController>();
        if (dc != null)
        {
            dc.PlayDialogue();
        }
        yield return new WaitForSeconds(displayDuration);
        dialogueObj.SetActive(false);
    }

    IEnumerator FadeImage(Image img, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color c = img.color;
        c.a = startAlpha;
        img.color = c;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            img.color = c;
            yield return null;
        }
        c.a = endAlpha;
        img.color = c;
    }
}
