using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;

public class LevelProgressionManager : Singleton<MonoBehaviour>, IDataPersistence
{
    public static int CurrentLevel = 1;
    public static int OldLevel = CurrentLevel;

    public static BigInteger CurrentLevelXP {
        get
        {
            return GetLevelTotalXP(CurrentLevel);
        }
    }
    /// <summary>
    /// XP to gain to get from previous level (Current Level - 1) to this level (Current Level)
    /// </summary>
    public static BigInteger CurrentLevelRelativeXP
    {
        get { return GetLevelRelativeXP(CurrentLevel); }
    }

    /// <summary>
    /// XP to get from Current Level to the next (CurrentLevel + 1)
    /// </summary>
    public static BigInteger NextLevelRelativeXP
    {
        get { return GetLevelRelativeXP(CurrentLevel + 1); }
    }

    /// <summary>
    /// XP so far
    /// </summary>
    public static BigInteger TotalXP = 0;

    /// <summary>
    /// XP gained from the beginning of CurrentLevel
    /// </summary>
    public static BigInteger CurrentRelativeXP
    {
        get { return TotalXP - CurrentLevelXP;  }
    }

    /// <summary>
    /// XP before new score added
    /// </summary>
    public static BigInteger OldTotalXP
    {
        get { return TotalXP - new BigInteger(int.Parse(ScoreManager.CurrentScore)); }
    }

    public static BigInteger OldLevelRelativeXP
    {
        get { return OldTotalXP - GetLevelTotalXP(OldLevel); }
    }

    public static float CompletionPercent
    {
        get { return (float)((double)CurrentRelativeXP / (double)NextLevelRelativeXP); }
    }

    public static float OldCompletionPercent
    {
        get
        {
            BigInteger OldNextLevelXP = GetLevelRelativeXP(OldLevel + 1);
            return (float)((double)OldLevelRelativeXP / (double)GetLevelRelativeXP(OldLevel + 1));
        }
    }
    public const float LvlExpFactor = 1.5f;
    public const int Multiplier = 13000;
    public const int maxLevelOfIncreasingXP = 20;
    // Start is called before the first frame update
    void Start() {
        //RecalculateLevelXP();
    }

    public static BigInteger GetLevelRelativeXP(int level)
    {
        int tmpLevel = level;
        if (tmpLevel <= 1)
        {
            return BigInteger.Zero;
        }
        if (tmpLevel > 20)
        {
            tmpLevel = 20;
        }
        BigInteger res = new BigInteger(Multiplier * Mathf.Pow(LvlExpFactor, tmpLevel));
        return res;
    }

    public static BigInteger GetLevelTotalXP(int level)
    {
        BigInteger levelXP = BigInteger.Zero;
        for (int l = 1; l <= level; l++)
        {
            levelXP += GetLevelRelativeXP(l);
        }
        return levelXP;
    }

    public static int GetLevelFromTotalXP(BigInteger totalXP)
    {
        int level = 1;
        while (GetLevelTotalXP(level + 1) < totalXP)
        {
            level++;
        }
        return level;
    }

    // Update is called once per frame
    public static void MakeLevelProgression(int newScore)
    {
        OldLevel = CurrentLevel;
        TotalXP += new BigInteger(newScore);
        while (CurrentRelativeXP >= NextLevelRelativeXP)
        {
            CurrentLevel += 1;
            //RecalculateLevelXP();
        }
    }

    void IDataPersistence.LoadData(GameData data)
    {
        TotalXP = BigInteger.Parse(data.TotalXP);
        //CurrentLevel = int.Parse(data.Level);
        CurrentLevel = GetLevelFromTotalXP(TotalXP);
        OldLevel = CurrentLevel;
    }

    void IDataPersistence.SaveData(ref GameData data)
    {
        data.TotalXP = TotalXP.ToString();
        data.Level = CurrentLevel.ToString();
        Debug.Log($"Saved Level Data: XP={data.TotalXP}, Level={data.Level}");
    }
}
