using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualHint : INotification
{
    public string hintText;
    public bool NonBlocking;
    public float Duration;
    protected HintTemplateController hintTemplateController;
    public VisualHint()
    {
        hintText = "";
        NonBlocking = false;
        hintTemplateController = GameGodSingleton.HintTemplateController;
        Duration = 3f;
    }

    public VisualHint(string s) : this()
    {
        hintText = s;
        GameGodSingleton.NotificationSystem.Enqueue(this);
    }
    public IEnumerator ExecuteCoroutine()
    {
        hintTemplateController.WriteHint(this.hintText);
        hintTemplateController.gameObject.SetActive(true);
        if (NonBlocking == false)
        {
            Time.timeScale = 0f;
        }
        yield return new WaitForSecondsRealtime(Duration);
        Time.timeScale = 1f;
        hintTemplateController.gameObject.SetActive(false);
    }



}
