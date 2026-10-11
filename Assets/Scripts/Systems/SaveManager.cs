using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField]
    private static SaveData saveData;

    //---------------------------------------------------------------------------
    // Helper struct and methods for save/load integration of specific classes
    //---------------------------------------------------------------------------

    // Populate this with save data structs
    [System.Serializable]
    public struct SaveData
    {
        // public AnimalManagerSaveData animalManagerData;
    }

    // Use this to call the load method(s) for every class instance
    private void HandleLoadData()
    {
        // GameManager.Instance.animalManager.Load(saveData.animalManagerData);
    }

    // Use this to call the save method for every class instance
    public bool HandleSaveData() 
    {
        bool hasErrors = false;
        // if (!GameManager.Instance.animalManager.Save(out saveData.animalManagerData))
        //     hasErrors = true;
        return !hasErrors;
    }

    //---------------------------------------------------------------------------
    // Save Manager internal logic
    //---------------------------------------------------------------------------

    public static string SaveFileName()
    {
        return Application.persistentDataPath + "/save.json";
    }

    public static bool SaveFileExists()
    {
        return File.Exists(SaveFileName());
    }

    public void Save()
    {
        if (!HandleSaveData())
        {
            Debug.LogError("Saving encountered errors, aborting");
            return;
        }
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SaveFileName(), json);
    }

    public void Load()
    {
        if (File.Exists(SaveFileName()))
        {
            string saveContent = File.ReadAllText(SaveFileName());
            saveData = JsonUtility.FromJson<SaveData>(saveContent);
            HandleLoadData();
        }
        else
        {
            Debug.LogWarning(SaveFileName() + "not found");
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(SaveFileName()))
            File.Delete(SaveFileName());
    }

    void OnApplicationQuit()
    {
        Save();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            Save();
    }
}
