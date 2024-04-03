using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Numerics;

public class ProgressBarBehaviour : BaseUIComponent
{
    // Start is called before the first frame update
    public GameObject ProgressBarSprite;
    public SettingsPanelManager settingsPanelManager;
    public int initialBarPositionX = -700;
    public int finalBarPositionX = 0;
    public Text LevelXPText;
    public Text LevelXPTitleText;
    public float LevelBarTime = 4f;
    public GameObject NewLevelBubble;
    void Start()
    {
        StartCoroutine(ProgressBarAnimation());
    }

    IEnumerator ProgressBarAnimation()
    {
        int oldLevel = LevelProgressionManager.OldLevel;
        int currentLevel = LevelProgressionManager.CurrentLevel;
        float oldCompletion = LevelProgressionManager.OldCompletionPercent;
        float currentCompletion = LevelProgressionManager.CompletionPercent;
        ProgressBarSprite.GetComponent<RectTransform>().localPosition = UnityEngine.Vector3.right * GetBarPositionX(oldCompletion);
        LevelXPText.text = LocalizedStringUser.GetLocalizedUIString("LEVEL") + ": <b>" + oldLevel + "</b>";
        LevelXPText.text += "\nEXP : <b>" + LevelProgressionManager.OldLevelRelativeXP + " / " + LevelProgressionManager.GetLevelRelativeXP(oldLevel + 1) + "</b>";
        yield return new WaitForSeconds(3f);

        int level = oldLevel;
        while (level <= currentLevel)
        {
            int fromPositionX = initialBarPositionX;
            int toPositionX = finalBarPositionX;
            BigInteger fromXP = BigInteger.Zero;
            BigInteger toXP = LevelProgressionManager.GetLevelRelativeXP(level + 1);
            BigInteger nextLevelXP = toXP;
            BigInteger XP = toXP;
            if (level == oldLevel)
            {
                fromPositionX = GetBarPositionX(oldCompletion);
                fromXP = LevelProgressionManager.OldLevelRelativeXP;
            }
            if (level == currentLevel)
            {
                toPositionX = GetBarPositionX(currentCompletion);
                toXP = LevelProgressionManager.CurrentRelativeXP;
            }
            for (float t = 0f; t < LevelBarTime; t += (Time.deltaTime * (currentLevel - level + 1)))
            {
                ProgressBarSprite.GetComponent<RectTransform>().localPosition = UnityEngine.Vector3.Lerp(
                    UnityEngine.Vector3.right * fromPositionX,
                    UnityEngine.Vector3.right * toPositionX,
                    GetProgressBarTimeParabola(t)
                    );
                XP = new BigInteger(Mathf.Lerp((int)fromXP, (int)toXP, GetProgressBarTimeParabola(t)));
                LevelXPText.text = LocalizedStringUser.GetLocalizedUIString("LEVEL") + ": <b>" + level + "</b>";
                LevelXPText.text += "\nEXP : <b>" + XP + " / " + nextLevelXP + "</b>";
                yield return new WaitForEndOfFrame();
            }
            level++;
            if (level <= currentLevel)
            {
                yield return StartCoroutine(NewLevelAnimation(level));
            }
        }
    }

    IEnumerator NewLevelAnimation(int level)
    {
        NewLevelBubble.GetComponentInChildren<Text>().text = LocalizedStringUser.GetLocalizedStringWithArray("NEW_LEVEL", level.ToString());
        NewLevelBubble.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        NewLevelBubble.SetActive(false);
        SettingsPanelManager.BaseSpecial special = settingsPanelManager.GetSpecialAtLevel(level);
        if (special != null)
        {
            NewLevelBubble.GetComponentInChildren<Text>().text = LocalizedStringUser.GetLocalizedStringWithArray("NEW_UNLOCKABLE", special.TitleOnButton.GetLocalizedString());
            NewLevelBubble.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            NewLevelBubble.SetActive(false);
        }
        NewLevelBubble.GetComponentInChildren<Text>().text = "";
    }

    float GetProgressBarTimeParabola(float x)
    {
        return (2 * LevelBarTime * x - x * x) / (LevelBarTime * LevelBarTime);
    }

    int GetBarPositionX(float completionPercent)
    {
        return (int)Mathf.Lerp(initialBarPositionX, finalBarPositionX, completionPercent);
    }
}
