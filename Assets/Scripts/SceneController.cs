using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Necessário para carregar cenas

public class SceneController : MonoBehaviour
{
    [Header("Fade de Cena Inicial")]
    public Image sceneFadeImage;
    public float sceneFadeDuration = 2f;

    [Header("Status do Protagonista")]
    public int day = 1;
    public float hunger = 0f;
    public float thirst = 0f;

    [Header("Display do Dia")]
    public TMP_Text dayDisplayTMP; // Exibe o dia atual

    [Header("Instruções (TMP)")]
    public TMP_Text interactTMP;
    public float textFadeDuration = 0.5f;

    [Header("Overlay de Interação")]
    public Image interactionOverlay;
    public float interactionFadeDuration = 0.5f;
    public float interactionDisplayTime = 1.5f;

    [Header("Áudio de Interação")]
    public AudioSource interactionAudioSource;
    public AudioClip waterAudioClip;
    public AudioClip foodAudioClip;

    [Header("Interação e Movimento")]
    public FirstPersonController playerMovement;

    [Header("Tags de Interação")]
    public string waterTag = "Interactables/Water";
    public string foodTag = "Interactables/Food";
    public string bedTag  = "Interactables/Bed";
    public string cardTag = "Interactables/Card";
    public string generatorTag = "Interactables/Generator";

    [Header("Diálogo Geral (Opcional)")]
    public GameObject dialogo; // Objeto com o DialogueController geral
    private DialogueController dialogScript;

    [Header("Diálogo de Água e Comida (Objects)")]
    public GameObject[] waterDialogueObjects; // Diálogo para água (índice: day-1)
    public GameObject[] foodDialogueObjects;  // Diálogo para comida (índice: day-1)

    [Header("Interação do Gerador")]
    public GameObject generatorDialogueObject;
    public float generatorDialogueDuration = 3f;
    public bool generatorIsOn = false;
    private AudioSource currentGeneratorAudioSource;

    // Flags para consumo único por dia
    private bool waterConsumedToday = false;
    private bool foodConsumedToday = false;

    // Interno
    private string currentInteraction = "";
    private bool isInteracting = false;

    [Header("Sistema de Sono")]
    public SleepSystem sleepSystem; // Referência ao SleepSystem

    [Header("Dialogue Duration")]
    public float defaultDialogueDuration = 3f;

    // ------------- CAMPOS ESPECIAIS PARA O DIA 4 -------------
    [Header("Dia 4 - Diálogos Aleatórios e Vigília")]
    [Tooltip("Array de diálogos aleatórios para o dia 4.")]
    public GameObject[] day4RandomDialogues;
    [Tooltip("Intervalo mínimo entre diálogos aleatórios (em segundos).")]
    public float randomDialogueIntervalMin = 5f;
    [Tooltip("Intervalo máximo entre diálogos aleatórios (em segundos).")]
    public float randomDialogueIntervalMax = 10f;
    [Tooltip("Duração de cada diálogo aleatório do dia 4 (em segundos).")]
    public float day4RandomDialogueDuration = 3f;
    [Tooltip("Diálogo final do dia 4, reproduzido após os 2 minutos de vigília.")]
    public GameObject day4EndDialogue;
    [Tooltip("Local para onde o player será teleportado ao final do dia 4 (não utilizado se for feita a transição de cena).")]
    public Transform day4TeleportLocation;
    [Tooltip("Tempo (em segundos) que o jogador deve ficar acordado no dia 4.")]
    public float day4AwakeDuration = 120f;
    private float day4AwakeTimer = 0f;
    private bool day4SleepReady = false;
    private Coroutine day4RandomDialogueCoroutine;
    private bool day4RoutineRunning = false;
    private int lastDay4DialogueIndex = -1; // Para evitar repetir o mesmo diálogo consecutivamente
    [Header("Dia 4 - Audio Source")]
    [Tooltip("AudioSource que será usado para os diálogos do dia 4.")]
    public AudioSource day4AudioSource;
    // ----------------- NOVA SEÇÃO PARA TRANSIÇÃO DE CENA -----------------
    [Header("Transição de Cena no Dia 4")]
    [Tooltip("Nome da cena a ser carregada ao final do dia 4.")]
    public string day4SceneToLoad;
    // ---------------------------------------------------------

