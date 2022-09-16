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

    [SerializeField]
    private GameObject mainGround;

    [SerializeField]
    private GameObject mainCameraObject;

    public static SnakeMovement SnakeMovement
    {
        get
        {
            return Instance.snakeMovement;
        }
    }

    public static InputHandler InputHandler
    {
        get
        {
            return SnakeMovement.gameObject.GetComponent<InputHandler>();
        }
    }

    public static SettingsPanelManager SettingsPanelManager
    {
        get
        {
            return Instance.GetComponent<SettingsPanelManager>();
        }
    }

    public static PlaygroundGenerator PlaygroundGenerator
    {
        get
        {
            return Instance.GetComponent<PlaygroundGenerator>();
        }
    }

    public static PowerUpSpawner PowerUpSpawner
    {
        get
        {
            return Instance.GetComponent<PowerUpSpawner>();
        }
    }

    public static GameObject MainGround
    {
        get
        {
            return Instance.mainGround;
        }
    }

    public static Camera MainCamera
    {
        get
        {
            return Instance.mainCameraObject.GetComponent<Camera>();
        }
    }
}
