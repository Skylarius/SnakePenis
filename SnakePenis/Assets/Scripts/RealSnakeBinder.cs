using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealSnakeBinder : MonoBehaviour
{
    public SnakeMovement snakeMovementScript;
    public Transform rootTail;
    private List<GameObject> OldSnakeStructureList;
    // Start is called before the first frame update
    void Start()
    {
        OldSnakeStructureList = new List<GameObject>();
        StoreOldStructure();
        UpdateBinder();
    }

    // Update is called once per frame
    public void UpdateBinder()
    {
        Transform snakeChildrenBody = rootTail;
        int snakeBodyIndex = snakeMovementScript.SnakeBody.Count - 1;
        int breakInfiniteLoop = 0;
        while (snakeChildrenBody != null)
        {
            snakeChildrenBody.parent = snakeMovementScript.SnakeBody[snakeBodyIndex].transform;
            snakeChildrenBody.localPosition = Vector3.zero;
            snakeChildrenBody.localRotation = Quaternion.Euler(Vector3.zero);
            snakeBodyIndex = (snakeBodyIndex > 0) ? snakeBodyIndex - 1 : 0;
            Transform[] children = snakeChildrenBody.GetComponentsInChildren<Transform>();
            if (children.Length > 1)
            {
                snakeChildrenBody = children[1];
            } else
            {
                break;
            }
            breakInfiniteLoop++;
            if (breakInfiniteLoop > 1000) break;
        }
    }

    void StoreOldStructure()
    {
        Transform snakeChildrenBody = rootTail;
        int breakInfiniteLoop = 0;
        while (snakeChildrenBody != null)
        {
            OldSnakeStructureList.Add(snakeChildrenBody.gameObject);
            Transform[] children = snakeChildrenBody.GetComponentsInChildren<Transform>();
            snakeChildrenBody = (children.Length > 1) ? children[1] : null;
            breakInfiniteLoop++;
            if (breakInfiniteLoop > 1000) break;
        }
    }

    public void ResetOldStructure()
    {
        Transform snakeChildrenBody = OldSnakeStructureList[OldSnakeStructureList.Count -1].transform;
        int breakInfiniteLoop = 0;
        for (int i= OldSnakeStructureList.Count - 2; i>-1; i--)
        {
            //snakeChildrenBody.DetachChildren();
            snakeChildrenBody.parent = OldSnakeStructureList[i].transform;
            snakeChildrenBody = OldSnakeStructureList[i].transform;
            breakInfiniteLoop++;
            if (breakInfiniteLoop > 1000) break;
        }
    }
}
