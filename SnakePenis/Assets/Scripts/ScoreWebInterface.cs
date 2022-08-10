using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

public static class ScoreWebInterface
{
    private const string secretKey = "Laola"; // Edit this value and make sure it's the same as the one stored on the server
    public const string addScoreURL = "http://snakepenis.com.preview.services/addscore.php?"; //be sure to add a ? to your url
    public const string highscoreURL = "http://snakepenis.com.preview.services/display.php";

    private static List<ScoreElem> scores;
    public static List<ScoreElem> Scores
    {
        get { return scores; }
    }

    // Send the new score to the database
    public static IEnumerator PostScores(string id, string name, int score, int length, Action<int> returnCode)
    {
        //This connects to a server side php script that will add the name and score to a MySQL DB.
        // Supply it with a string representing the players name and the players score.
        //string hash = BitConverter.ToString((new MD5CryptoServiceProvider()).ComputeHash(Encoding.ASCII.GetBytes(name + score + secretKey)));
        byte[] asciiBytes = Encoding.ASCII.GetBytes(name + score + secretKey);
        byte[] hashedBytes = MD5.Create().ComputeHash(asciiBytes);
        string hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

        string post_url = addScoreURL + "id=" + id + "&name=" + WWW.EscapeURL(name) + "&score=" + score + "&length=" + length + "&hash=" + hash;

        // Post the URL to the site and create a download object to get the result.
        Debug.Log("Submitting score");
        WWW hs_post = new WWW(post_url);
        yield return hs_post; // Wait until the download is done
        Debug.Log("Score submitted");

        if (hs_post.error != null)
        {
            Debug.Log("There was an error posting the high score: " + hs_post.error);
            returnCode(1);
        } else
        {
            returnCode(0);
        }
    }

    public struct ScoreElem
    {
        public ScoreElem(string ID, string name, string length, string score)
        {
            this.ID = ID;
            this.name = name;
            this.length = length;
            this.score = score;
        }
        public string ID;
        public string name;
        public string length;
        public string score;

        public string ToString()
        {
            return ID + ") " + name + ": " + length + " cm - " + score;
        }
    }

    // Get the scores from the database
    public static IEnumerator GetScores(Action<int> returnCode)
    {
        scores = new List<ScoreElem>();
        scores.Add(new ScoreElem(".", "Loading", "Scores", "..."));

        WWW hs_get = new WWW(highscoreURL);
        yield return hs_get;

        if (hs_get.error != null)
        {
            Debug.Log("There was an error getting the high score: " + hs_get.error);
            scores.Clear();
            scores.Add(new ScoreElem("0","ERROR:", "There was an error getting the high score", hs_get.error));
            returnCode(1);
        }
        else
        {
            // split the results into an array
            Regex regex = new Regex(@"[\t\n]");
            string[] rawScores = regex.Split(hs_get.text);

            // Restructure the string array into an array of KeyValuePairs
            scores.Clear();
            int rawScoreIndex = 0;
            for (int i = 0; i < rawScores.Length / 4; i++)
            {
                scores.Add(new ScoreElem(rawScores[rawScoreIndex], rawScores[rawScoreIndex + 1], rawScores[rawScoreIndex + 2], rawScores[rawScoreIndex +3 ]));
                rawScoreIndex += 4;
            }
            returnCode(0);
        }
    }

}