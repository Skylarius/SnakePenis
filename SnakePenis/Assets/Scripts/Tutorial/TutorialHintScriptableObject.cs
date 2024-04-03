using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Events;
using JetBrains.Annotations;

public enum TutorialType
{
    TimeTriggered,
    EventTriggered
}

public enum TemplateType
{
    None,
    HintTemplate,
    SubtitleHintTemplate,
    SmallVisualHintTemplate,
    SaveGameHintTemplate,
    CustomTemplate
}

public enum PointToEnum
{
    None,
    Object,
    Tag
};

[CreateAssetMenu(fileName = "TutorialData", menuName = "ScriptableObjects/TutorialHint", order = 1)]
public class TutorialHintScriptableObject : ScriptableObject, INotification
{
    public string UID;
    [SerializeField]
    public LocalizedString Text;
    [Space]
    [SerializeField]
    public TutorialType type;

    
    [Header("Event Triggered Settings")]
    public SnakeEvents ActivateOnEvent;
    public SnakeEvents DeactivateOnEvent;
    public float DeactivateAfter = 3f;
    public bool IsTimeBasedDeactivation = false;

    [Header("Time Triggered Settings")]
    [SerializeField]
    public float Duration = 3f;
    [SerializeField]
    public bool Freeze = false;

    [Header("Highlight Object")]
    public PointToEnum PointTo;
    public GameObject ObjectToPoint = null;
    public string ObjectToPointTag = "";

    [Header("Misc Settings")]
    [SerializeField]
    public bool SlowMotion = false;
    [SerializeField]
    public int RepeatTimes = 0;
    private int _repetitions = 0;
    [SerializeField]
    public TemplateType TemplateType = TemplateType.SubtitleHintTemplate;
    [SerializeField]
    public GameObject CustomHintTemplate;
    [HideInInspector]
    public HintTemplateController hintTemplateController;
    [Space]
    // Used so if the game is loaded after this tutorial (intermediate) this tutorial will go again, and so the next ones, 
    // Until one doesn't disable it and saves all the previous ones.
    [SerializeField]
    public bool DisableSavingAfterThisTutorialCompletes;
    [SerializeField]
    public bool DoNotHideTutorialIfDie = false;

    private bool _IsComplete = false;
    public bool IsComplete { get { return _IsComplete; } }

    public void ActivateCallback()
    {
        _IsComplete = false;
    }

    public void DeactivateCallback()
    {
        _IsComplete = true;
        if (RepeatTimes > 0)
        {
            ++_repetitions;
        }
    }

    public bool IsRepeating()
    {
        return _repetitions < RepeatTimes;
    }

    public IEnumerator ExecuteCoroutine()
    {
        hintTemplateController.WriteHint(Text.GetLocalizedString());
        hintTemplateController.gameObject.SetActive(true);
        if (type == TutorialType.TimeTriggered)
        {
            if (Freeze)
            {
                float currentTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                yield return new WaitForSecondsRealtime(Duration);
                Time.timeScale = currentTimeScale;
            } else
            {
                yield return new WaitForSeconds(Duration);
            }
            DeactivateCallback();
            GameGodSingleton.TutorialSystem.CurrentHintRunning = null;
        }
        while (!_IsComplete)
        {
            yield return null;
        }
        hintTemplateController.WriteHint("");
        hintTemplateController.gameObject.SetActive(false);
    }

    public IEnumerator SlowMotionUntilComplete()
    {
        if (_IsComplete)
        {
            yield break;
        }
        float start_t = Time.unscaledTime;
        float t = 0;
        while (t <= 1)
        {
            Time.timeScale = Mathf.Lerp(1, 0.3f, t);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Time.timeScale = 0.3f;
        while (!_IsComplete)
        {
            yield return null;
        }
        start_t = Time.unscaledTime;
        t = 0;
        while (t <= 1)
        {
            Time.timeScale = Mathf.Lerp(0.3f, 1f, t);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Time.timeScale = 1f;
    }

    public void Reset()
    {
        _IsComplete = false;
        _repetitions = 0;
    }

    public void SetNotCompleted()
    {
        _IsComplete = false;
    }

    public void Notify()
    {
        // Add to notification system
        GameGodSingleton.NotificationSystem.Enqueue(this);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TutorialHintScriptableObject))]
    class TutorialHintScriptableObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TutorialHintScriptableObject self = (TutorialHintScriptableObject)target;

            List<string> excludedProperties = new List<string>();
            serializedObject.Update();

            if (string.IsNullOrEmpty(self.UID))
            {
                self.UID = GUID.Generate().ToString();
                EditorUtility.SetDirty(self);
            }

            if (self.TemplateType != TemplateType.CustomTemplate)
            {
                excludedProperties.Add("CustomHintTemplate");
            }

            if (self.type == TutorialType.TimeTriggered)
            {
                excludedProperties.Add("ActivateOnEvent");
                excludedProperties.Add("DeactivateOnEvent");
                excludedProperties.Add("DeactivateAfter");
                excludedProperties.Add("IsTimeBasedDeactivation"); 
            }
            else if (self.type == TutorialType.EventTriggered)
            {
                excludedProperties.Add("Duration");
                excludedProperties.Add("Freeze");
                if (self.IsTimeBasedDeactivation)
                {
                    self.DeactivateOnEvent = SnakeEvents.None;
                    excludedProperties.Add("DeactivateOnEvent");
                }
                else
                {
                    excludedProperties.Add("DeactivateAfter");
                }
            }
            if (self.PointTo != PointToEnum.Object)
            {
                excludedProperties.Add("ObjectToPoint");
            }
            if (self.PointTo != PointToEnum.Tag)
            {
                excludedProperties.Add("ObjectToPointTag");
            }
            DrawPropertiesExcluding(serializedObject, excludedProperties.ToArray());
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
