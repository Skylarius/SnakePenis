using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGodSingleton : MonoBehaviour
{
    public static GameGodSingleton Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Game God Singleton in the scene.");
        }
        Instance = this;
    }


    [SerializeField]
    private SnakeMovement snakeMovement;

    public SnakeMovement GetSnakeMovementScript()
    {
        return snakeMovement;
    }
}
