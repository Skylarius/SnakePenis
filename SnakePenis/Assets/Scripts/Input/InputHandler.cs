using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SnakeCommand
{
    private float Timestamp { get; }
    protected static float LastCommandTimestamp;
    public SnakeCommand()
    {
        Timestamp = Time.time;
        LastCommandTimestamp = Time.time;
    }

    public static float GetLastCommandTimestamp()
    {
        return LastCommandTimestamp;
    }

    public bool IsCoincident(SnakeCommand snakeCommand) 
    {
        return Mathf.Abs(snakeCommand.Timestamp - Timestamp) < 2*Time.deltaTime;
    }

    public bool IsCoincidentWithLastCommandTimestamp()
    {
        return Mathf.Abs(Timestamp - LastCommandTimestamp) < Time.deltaTime;
    }

    public virtual void execute()
    {
        Debug.Log("Executing command: " + GetType());
    }
}

public abstract class SnakeAction : SnakeCommand
{
    protected SnakeMovement SnakeMovement { get; }
    public SnakeAction(SnakeMovement snakeMovement) : base() 
    {
        SnakeMovement = snakeMovement;
    }
}

public class DirectionAction : SnakeAction
{
    private Vector2 Direction { get; }
    
    public DirectionAction(SnakeMovement snakeMovement, Vector2 direction) : base(snakeMovement)
    {
        Direction = direction;
    }

    public override void execute()
    {
        base.execute();
        SnakeMovement.direction = Direction;
    }
}

public class JumpAction : SnakeAction
{
    public delegate void JumpFunction();
    public JumpFunction JumpFunc;

    public JumpAction(SnakeMovement snakeMovement, JumpFunction jumpFunction) : base(snakeMovement)
    {
        JumpFunc = jumpFunction;
    }

    public override void execute() 
    {
        base.execute();
        JumpFunc(); 
    }
}

public class UICommand : SnakeCommand
{
    public delegate void UICommandFunction();
    public UICommandFunction CommandFunc;

    public UICommand(UICommandFunction command) : base()
    { 
        CommandFunc = command; 
    }

    public override void execute()
    {
        base.execute();
        CommandFunc();
    }
}

public class InputHandler : BaseSnakeComponent
{
    public delegate void MovementType();
    public MovementType move = null;

    public delegate void Action();
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
    private float AndroidInitialSwipeTime;
    public float maxAndroidSwipeDeltaTime = 1f;
    public float minAndroidMovementSwipeThreshold = 10f;
    private bool isTurning90Deg = false;

    public Queue<SnakeAction> SnakeActions;
    public Queue<SnakeCommand> SnakeCommands;

    /// <summary>
    /// Request to set the SnakeDirection to specified direction
    /// </summary>
    /// <param name="direction"></param>
    public void RequestDirectionAction(Vector2 direction)
    {
        DirectionAction action = new DirectionAction(snakeMovement, direction);
        SnakeActions.Enqueue(action);
    }

    /// <summary>
    /// Request to Jump
    /// </summary>
    public void RequestJumpAction()
    {
        JumpAction action = new JumpAction(snakeMovement, Jump);
        SnakeActions.Enqueue(action);
    }

