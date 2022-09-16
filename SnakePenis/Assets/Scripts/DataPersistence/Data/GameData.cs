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

    // Settings
    public float FieldOfView;
    public bool Sound;
    public float TouchSensitivity;

    // Daily Bonus
    public string DailyBonusTimeStamp;

    // Specials
    public bool RoundedBalls;
    public bool AfroStyle;
    public bool DickingJump;
    public bool RainbowStyle;
    public bool MovingWalls;
    public bool Free360Movement;
    public bool PillsBlower;
    public bool Teleport;
    public bool ACappella;
    public bool FirstPerson;
    public bool Labyrinth;

    // Stats
    public StatsData Stats;

    public GameData()
    {
        PlayerID = "NaN";
        Name = "";
        TotalXP = "0";
        Level = "1";

        //Daily Bonus TimeStamp
        DailyBonusTimeStamp = "";

        //Sound
        Sound = true;

        //CameraFieldOfView
        FieldOfView = 0f;

        //Touch Sensitivity
        TouchSensitivity = 0f;

        //Unlockables settings
        RoundedBalls = false;
        AfroStyle = false;
        DickingJump = false;
        RainbowStyle = false;
        MovingWalls = false;
        Free360Movement = false;
        PillsBlower = false;
        Teleport = false;
        ACappella = false;
        Labyrinth = false;

        Stats = new StatsData();
    }
}
