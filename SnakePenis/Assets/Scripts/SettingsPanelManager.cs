using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelManager : MonoBehaviour, IDataPersistence
{
    public ScoreManager scoreManager;
    public InputField inputNameField;
    public Text infoText;

    [System.Serializable]
    public struct Unlockable
    {
        public GameObject Menu;
        public int Level;
    }

    public struct Bonus
    {
        public string Name;
        public int Percent;
        public int XPAmount;
    }
    [Header("Daily Bonus")]
    public string DailyBonusTimeStamp;
    public int DailyBonusXPAmount;
    public string DailyBonusTitle;

    public List<Unlockable> Unlockables;

    [Header("Rounded Ball unlockable")]
    public bool isRoundedBallsEnabled = false;
    public Text RoundedBallsButtonText;
    public GameObject RoundedBalls;
    private GameObject newRoundedBalls = null;
    public GameObject SnakeHead;
    public GameObject OriginalBalls;
    public string roundedBallInfo;
    public int RoundedBallsPercent = 0;
    public string RoundedBallsBonusTitle;

    [Header("Afro Style unlockable")]
    public bool isAfroStyleEnabled = false;
    public GameObject OriginalPenisMesh;
    public Material Brown, DarkBrown;
    public Material OriginalPink, OriginalDarkPink;
    public Text AfroStyleButtonText;
    public string afroStyleInfo;
    public int AfroStyleBonusPercent = 0;
    public string AfroStyleBonusTitle;

    [Header("Jump unlockable")]
    public bool isJumpEnabled = false;
    public Text DickingJumpButtonText;
    public string jumpInfo;
    public int JumpBonusPercent = 0;
    public string JumpBonusTitle;

    [Header("Rainbow unlockable")]
    public bool isRainbowEnabled = false;
    public Material RainbowMaterial;
    public ParticleSystem snakeParticleSystem;
    public Material particleSystemOriginalMaterial;
    public Text RainbowStyleButtonText;
    public string rainbowInfo;
    public int RainbowBonusPercent = 0;
    public string RainbowBonusTitle;

    [Header("Moving Walls")]
    public bool isMovingWallsEnabled = false;
    public GameObject MovingWall1;
    public GameObject MovingWall2;
    public int MovingWallsBonusPercent = 12;
    public string MovingWallsBonusTitle;
    private GameObject newMovingWall1 = null, newMovingWall2 = null;
    public Text MovingWallsButtonText;
    public string movingWallsInfo;

    // Start is called before the first frame update
    void Start()
    {
        if (ScoreManager.CurrentScoreName!="" && ScoreManager.CurrentID != "")
        {
            inputNameField.text = ScoreManager.CurrentScoreName;
        }
        foreach (Unlockable unlockable in Unlockables)
        {
            unlockable.Menu.SetActive(LevelProgressionManager.CurrentLevel >= unlockable.Level);
        }
        infoText.text = "";
    }

    public IEnumerable<Bonus> BonusGenerator()
    {
        Bonus bonus;
        System.DateTime dt1 = System.DateTime.Today;
        System.DateTime dt2 = System.DateTime.ParseExact(DailyBonusTimeStamp, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
        if (dt1 > dt2)
        {
            bonus = new Bonus();
            bonus.Name = DailyBonusTitle;
            bonus.XPAmount = DailyBonusXPAmount;
            yield return bonus;
            DailyBonusTimeStamp = System.DateTime.Today.ToString("dd-MM-yyyy");
        }
        if (isRoundedBallsEnabled)
        {
            bonus = new Bonus();
            bonus.Name = JumpBonusTitle;
            bonus.Percent = JumpBonusPercent;
            yield return bonus;
        }
        if (isJumpEnabled)
        {
            bonus = new Bonus();
            bonus.Name = JumpBonusTitle;
            bonus.Percent = JumpBonusPercent;
            yield return bonus;
        }
        if (isAfroStyleEnabled)
        {
            bonus = new Bonus();
            bonus.Name = AfroStyleBonusTitle;
            bonus.Percent = AfroStyleBonusPercent;
            yield return bonus;
        }
        if (isRainbowEnabled)
        {
            bonus = new Bonus();
            bonus.Name = RainbowBonusTitle;
            bonus.Percent = RainbowBonusPercent;
            yield return bonus;
        }
        if (isMovingWallsEnabled)
        {
            bonus = new Bonus();
            bonus.Name = MovingWallsBonusTitle;
            bonus.Percent = MovingWallsBonusPercent;
            yield return bonus;
        }
    }


    public void ChangeName() {
        StartCoroutine(ChangeNameOnHighScores());
    }

    IEnumerator ChangeNameOnHighScores()
    {
        if (inputNameField.text != "")
        {
            ScoreManager.CurrentScoreName = StringFormatter.FormatKeyboardName(inputNameField.text);
            yield return StartCoroutine(scoreManager.ChangeName(infoText));
            DataPersistenceManager.Instance.SaveGame();
        }
    }

    public void SwitchBallsWithRoundedBalls()
    {
        SwitchBallsWithRoundedBalls(!isRoundedBallsEnabled);
        infoText.text = roundedBallInfo;
    }

    void SwitchBallsWithRoundedBalls(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        RealSnakeBinder realSnakeBinder = SnakeHead.GetComponent<RealSnakeBinder>();
        Renderer[] initialBallsRenderers = snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1].GetComponentsInChildren<Renderer>();
        if (condition == true && isRoundedBallsEnabled==false) {
            newRoundedBalls = Instantiate(RoundedBalls);
            Renderer[] newBallsRenderers = newRoundedBalls.GetComponentsInChildren<Renderer>();
            for (int i=0; i<newBallsRenderers.Length; i++)
            {
                newBallsRenderers[i].material = initialBallsRenderers[i].material;
            }
            newRoundedBalls.transform.position = OriginalBalls.transform.position;
            snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1] = newRoundedBalls;
            realSnakeBinder.UpdateBinder();
            OriginalBalls.SetActive(false);
            isRoundedBallsEnabled = true;
        } else if (condition == false && isRoundedBallsEnabled == true)
        {
            if (OriginalBalls.activeSelf == false)
            {
                OriginalBalls.SetActive(true);
            }
            if (newRoundedBalls)
            {
                OriginalBalls.transform.position = newRoundedBalls.transform.position;
                Destroy(newRoundedBalls);
            }
            snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1] = OriginalBalls;
            Renderer[] OriginalBallsRenderers = OriginalBalls.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < OriginalBallsRenderers.Length; i++)
            {
                OriginalBallsRenderers[i].material = initialBallsRenderers[i].material;
            }
            realSnakeBinder.UpdateBinder();
            isRoundedBallsEnabled = false;
        }
        RoundedBallsButtonText.text = (isRoundedBallsEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchAfroStyle()
    {
        if (isRainbowEnabled)
        {
            SwitchRainbowStyle(false);
        }
        SwitchAfroStyle(!isAfroStyleEnabled);
        infoText.text = afroStyleInfo;
    }

    void SwitchAfroStyle(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        Renderer snakeRenderer = OriginalPenisMesh.GetComponent<Renderer>();
        Renderer[] ballsRenderers = snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1].GetComponentsInChildren<Renderer>();
        if (condition == true && isAfroStyleEnabled == false)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = DarkBrown;
            }
            snakeRenderer.material = Brown;
            isAfroStyleEnabled = true;
        }
        else if (condition == false && isAfroStyleEnabled == true)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = OriginalDarkPink;
            }
            snakeRenderer.material = OriginalPink;
            isAfroStyleEnabled = false;
        }
        AfroStyleButtonText.text = (isAfroStyleEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchDickingJump()
    {
        SwitchDickingJump(!isJumpEnabled);
        infoText.text = jumpInfo;
    }

    void SwitchDickingJump(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        if (condition == true && isJumpEnabled == false)
        {
            snakeMovement.isJumpEnabled = true;
            isJumpEnabled = true;
        }
        else if (condition == false && isJumpEnabled == true)
        {
            snakeMovement.isJumpEnabled = false;
            isJumpEnabled = false;
        }
        DickingJumpButtonText.text = (isJumpEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchRainbowStyle()
    {
        if (isAfroStyleEnabled)
        {
            SwitchAfroStyle(false);
        }
        SwitchRainbowStyle(!isRainbowEnabled);
        infoText.text = rainbowInfo;
    }

    void SwitchRainbowStyle(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        Renderer snakeRenderer = OriginalPenisMesh.GetComponent<Renderer>();
        Renderer[] ballsRenderers = snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1].GetComponentsInChildren<Renderer>();
        if (condition == true && isRainbowEnabled == false)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = RainbowMaterial;
            }
            snakeRenderer.material = RainbowMaterial;
            snakeParticleSystem.GetComponent<Renderer>().material = RainbowMaterial;
            isRainbowEnabled = true;
        }
        else if (condition == false && isRainbowEnabled == true)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = OriginalDarkPink;
            }
            snakeRenderer.material = OriginalPink;
            snakeParticleSystem.GetComponent<Renderer>().material = particleSystemOriginalMaterial;
            isRainbowEnabled = false;
        }
        RainbowStyleButtonText.text = (isRainbowEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchMovingWalls()
    {
        SwitchMovingWalls(!isMovingWallsEnabled);
        infoText.text = movingWallsInfo;
    }

    void SwitchMovingWalls(bool condition)
    {
        if (condition == true && isMovingWallsEnabled == false)
        {
            newMovingWall1 = Instantiate(MovingWall1);
            newMovingWall2 = Instantiate(MovingWall2);
            isMovingWallsEnabled = true;
        }
        else if (condition == false && isMovingWallsEnabled == true)
        {
            if (newMovingWall1)
            {
                Destroy(newMovingWall1);
            }
            if (newMovingWall2)
            {
                Destroy(newMovingWall2);
            }
            isMovingWallsEnabled = false;
        }
        MovingWallsButtonText.text = (isMovingWallsEnabled) ? "Disattiva" : "Attiva";
    }

    public void LoadData(GameData data)
    {
        if (data.DailyBonusTimeStamp != "")
        {
            DailyBonusTimeStamp = data.DailyBonusTimeStamp;
        } else
        {
            DailyBonusTimeStamp = System.DateTime.Today.AddDays(-1).ToString("dd-MM-yyyy");
        }
        if (data.RoundedBalls == true)
        {
            SwitchBallsWithRoundedBalls(true);
            RoundedBallsButtonText.text = "Disattiva";
        } else
        {
            print("NO ROUND BALLS");
            isRoundedBallsEnabled = false;
            RoundedBallsButtonText.text = "Attiva";
        }

        if (data.AfroStyle == true)
        {
            SwitchAfroStyle(true);
            if (isRainbowEnabled)
            {
                SwitchRainbowStyle(false);
                RainbowStyleButtonText.text = "Attiva";
            }
            AfroStyleButtonText.text = "Disattiva";
        } else
        {
            print("NO AFRO");
            isAfroStyleEnabled = false;
            AfroStyleButtonText.text = "Attiva";
        }

        if (data.DickingJump == true)
        {
            SwitchDickingJump(true);
            DickingJumpButtonText.text = "Disattiva";
        }
        else
        {
            print("NO JUMP");
            isJumpEnabled = false;
            DickingJumpButtonText.text = "Attiva";
        }

        if (data.RainbowStyle == true)
        {
            SwitchRainbowStyle(true);
            if (isAfroStyleEnabled)
            {
                SwitchAfroStyle(false);
                AfroStyleButtonText.text = "Attiva";
            }
            RainbowStyleButtonText.text = "Disattiva";
        }
        else
        {
            print("NO RAINBOW");
            isRainbowEnabled = false;
            RainbowStyleButtonText.text = "Attiva";
        }

        if (data.MovingWalls == true)
        {
            SwitchMovingWalls(true);
            MovingWallsButtonText.text = "Disattiva";
        }
        else
        {
            print("NO MOVING WALLS");
            isMovingWallsEnabled = false;
            MovingWallsButtonText.text = "Attiva";
        }
    }

    public void SaveData(ref GameData data)
    {
        data.DailyBonusTimeStamp = System.DateTime.ParseExact(
            DailyBonusTimeStamp, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture
            ).ToString("dd-MM-yyyy");
        data.RoundedBalls = isRoundedBallsEnabled;
        data.AfroStyle = isAfroStyleEnabled;
        data.DickingJump = isJumpEnabled;
        data.RainbowStyle = isRainbowEnabled;
        data.MovingWalls = isMovingWallsEnabled;
    }
}
