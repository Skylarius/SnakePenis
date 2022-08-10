using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static List<ScoreWebInterface.ScoreElem> LocalScores;
    public static string CurrentID = "";
    public static string CurrentScoreName = "";
    public static string CurrentLength = "";
    public static string CurrentScore = "";

    private void Start()
    {
        LocalScores = new List<ScoreWebInterface.ScoreElem>();
    }
    public IEnumerator LoadScores(Text scoreText=null)
    {
        // Fetch the global high score list from database
        if (scoreText)
        {
            scoreText.text = "Caricamento punteggio...";
        }
        int returnCode = -1;
        yield return StartCoroutine(ScoreWebInterface.GetScores(status => returnCode = status));

        if (returnCode == 0)
        {
            LocalScores.Clear();
            LocalScores = ScoreWebInterface.Scores;
        }
        else if (returnCode == 1)
        {
            print("Error in loading scores");
            if (scoreText)
            {
                scoreText.text = "Errore caricamento punteggio";
            }
            yield return 2f;
        }
    }

    public IEnumerator CommitScores(Text scoreText = null)
    {
        if (scoreText)
        {
            scoreText.text = "Salvataggio nuovo punteggio...";
        }
        int returnCode = -1;
        yield return StartCoroutine(ScoreWebInterface.PostScores(CurrentID, CurrentScoreName, int.Parse(CurrentScore), int.Parse(CurrentLength), status => returnCode = status));
        if (returnCode == 0)
        {
            print("Success: score committed");
            if (scoreText)
            {
                scoreText.text = "Punteggio Salvato";
            }
            yield return 1f;
        }
        if (returnCode == 1)
        {
            print("Error in commiting scores");
            if (scoreText)
            {
                scoreText.text = "Errore salvataggio punteggio";
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
}