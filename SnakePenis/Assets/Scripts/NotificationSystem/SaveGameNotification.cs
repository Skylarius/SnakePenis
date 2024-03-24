using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameNotification : INotification
{
    public string Text;
    protected HintTemplateController saveGameHintTemplateController;
    private bool hasSaved;
    public SaveGameNotification()
    {
        saveGameHintTemplateController = GameGodSingleton.SaveGameHintTemplateController;
        hasSaved = false;
        GameGodSingleton.NotificationSystem.Enqueue(this);
    }

    public void SetHasSaved(bool condition) { hasSaved = condition; }

    public bool HasSaved() { return hasSaved;  }

    public SaveGameNotification(string s) : this()
    {
        Text = s;
    }
    public IEnumerator ExecuteCoroutine()
    {
        // Cambia sta spacchio di funzione
        saveGameHintTemplateController.WriteHint(this.Text);
        saveGameHintTemplateController.gameObject.SetActive(true);
        while (hasSaved == false)
        {
            yield return new WaitForEndOfFrame();
        }
        saveGameHintTemplateController.gameObject.SetActive(false);
    }



}
