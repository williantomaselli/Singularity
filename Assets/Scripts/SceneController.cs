using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public GameObject[] waterDialogueObjects; // Objeto para diálogo se já consumiu água (índice: day-1)
    public GameObject[] foodDialogueObjects;  // Objeto para diálogo se já consumiu comida (índice: day-1)

    [Header("Interação do Gerador")]
    public GameObject generatorDialogueObject; // Diálogo para ativação do gerador
    public float generatorDialogueDuration = 3f; // Duração do diálogo do gerador
    public bool generatorIsOn = false;           // Flag que indica se o gerador foi ativado
    private AudioSource currentGeneratorAudioSource; // Áudio do objeto gerador (obtido na colisão)

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

    void Start()
    {
        // Ativa o diálogo geral, se houver
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

        // Inscreve-se no evento do SleepSystem para atualizar os status ao final do sono
        if (sleepSystem != null)
            sleepSystem.OnSleepFinished += OnSleepFinished;
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
    }

    void Update()
    {
        UpdateDayDisplay();

        // Se o jogador pressiona E e há uma interação disponível
        if (!isInteracting && !string.IsNullOrEmpty(currentInteraction) && Input.GetKeyDown(KeyCode.E))
        {
            if (currentInteraction == waterTag)
            {
                // Se já consumiu água, apenas toca o diálogo e não bloqueia o movimento
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
                if (!generatorIsOn) // Ativa o gerador somente se ainda não estiver ligado
                {
                    isInteracting = true;
                    HideInteractText();

                    // Toca o diálogo configurado para o gerador
                    if (generatorDialogueObject != null)
                    {
                        StartCoroutine(ActivateAndPlayDialogue(generatorDialogueObject, generatorDialogueDuration));
                    }
                    // Define o gerador como ligado
                    generatorIsOn = true;
                    // Toca o áudio presente no objeto de colisão (se houver)
                    if (currentGeneratorAudioSource != null)
                    {
                        currentGeneratorAudioSource.Play();
                    }
                    isInteracting = false;
                }
            }
            else if (currentInteraction == bedTag)
            {
                // Para o dia 2, exige que as três missões sejam cumpridas: água, comida e gerador
                if (day == 2)
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
                    // Para os demais dias, apenas água e comida são exigidos
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
        // Para interações de consumo que ainda não foram feitas, desabilita o movimento
        if (playerMovement != null)
            playerMovement.enabled = false;

        HideInteractText();

        // Inicia a animação de overlay: fade in
        if (interactionOverlay != null)
            yield return StartCoroutine(FadeImage(interactionOverlay, 0f, 1f, interactionFadeDuration));

        // Toca o áudio e atualiza os status
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

        // Fade out do overlay
        if (interactionOverlay != null)
            yield return StartCoroutine(FadeImage(interactionOverlay, 1f, 0f, interactionFadeDuration));

        // Reabilita o movimento do player
        if (playerMovement != null)
            playerMovement.enabled = true;

        isInteracting = false;
    }

    IEnumerator ActivateAndPlayDialogue(GameObject dialogueObj, float duration)
    {
        dialogueObj.SetActive(true);
        DialogueController dc = dialogueObj.GetComponent<DialogueController>();
        if (dc != null)
        {
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
}
