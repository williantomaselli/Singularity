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
    public GameObject[] sleepDialogueObjects; // Diálogos exibidos durante o sono

    [Header("Wake-Up Dialogue Objects")]
    public GameObject[] wakeUpDialogueObjects;  // Diálogos exibidos ao acordar

    [Header("Not Ready Dialogue Object")]
    public GameObject notReadyDialogueObject;   // Diálogo caso não esteja pronto para dormir

    [Header("Dialogue Duration")]
    [Tooltip("Duração padrão para cada diálogo, se não houver duração específica.")]
    public float defaultDialogueDuration = 3f;

    [Header("Player Status")]
    public int day = 1;
    public float hunger = 0f;  // Status de fome
    public float thirst = 0f;  // Status de sede

    [Header("Player Movement")]
    public FirstPersonController playerMovement; // Referência ao script de movimentação

    // Evento para notificar que o ciclo de sono terminou
    public event Action<int, float, float> OnSleepFinished;

    private bool isSleeping = false;
    public float sleepDuration = 5f;

    // Flag para indicar se os requisitos para dormir foram cumpridos
    private bool sleepReady = false;

    void Start()
    {
        // Opcional: ao iniciar o dia 1, toca o diálogo de acordar
        if (day == 1 && wakeUpDialogueObjects != null && wakeUpDialogueObjects.Length >= 1)
        {
            StartCoroutine(ActivateAndPlayDialogue(wakeUpDialogueObjects[0], defaultDialogueDuration));
        }
    }

    /// <summary>
    /// Indica que os requisitos para dormir foram cumpridos (por exemplo, depois de comer e beber).
    /// </summary>
    public void SetSleepReady()
    {
        sleepReady = true;
        Debug.Log("[SleepSystem] Jogador pronto para dormir no dia " + day);
    }

    /// <summary>
    /// Tenta iniciar o ciclo de sono. Se canSleep for true e sleepReady estiver ativo, inicia o sono; caso contrário, dispara o diálogo “não pronto”.
    /// </summary>
    public void TrySleep(bool canSleep)
    {
        if (isSleeping)
            return;
        Debug.Log("[SleepSystem] TrySleep chamado. canSleep: " + canSleep + ", sleepReady: " + sleepReady);
        if (canSleep && sleepReady)
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

        // Fade in: escurece a tela
        if (sceneFadeImage != null)
            yield return StartCoroutine(FadeImage(sceneFadeImage, 0f, 1f, sceneFadeDuration));

        yield return new WaitForSeconds(waitTimeDuringSleep);

        // Executa o diálogo de sono para o dia atual
        if (sleepDialogueObjects != null && sleepDialogueObjects.Length >= day)
        {
            yield return StartCoroutine(ActivateAndPlayDialogue(sleepDialogueObjects[day - 1], defaultDialogueDuration));
        }

        yield return new WaitForSeconds(sleepDuration);

        // Fade out: esclarece a tela (ao acordar)
        if (sceneFadeImage != null)
            yield return StartCoroutine(FadeImage(sceneFadeImage, 1f, 0f, sceneFadeDuration));

        // Incrementa o dia e reseta os status
        day++;
        hunger = 0f;
        thirst = 0f;
        Debug.Log("[SleepSystem] Novo dia: " + day);

        // Reseta a flag para o próximo dia
        sleepReady = false;

        // Toca o diálogo de acordar, se houver
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
