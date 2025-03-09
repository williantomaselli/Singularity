using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Diálogo")]
    [Tooltip("Objeto que contém o componente de diálogo (com DialogueController)")]
    public GameObject dialogo;
    public GameObject cinematicBars;
    private DialogueController dialogScript;

    [Header("Cutscene Settings")]
    [Tooltip("Marque se deseja usar uma cutscene ao entrar no trigger")]
    public bool useCutscene = false;

    [Tooltip("Câmera principal do jogo (será desativada durante a cutscene)")]
    public Camera mainCamera;

    [Tooltip("Câmera da cutscene (será ativada durante a cutscene)")]
    public Camera cutsceneCamera;

    [Tooltip("Duração total da cutscene (em segundos)")]
    public float cutsceneDuration = 5f;

    [Tooltip("Animator opcional para reproduzir animações na câmera da cutscene")]
    public Animator cutsceneAnimator;

    [Tooltip("Script de movimento do player para travar/destravar durante a cutscene")]
    public FirstPersonController playerController;

    [Header("Dormir Após Cutscene")]
    [Tooltip("Se marcado, ao final da cutscene, o jogador será teleportado, a tela dará fade out/in e um diálogo de sono será reproduzido.")]
    public bool dormirAposCutscene = false;

    [Tooltip("Local para onde o jogador será teleportado após a cutscene, se 'dormirAposCutscene' estiver ativo.")]
    public Transform sleepTeleportLocation;

    [Tooltip("Objeto de diálogo de sono a ser reproduzido ao final da cutscene.")]
    public GameObject sleepDialogueObject;

    [Tooltip("Duração do diálogo de sono (em segundos).")]
    public float sleepDialogueDuration = 3f;

    [Tooltip("Imagem para o fade da tela (pode ser a mesma usada em outros scripts).")]
    public Image sceneFadeImage;

    [Tooltip("Duração do fade out/in para o sono (em segundos).")]
    public float sleepFadeDuration = 2f;

    private void Start()
    {
        // Desativa a câmera da cutscene no início
        if (cutsceneCamera != null)
        {
            cutsceneCamera.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!useCutscene)
            {
                // ----- Modo NORMAL (sem cutscene) -----
                if (dialogo != null)
                {
                    dialogo.SetActive(true);
                    dialogScript = dialogo.GetComponent<DialogueController>();
                    if (dialogScript != null)
                    {
                        dialogScript.PlayDialogue();
                    }
                }
                Destroy(this);
            }
            else
            {
                // ----- Modo CUTSCENE -----
                StartCoroutine(CutsceneRoutine());
            }
        }
    }

    private IEnumerator CutsceneRoutine()
    {
        if (cinematicBars != null)
            cinematicBars.SetActive(true);

        // 1. Travar o movimento do jogador
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // 2. Desativar a câmera principal e ativar a câmera da cutscene
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
        }
        if (cutsceneCamera != null)
        {
            cutsceneCamera.gameObject.SetActive(true);
        }

        // 3. Tocar animação na câmera da cutscene, se existir
        if (cutsceneAnimator != null)
        {
            // Altere "NomeDaAnimacao" para o nome exato da animação desejada
            cutsceneAnimator.Play("NomeDaAnimacao");
        }

        // 4. Executar o diálogo (caso haja)
        if (dialogo != null)
        {
            dialogo.SetActive(true);
            dialogScript = dialogo.GetComponent<DialogueController>();
            if (dialogScript != null)
            {
                dialogScript.PlayDialogue();
            }
        }

        // 5. Aguarda o tempo definido da cutscene
        yield return new WaitForSeconds(cutsceneDuration);

        // Se não for dormir após a cutscene, restaura imediatamente
        if (!dormirAposCutscene)
        {
            if (mainCamera != null)
            {
                mainCamera.gameObject.SetActive(true);
            }
            if (cutsceneCamera != null)
            {
                Destroy(cutsceneCamera.gameObject);
            }
            if (playerController != null)
            {
                playerController.enabled = true;
            }
            if (cinematicBars != null)
                cinematicBars.SetActive(false);
            Destroy(this);
            yield break;
        }

        // ----- Modo "Dormir Após Cutscene" -----
        // 6. Inicia a transição de sono: fade out
        if (sceneFadeImage != null)
        {
            yield return StartCoroutine(FadeImage(sceneFadeImage, 0f, 1f, sleepFadeDuration));
        }

        // 7. Teleporta o jogador para o local de sono
        if (sleepTeleportLocation != null && playerController != null)
        {
            playerController.transform.position = sleepTeleportLocation.position;
            playerController.transform.rotation = sleepTeleportLocation.rotation;
        }

        // 8. Toca o diálogo de sono enquanto a tela permanece escura
        if (sleepDialogueObject != null)
        {
            sleepDialogueObject.SetActive(true);
            DialogueController sleepDC = sleepDialogueObject.GetComponent<DialogueController>();
            if (sleepDC != null)
            {
                sleepDC.PlayDialogue();
            }
            yield return new WaitForSeconds(sleepDialogueDuration);
            sleepDialogueObject.SetActive(false);
        }

        // 9. Fade in: clareia a tela (simulando o despertar no próximo dia)
        if (sceneFadeImage != null)
        {
            yield return StartCoroutine(FadeImage(sceneFadeImage, 1f, 0f, sleepFadeDuration));
        }

        // 10. Restaura a câmera principal e libera o movimento do jogador
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
        }
        if (cutsceneCamera != null)
        {
            Destroy(cutsceneCamera.gameObject);
        }
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        if (cinematicBars != null)
            cinematicBars.SetActive(false);

        // Finaliza e remove o script
        Destroy(this);
    }

    private IEnumerator FadeImage(Image img, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color c = img.color;
        c.a = startAlpha;
        img.color = c;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(startAlpha, endAlpha, t);
            img.color = c;
            yield return null;
        }
        c.a = endAlpha;
        img.color = c;
    }
}
