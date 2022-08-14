using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestLevelProgressionManager
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestSetLengthAndScore()
    {
        ScoreManager.SetLengthAndScore(30, 20000);
        Assert.AreEqual("20000", ScoreManager.CurrentScore);
        Assert.AreEqual("30", ScoreManager.CurrentLength);
        Reset();
    }

    [Test]
    public void TestLevelProgressionFromOneToTwo()
    {
        ScoreManager.SetLengthAndScore(30, (int)(LevelProgressionManager.Multiplier * LevelProgressionManager.LvlExpFactor * LevelProgressionManager.LvlExpFactor + 10));
        LevelProgressionManager.MakeLevelProgression(int.Parse(ScoreManager.CurrentScore));
        Assert.AreEqual(new BigInteger(10), LevelProgressionManager.CurrentRelativeXP);
        Assert.AreEqual(2, LevelProgressionManager.CurrentLevel);
        Reset();
    }

    [Test]
    public void TestLevelProgressionFromOneToThree()
    {
        float lvlExpF = LevelProgressionManager.LvlExpFactor;
        int multi = LevelProgressionManager.Multiplier;
        ScoreManager.SetLengthAndScore(30, (int)(multi * (Mathf.Pow(lvlExpF, 2) + Mathf.Pow(lvlExpF, 3)) + 10));
        LevelProgressionManager.MakeLevelProgression(int.Parse(ScoreManager.CurrentScore));
        Assert.AreEqual(new BigInteger(10), LevelProgressionManager.CurrentRelativeXP);
        Assert.AreEqual(3, LevelProgressionManager.CurrentLevel);
        ScoreManager.SetLengthAndScore(30, 0);
        Reset();
    }

    [Test]
    public void TestLevelProgressionFromTwoToFour()
    {
        float lvlExpF = LevelProgressionManager.LvlExpFactor;
        int multi = LevelProgressionManager.Multiplier;
        ScoreManager.SetLengthAndScore(30, (int)(multi * (Mathf.Pow(lvlExpF, 2)) + 20));
        LevelProgressionManager.MakeLevelProgression(int.Parse(ScoreManager.CurrentScore));
        Assert.AreEqual(new BigInteger(20), LevelProgressionManager.CurrentRelativeXP);
        Assert.AreEqual(2, LevelProgressionManager.CurrentLevel);

        ScoreManager.SetLengthAndScore(30, (int)(multi * (Mathf.Pow(lvlExpF, 3) + Mathf.Pow(lvlExpF, 4)) + 30));
        LevelProgressionManager.MakeLevelProgression(int.Parse(ScoreManager.CurrentScore));
        Assert.AreEqual(4, LevelProgressionManager.CurrentLevel);
        Assert.AreEqual(new BigInteger(20 + 30), LevelProgressionManager.CurrentRelativeXP);
        Reset();
    }

    [Test]
    public void TestLevelPercentCompletion()
    {
        float lvlExpF = LevelProgressionManager.LvlExpFactor;
        int multi = LevelProgressionManager.Multiplier;
        ScoreManager.SetLengthAndScore(30, (int)(multi * (Mathf.Pow(lvlExpF, 2)/2)));
        LevelProgressionManager.MakeLevelProgression(int.Parse(ScoreManager.CurrentScore));
        Assert.AreEqual(0.5f, LevelProgressionManager.CompletionPercent);
        Reset();
    }

    [Test]
    public void TestOldLevelPercentCompletion()
    {
        float lvlExpF = LevelProgressionManager.LvlExpFactor;
        int multi = LevelProgressionManager.Multiplier;
        ScoreManager.SetLengthAndScore(30, (int)(multi * (Mathf.Pow(lvlExpF, 2) / 2)));
        LevelProgressionManager.MakeLevelProgression(int.Parse(ScoreManager.CurrentScore));
        Assert.AreEqual(0.5f, LevelProgressionManager.CompletionPercent);
        ScoreManager.SetLengthAndScore(30, (int)(multi * (Mathf.Pow(lvlExpF, 2) / 2)));
        LevelProgressionManager.MakeLevelProgression(int.Parse(ScoreManager.CurrentScore));
        Assert.AreEqual(0f, LevelProgressionManager.CompletionPercent);
        Assert.AreEqual(0.5f, LevelProgressionManager.OldCompletionPercent);
        Reset();
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
    void Reset()
    {
        LevelProgressionManager.CurrentLevel = 1;
        LevelProgressionManager.TotalXP = 0;
    }
}
