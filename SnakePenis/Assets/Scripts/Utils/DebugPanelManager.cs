using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanelManager : MonoBehaviour, IDataPersistence
{
    public int MaxStatsSampleToStore = 100;
    public StatsData Stats;
    public Text infoText;
    private bool isCoroutineRunning = false;
    public void Start()
    {
        infoText.text = "";
    }

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

    public void SimulateGameAndLogStats()
    {
        StartCoroutine(SimulateGameAndLogStatsCoroutine());
    }

    IEnumerator SimulateGameAndLogStatsCoroutine()
    {
        if (isCoroutineRunning)
        {
            yield break;
        }
        isCoroutineRunning = true;
        FileDataHandler dataHandler = new FileDataHandler(Application.persistentDataPath, "Stats.txt", false);
        dataHandler.CreateFile();
        float totalTime = 0f;
        int LevelToReach = 20;
        SettingsPanelManager settingsPanelManager = GameGodSingleton.Instance.SettingsPanelManager;


        BigInteger StartTotalXP = LevelProgressionManager.TotalXP;
        int StartLevel = LevelProgressionManager.CurrentLevel;
        int oldJumpCount = InputHandler.JumpsAmount;
        int oldPortalUsage = PortalManager.PortalUsage;

        LevelProgressionManager.TotalXP = BigInteger.Zero;
        LevelProgressionManager.CurrentLevel = 1;
        int countGamesForEachLevel = 0;
        float lastTotalTime = 0f;


        while (LevelProgressionManager.CurrentLevel < LevelToReach)
        {
            countGamesForEachLevel++;
            int levelAtStartGame = LevelProgressionManager.CurrentLevel;
            bool[] InitialSpecialsEnabled = new bool[settingsPanelManager.Specials.Count];
            for (int i = 0; i<settingsPanelManager.Specials.Count; i++)
            {
                InitialSpecialsEnabled[i] = settingsPanelManager.Specials[i].enabled;
            }
            //Set up parameters
            ActivateSpecialsWithLevel(settingsPanelManager, levelAtStartGame);
            StatsSample statsSample = Stats.Samples[Random.Range(0, Stats.Samples.Count)];

            InputHandler.JumpsAmount = statsSample.JumpsCount;
            PortalManager.PortalUsage = statsSample.PortalUsage;

            dataHandler.WriteDataToFile(
                $"Level {LevelProgressionManager.CurrentLevel} , Game #{countGamesForEachLevel}, Duration {statsSample.GameDuration}\n"
                );
            if (settingsPanelManager.JumpingDickSpecial.enabled)
            {
                dataHandler.WriteDataToFile($"Jumps: {InputHandler.JumpsAmount}, ");
            }
            if (settingsPanelManager.TeleportSpecial.enabled)
            {
                dataHandler.WriteDataToFile($"Portal usage: {PortalManager.PortalUsage}\n");
            }

            ScoreManager.SetLengthAndScore(statsSample.Length, (statsSample.Length - 5) * (int)statsSample.GameDuration / 10 * (int)statsSample.RealSpeed);
            dataHandler.WriteDataToFile($"Base Score: {ScoreManager.CurrentScore}\n");

            ScoreManager.ScoreBeforeBonuses = ScoreManager.CurrentScore;
            // Add Bonuses Score
            settingsPanelManager.RecalculateBonuses();
            foreach (SettingsPanelManager.Bonus bonus in settingsPanelManager.Bonuses)
            {
                dataHandler.WriteDataToFile($"+{bonus.Name} = {bonus.TotalBonus}\n");
                ScoreManager.AddBonusScore(bonus.TotalBonus);
            }
            dataHandler.WriteDataToFile($"XP Gained: {ScoreManager.CurrentScore}\n");

            // Add XP
            LevelProgressionManager.MakeLevelProgression(int.Parse(ScoreManager.CurrentScore));

            if (levelAtStartGame != LevelProgressionManager.CurrentLevel)
            {
                dataHandler.WriteDataToFile(
                    $"--> Reached Level {LevelProgressionManager.CurrentLevel} in {countGamesForEachLevel} games (+{(totalTime - lastTotalTime )/ (60 * 60)} hours).\n", infoText
                    );
                lastTotalTime = totalTime;
                countGamesForEachLevel = 0;
            }
            dataHandler.WriteDataToFile($"Level Progr.: {LevelProgressionManager.CurrentRelativeXP} / {LevelProgressionManager.NextLevelRelativeXP} ({(int)(LevelProgressionManager.CompletionPercent * 100)} % )\n");
            infoText.text = $"Level {LevelProgressionManager.CurrentLevel} , Game #{countGamesForEachLevel}, Time Passed {(int)(totalTime - lastTotalTime) / 3600} h {(int)(totalTime - lastTotalTime) / 60 % 60} m\n ||";
            int completionPercent = (int)(LevelProgressionManager.CompletionPercent * 100);
            for (int i = 0; i < completionPercent; i+=5)
            {
                infoText.text += "=";
            }
            infoText.text += ">>";

            totalTime += statsSample.GameDuration;
            if (totalTime > 60 * 60 * 24 * 100)
            {
                dataHandler.WriteDataToFile(
                    "Taking more than 100 days. Makes no sense: abort.\n"
                    );
                break;
            }

            // Reset
            ScoreManager.CurrentScore = ScoreManager.CurrentLength = "";
            for (int i=0; i< InitialSpecialsEnabled.Length; i++)
            {
                settingsPanelManager.Specials[i].enabled = InitialSpecialsEnabled[i];
            }
            dataHandler.WriteDataToFile("\n");
            yield return new WaitForSecondsRealtime(0.05f);
        }
        dataHandler.WriteDataToFile(
            $"\n\n --> According to statistics, you will reach level {LevelProgressionManager.CurrentLevel} in {totalTime / (60 * 60)} hours ({totalTime / (60 * 60 * 24)} days)",
            infoText);
        InputHandler.JumpsAmount = oldJumpCount;
        PortalManager.PortalUsage = oldPortalUsage;
        settingsPanelManager.ResetBonuses();
        LevelProgressionManager.TotalXP = StartTotalXP;
        LevelProgressionManager.CurrentLevel = StartLevel;
        isCoroutineRunning = false;
    }

    public void ResetAllData()
    {
        ScoreManager.CurrentID = "";
        ScoreManager.CurrentScoreName = "";
        LevelProgressionManager.TotalXP = BigInteger.Zero;
    }

    public void ShowStats()
    {
        infoText.text = "";
        foreach (string statString in Stats.StatsToString())
        {
            infoText.text += statString + "\n";
        }
    }

    /// <summary>
    /// Level prog Simulation: once a new unlockable is available it's immediately activated.
    /// </summary>
    /// <param name="settingsPanelManager"></param>
    /// <param name="level"></param>
    void ActivateSpecialsWithLevel(SettingsPanelManager settingsPanelManager, int level)
    {
        foreach (SettingsPanelManager.BaseSpecial special in settingsPanelManager.Specials)
        {
            special.enabled = (special.Unlockable.Level <= level);
        }
        if (settingsPanelManager.AfroStyleSpecial.IsEnabled() && settingsPanelManager.RainbowSpecial.IsEnabled())
        {
            settingsPanelManager.AfroStyleSpecial.enabled = false;
        }
    }

    public void LoadData(GameData data)
    {
        Stats = data.Stats;
    }

    public void SaveData(ref GameData data)
    {
        if (int.Parse(ScoreManager.ScoreBeforeBonuses) == 0)
        {
            return;
        }
        if (data.Stats == null)
        {
            data.Stats = new StatsData();
        }

        StatsSample sample = new StatsSample();
        sample.GameDuration = GameGodSingleton.Instance.SnakeMovement.levelTime;
        sample.Length = GameGodSingleton.Instance.SnakeMovement.SnakeBody.Count;
        sample.RealSpeed = GameGodSingleton.Instance.SnakeMovement.realSpeed;
        sample.Level = LevelProgressionManager.CurrentLevel;
        if (InputHandler.JumpsAmount > 0)
        {
            sample.JumpsCount = InputHandler.JumpsAmount;
        }
        if (PortalManager.PortalUsage > 0)
        {
            sample.PortalUsage = PortalManager.PortalUsage;
        }
        data.Stats.Samples.Add(sample);
        Debug.Log($"Saved {sample.ToString()}");

    }
}
