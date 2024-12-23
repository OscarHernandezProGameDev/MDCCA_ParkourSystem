using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void MenuToggle()
    {
        var value = !gameObject.activeSelf;

        gameObject.SetActive(value); //toggle()
        Cursor.visible = value;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("FinalScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
