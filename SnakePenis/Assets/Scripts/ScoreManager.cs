using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour, IDataPersistence
{
    public static List<ScoreWebInterface.ScoreElem> LocalScores;
    public static string CurrentID = "";
    public static string CurrentScoreName = "";
    public static string CurrentLength = "";
    public static string CurrentScore = "";

    public static string ScoreBeforeBonuses = "";

    public string Name
    {
        get { 
            return CurrentScoreName;
        }
    }

    private void Start()
    {
        LocalScores = new List<ScoreWebInterface.ScoreElem>();
    }
    public IEnumerator LoadScores(Text scoreText=null)
    {
        // Fetch the global high score list from database
        if (scoreText)
        {
            scoreText.text = LocalizedStringUser.GetLocalizedUIString("LOADING_SCORE");
        }
        int returnCode = -1;
        yield return StartCoroutine(ScoreWebInterface.GetScores(status => returnCode = status));

        if (returnCode == 0)
        {
            scoreText.text = "";
            LocalScores.Clear();
            LocalScores = ScoreWebInterface.Scores;
        }
        else if (returnCode == 1)
        {
            print("Error in loading scores");
            if (scoreText)
            {
                scoreText.text = LocalizedStringUser.GetLocalizedUIString("ERROR_LOADING_SCORE");
            }
            yield return 2f;
        }
    }

    public IEnumerator CommitScores(Text scoreText = null)
    {
        if (scoreText)
        {
            scoreText.text = LocalizedStringUser.GetLocalizedUIString("SAVING_NEW_SCORE");
        }
        int returnCode = -1;
        yield return StartCoroutine(ScoreWebInterface.PostScores(CurrentID, CurrentScoreName, int.Parse(CurrentScore), int.Parse(CurrentLength), status => returnCode = status));
        if (returnCode == 0)
        {
            print("Success: score committed");
            if (scoreText)
            {
                scoreText.text = LocalizedStringUser.GetLocalizedUIString("SCORE_SAVED"); ;
            }
            yield return 1f;
        }
        if (returnCode == 1)
        {
            print("Error in commiting scores");
            if (scoreText)
            {
                scoreText.text = LocalizedStringUser.GetLocalizedUIString("ERROR_SAVING_SCORE");
            }
            yield return 2f;
        }
    }

    public IEnumerator ChangeName(Text infoText = null)
    {
        // Fetch the global high score list from database
        if (infoText)
        {
            infoText.text = LocalizedStringUser.GetLocalizedUIString("UPDATING_NAME");
        }
        int returnCode = -1;
        yield return StartCoroutine(ScoreWebInterface.UpdateName(CurrentID, CurrentScoreName, status => returnCode = status));
        if (returnCode == 0)
        {
            print("Success: name updated");
            if (infoText)
            {
                infoText.text = LocalizedStringUser.GetLocalizedUIString("NAME_UPDATED");
            }
            yield return 2f;
        }
        if (returnCode == 1)
        {
            print("Errore aggiornamento nome");
            if (infoText)
            {
                infoText.text = LocalizedStringUser.GetLocalizedUIString("ERROR_UPDATING_NAME");
            }
            yield return 2f;
        }
    }

    public ScoreWebInterface.ScoreElem CreateNewPlayerScore()
    {
        return new ScoreWebInterface.ScoreElem(ScoreManager.CurrentID, ScoreManager.CurrentScoreName, ScoreManager.CurrentLength.ToString(), ScoreManager.CurrentScore.ToString());
    }

    public static void SetLengthAndScore(int length, int score)
    {
        CurrentLength = length.ToString();
        CurrentScore = score.ToString();
    }

    public static void AddBonusScore(int bonusScore)
    {
        int currentScore = int.Parse(CurrentScore);
        currentScore += bonusScore;
        CurrentScore = currentScore.ToString();
    }

    void IDataPersistence.LoadData(GameData data)
    {
        CurrentScoreName = data.Name;
        CurrentID = data.PlayerID;
    }

    void IDataPersistence.SaveData(ref GameData data)
    {
        data.Name = CurrentScoreName;
        data.PlayerID = CurrentID;
        Debug.Log($"Saved Score Data: ID={data.PlayerID}, Name={data.Name}");
    }
}
