using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SubtitleHint : VisualHint
{
    public SubtitleHint(string s) : base(s) 
    {
        NonBlocking = true;
        hintTemplateController = GameGodSingleton.SubtitleHintTemplateController;
    }

    public SubtitleHint(string s, float duration) : base(s)
    {
        NonBlocking = true;
        hintTemplateController = GameGodSingleton.SubtitleHintTemplateController;
        Duration = duration;
    }
}
