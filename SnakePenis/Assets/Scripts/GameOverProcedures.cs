using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameOverProcedures : MonoBehaviour
{
    public GameObject GameOverUI;
    public Text PenisQuoteUI;
    public Text ScoreText;
    public Text BonusText;
    public List<string> PenisQuotes;
    private ScoreManager scoreManager;
    private LevelProgressionManager levelProgressionManager;
    public InputField inputNameField;
    private SettingsPanelManager settingsPanelManager;
    public int MaxScoreCount = 10;

    public string[] AndroidInputTexts = { "" };
    private TouchScreenKeyboard keyboard;

    // Start is called before the first frame update
    void Start()
    {
        scoreManager = GetComponent<ScoreManager>();
        levelProgressionManager = GetComponent<LevelProgressionManager>();
        settingsPanelManager = GetComponent<SettingsPanelManager>();
    }

    public IEnumerator StartGameOverProcedure()
    {
        // Reset All Textes
        ScoreText.text = "";
        BonusText.text = "";
        PenisQuoteUI.text = "";
        GameOverUI.SetActive(true);

        // Write Wisdom and Length
        PenisQuoteUI.text = "Dice il saggio: \"<b>" + PenisQuotes[UnityEngine.Random.Range(0, PenisQuotes.Count)] + "</b>\"";
        yield return new WaitForSeconds(0.8f);
        PenisQuoteUI.text += "\nMassima Erezione : <b>" + ScoreManager.CurrentLength + " cm</b>";
        yield return new WaitForSeconds(0.8f);

        // Add bonuses
        foreach (SettingsPanelManager.Bonus bonus in settingsPanelManager.Bonuses)
        {
            BonusText.text += $"{bonus.Name}: {bonus.TotalBonus}\n";
            yield return new WaitForSeconds(0.8f);
        }

        // Show score
        PenisQuoteUI.text += "\nPunteggio : <b>" + ScoreManager.CurrentScore + "</b>";

        // Get Name
        bool isNewUser = false;
        if (ScoreManager.CurrentScoreName == "")
        {
            isNewUser = true;
            yield return StartCoroutine(CreateNewUser());
        }

        // Save if new user
        if (isNewUser)
        {
            DataPersistenceManager.Instance.SaveGame();
        }

        // Commit Score and Name
        if (ScoreManager.CurrentScoreName != "")
        {
            yield return StartCoroutine(scoreManager.CommitScores(ScoreText));
        }
        yield return StartCoroutine(scoreManager.LoadScores(ScoreText));

        // Show Leaderboard
        if (ScoreManager.LocalScores.Count > 0)
        {
            // Truncate after MaxScoreCount
            if (ScoreManager.LocalScores.Count > MaxScoreCount)
            {
                ScoreManager.LocalScores.RemoveRange(MaxScoreCount, ScoreManager.LocalScores.Count - MaxScoreCount);
            }

            // Write score text
            foreach (ScoreWebInterface.ScoreElem scoreElem in ScoreManager.LocalScores)
            {
                // Highlight personal scores (if present in Leaderboard)
                string scoreLine = "<b>" + scoreElem.name + "</b>\t" + scoreElem.length + "cm\t<b>" + scoreElem.score + "</b>\n";
                if (scoreElem.ID == ScoreManager.CurrentID)
                {
                    scoreLine = "<color=red>" + scoreLine + "</color>";
                }
                ScoreText.text += scoreLine;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public IEnumerator InputPlayerNameMobile()
    {
        yield return new WaitForSeconds(2f);
        inputNameField.gameObject.SetActive(true);
        keyboard = TouchScreenKeyboard.Open(AndroidInputTexts[Random.Range(0, AndroidInputTexts.Length)], TouchScreenKeyboardType.Default);
        while (keyboard.status == TouchScreenKeyboard.Status.Visible)
        {
            yield return null;
            inputNameField.text = keyboard.text;
        }
        ScoreManager.CurrentScoreName = StringFormatter.FormatKeyboardName(inputNameField.text);
        print(ScoreManager.CurrentScoreName);
        inputNameField.gameObject.SetActive(false);
    }

    public IEnumerator InputPlayerNameWindows()
    {
        inputNameField.gameObject.SetActive(true);
        while (Input.GetKeyDown(KeyCode.Return) == false)
        {
            yield return null;
        }
        ScoreManager.CurrentScoreName = StringFormatter.FormatKeyboardName(inputNameField.text);
        print(ScoreManager.CurrentScoreName);
        inputNameField.gameObject.SetActive(false);
    }

    public IEnumerator CreateNewUser()
    {
#if UNITY_EDITOR_WIN
        yield return StartCoroutine(InputPlayerNameWindows());
#else
        yield return StartCoroutine(InputPlayerNameMobile());
#endif
        int id = Random.Range(17, 997);
        yield return StartCoroutine(scoreManager.LoadScores(ScoreText));
        int attempt;
        for (attempt=0; attempt<1000; attempt++)
        {
            if (ScoreManager.LocalScores.Exists(e => e.ID == id.ToString())) {
                id = (id+1)%(997-17) + 17;
            } else
            {
                break;
            }
        }
        if (attempt < 1000)
        {
            ScoreManager.CurrentID = id.ToString();
        }
        else
        {
            Debug.LogError("User not created. Tried 1000 times (bad luck, try later!)");
        }
    }
}
