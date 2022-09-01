using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject PortalINPoint, PortalOUTPoint;
    public GameObject SelfPortalOUTPoint;
    private GameObject SnakeHead;
    public GameObject SnakeMesh;
    public static int PortalUsage;

    [Header("Settings")]
    public float TailDistanceToDestroyCopiedSnake = 10f;
    public float speedSnakeIntoPortal = 10f;

    [Header("Animation")]
    public float SnakeDistanceToTriggerAnimation = 15f;
    public float SnakeDistanceForAnimationToComplete = 4f;
    public float PortalAnimationSpeed = 0.5f;
    private float animationValue = 0f;
    //private GameObject SnakeMeshCopy;
    private SnakeMovement snakeMovement;
    private PortalManager portalOUTManager;
    private bool isPortalCoroutineRunning = false;

    [System.Serializable]
    public struct PortalSimpleAnimation
    {
        public Transform ObjectToAnimate;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public Vector3 StartRotationEuler;
        public Vector3 EndRotationEluer;
    }

    public PortalSimpleAnimation[] PortalAnimation;

    void Start()
    {
        snakeMovement = GameGodSingleton.Instance.SnakeMovement;
        SnakeHead = snakeMovement.gameObject;
        portalOUTManager = PortalOUTPoint.GetComponent<PortalManager>();
        PortalUsage = 0;
    }

    private void Update()
    {
        // Portal Animation
        float distance = Vector3.Distance(transform.position, SnakeHead.transform.position);
        float finalValue = 1 - (distance - SnakeDistanceForAnimationToComplete) / SnakeDistanceToTriggerAnimation;
        finalValue = Mathf.Clamp(finalValue, 0, 1);
        if (isPortalCoroutineRunning == false) 
        { 
            animationValue = Mathf.Lerp(animationValue, finalValue, Time.deltaTime * PortalAnimationSpeed);
        }
        for (int i = 0; i < PortalAnimation.Length; i++)
        {
            PortalAnimation[i].ObjectToAnimate.localPosition = Vector3.Lerp(
                PortalAnimation[i].StartPosition,
                PortalAnimation[i].EndPosition,
                animationValue
                );
            PortalAnimation[i].ObjectToAnimate.localEulerAngles = Vector3.Lerp(
                PortalAnimation[i].StartRotationEuler,
                PortalAnimation[i].EndRotationEluer,
                animationValue
                );
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == SnakeHead && isPortalCoroutineRunning == false && SnakeMovement.isGameOver == false)
        {
            StartCoroutine(PortalCoroutine());
        }
    }

    IEnumerator PortalCoroutine()
    {
        isPortalCoroutineRunning = true;
        Vector2 oldDirection = snakeMovement.direction;
        SnakeHead.GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(0.2f);

        //Create new snake;
        GameObject NewSnake = Instantiate(SnakeHead);

        SetupNewSnake(NewSnake);

        SnakeMovement newSnakeMovement = NewSnake.GetComponent<SnakeMovement>();

        // Copy all the body for the new snake
        newSnakeMovement.SnakeBody[0] = NewSnake;
        for (int i = 1; i < snakeMovement.SnakeBody.Count; i++)
        {
            newSnakeMovement.SnakeBody[i] = Instantiate(snakeMovement.SnakeBody[i]);
            RemovePreviousBoneFromSnakeBody(newSnakeMovement.SnakeBody[i]);
        }
        newSnakeMovement.Tail = newSnakeMovement.SnakeBody[newSnakeMovement.SnakeBody.Count - 1];
        newSnakeMovement.LeftBall = newSnakeMovement.Tail.transform.Find("LeftBall").gameObject;
        newSnakeMovement.RightBall = newSnakeMovement.Tail.transform.Find("RightBall").gameObject;

        //Set unative the tail of the original snake to make it invisible
        snakeMovement.Tail.SetActive(false);

        //Activate SnakeMeshCopy and set it up to the newSnake
        GameObject SnakeMeshCopy = Instantiate(SnakeMesh);
        SetupNewSnakeMesh(NewSnake, SnakeMeshCopy);

        //Reset SwallowWaveGenerstor to make it work again
        //SwallowWaveGenerator swallowWaveGenerator = NewSnake.GetComponent<SwallowWaveGenerator>();
        //if (swallowWaveGenerator)
        //{
        //    swallowWaveGenerator.RestartSwallowAnimationCoroutine();
        //}

        //If PORTAL OUT has a PortalManager Component, disable it and re-enable it at the end
        if (portalOUTManager)
        {
            portalOUTManager.enabled = false;
        }

        // Teleport all SNAKE to PORTAL OUT
        snakeMovement.SetBodyTargetPositionsToValue(PortalOUTPoint.transform.position);
        foreach (GameObject body in snakeMovement.SnakeBody)
        {
            body.transform.position = PortalOUTPoint.transform.position;
        }
        SnakeHead.transform.position = PortalOUTPoint.transform.position;
        snakeMovement.targetRealPosition = Vector2.right * PortalOUTPoint.transform.position.x + Vector2.up * PortalOUTPoint.transform.position.z;
        snakeMovement.direction = (Vector2.right * PortalOUTPoint.transform.forward.x + Vector2.up * PortalOUTPoint.transform.forward.z);
        snakeMovement.BlockInputForSnake(true);
        yield return new WaitForSeconds(0.1f);
        SnakeHead.GetComponent<Collider>().enabled = true;
        snakeMovement.BlockInputForSnake(false);

        //Wait until IN snake is entirely in the portal
        while (IsAllSnakeInPortal(newSnakeMovement.Tail) == false)
        {
            newSnakeMovement.direction = Vector2.zero;
            newSnakeMovement.targetRealPosition = Vector2.right * PortalINPoint.transform.position.x + Vector2.up * PortalINPoint.transform.position.z;
            //Slightly help Tail get closer to PortalINPoint...
            newSnakeMovement.Tail.transform.position = Vector3.Lerp(newSnakeMovement.Tail.transform.position,
                PortalINPoint.transform.position,
                0.001f);
            yield return new WaitForEndOfFrame();
        }
        //Set the tail visible again once ALL the snake copy is inside the portal
        snakeMovement.Tail.SetActive(true);

        //Destroy all newSnake (which was just a copy)
        NewSnake.GetComponent<RealSnakeBinder>().ResetOldStructure();
        foreach (GameObject body in newSnakeMovement.SnakeBody)
        {
            Destroy(body);
        }
        Destroy(SnakeMeshCopy);
        Destroy(NewSnake);
        if (portalOUTManager)
        {
            portalOUTManager.enabled = true;
        }
        isPortalCoroutineRunning = false;
        PortalUsage++;
    }

    private void SetupNewSnake(GameObject newSnake)
    {
        newSnake.GetComponent<InputHandler>().enabled = false;
        newSnake.GetComponent<AudioSystem>().enabled = false;
        foreach (BaseSnakeComponent baseSnakeComponent in newSnake.GetComponents<BaseSnakeComponent>())
        {
            baseSnakeComponent.ResetSnakeMovementScript();
        }
        newSnake.GetComponent<SnakeMovement>().targetRealPosition = Vector2.right * PortalINPoint.transform.position.x + Vector2.up * PortalINPoint.transform.position.z;
        //newSnake.GetComponent<SnakeMovement>().direction = (PortalINPoint.transform.position - newSnake.transform.position).normalized;
    }

    private void SetupNewSnakeMesh(GameObject NewSnake, GameObject NewSnakeMesh)
    {
        NewSnake.GetComponent<RealSnakeBinder>().SnakeMeshObj = NewSnakeMesh;
        RealSnakeBinder originalRealSnakeBinder = SnakeHead.GetComponent<RealSnakeBinder>();

        NewSnakeMesh.GetComponentInChildren<Renderer>().material = originalRealSnakeBinder.SnakeMeshObj.GetComponentInChildren<Renderer>().material;
        NewSnake.GetComponent<RealSnakeBinder>().AttachNewSnakeMesh(NewSnakeMesh);
    }

    bool IsAllSnakeInPortal(GameObject SnakeTail)
    {
        return Vector3.Distance(SnakeTail.transform.position, PortalINPoint.transform.position) < TailDistanceToDestroyCopiedSnake;
    }

    void RemovePreviousBoneFromSnakeBody(GameObject body)
    {
        foreach (Transform child in body.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.name.Contains("Bone"))
            {
                child.parent = null;
                Destroy(child.gameObject);
            }
        }
    }
}
