using UnityEngine;
using System.Collections;

public class CardPickup : MonoBehaviour
{
    public GameObject doorObj;
    private DoorController doorController;
    public GameObject cardDialogueObject; // Objeto de diálogo ao pegar o cartão
    private DialogueController dialogScript;
    public float cardDialogueDuration = 5f;

    void Start()
    {
        if (cardDialogueObject != null)
        {
            dialogScript = cardDialogueObject.GetComponent<DialogueController>();
        }

        if (doorObj != null)
        {
            doorController = doorObj.GetComponent<DoorController>();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (doorController != null)
                {
                    doorController.haveCard = true;
                    
                }

                if (cardDialogueObject != null)
                {
                    StartCoroutine(ActivateAndPlayDialogue(cardDialogueObject, cardDialogueDuration));
                }
                Destroy(this.gameObject);
            }
        }
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
}
