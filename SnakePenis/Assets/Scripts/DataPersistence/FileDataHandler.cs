using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private bool useEncryption = false;
    private readonly string encryptionCodeWord = "Laola";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try 
            {
                // Load the serialized file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                // deserialize the data from JSon back into the C# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.Log($"Error occurred when trying to LOAD data from file: {fullPath}\n {e}");
            }

        }
        return loadedData;
    }

    public void Save(GameData data)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            // create the directory the file will be written
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Serialize in json
            string dataToStore = JsonUtility.ToJson(data, true);

            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // Write file to FS
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error occurred when trying to SAVE data to file: {fullPath}\n {e}");
        }
    }

    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";
        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char) (data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    }

    public void WriteDataToFile(string dataToWrite, UnityEngine.UI.Text LogText = null, bool debugLog = false)
    {
        if (debugLog)
        {
            Debug.Log(dataToWrite);
        }
        if (LogText)
        {
            LogText.text = dataToWrite;
        }
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            // create the directory the file will be written
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            if (useEncryption)
            {
                dataToWrite = EncryptDecrypt(dataToWrite);
            }

            // Write file to FS
            using (FileStream stream = new FileStream(fullPath, FileMode.Append))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToWrite);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error occurred when trying to WRITE data to file: {fullPath}\n {e}");
        }
    }

    public void CreateFile()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            // create the directory the file will be written
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Write file to FS
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write("");
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error occurred when trying to CREATE file: {fullPath}\n {e}");
        }
    }
}
