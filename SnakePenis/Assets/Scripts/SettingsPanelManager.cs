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

    public List<Unlockable> Unlockables;

    [Header("Rounded Ball unlockable")]
    public bool isRoundedBallsEnabled = false;
    public Text RoundedBallsButtonText;
    public GameObject RoundedBalls;
    private GameObject newRoundedBalls = null;
    public GameObject SnakeHead;
    public GameObject OriginalBalls;
    public string roundedBallInfo;

    [Header("Afro Style unlockable")]
    public bool isAfroStyleEnabled = false;
    public GameObject OriginalPenisMesh;
    public Material Brown, DarkBrown;
    public Material OriginalPink, OriginalDarkPink;
    public Text AfroStyleButtonText;
    public string afroStyleInfo;

    [Header("Jump unlockable")]
    public bool isJumpEnabled = false;
    public Text DickingJumpButtonText;
    public string jumpInfo;

    [Header("Rainbow unlockable")]
    public bool isRainbowEnabled = false;
    public Material RainbowMaterial;
    public ParticleSystem particleSystem;
    public Material particleSystemOriginalMaterial;
    public Text RainbowStyleButtonText;
    public string rainbowInfo;

    private TouchScreenKeyboard keyboard;
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
            particleSystem.GetComponent<Renderer>().material = RainbowMaterial;
            isRainbowEnabled = true;
        }
        else if (condition == false && isRainbowEnabled == true)
        {
            foreach (Renderer r in ballsRenderers)
            {
                r.material = OriginalDarkPink;
            }
            snakeRenderer.material = OriginalPink;
            particleSystem.GetComponent<Renderer>().material = particleSystemOriginalMaterial;
            isRainbowEnabled = false;
        }
        RainbowStyleButtonText.text = (isRainbowEnabled) ? "Disattiva" : "Attiva";
    }

    public void LoadData(GameData data)
    {
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
    }

    public void SaveData(ref GameData data)
    {
        data.RoundedBalls = isRoundedBallsEnabled;
        data.AfroStyle = isAfroStyleEnabled;
        data.DickingJump = isJumpEnabled;
        data.RainbowStyle = isRainbowEnabled;
    }
}
