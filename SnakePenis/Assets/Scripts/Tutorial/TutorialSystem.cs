using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class TutorialSystem : MonoBehaviour, IDataPersistence
{
    [SerializeField]
    TutorialHintScriptableObject[] TutorialHints;
    private SnakeMovement snakeMovement;
    [SerializeField]
    bool UseSaveTutorialData = false;
    private List<string> CompletedTutorials = new List<string>();
    public bool DoDisable = false;

    public TutorialHintScriptableObject CurrentHintRunning;


    private void Start()
    {
        // Init all events
        snakeMovement = GameGodSingleton.SnakeMovement;
        snakeMovement.OnPowerUpTakenEvent += (GameObject g) => OnEventTriggeredTutorial(SnakeEvents.OnPowerUpTaken, g);
        snakeMovement.OnDeathEvent += () => { OnEventTriggeredTutorial(SnakeEvents.OnDeath, -1); DeathComplementaryCallback(); };
        snakeMovement.OnSnakeActionEvent += (SnakeAction sa) => OnEventTriggeredTutorial(SnakeEvents.OnSnakeAction, sa);
        GameGodSingleton.NotificationSystem.OnNotificationExecuted += (INotification n) => OnEventTriggeredTutorial(SnakeEvents.OnNotificationComplete, n);

        //Init all hints
        foreach (TutorialHintScriptableObject hint in TutorialHints) 
        {
            if (hint.hintTemplateController == null)
            {
                hint.hintTemplateController = GameGodSingleton.SubtitleHintTemplateController;
            }
            if (UseSaveTutorialData)
            {
                if (CompletedTutorials.Contains(hint.UID) == false) //If the tutorial has not been completed
                {
                    hint.Reset();
                }
            } else
            {
                hint.Reset();
            }
        }
        UpdateTutorialHintsNotifications();
    }

    void DeathComplementaryCallback()
    {
        HideTutorialScreen();
        if (!CurrentHintRunning.DoNotHideTutorialIfDie) 
        {
            DoDisable = true;
        }
        Time.timeScale = 1.0f;
    }

    void HideTutorialScreen()
    {
        foreach (TutorialHintScriptableObject hint in TutorialHints)
        {
            if (!hint.DoNotHideTutorialIfDie)
            {
                hint.hintTemplateController.gameObject.SetActive(false);
            }
        }
    }

    void UpdateTutorialHintsNotifications()
    {
        for (int i = 0; i < TutorialHints.Length; i++)
        {
            TutorialHintScriptableObject hint = TutorialHints[i];
            // Notify all the time-based from the first non-complete to the one before the event triggered one
            if (hint.IsComplete)
            {
                //if completed but less than RepeatTimes times
                if (hint.IsRepeating())
                {
                    hint.SetNotCompleted();
                }
                else
                {
                    continue;
                }
            }

            if (hint.type == TutorialType.TimeTriggered)
            {
                hint.Notify();
                CurrentHintRunning = hint;
                if (hint.SlowMotion)
                {
                    StartCoroutine(hint.SlowMotionUntilComplete());
                }
            }
            else if (hint.type == TutorialType.EventTriggered)
            {
                break;
            }
        }
    }

    void OnEventTriggeredTutorial<T>(SnakeEvents snakeEvent, T payload)
    {
        if (DoDisable)
        {
            return;
        }
        for (int i = 0; i < TutorialHints.Length; i++)
        {
            TutorialHintScriptableObject hint = TutorialHints[i];
            
            if (hint.IsComplete)
            {
                //if completed but less than RepeatTimes times
                if (hint.IsRepeating())
                {
                    hint.SetNotCompleted();
                } else
                {
                    continue;
                }
            }

            if (hint.type != TutorialType.EventTriggered)
            {
                Debug.Log("Tutorial will not start, as another non-event-triggered is due next");
                break;
            }

            if (hint.ActivateOnEvent == snakeEvent)
            {
                hint.ActivateCallback(); //Set the tutorial ready to be active
                hint.Notify();
                CurrentHintRunning = hint;
                if (hint.IsTimeBasedDeactivation)
                {
                    StartCoroutine(DeactivateHintAfter(hint, hint.DeactivateAfter));
                }
                if (hint.SlowMotion)
                {
                    StartCoroutine(hint.SlowMotionUntilComplete());
                }
                break;
            }
            if (hint.DeactivateOnEvent == snakeEvent)
            {
                hint.DeactivateCallback(); //Set the tutorial to be inactive (completed)
                UpdateTutorialHintsNotifications();
                break;
            }
        }
    }

    IEnumerator DeactivateHintAfter(TutorialHintScriptableObject hint, float duration)
    {
        yield return new WaitForSeconds(duration);
        hint.DeactivateCallback();
        UpdateTutorialHintsNotifications();
    }

    public void LoadData(GameData data)
    {
        if (!UseSaveTutorialData)
        {
            return;
        }
        foreach(string uid in data.CompletedTutorials)
        {
            CompletedTutorials.Add(uid);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (!UseSaveTutorialData)
        {
            return;
        }
        CompletedTutorials.Clear();
        List<string> unsavedCompletedTutorials = new List<string>();
        foreach (TutorialHintScriptableObject hint in TutorialHints)
        {
            if (hint.IsComplete)
            {
                if (hint.DisableSavingAfterThisTutorialCompletes)
                {
                    unsavedCompletedTutorials.Add(hint.UID);
                } else
                {
                    foreach (string uid in unsavedCompletedTutorials)
                    {
                        CompletedTutorials.Add(uid);
                    }
                    unsavedCompletedTutorials.Clear();
                    CompletedTutorials.Add(hint.UID);
                }
            }
        }
        data.CompletedTutorials = CompletedTutorials;

    }
}