    void Start()
    {
        if (dialogo != null)
        {
            dialogo.SetActive(true);
            dialogScript = dialogo.GetComponent<DialogueController>();
            if (dialogScript != null)
                dialogScript.PlayDialogue();
        }

        if (sceneFadeImage != null)
            StartCoroutine(FadeImage(sceneFadeImage, 1f, 0f, sceneFadeDuration));

        if (interactTMP != null)
        {
            interactTMP.text = "";
            Color c = interactTMP.color;
            c.a = 0f;
            interactTMP.color = c;
        }

        if (interactionOverlay != null)
        {
            Color oc = interactionOverlay.color;
            oc.a = 0f;
            interactionOverlay.color = oc;
        }

        UpdateDayDisplay();

        if (sleepSystem != null)
            sleepSystem.OnSleepFinished += OnSleepFinished;

        // Se for o dia 4, inicia a rotina especial
        if (day == 4)
        {
            StartCoroutine(Day4Routine());
        }
    }

    private void OnSleepFinished(int newDay, float newHunger, float newThirst)
    {
        day = newDay;
        hunger = newHunger;
        thirst = newThirst;
        waterConsumedToday = false;
        foodConsumedToday = false;
        generatorIsOn = false;
        UpdateDayDisplay();

        if (day != 4 && day4RandomDialogueCoroutine != null)
        {
            StopCoroutine(day4RandomDialogueCoroutine);
            day4RandomDialogueCoroutine = null;
        }
        if (day == 4)
        {
            StartCoroutine(Day4Routine());
        }
        else
        {
            day4AwakeTimer = 0f;
            day4SleepReady = false;
        }
    }

