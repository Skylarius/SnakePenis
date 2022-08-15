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

    [Header("Afro Style unlockable")]
    public bool isAfroStyleEnabled = false;
    public GameObject OriginalPenisMesh;
    public Material Brown, DarkBrown;
    public Material OriginalPink, OriginalDarkPink;
    public Text AfroStyleButtonText;

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
    }

    void SwitchBallsWithRoundedBalls(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        RealSnakeBinder realSnakeBinder = SnakeHead.GetComponent<RealSnakeBinder>();
        if (condition == true && isRoundedBallsEnabled==false) {
            newRoundedBalls = Instantiate(RoundedBalls);
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
            realSnakeBinder.UpdateBinder();
            isRoundedBallsEnabled = false;
        }
        RoundedBallsButtonText.text = (isRoundedBallsEnabled) ? "Disattiva" : "Attiva";
    }

    public void SwitchAfroStyle()
    {
        SwitchAfroStyle(!isAfroStyleEnabled);
    }

    void SwitchAfroStyle(bool condition)
    {
        SnakeMovement snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
        Renderer snakeRenderer = OriginalPenisMesh.GetComponent<Renderer>();
        Renderer[] ballsRenderers = OriginalBalls.GetComponentsInChildren<Renderer>();
        RealSnakeBinder realSnakeBinder = SnakeHead.GetComponent<RealSnakeBinder>();
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
            AfroStyleButtonText.text = "Disattiva";
        } else
        {
            print("NO AFRO");
            isAfroStyleEnabled = false;
            AfroStyleButtonText.text = "Attiva";
        }
    }

    public void SaveData(ref GameData data)
    {
        data.RoundedBalls = isRoundedBallsEnabled;
        data.AfroStyle = isAfroStyleEnabled;
    }
}
