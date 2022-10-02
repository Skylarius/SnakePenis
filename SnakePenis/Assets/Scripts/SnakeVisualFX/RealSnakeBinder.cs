using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealSnakeBinder : BaseSnakeComponent
{
    public Transform rootTail;
    public GameObject SnakeMeshObj;
    private List<GameObject> OldSnakeStructureList;
    // Start is called before the first frame update
    void Start()
    {
        snakeMovement = GetComponent<SnakeMovement>();
        OldSnakeStructureList = new List<GameObject>();
        StoreOldStructure();
        UnparentAllBonesOfOldStructure();
    }

    [Obsolete("Use UpdateBinderV2")]
    public void UpdateBinder()
    {
        Transform snakeChildrenBody = rootTail;
        int snakeBodyIndex = snakeMovement.SnakeBody.Count - 1;
        int breakInfiniteLoop = 0;
        while (snakeChildrenBody != null)
        {
            snakeChildrenBody.parent = snakeMovement.SnakeBody[snakeBodyIndex].transform;
            snakeChildrenBody.localPosition = Vector3.zero;
            snakeChildrenBody.localRotation = Quaternion.Euler(Vector3.right * 90);
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

    void UnparentAllBonesOfOldStructure()
    {
        if (OldSnakeStructureList == null)
        {
            return;
        }
        for (int i=0; i<OldSnakeStructureList.Count; i++)
        {
            OldSnakeStructureList[i].transform.parent = SnakeMeshObj.transform;
        }
    }

    public void UpdateBinderV2()
    {
        GameObject target;
        for (int i=0; i<OldSnakeStructureList.Count; i++)
        {
            if (i < OldSnakeStructureList.Count - snakeMovement.SnakeBody.Count + 1)
            {
                target = snakeMovement.Tail;
            } else
            {
                int snakeBodyIndex = OldSnakeStructureList.Count - i;
                target = snakeMovement.SnakeBody[snakeBodyIndex];
            }
            OldSnakeStructureList[i].transform.position = target.transform.position;
            OldSnakeStructureList[i].transform.rotation = target.transform.rotation * Quaternion.Euler(Vector3.right * 90);
        }
    }

    public GameObject GetSnakeMeshBoneFromSnakeBodyIndex(int index)
    {
        if (OldSnakeStructureList.Count == 0)
        {
            return null;
        }
        if (index == snakeMovement.SnakeBody.Count - 1)
        {
            return snakeMovement.Tail;
        }
        else
        {
            int i = OldSnakeStructureList.Count - 1 - index;
            if (i < OldSnakeStructureList.Count && i > 0)
            {
                return OldSnakeStructureList[OldSnakeStructureList.Count - 1 - index];
            }
            return null;
        }
    }

    private void LateUpdate()
    {
        UpdateBinderV2();
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
            // snakeChildrenBody.Rotate(Vector3.right * -90);
            snakeChildrenBody.parent = OldSnakeStructureList[i].transform;
            snakeChildrenBody = OldSnakeStructureList[i].transform;
            breakInfiniteLoop++;
            if (breakInfiniteLoop > 1000) break;
        }
    }

    public void AttachNewSnakeMesh(GameObject ObjectWithSnakeMesh)
    {
        GameObject ChildToCheck = ObjectWithSnakeMesh;
        Transform[] ChildrenTransform = ChildToCheck.GetComponentsInChildren<Transform>();
        rootTail = ObjectWithSnakeMesh.transform.Find("RootBone");
        while (rootTail == null && ChildrenTransform.Length > 1)
        {
            ChildToCheck = ChildrenTransform[1].gameObject;
            ChildrenTransform = ChildToCheck.GetComponentsInChildren<Transform>();
            rootTail = ChildToCheck.transform.Find("RootBone");
            //Debug.LogError("No root Tail FOund. Abort.");
        }
        if (OldSnakeStructureList == null)
        {
            OldSnakeStructureList = new List<GameObject>();
        }
        OldSnakeStructureList.Clear();
        StoreOldStructure();
        UpdateBinderV2();
    }

    public void RecalculateMeshBounds()
    {
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer)
        {
            skinnedMeshRenderer.sharedMesh.RecalculateBounds();
        }
    }
}
