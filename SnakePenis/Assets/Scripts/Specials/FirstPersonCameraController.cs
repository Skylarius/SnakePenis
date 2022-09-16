using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 offset;
    public Transform target;
    private Transform anchor;
    public float speed = 1;
    public float rotationSpeed = 1;
    public float distanceFromPlayerMultiplier = 1;
    public bool blocked = false;
    public float getCloserThreshold = 1;
    public float cameraHeight = 0;

    void Start()
    {
        target = GameGodSingleton.SnakeMovement.gameObject.transform;
        anchor = target;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (blocked) return;
        transform.position = Vector3.Slerp(
            transform.position,
            CameraTargetPosition(),
            Time.deltaTime * speed
        );

        Vector3 targetPoint = target.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetPoint, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );

    }

    Vector3 CameraTargetPosition()
    {
        Vector3 cameraTargetPosition;
        Vector3 newOffset = (target.position - anchor.position).normalized;
        newOffset *= (Vector3.right * offset.x + Vector3.up * offset.z).magnitude;
        //cameraTargetPosition = target.position + target.TransformVector(offset * distanceFromPlayerMultiplier) + Vector3.up * cameraHeight;
        cameraTargetPosition = target.position + newOffset * distanceFromPlayerMultiplier + Vector3.up * offset.y * cameraHeight;
        return cameraTargetPosition;
    }
}