    /// <summary>
    /// Request a UI command (pause, exit, ...)
    /// </summary>
    /// <param name="commandFunction"></param>
    public void RequestUICommand(UICommand.UICommandFunction commandFunction)
    {
        UICommand uiCommand = new UICommand(commandFunction);
        SnakeCommands.Enqueue(uiCommand);
    }



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
        SnakeCommands = new Queue<SnakeCommand>();
        SnakeActions = new Queue<SnakeAction>();
    }

    private void Update()
    {
        GetInputActions();
    }

    void GetInputActions()
    {
        // Handle inputs (movement and additional actions)
        foreach (Action action in actions)
        {
            action();
        }
        move();
    }

    private void LateUpdate()
    {
        ProcessActionsQueue();
    }

    void ProcessActionsQueue()
    {
        float lastCommandTimestamp = SnakeCommand.GetLastCommandTimestamp();
        if (lastCommandTimestamp + 2*Time.deltaTime > Time.time)
        {
            return;
        }
        //Process queue of commands
        SnakeCommand command = null;
        SnakeAction action = null;
        if (SnakeCommands.Count > 0)
        {
            command = SnakeCommands.Dequeue();
        }
        if (SnakeActions.Count > 0)
        {
            action = SnakeActions.Dequeue();
        }
        if (action != null && command != null && (action.IsCoincident(command) || action.IsCoincidentWithLastCommandTimestamp()))
        {
            //Discard action
            Debug.Log("Discarded action " + action.ToString() + " for comnnand " + command.ToString());
            command.execute();
            return;
        }
        if (command != null)
        {
            command.execute();
        }
        if (action != null && !SnakeMovement.isPause)
        {
            snakeMovement.actionQueue.Enqueue(action);
        }
    }

    /// <summary>
    /// GetStandardDirection
    /// </summary>
    /// <param name="x">horizontal</param>
    /// <param name="z">vertical</param>
    /// <returns>Return zero if no input has been given</returns>
    Vector2 GetStandardDirection(float x, float z)
    {
        if (Mathf.Abs(x) < 0.01f)
        {
            x = 0;
        }
        if (Mathf.Abs(z) < 0.01f)
        {
            z = 0;
        }

        if (x == 0f && z == 0f)
        {
            return Vector2.zero;
        }

        if ((int)snakeMovement.direction.x * x < 0 || (int)snakeMovement.direction.y * z < 0) 
        {
            return Vector2.zero; // false return, to not update direction
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

    void StandardWindowsMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector2 newDirection = GetStandardDirection(x, z);
        if (newDirection.x == 0 && newDirection.y == 0)
        {
            return;
        }
        RequestDirectionAction(newDirection);
    }

    void StandardAndroidMovement()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    //TODO: Check if it's in the AVOID ZONE (pause, exit)...
                    Vector3 tapPoint = ConvertTouchToPositionInWorld(touch);
                    float x = tapPoint.x - transform.position.x;
                    float z = tapPoint.z - transform.position.z;
                    if (snakeMovement.direction.y != 0)
                    {
                        z = 0f;
                    }
                    if (snakeMovement.direction.x != 0)
                    {
                        x = 0f;
                    }
                    Vector2 newDirection = GetStandardDirection(x, z);
                    if (newDirection.x == 0 && newDirection.y == 0)
                    {
                        return;
                    }
                    RequestDirectionAction(newDirection);
                    break;
            }
        }
    }

    void StandardAndroidMovementSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    AndroidInitialTouchPosition = touch.position;
                    AndroidInitialSwipeTime = Time.time;
                    isTurning90Deg = false;
                    break;
                case TouchPhase.Ended:
                    if (Time.time - AndroidInitialSwipeTime > maxAndroidSwipeDeltaTime || isTurning90Deg)
                    {
                        return;
                    }
                    float touchDirectionX = touch.position.x - AndroidInitialTouchPosition.x;
                    float touchDirectionY = touch.position.y - AndroidInitialTouchPosition.y;
                    if (IsMoreThanMinimumSwipe(touchDirectionX) == false && IsMoreThanMinimumSwipe(touchDirectionY) == false)
                    {
                        return;
                    }
                    if (touchDirectionX * touchDirectionX > touchDirectionY * touchDirectionY)
                    {
                        touchDirectionY = 0;
                        touchDirectionX = (touchDirectionX > 0) ? 1 : -1;
                    } else
                    {
                        touchDirectionX = 0;
                        touchDirectionY = (touchDirectionY > 0) ? 1 : -1;
                    }
                    Vector2 newDirection = GetStandardDirection(touchDirectionX, touchDirectionY);
                    if (newDirection.x == 0 && newDirection.y == 0)
                    {
                        return;
                    }
                    RequestDirectionAction(newDirection);
                    isTurning90Deg = false;
                    break;
            }
        }
    }

    bool IsMoreThanMinimumSwipe(float value)
    {
        return value > minAndroidMovementSwipeThreshold || value < -minAndroidMovementSwipeThreshold;
    }

    void Windows360DegreeMovement()
    {
        float x = Input.GetAxis("Mouse X");
        Vector3 rotatedVector3 = 
            Quaternion.AngleAxis(x*windowsRotation360Speed*Time.deltaTime, Vector3.up) * 
            (Vector3.right * snakeMovement.direction.x + Vector3.forward * snakeMovement.direction.y);
        Vector2 newDirection = Vector2.right * rotatedVector3.x + Vector2.up * rotatedVector3.z;
        if (newDirection == snakeMovement.direction)
        {
            return;
        }
        RequestDirectionAction(newDirection);
    }

    void Android360DegreeMovement()
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
                    if (dir == snakeMovement.direction)
                    {
                        return;
                    }
                    RequestDirectionAction(dir);
                    break;
            }
        }
    }
    private void WindowsFirstPersonMovement()
    {
        Windows360DegreeMovement();
    }

    void AndroidFirstPersonMovement()
    {
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
                    if(dir == snakeMovement.direction)
                    {
                        return;
                    }
                    RequestDirectionAction(dir);
                    break;
            }
        }
    }

    void AndroidFirstPersonMovementSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    AndroidInitialTouchPosition = touch.position;
                    AndroidInitialDirection = snakeMovement.direction;
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    float touchDirectionX = touch.position.x - AndroidInitialTouchPosition.x;

                    Vector3 rotatedVector3 = Quaternion.AngleAxis(touchDirectionX * AndroidRotationFirstPersonSpeed * Time.deltaTime, Vector3.up) * (Vector3.right * AndroidInitialDirection.x + Vector3.forward * AndroidInitialDirection.y);
                    Vector2 newDirection = Vector2.right * rotatedVector3.x + Vector2.up * rotatedVector3.z;
                    if (newDirection == snakeMovement.direction)
                    {
                        return;
                    }
                    RequestDirectionAction(newDirection);
                    break;
            }
        }
    }

    void AndroidFirstPerson90RotationMovementSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    AndroidInitialTouchPosition = touch.position;
                    AndroidInitialSwipeTime = Time.time;
                    isTurning90Deg = false;
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (Time.time - AndroidInitialSwipeTime > maxAndroidSwipeDeltaTime || isTurning90Deg)
                    {
                        return;
                    }
                    float touchDirectionX = touch.position.x - AndroidInitialTouchPosition.x;
                    if (touchDirectionX > minAndroidMovementSwipeThreshold || touchDirectionX < -minAndroidMovementSwipeThreshold)
                    {
                        if (touchDirectionX > 0)
                        {
                            touchDirectionX = 90f;
                        }
                        if (touchDirectionX < 0)
                        {
                            touchDirectionX = -90f;
                        }
                        Vector3 rotatedVector3 = 
                            Quaternion.AngleAxis(touchDirectionX, Vector3.up) * 
                            (Vector3.right * snakeMovement.direction.x + Vector3.forward * snakeMovement.direction.y);
                        Vector2 newDirection = Vector2.right * rotatedVector3.x + Vector2.up * rotatedVector3.z;
                        if (newDirection == snakeMovement.direction)
                        {
                            return;
                        }
                        RequestDirectionAction(newDirection);
                        isTurning90Deg = true;
                    }
                    break;
                case TouchPhase.Ended:
                    isTurning90Deg = false;
                    break;
            }
        }
    }

    void WindowsFirstPerson90RotationMovement()
    {
        float x = Input.GetAxis("Horizontal");
        if (x > 0 && !isTurning90Deg)
        {
            x = 90f;
            isTurning90Deg = true;
        }
        else if (x < 0 && !isTurning90Deg)
        {
            x = -90f;
            isTurning90Deg = true;
        }
        else if (x == 0 && isTurning90Deg)
        {
            isTurning90Deg = false;
        } else
        {
            x = 0f;
        }
        Vector3 rotatedVector3 = 
            Quaternion.AngleAxis(x, Vector3.up) * 
            (Vector3.right * snakeMovement.direction.x + Vector3.forward * snakeMovement.direction.y);
        Vector2 newDirection = Vector2.right * rotatedVector3.x + Vector2.up * rotatedVector3.z;
        if (newDirection == snakeMovement.direction)
        {
            return;
        }
        RequestDirectionAction(newDirection);
    }

    void StandardWindowsJump()
    {
        if (isJumping)
        {
            return;
        }
        if (Input.GetButtonDown("Jump"))
        {
            RequestJumpAction();
        }
    }

    void StandardAndroidJump()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    //TODO: Check if it's in the AVOID ZONE (pause, exit)...
                    Vector3 tapPoint = ConvertTouchToPositionInWorld(touch);
                    if (isJumping)
                    {
                        return;
                    }
                    if (Vector2.SqrMagnitude(Vector2.right * (tapPoint.x - transform.position.x) + Vector2.up * (tapPoint.z - transform.position.z)) < SqrJumpTapRange)
                    {
                        RequestJumpAction();
                    }
                    break;
            }
        }
    }

    void FirstPersonWindowsJump()
    {
        StandardWindowsJump();
    }

    void FirstPersonAndroidJumpSwipe()
    {
        if (isJumping)
        {
            return;
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
                        return;
                    }
                    float touchDirectionY = touch.position.y - AndroidInitialTouchPosition.y;
                    if (touchDirectionY > minAndroidSwipeJumpThreshold)
                    {
                        RequestJumpAction();
                    }
                    break;
            }
        }
    }

    void Jump()
    {
        StartCoroutine(JumpCoroutine());
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

    internal void SetFirstPerson90RotationMovementType()
    {
        move = (PlatformUtils.platform == RuntimePlatform.Android) ?
                AndroidFirstPerson90RotationMovementSwipe : WindowsFirstPerson90RotationMovement;
    }

    public void SetFirstPersonFree360MovementType()
    {
        move = (PlatformUtils.platform == RuntimePlatform.Android) ?
                AndroidFirstPersonMovementSwipe : WindowsFirstPersonMovement;
    }

    public void SetStandardMovementType()
    {
        move = (PlatformUtils.platform == RuntimePlatform.Android) ?
                StandardAndroidMovementSwipe : StandardWindowsMovement;
    }
}
