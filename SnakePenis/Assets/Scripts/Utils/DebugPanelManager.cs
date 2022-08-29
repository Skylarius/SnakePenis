using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class DebugPanelManager : MonoBehaviour
{
    public void AddTenThousandXP()
    {
        LevelProgressionManager.MakeLevelProgression(10000);
    }

    public void IncreaseLevel()
    {
        BigInteger CurrentRelativeXP = LevelProgressionManager.CurrentRelativeXP;
        BigInteger NextLevelXP = LevelProgressionManager.NextLevelRelativeXP;
        LevelProgressionManager.MakeLevelProgression(int.Parse((NextLevelXP - CurrentRelativeXP).ToString()));
    }

    public void ResetCurrentLevelXP()
    {
        if (LevelProgressionManager.CurrentLevel > 1)
        {
            LevelProgressionManager.TotalXP -= LevelProgressionManager.CurrentRelativeXP;
            LevelProgressionManager.CurrentLevel = LevelProgressionManager.GetLevelFromTotalXP(LevelProgressionManager.TotalXP);

        }
        else
        {
            LevelProgressionManager.TotalXP = BigInteger.Zero;
        }
    }

    public void DecreaseLevel()
    {
        ResetCurrentLevelXP();
        if (LevelProgressionManager.CurrentLevel > 1)
        {
            LevelProgressionManager.TotalXP -= LevelProgressionManager.CurrentRelativeXP;
            LevelProgressionManager.CurrentLevel = LevelProgressionManager.GetLevelFromTotalXP(LevelProgressionManager.TotalXP);
        } else
        {
            Debug.LogWarning("You reached the minimum level 1");
        }
    }

    public void PowerUpSpawnerBoost()
    {
        PowerUpSpawner powerUpSpawner = GameGodSingleton.Instance.gameObject.GetComponent<PowerUpSpawner>();
        if (powerUpSpawner)
        {
            powerUpSpawner.spawnFrequency = 10;
            powerUpSpawner.MaxPowerUpAmout = 10;
        }
    }

    public void ResetAllData()
    {
        ScoreManager.CurrentID = "";
        ScoreManager.CurrentScoreName = "";
        LevelProgressionManager.TotalXP = BigInteger.Zero;
    }
}
