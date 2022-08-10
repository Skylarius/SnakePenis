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
    public List<string> PenisQuotes;
    private ScoreManager scoreManager;
    public InputField inputNameField;

    public string AndroidInputText = "text";
    private TouchScreenKeyboard keyboard;

    // Start is called before the first frame update
    void Start()
    {
        scoreManager = GetComponent<ScoreManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator StartGameOverProcedure()
    {
        ScoreText.text = "";
        GameOverUI.SetActive(true);
        PenisQuoteUI.text = "Dice il saggio: \"<b>" + PenisQuotes[UnityEngine.Random.Range(0, PenisQuotes.Count)] + "</b>\"";
        PenisQuoteUI.text += "\nMassima Erezione : <b>" + ScoreManager.CurrentLength + " cm</b>";
        PenisQuoteUI.text += "\nPunteggio : <b>" + ScoreManager.CurrentScore + "</b>";
        if (ScoreManager.CurrentScoreName == "")
        {
#if UNITY_EDITOR_WIN
            yield return StartCoroutine(InputPlayerNameWindows());
#else
            yield return StartCoroutine(InputPlayerNameMobile());
#endif
            ScoreManager.CurrentID = Random.Range(17, 997).ToString();
        }
        if (ScoreManager.CurrentScoreName != "")
        {
            yield return StartCoroutine(scoreManager.CommitScores(ScoreText));
        }
        yield return StartCoroutine(scoreManager.LoadScores(ScoreText));
        if (ScoreManager.LocalScores.Count > 0)
        {
            ScoreText.text = "";
            foreach (ScoreWebInterface.ScoreElem scoreElem in ScoreManager.LocalScores)
            {
                ScoreText.text += ("<b>" + scoreElem.name + "</b>\t" + scoreElem.length + "cm\t<b>" + scoreElem.score + "</b>\n");
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public IEnumerator InputPlayerNameMobile()
    {
        inputNameField.gameObject.SetActive(true);
        keyboard = TouchScreenKeyboard.Open(AndroidInputText, TouchScreenKeyboardType.Default);
        while (keyboard.status == TouchScreenKeyboard.Status.Visible)
        {
            yield return null;
            inputNameField.text = keyboard.text;
        }
        ScoreManager.CurrentScoreName = FormatKeyboardName(inputNameField.text);
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
        ScoreManager.CurrentScoreName = FormatKeyboardName(inputNameField.text);
        print(ScoreManager.CurrentScoreName);
        inputNameField.gameObject.SetActive(false);
    }

    string FormatKeyboardName(string name)
    {
        return name.Replace("-", "");
    }
}
