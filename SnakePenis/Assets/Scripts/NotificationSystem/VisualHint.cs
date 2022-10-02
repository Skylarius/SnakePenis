using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualHint : INotification
{
    string hintText;
    bool NonBlocking;
    VisualHint()
    {
        hintText = "";
        NonBlocking = false;
    }

    public VisualHint(string s) : base()
    {
        hintText = s;
        GameGodSingleton.NotificationSystem.Enqueue(this);
    }
    public IEnumerator ExecuteCoroutine()
    {
        HintTemplateController hintTemplateController = GameGodSingleton.HintTemplateController;
        Time.timeScale = 0f;
        hintTemplateController.WriteHint(this.hintText);
        GameGodSingleton.HintTemplateController.gameObject.SetActive(true);
        if (NonBlocking == false)
        {
            yield return new WaitForSecondsRealtime(3f);
        }
        GameGodSingleton.HintTemplateController.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }



}
