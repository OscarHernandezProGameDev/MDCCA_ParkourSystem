using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Esta clase fué creada para poder reiniciar la escena y salir de la aplicación cuando se probase la Build.
public class PauseMenu : MonoBehaviour
{
    
    public void RestartGame()
    {
        SceneManager.LoadScene("FinalScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
