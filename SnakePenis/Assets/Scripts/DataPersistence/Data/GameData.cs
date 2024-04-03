using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[System.Serializable]
public class SpecialData
{
    public bool Active;
    public bool UsedOnce;
    public SpecialData() { Active = false; UsedOnce = false; }
    public SpecialData(bool active, bool usedOnce) { Active = active; UsedOnce = usedOnce; }

}

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
    public SpecialData RoundedBalls;
    public SpecialData AfroStyle;
    public SpecialData DickingJump;
    public SpecialData RainbowStyle;
    public SpecialData MovingWalls;
    public SpecialData Free360Movement;
    public SpecialData PillsBlower;
    public SpecialData Teleport;
    public SpecialData ACappella;
    public SpecialData FirstPerson;
    public SpecialData Labyrinth;

    //Tutorial
    public List<string> CompletedTutorials;

    //Language
    public string Language;

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
        RoundedBalls = new SpecialData();
        AfroStyle = new SpecialData();
        DickingJump = new SpecialData();
        RainbowStyle = new SpecialData();
        MovingWalls = new SpecialData();
        Free360Movement = new SpecialData();
        PillsBlower = new SpecialData();
        Teleport = new SpecialData();
        ACappella = new SpecialData();
        Labyrinth = new SpecialData();

        //Language
        Language = "";

        //Tutorial
        CompletedTutorials = new List<string>();

        Stats = new StatsData();
    }
}
