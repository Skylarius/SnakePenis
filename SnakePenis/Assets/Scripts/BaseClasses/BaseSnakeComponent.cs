using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSnakeComponent : MonoBehaviour
{
    protected SnakeMovement snakeMovement = null;
    /// <summary>
    /// Set SnakeMovement as the one in the component.
    /// Used when copying the snake, as the RealSnakeBinder still points to the old SnakeMovement script;
    /// </summary>
    public void ResetSnakeMovementScript()
    {
        snakeMovement = GetComponent<SnakeMovement>();
    }
}
