using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;


[System.Serializable]
public class GameData
{
    public string PlayerID;
    public string Name;
    public string TotalXP;
    public string Level;

    // Unlockables
    public bool RoundedBalls;
    public bool AfroStyle;
    public bool DickingJump;

    public GameData()
    {
        PlayerID = "NaN";
        Name = "";
        TotalXP = "0";
        Level = "1";

        //Unlockables settings
        RoundedBalls = false;
        AfroStyle = false;
        DickingJump = false;
    }
}
