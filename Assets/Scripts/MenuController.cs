using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Nome da cena a ser carregada (defina no Inspector)
    public string sceneName;

    void Update()
    {
        // Verifica se a tecla E foi pressionada
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Tecla [E] pressionada. Carregando cena: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }
}
