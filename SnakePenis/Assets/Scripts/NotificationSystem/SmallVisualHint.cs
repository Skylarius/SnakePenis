using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallVisualHint : VisualHint
{
    public SmallVisualHint(string s) : base(s)
    {
        NonBlocking = true;
        hintTemplateController = GameGodSingleton.SmallVisualHintTemplateController;
    }

    public SmallVisualHint(string s, float duration) : base(s)
    {
        NonBlocking = true;
        hintTemplateController = GameGodSingleton.SmallVisualHintTemplateController;
        Duration = duration;
    }
}
