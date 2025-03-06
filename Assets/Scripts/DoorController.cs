using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("Transform da porta que será animada")]
    public Transform door;
    [Tooltip("Porta requer cartão?")]
    public bool cardNeeded = false;
    [Tooltip("Deslocamento que define a posição aberta da porta (em relação à posição fechada)")]
    public Vector3 openOffset = new Vector3(2f, 0f, 0f);
    [Tooltip("Velocidade de abertura/fechamento da porta")]
    public float openSpeed = 2f;
    [Tooltip("Dia mínimo necessário para usar a porta (abrir ou teleportar)")]
    public float openInDay = 1;

    [Header("Audio Settings")]
    [Tooltip("Fonte de áudio para reproduzir os sons")]
    public AudioSource doorAudioSource;
    [Tooltip("Som a ser reproduzido quando a porta abre")]
    public AudioClip openSound;
    [Tooltip("Som a ser reproduzido quando a porta fecha")]
    public AudioClip closeSound;

    [Header("Interaction Settings")]
    [Tooltip("Objeto que contém o texto de interação (opcional)")]
    public GameObject interactionText;

    [Header("Sleep System Reference")]
    [Tooltip("Objeto que contém o script SleepSystem (para checar o dia)")]
    public GameObject sleepSystemObj;
    private SleepSystem sleepSystem;

    // ---------- NOVOS CAMPOS PARA TELEPORTE ----------
    [Header("Teleport Settings")]
    [Tooltip("Se marcado, em vez de abrir/fechar, esta porta teleporta o jogador")]
    public bool isTeleportable = false;
    [Tooltip("Alvo para onde o jogador será teleportado")]
    public Transform teleportTarget;
    [Tooltip("Imagem usada para fazer o fade (tela escurecer/clarear)")]
    public Image fadeOverlay;
    [Tooltip("Duração do fade in/out")]
    public float fadeDuration = 1f;
    // --------------------------------------------------

    // ---------- NOVOS CAMPOS PARA DIÁLOGO DE CARTÃO ----------
    [Header("Dialogue for Card Requirement")]
    [Tooltip("Objeto de diálogo a ser exibido se o jogador não possuir o cartão")]
    public GameObject cardDialogueObject;
    [Tooltip("Duração do diálogo para o cartão (em segundos)")]
    public float cardDialogueDuration = 3f;
    // --------------------------------------------------

    // Posições da porta (modo normal)
    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool doorOpen = false;
    private bool isAnimating = false;
    private bool playerInRange = false;
    public bool haveCard = false;

    void Start()
    {
        // Armazena a posição inicial como a posição fechada
        if (door != null)
        {
            closedPosition = door.position;
            openPosition = closedPosition + openOffset;
        }

        // Obtém o script SleepSystem do objeto designado
        if (sleepSystemObj != null)
        {
            sleepSystem = sleepSystemObj.GetComponent<SleepSystem>();
        }
    }

    // Quando o jogador entra na área de trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Primeiro, checamos se o dia do SleepSystem é suficiente
        if (sleepSystem != null && sleepSystem.day < openInDay)
        {
            // Se o dia for menor que o necessário, não faz nada
            return;
        }

        // Se for uma porta do tipo "teleportável"
        if (isTeleportable)
        {
            // Teleporta o jogador (ignora a lógica de abrir/fechar porta)
            if (!isAnimating)
            {
                StartCoroutine(TeleportPlayer(other.transform));
            }
        }
        else
        {
            // Caso seja porta "normal"
            if (!cardNeeded)
            {
                playerInRange = true;
                if (!doorOpen && !isAnimating)
                {
                    StartCoroutine(OpenDoor());
                }
            }
            else
            {
                // Se a porta requer cartão:
                if (haveCard)
                {
                    playerInRange = true;
                    if (!doorOpen && !isAnimating)
                    {
                        StartCoroutine(OpenDoor());
                    }
                }
                else
                {
                    // Jogador não possui o cartão: ativa o diálogo de aviso
                    if (!isAnimating && cardDialogueObject != null)
                    {
                        StartCoroutine(ActivateAndPlayDialogue(cardDialogueObject, cardDialogueDuration));
                    }
                }
            }
        }
    }

    // Quando o jogador sai da área de trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Só faz sentido fechar se for uma porta normal (não teleportável)
            if (!isTeleportable && doorOpen && !isAnimating)
            {
                StartCoroutine(CloseDoor());
            }
        }
    }

    // ------------------ LÓGICA DE ABRIR/FECHAR PORTA NORMAL ------------------
    IEnumerator OpenDoor()
    {
        isAnimating = true;
        if (doorAudioSource != null && openSound != null)
        {
            doorAudioSource.PlayOneShot(openSound);
        }
        while (Vector3.Distance(door.position, openPosition) > 0.01f)
        {
            door.position = Vector3.Lerp(door.position, openPosition, Time.deltaTime * openSpeed);
            yield return null;
        }
        door.position = openPosition;
        doorOpen = true;
        isAnimating = false;
    }

    IEnumerator CloseDoor()
    {
        isAnimating = true;
        if (doorAudioSource != null && closeSound != null)
        {
            doorAudioSource.PlayOneShot(closeSound);
        }
        while (Vector3.Distance(door.position, closedPosition) > 0.01f)
        {
            door.position = Vector3.Lerp(door.position, closedPosition, Time.deltaTime * openSpeed);
            yield return null;
        }
        door.position = closedPosition;
        doorOpen = false;
        isAnimating = false;
    }

    // ------------------ LÓGICA DE TELEPORTE ------------------
    IEnumerator TeleportPlayer(Transform player)
    {
        isAnimating = true;

        // 1. Fade out (tela escurece)
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeImage(fadeOverlay, 0f, 1f, fadeDuration));
        }

        // 2. Teleporta o jogador
        if (teleportTarget != null)
        {
            player.position = teleportTarget.position;
            player.rotation = teleportTarget.rotation; // opcional, se quiser rotacionar o jogador
        }

        // 3. Fade in (tela clareia)
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeImage(fadeOverlay, 1f, 0f, fadeDuration));
        }

        isAnimating = false;
    }

    // ------------------ MÉTODO DE FADE AUXILIAR ------------------
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

        // Garante que fique no valor final
        c.a = endAlpha;
        img.color = c;
    }

    // ------------------ MÉTODO PARA ATIVAR E EXECUTAR DIÁLOGO ------------------
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
}