    void Update()
    {
        UpdateDayDisplay();

        // Interações padrão (água, comida, gerador, cama)
        if (!isInteracting && !string.IsNullOrEmpty(currentInteraction) && Input.GetKeyDown(KeyCode.E))
        {
            if (currentInteraction == waterTag)
            {
                if (waterConsumedToday)
                {
                    if (waterDialogueObjects != null && waterDialogueObjects.Length >= day)
                    {
                        StartCoroutine(ActivateAndPlayDialogue(waterDialogueObjects[day - 1], defaultDialogueDuration));
                    }
                }
                else
                {
                    StartCoroutine(HandleInteraction(waterTag));
                }
            }
            else if (currentInteraction == foodTag)
            {
                if (foodConsumedToday)
                {
                    if (foodDialogueObjects != null && foodDialogueObjects.Length >= day)
                    {
                        StartCoroutine(ActivateAndPlayDialogue(foodDialogueObjects[day - 1], defaultDialogueDuration));
                    }
                }
                else
                {
                    StartCoroutine(HandleInteraction(foodTag));
                }
            }
            else if (currentInteraction == generatorTag)
            {
                if (!generatorIsOn)
                {
                    isInteracting = true;
                    HideInteractText();
                    if (generatorDialogueObject != null)
                    {
                        StartCoroutine(ActivateAndPlayDialogue(generatorDialogueObject, generatorDialogueDuration));
                    }
                    generatorIsOn = true;
                    if (currentGeneratorAudioSource != null)
                    {
                        currentGeneratorAudioSource.Play();
                    }
                    isInteracting = false;
                }
            }
            else if (currentInteraction == bedTag)
            {
                // Para o dia 4, a rotina de sono é gerenciada na Day4Routine; se o jogador interagir com a cama, exibimos "não pronto"
                if (day == 4)
                {
                    if (sleepSystem != null)
                        sleepSystem.TrySleep(false);
                }
                else if (day == 2)
                {
                    if (waterConsumedToday && foodConsumedToday && generatorIsOn)
                    {
                        if (sleepSystem != null)
                        {
                            sleepSystem.day = day;
                            sleepSystem.hunger = hunger;
                            sleepSystem.thirst = thirst;
                            sleepSystem.TrySleep(true);
                        }
                    }
                    else
                    {
                        if (sleepSystem != null)
                            sleepSystem.TrySleep(false);
                    }
                }
                else
                {
                    if (waterConsumedToday && foodConsumedToday)
                    {
                        if (sleepSystem != null)
                        {
                            sleepSystem.day = day;
                            sleepSystem.hunger = hunger;
                            sleepSystem.thirst = thirst;
                            sleepSystem.TrySleep(true);
                        }
                    }
                    else
                    {
                        if (sleepSystem != null)
                            sleepSystem.TrySleep(false);
                    }
                }
            }
        }

        // Lógica especial para o dia 4
        if (day == 4)
        {
            // Incrementa o timer do dia 4 (para ficar 2 minutos acordado)
            if (!day4SleepReady)
            {
                day4AwakeTimer += Time.deltaTime;
                if (day4AwakeTimer >= day4AwakeDuration)
                {
                    day4SleepReady = true;
                    if (sleepSystem != null)
                    {
                        sleepSystem.SetSleepReady();
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(waterTag))
        {
            currentInteraction = waterTag;
            ShowInteractText("Press E to drink water");
        }
        else if (other.CompareTag(foodTag))
        {
            currentInteraction = foodTag;
            ShowInteractText("Press E to eat food");
        }
        else if (other.CompareTag(bedTag))
        {
            currentInteraction = bedTag;
            ShowInteractText("Press E to sleep");
        }
        else if (other.CompareTag(cardTag))
        {
            currentInteraction = cardTag;
            ShowInteractText("Press E to get the card");
        }
        else if (other.CompareTag(generatorTag))
        {
            currentInteraction = generatorTag;
            ShowInteractText("Press E to turn on the generator");
            currentGeneratorAudioSource = other.GetComponent<AudioSource>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag(waterTag) && currentInteraction == waterTag) ||
            (other.CompareTag(foodTag) && currentInteraction == foodTag) ||
            (other.CompareTag(bedTag) && currentInteraction == bedTag) ||
            (other.CompareTag(cardTag) && currentInteraction == cardTag) ||
            (other.CompareTag(generatorTag) && currentInteraction == generatorTag))
        {
            HideInteractText();
            currentInteraction = "";
            if (other.CompareTag(generatorTag))
            {
                currentGeneratorAudioSource = null;
            }
        }
    }

    void ShowInteractText(string message)
    {
        if (interactTMP != null)
        {
            interactTMP.text = message;
            StartCoroutine(FadeTMP(interactTMP, 0f, 1f, textFadeDuration));
        }
    }

    void HideInteractText()
    {
        if (interactTMP != null)
            StartCoroutine(FadeTMP(interactTMP, interactTMP.color.a, 0f, textFadeDuration));
    }

    IEnumerator HandleInteraction(string type)
    {
        isInteracting = true;
        if (playerMovement != null)
            playerMovement.enabled = false;

        HideInteractText();

        if (interactionOverlay != null)
            yield return StartCoroutine(FadeImage(interactionOverlay, 0f, 1f, interactionFadeDuration));

        if (interactionAudioSource != null)
        {
            if (type == waterTag)
            {
                if (waterAudioClip != null)
                    interactionAudioSource.PlayOneShot(waterAudioClip);
                thirst = Mathf.Min(thirst + 5f, 100f);
                waterConsumedToday = true;
            }
            else if (type == foodTag)
            {
                if (foodAudioClip != null)
                    interactionAudioSource.PlayOneShot(foodAudioClip);
                hunger = Mathf.Min(hunger + 5f, 100f);
                foodConsumedToday = true;
            }
        }

        yield return new WaitForSeconds(interactionDisplayTime);

        if (interactionOverlay != null)
            yield return StartCoroutine(FadeImage(interactionOverlay, 1f, 0f, interactionFadeDuration));

        if (playerMovement != null)
            playerMovement.enabled = true;

        isInteracting = false;

        // Para dias que não são o 4, se água e comida foram consumidos, define sleep ready
        if (day != 4 && waterConsumedToday && foodConsumedToday && sleepSystem != null)
        {
            sleepSystem.SetSleepReady();
        }
    }

    IEnumerator ActivateAndPlayDialogue(GameObject dialogueObj, float duration)
    {
        dialogueObj.SetActive(true);
        DialogueController dc = dialogueObj.GetComponent<DialogueController>();
        if (dc != null)
        {
            // Se for do dia 4, e se o audio source para dia 4 foi atribuído, sobrescreve o áudio dele.
            if (day == 4 && day4AudioSource != null)
            {
                dc.audioSource = day4AudioSource;
            }
            dc.PlayDialogue();
        }
        yield return new WaitForSeconds(duration);
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

    void UpdateDayDisplay()
    {
        if (dayDisplayTMP != null)
            dayDisplayTMP.text = "Day " + day;
    }

    // ------------- ROTINA ESPECIAL DO DIA 4 -------------
    IEnumerator Day4Routine()
    {
        if (day4RoutineRunning) yield break;
        day4RoutineRunning = true;

        // Controle do movimento do player no dia 4 (ajuste conforme desejado)
        if (playerMovement != null)
            playerMovement.enabled = true;

        float elapsed = 0f;

        // Inicia a rotina de diálogos aleatórios
        day4RandomDialogueCoroutine = StartCoroutine(Day4RandomDialogueRoutine());

        // Aguarda os 2 minutos de vigília
        while (elapsed < day4AwakeDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Para os diálogos aleatórios
        if (day4RandomDialogueCoroutine != null)
            StopCoroutine(day4RandomDialogueCoroutine);

        // Toca o diálogo final do dia 4
        if (day4EndDialogue != null)
        {
            yield return StartCoroutine(ActivateAndPlayDialogue(day4EndDialogue, defaultDialogueDuration));
        }

        // Fade out para escurecer a tela
        if (sceneFadeImage != null)
            yield return StartCoroutine(FadeImage(sceneFadeImage, 0f, 1f, sceneFadeDuration));

        // Em vez de acordar no dia 5, carrega a cena definida no inspetor
        if (!string.IsNullOrEmpty(day4SceneToLoad))
        {
            SceneManager.LoadScene(day4SceneToLoad);
        }
        else
        {
            Debug.LogWarning("Day4SceneToLoad não foi definida no inspetor!");
        }

        day4RoutineRunning = false;
    }

    IEnumerator Day4RandomDialogueRoutine()
    {
        while (day == 4 && !day4SleepReady)
        {
            float waitTime = Random.Range(randomDialogueIntervalMin, randomDialogueIntervalMax);
            yield return new WaitForSeconds(waitTime);
            PlayDay4RandomDialogue();
        }
    }

    void PlayDay4RandomDialogue()
    {
        if (day4RandomDialogues != null && day4RandomDialogues.Length > 0)
        {
            int index = Random.Range(0, day4RandomDialogues.Length);
            // Evita repetir o mesmo diálogo consecutivamente, se possível
            if (day4RandomDialogues.Length > 1)
            {
                while (index == lastDay4DialogueIndex)
                {
                    index = Random.Range(0, day4RandomDialogues.Length);
                }
            }
            lastDay4DialogueIndex = index;
            GameObject dialogueObj = day4RandomDialogues[index];
            if (dialogueObj != null)
            {
                StartCoroutine(ActivateAndPlayDialogue(dialogueObj, day4RandomDialogueDuration));
            }
        }
    }
}
