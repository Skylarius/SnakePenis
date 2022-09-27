using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringLocalizationHelper : MonoBehaviour
{
    public string PlayerName
    {
        get
        {
            if (ScoreManager.CurrentScoreName == "")
            {
                return LocalizedStringUser.GetLocalizedUIString("NEW_PLAYER_PLACEHOLDER");
            }
            return ScoreManager.CurrentScoreName;
        }
    }
}
