using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject PortalIN, PortalOUT;
    public GameObject SnakeHead;
    private SnakeMovement snakeMovement;
    private bool isPortalCoroutineRunning = false;
    private bool isTailInPortalIN = false;
    void Start()
    {
        snakeMovement = SnakeHead.GetComponent<SnakeMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == SnakeHead && isPortalCoroutineRunning == false)
        {
            StartCoroutine(PortalCoroutine());
        }
        else if (other.gameObject == snakeMovement.SnakeBody[snakeMovement.SnakeBody.Count - 1] && isTailInPortalIN == false)
        {
            isTailInPortalIN = true;
        }
    }

    IEnumerator PortalCoroutine()
    {
        isPortalCoroutineRunning = true;
        yield return new WaitForSeconds(0.2f);
        SnakeHead.GetComponent<Collider>().enabled = false;

        //Wait until IN snake is entirely in the portal
        Vector2 oldDirection = snakeMovement.direction;
        while(IsAllSnakeInPortal() == false)
        {
            snakeMovement.direction = Vector2.zero;
            yield return new WaitForEndOfFrame();
        }

        // Teleport all SNAKE to PORTAL OUT
        SnakeHead.transform.position = PortalOUT.transform.position;
        snakeMovement.targetRealPosition = Vector2.right * PortalOUT.transform.position.x + Vector2.up * PortalOUT.transform.position.z;
        snakeMovement.direction = oldDirection;
        foreach (GameObject Body in snakeMovement.SnakeBody)
        {
            Body.transform.position = PortalOUT.transform.position;
        }
        yield return new WaitForSeconds(0.1f);
        SnakeHead.GetComponent<Collider>().enabled = true;
        isPortalCoroutineRunning = false;
    }

    bool IsAllSnakeInPortal()
    {
        if (isTailInPortalIN)
        {
            isTailInPortalIN = false;
            return true;
        }
        return false;
    }
}
