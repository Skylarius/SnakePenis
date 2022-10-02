using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintTemplateController : MonoBehaviour
{
    [SerializeField]
    private Text HintText;

    public void WriteHint(string s)
    {
        HintText.text = s;
    }
}