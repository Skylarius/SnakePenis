using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject PortalIN, PortalOUT;
    public GameObject SnakeHead;
    public GameObject SnakeMesh;

    [Header("Settings")]
    public float TailDistanceToDestroyCopiedSnake = 7f;
    //private GameObject SnakeMeshCopy;
    private SnakeMovement snakeMovement;
    private bool isPortalCoroutineRunning = false;
    void Start()
    {
        snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == SnakeHead && isPortalCoroutineRunning == false)
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
        }
        newSnakeMovement.Tail = newSnakeMovement.SnakeBody[newSnakeMovement.SnakeBody.Count - 1];
        newSnakeMovement.LeftBall = newSnakeMovement.Tail.transform.Find("LeftBall").gameObject;
        newSnakeMovement.RightBall = newSnakeMovement.Tail.transform.Find("RightBall").gameObject;

        //Activate SnakeMeshCopy and set it up to the newSnake
        GameObject SnakeMeshCopy = Instantiate(SnakeMesh);
        NewSnake.GetComponent<RealSnakeBinder>().AttachNewSnakeMesh(SnakeMeshCopy);

        //Reset SwallowWaveGenerstor to make it work again
        SwallowWaveGenerator swallowWaveGenerator = NewSnake.GetComponent<SwallowWaveGenerator>();
        if (swallowWaveGenerator)
        {
            swallowWaveGenerator.RestartSwallowAnimationCoroutine();
        }

        // Teleport all SNAKE to PORTAL OUT
        SnakeHead.transform.position = PortalOUT.transform.position;
        snakeMovement.targetRealPosition = Vector2.right * PortalOUT.transform.position.x + Vector2.up * PortalOUT.transform.position.z;
        snakeMovement.direction = oldDirection;
        foreach (GameObject body in snakeMovement.SnakeBody)
        {
            body.transform.position = PortalOUT.transform.position;
        }
        yield return new WaitForSeconds(0.1f);
        SnakeHead.GetComponent<Collider>().enabled = true;

        //Wait until IN snake is entirely in the portal
        while (IsAllSnakeInPortal(newSnakeMovement.Tail) == false)
        {
            newSnakeMovement.direction = Vector2.zero;
            yield return new WaitForEndOfFrame();
        }

        //Destroy all newSnake (which was just a copy)
        NewSnake.GetComponent<RealSnakeBinder>().ResetOldStructure();
        foreach (GameObject body in newSnakeMovement.SnakeBody)
        {
            Destroy(body);
        }
        Destroy(SnakeMeshCopy);
        Destroy(NewSnake);
        isPortalCoroutineRunning = false;
    }

    private void SetupNewSnake(GameObject newSnake)
    {
        newSnake.GetComponent<InputHandler>().enabled = false;
        newSnake.GetComponent<AudioSystem>().enabled = false;
        foreach (BaseSnakeComponent baseSnakeComponent in newSnake.GetComponents<BaseSnakeComponent>())
        {
            baseSnakeComponent.ResetSnakeMovementScript();
        }
    }

    bool IsAllSnakeInPortal(GameObject SnakeTail)
    {
        return Vector3.Distance(SnakeTail.transform.position, PortalIN.transform.position) < TailDistanceToDestroyCopiedSnake;
    }
}
