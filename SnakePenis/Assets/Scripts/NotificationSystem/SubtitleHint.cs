using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubtitleHint : VisualHint
{
    public SubtitleHint(string s) : base(s) 
    {
        NonBlocking = true;
        hintTemplateController = GameGodSingleton.SubtitleHintTemplateController;
    }
}
