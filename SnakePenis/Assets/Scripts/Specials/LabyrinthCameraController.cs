using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabyrinthCameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform target;
    public float speed = 1;
    public float rotationSpeed = 1;
    public bool blocked = false;
    public float cameraHeight = 30;

    void Start()
    {
        target = GameGodSingleton.SnakeMovement.gameObject.transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (blocked || SnakeMovement.isGameOver) return;
        Vector3 cameraTargetPosition = new Vector3(target.position.x, cameraHeight, target.position.z);
        transform.position = Vector3.Slerp(
            transform.position,
            cameraTargetPosition,
            Time.deltaTime * speed
        );

    }
}
