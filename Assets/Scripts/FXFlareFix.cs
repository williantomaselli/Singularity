using UnityEngine;

public class FXFlareFix : MonoBehaviour
{
    [Header("Objeto a ativar no Dia 4")]
    public GameObject day4Object; // Defina este objeto no inspetor

    [Header("Referência ao SceneController")]
    public SceneController sceneController; // Pode ser atribuído via inspetor ou será buscado automaticamente

    void Start()
    {
        // Caso não tenha sido atribuído via inspetor, busca o SceneController na cena
        if (sceneController == null)
        {
            sceneController = FindObjectOfType<SceneController>();
        }
    }

    void Update()
    {
        // Verifica se o SceneController está atribuído e se o dia atual é 4
        if (sceneController != null && sceneController.day == 4)
        {
            // Ativa o objeto designado para o dia 4
            if (day4Object != null)
            {
                day4Object.SetActive(true);
            }
            // Desativa o GameObject deste script
            gameObject.SetActive(false);
        }
    }
}
