using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleHighlighter : BaseUIComponent
{
    private Transform TargetToPoint;
    private string TagOfTargetToPoint;
    private RectTransform Rect;
    private RectTransform CanvasRect;
    [SerializeField]
    Vector2 offset = Vector2.zero;
    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private float angularSpeed = 5;
    private Animator animator;
    private float Rotation = 0;

    private void Start()
    {
        Rect = GetComponent<RectTransform>();
        CanvasRect = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>();
        animator = GetComponent<Animator>();
    }

    public void SetTarget(Transform target)
    {
        TargetToPoint = target;
    }

    public void SetTagOfGenericTarget(string tag)
    {
        TagOfTargetToPoint = tag;
    }

    /// <summary>
    /// Update TargetToPoint from tag. 
    /// REMEMBER: the object must also have a component deriving from BaseObjectComponent
    /// </summary>
    private void UpdateTargetFromTag()
    {
        if (string.IsNullOrEmpty(TagOfTargetToPoint))
        {
            return;
        }
        // Get all gameobjects with tag
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(TagOfTargetToPoint);
        foreach (GameObject g in gameObjects)
        {
            // Check if they have
            BaseObjectComponent baseComponent = g.GetComponent<BaseObjectComponent>();
            if (baseComponent == null || !baseComponent.IsActiveInGame())
            {
                continue;
            }
            TargetToPoint = g.transform;
            return;
        }
        TargetToPoint = null;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        SetActiveInGame(true);
    }

    public void Activate(Transform target)
    {
        TargetToPoint = target;
        Activate();
    }

    public void Deactivate()
    {
        TargetToPoint = null;
        SetActiveInGame(false);
    }

    private void LateUpdate()
    {
        AnimatorSM();
        if (!IsActiveInGame())
        {
            return;
        }
        if (TargetToPoint == null || TargetToPoint.gameObject.GetComponent<BaseObjectComponent>().IsActiveInGame() ==  false)
        {
            UpdateTargetFromTag();
            if (TargetToPoint == null)
            {
                return;
            }
        }
        Camera cam = (Camera.current == null) ? Camera.main : Camera.current; 
        Vector2 ViewportPosition = cam.WorldToViewportPoint(TargetToPoint.position);
        Vector2 WorldObject_ScreenPosition = new Vector2(
            (ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f),
            (ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)
        );

        // calculate speed
        Vector2 vectorDistance = WorldObject_ScreenPosition + offset - Rect.anchoredPosition;
        if (vectorDistance.magnitude < speed)
        {
            Rect.anchoredPosition = WorldObject_ScreenPosition + offset;
        } else
        {
            Rect.anchoredPosition += vectorDistance.normalized * speed;
        }

        Rotation = (int)WorldObject_ScreenPosition.x % 2 != 0 ? 90 : 0;
        Rect.localEulerAngles = Vector3.forward * Mathf.LerpAngle(Rect.localEulerAngles.z, Rotation, (Mathf.Abs(vectorDistance.x) > 1 ) ? 1/ Mathf.Abs(vectorDistance.x) : 1);
    }

    void OnFadeOutAnimationEnd() // Called by animation trigger
    {
        gameObject.SetActive(false);
    }

    void AnimatorSM()
    {
        animator.SetBool("IsActive", IsActiveInGame());
    }
}
