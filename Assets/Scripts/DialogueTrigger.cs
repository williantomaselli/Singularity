using System.Collections;
using UnityEngine;

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

    private void Start()
    {
        // Se já quiser deixar a câmera da cutscene desativada no início
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
                // Faz o que o script já fazia originalmente
                if (dialogo != null)
                {
                    dialogo.SetActive(true);
                    dialogScript = dialogo.GetComponent<DialogueController>();
                    if (dialogScript != null)
                    {
                        dialogScript.PlayDialogue();
                    }
                    
                }
                
                // Remove este componente para não repetir a ação
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
            // Altere "NomeDaAnimacao" para o nome exato da animação
            cutsceneAnimator.Play("NomeDaAnimacao");
        }

        // 4. Executar o diálogo, se houver
        if (dialogo != null)
        {
            dialogo.SetActive(true);
            dialogScript = dialogo.GetComponent<DialogueController>();
            if (dialogScript != null)
            {
                dialogScript.PlayDialogue();
            }
         
        }

        // 5. Aguardar o tempo definido da cutscene
        yield return new WaitForSeconds(cutsceneDuration);

        // 6. Restaura a câmera principal
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
        }

        // Destruir a câmera da cutscene (GameObject)
        if (cutsceneCamera != null)
        {
            Destroy(cutsceneCamera.gameObject);
        }

        // Liberar o movimento do jogador
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        cinematicBars.SetActive(false);


        // Por fim, destruir este script para não repetir o evento
        Destroy(this);
    }
}
