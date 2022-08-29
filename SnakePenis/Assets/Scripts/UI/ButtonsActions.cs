using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonsActions : MonoBehaviour
{
    public Sprite playImage;
    public Sprite pauseImage;
    public GameObject SettingsMenuObj;
    public GameObject DebugModePanel;
    public void ExitButtonFunction()
    {
        Application.Quit();
    }

    public void PauseButtonFunction(Image image)
    {
        if (SnakeMovement.isGameOver)
        {
            return;
        }
        Time.timeScale = (Time.timeScale == 0) ? 1 : 0;
        if (playImage != null && pauseImage != null && image != null)
        {
            image.sprite = (Time.timeScale == 0) ? playImage : pauseImage;
        }
        if (SettingsMenuObj != null)
        {
            SettingsMenuObj.SetActive(Time.timeScale == 0 ? true : false);
        }
    }

    public void ReplayButtonFunction()
    {
        print("Scene reloaded");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void DebugModeButtonFunction()
    {
        if (SnakeMovement.isGameOver)
        {
            return;
        }
        Time.timeScale = (Time.timeScale == 0) ? 1 : 0;
        if (DebugModePanel != null)
        {
            DebugModePanel.SetActive(Time.timeScale == 0 ? true : false);
        }
    }
}
