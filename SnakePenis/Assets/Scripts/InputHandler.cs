using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : BaseSnakeComponent
{

    public delegate void MovementType(ref Vector2 direction);
    public MovementType move = null;

    public delegate bool Action();
    public List<Action> actions = null;

    [Header("Jump settings")]
    public Vector3 jumpDirection;
    public float jumpTime = 0.8f;
    public float jumpForce = 10f;
    public float jumpTapRange = 10f;
    private float SqrJumpTapRange { get { return jumpTapRange * jumpTapRange; } }

    public float jumpDoubleTapDeltaTime = 0.3f;
    private bool isJumping = false;
    public static int JumpsAmount;

    [Header("Free 360 degrees movement")]
    public float windowsRotation360Speed = 10f;

    [Header("First person movement")]
    public float AndroidRotationFirstPersonSpeed = 1f;
    private Vector2 AndroidInitialTouchPosition;
    private Vector3 AndroidInitialDirection;
    public float minAndroidSwipeJumpThreshold = 10f;
    private float AndroidInitialJumpTime;
    public float maxAndroidSwipeJumpDeltaTime = 1f;

    private void Start()
    {
        if (move == null)
        {
            SetStandardMovementType();
        }
        if (actions == null)
        {
            actions = new List<Action>();
        }
        snakeMovement = GetComponent<SnakeMovement>();
        JumpsAmount = 0;
    }

    Vector3 GetStandardDirection(ref float x, ref float z, Vector2 direction)
    {
        if (Mathf.Abs(direction.x) < 0.01f)
        {
            direction.x = 0;
        }
        if (Mathf.Abs(direction.y) < 0.01f)
        {
            direction.y = 0;
        }
        if (x == 0f && z == 0f || direction.x * x < 0 || direction.y * z < 0)
        {
            return direction; // false return, to not update direction
        }
        if (x != 0 && z != 0) //Avoid multiple input
        {
            z = 0f;
        }
        if (x != 0f)
        {
            x = x > 0 ? 1 : -1;
        }
        if (z != 0f)
        {
            z = z > 0 ? 1 : -1;
        }
        return Vector2.right * x + Vector2.up * z;
    }

    void StandardWindowsMovement(ref Vector2 direction)
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        direction = GetStandardDirection(ref x, ref z, direction);
    }

    void StandardAndroidMovement(ref Vector2 direction)
    {
        float x = 0f;
        float z = 0f;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    //TODO: Check if it's in the AVOID ZONE (pause, exit)...
                    Vector3 tapPoint = ConvertTouchToPositionInWorld(touch);
                    x = tapPoint.x - transform.position.x;
                    z = tapPoint.z - transform.position.z;
                    if (direction.y != 0)
                    {
                        z = 0f;
                    }
                    if (direction.x != 0)
                    {
                        x = 0f;
                    }
                    direction = GetStandardDirection(ref x, ref z, direction);
                    break;
            }
        }
    }

    void Windows360DegreeMovement(ref Vector2 direction)
    {
        float x = Input.GetAxis("Mouse X");
        Vector3 rotatedVector3 = Quaternion.AngleAxis(x*windowsRotation360Speed*Time.deltaTime, Vector3.up) * (Vector3.right * direction.x + Vector3.forward * direction.y);
        direction = Vector2.right * rotatedVector3.x + Vector2.up * rotatedVector3.z;
    }

    void Android360DegreeMovement(ref Vector2 direction)
    {
        float x = 0f;
        float z = 0f;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    //TODO: Check if it's in the AVOID ZONE (pause, exit)...
                    Vector3 tapPoint = ConvertTouchToPositionInWorld(touch);
                    x = tapPoint.x - transform.position.x;
                    z = tapPoint.z - transform.position.z;
                    Vector2 dir = Vector2.right * x + Vector2.up * z;
                    dir.Normalize();
                    direction = dir;
                    break;
            }
        }
    }
    private void WindowsFirstPersonMovement(ref Vector2 direction)
    {
        Windows360DegreeMovement(ref direction);
    }

    void AndroidFirstPersonMovement(ref Vector2 direction)
    {
        float x = 0f;
        float z = 0f;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    //TODO: Check if it's in the AVOID ZONE (pause, exit)...
                    Vector3 tapDirection = ConvertTouchToDirectionInWorld(touch);
                    //x = tapDirection.x - transform.position.x;
                    //z = tapDirection.z - transform.position.z;
                    Vector2 dir = Vector2.right * tapDirection.x + Vector2.up * tapDirection.z;
                    dir.Normalize();
                    direction = dir;
                    break;
            }
        }
    }

    void AndroidFirstPersonMovementSwipe(ref Vector2 direction)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    AndroidInitialTouchPosition = touch.position;
                    AndroidInitialDirection = direction;
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    float touchDirectionX = touch.position.x - AndroidInitialTouchPosition.x;

                    Vector3 rotatedVector3 = Quaternion.AngleAxis(touchDirectionX * AndroidRotationFirstPersonSpeed * Time.deltaTime, Vector3.up) * (Vector3.right * AndroidInitialDirection.x + Vector3.forward * AndroidInitialDirection.y);
                    direction = Vector2.right * rotatedVector3.x + Vector2.up * rotatedVector3.z;
                    break;
            }
        }
    }

    bool StandardWindowsJump()
    {
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            StartCoroutine(JumpCoroutine());
            return true;
        }
        return false;
    }

    bool StandardAndroidJump()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    //TODO: Check if it's in the AVOID ZONE (pause, exit)...
                    Vector3 tapPoint = ConvertTouchToPositionInWorld(touch);
                    if (!isJumping)
                    {
                        if (Vector2.SqrMagnitude(Vector2.right * (tapPoint.x - transform.position.x) + Vector2.up * (tapPoint.z - transform.position.z)) < SqrJumpTapRange)
                        {
                            StartCoroutine(JumpCoroutine());
                            return true;
                        }
                    }
                    break;
            }
        }
        return false;
    }

    bool FirstPersonWindowsJump()
    {
        return StandardWindowsJump();
    }

    bool FirstPersonAndroidJump()
    {
        return false; // TO DO: IMPLEMENT
    }

    bool FirstPersonAndroidJumpSwipe()
    {
        if (isJumping)
        {
            return false;
        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    AndroidInitialTouchPosition = touch.position;
                    AndroidInitialJumpTime = Time.time;
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (Time.time - AndroidInitialJumpTime > maxAndroidSwipeJumpDeltaTime)
                    {
                        return false;
                    }
                    float touchDirectionY = touch.position.y - AndroidInitialTouchPosition.y;
                    if (touchDirectionY > minAndroidSwipeJumpThreshold)
                    {
                        StartCoroutine(JumpCoroutine());
                        return true;
                    }
                    break;
            }
        }
        return false;
    }

    IEnumerator JumpCoroutine()
    {
        float t = 0f;
        isJumping = true;
        while (t < jumpTime)
        {
            snakeMovement.targetPositionOnGrid.y = jumpForce;
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        snakeMovement.targetPositionOnGrid.y = 0;
        isJumping = false;
        JumpsAmount++;
    }

    Vector3 ConvertTouchToPositionInWorld(Touch touch)
    {
        Vector3 returnVector = Vector3.zero;
        // create ray from the camera and passing through the touch position:
        Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        // create a logical plane at this object's position
        // and perpendicular to world Y:
        Plane plane = new Plane(Vector3.up, transform.position);
        float distance = 0; // this will return the distance from the camera
        if (plane.Raycast(ray, out distance))
        { // if plane hit...
            returnVector = ray.GetPoint(distance); // get the point
        }
        return returnVector;
    }

    Vector3 ConvertTouchToDirectionInWorld(Touch touch)
    {
        // create ray from the camera and passing through the touch position:
        Ray ray = Camera.current.ScreenPointToRay(Input.GetTouch(0).position);
        Vector3 direction = ray.direction;
        direction.y = 0f;
        Vector3 origin = ray.origin;
        origin.y = 0f;
        Debug.DrawRay(origin, direction * 1000, Color.red, 10f);
        return direction;
    }

    public void EnableStandardJump()
    {
        actions.Add(
            (PlatformUtils.platform == RuntimePlatform.Android) ?
                StandardAndroidJump : StandardWindowsJump
            );
    }

    public void DisableStandardJump() {
        actions.Remove(
            (PlatformUtils.platform == RuntimePlatform.Android) ?
                StandardAndroidJump : StandardWindowsJump
            );
    }

    public void EnableFirstPersonJump()
    {
        actions.Add(
            (PlatformUtils.platform == RuntimePlatform.Android) ?
                FirstPersonAndroidJumpSwipe : FirstPersonWindowsJump
            );
    }

    public void DisableFirstPersonJump()
    {
        actions.Remove(
            (PlatformUtils.platform == RuntimePlatform.Android) ?
                FirstPersonAndroidJumpSwipe : FirstPersonWindowsJump
            );
    }

    public void SetFree360MovementType()
    {
        move = (PlatformUtils.platform == RuntimePlatform.Android) ?
                Android360DegreeMovement : Windows360DegreeMovement;
    }

    public void SetFirstPersonMovementType()
    {
        move = (PlatformUtils.platform == RuntimePlatform.Android) ?
                AndroidFirstPersonMovementSwipe : WindowsFirstPersonMovement;
    }

    public void SetStandardMovementType()
    {
        move = (PlatformUtils.platform == RuntimePlatform.Android) ?
                StandardAndroidMovement : StandardWindowsMovement;
    }
}
