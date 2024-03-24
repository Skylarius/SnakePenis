using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonsActions : MonoBehaviour
{
    public Sprite playImage;
    public Sprite pauseImage;
    [SerializeField]
    private Image PauseButtonImage;
    public GameObject SettingsMenuObj;
    public GameObject DebugModePanel;
    public GameObject LanguagePanel;

    void RequestCommand(UICommand.UICommandFunction bcf)
    {
        InputHandler inputHandler = GameGodSingleton.SnakeMovement.GetInputHandler();
        if (inputHandler != null )
        {
            inputHandler.RequestUICommand(bcf);
        }
    }

    void ExitFunction()
    {
        if (DataPersistenceManager.IsGameSaving()) 
        {
            Debug.Log("Waiting for saving...");
            return; 
        }
        Application.Quit();
    }

    public void ExitButtonFunction()
    {
        RequestCommand(ExitFunction);
    }

    void PauseFunction()
    {
        if (SnakeMovement.isGameOver)
        {
            return;
        }
        bool isPause = (Time.timeScale == 0);
        Time.timeScale = isPause ? 1 : 0;
        if (playImage != null && pauseImage != null && PauseButtonImage != null)
        {
            PauseButtonImage.sprite = !isPause ? playImage : pauseImage;
        }
        if (SettingsMenuObj != null)
        {
            SettingsMenuObj.SetActive(!isPause);
        }

    }

    public void PauseButtonFunction()
    {
        RequestCommand(PauseFunction);
    }

    void ReplayFunction()
    {
        if (DataPersistenceManager.IsGameSaving()) 
        {
            Debug.Log("Waiting for saving...");
            return;
        }
        print("Scene reloaded");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReplayButtonFunction()
    {
        RequestCommand(ReplayFunction);
    }

    public void DebugModeButtonFunction()
    {
        RequestCommand(DebugModeFunction);
    }

    void DebugModeFunction()
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

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            DebugModeButtonFunction();
        }
    }

    private int hiddenButtonCounter = 0;
    public void DebugModeHiddenButtonFunction()
    {
        RequestCommand(DebugModeHiddenFunction);
    }

    void DebugModeHiddenFunction()
    {
        if (SnakeMovement.isGameOver)
        {
            return;
        }
        if (hiddenButtonCounter > 3)
        {
            hiddenButtonCounter = 0;
            DebugModeButtonFunction();
        }
        else
        {
            hiddenButtonCounter++;
        }
    }

    public void SetLanguageButtonFunction(UnityEngine.Localization.Locale l)
    {
        LocalizedStringUser.SetLanguage(l);
        LanguagePanel.SetActive(false);
    }

    public void ToggleLanguagePanel()
    {
        LanguagePanel.SetActive(!LanguagePanel.activeSelf);
    }
}
