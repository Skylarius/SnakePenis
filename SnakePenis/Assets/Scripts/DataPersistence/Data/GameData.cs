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

    public GameData()
    {
        PlayerID = "NaN";
        Name = "";
        TotalXP = "0";
        Level = "1";
    }

}
