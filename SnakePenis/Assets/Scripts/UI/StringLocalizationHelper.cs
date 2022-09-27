using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringLocalizationHelper : MonoBehaviour
{
    public string PlayerName
    {
        get
        {
            return ScoreManager.CurrentScoreName;
        }
    }
}
