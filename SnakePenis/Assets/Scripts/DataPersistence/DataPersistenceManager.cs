using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class DataPersistenceManager : MonoBehaviour

{
    public static DataPersistenceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        Instance = this;
    }

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    private GameData GameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    public void NewGame()
    {
        this.GameData = new GameData();
    }

    public void OpenDataFileLocation()
    {
        string itemPath = Application.persistentDataPath;
        itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
        System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
    }

    public void LoadGame()
    {
        // Load any save data from a file unsing the data handler 
        this.GameData = dataHandler.Load();

        // if no data can be loaded initalize to a new game
        if (this.GameData == null)
        {
            Debug.Log("No data was found. Initializing data to defaults.");
            NewGame();
        }
        // Push the loaded data to all other scripts that need it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(GameData);
        }
        Debug.Log($"Game Loaded for player {ScoreManager.CurrentID}){ScoreManager.CurrentScoreName}, level {LevelProgressionManager.CurrentLevel} - XP {LevelProgressionManager.TotalXP}");
        SendMessage("OnLoadComplete");
    }

    public void SaveGame()
    {
        // pass the data to other  so they can update it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref GameData);
        }

        // save that data to a file with the data handler
        dataHandler.Save(GameData);
    }

    private void Start()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
