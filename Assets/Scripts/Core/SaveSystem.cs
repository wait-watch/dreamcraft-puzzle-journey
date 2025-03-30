using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Handles saving and loading game data using PlayerPrefs for mobile platforms
/// </summary>
public class SaveSystem : MonoBehaviour
{
    private const string SAVE_KEY = "DreamCraft_SaveData";

    /// <summary>
    /// Save game data to device
    /// </summary>
    public void SaveGameData(GameData data)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, jsonData);
            PlayerPrefs.Save();
            
            Debug.Log("Game data saved successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving game data: " + e.Message);
        }
    }

    /// <summary>
    /// Load game data from device
    /// </summary>
    public GameData LoadGameData()
    {
        try
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string jsonData = PlayerPrefs.GetString(SAVE_KEY);
                GameData data = JsonUtility.FromJson<GameData>(jsonData);
                
                Debug.Log("Game data loaded successfully");
                return data;
            }
            else
            {
                Debug.Log("No saved game data found, creating new data");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading game data: " + e.Message);
            return null;
        }
    }

    /// <summary>
    /// Delete saved game data
    /// </summary>
    public void DeleteSaveData()
    {
        try
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                PlayerPrefs.DeleteKey(SAVE_KEY);
                PlayerPrefs.Save();
                Debug.Log("Save data deleted successfully");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error deleting save data: " + e.Message);
        }
    }

    /// <summary>
    /// Check if save data exists
    /// </summary>
    public bool SaveDataExists()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    /// <summary>
    /// Save a specific level's progress
    /// </summary>
    public void SaveLevelProgress(int levelIndex, int starsEarned, float completionTime)
    {
        try
        {
            string levelKey = "Level_" + levelIndex;
            string levelData = starsEarned + "," + completionTime;
            
            PlayerPrefs.SetString(levelKey, levelData);
            PlayerPrefs.Save();
            
            Debug.Log("Level progress saved for level " + levelIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving level progress: " + e.Message);
        }
    }

    /// <summary>
    /// Load a specific level's progress
    /// </summary>
    public (int, float) LoadLevelProgress(int levelIndex)
    {
        try
        {
            string levelKey = "Level_" + levelIndex;
            
            if (PlayerPrefs.HasKey(levelKey))
            {
                string levelData = PlayerPrefs.GetString(levelKey);
                string[] parts = levelData.Split(',');
                
                int stars = int.Parse(parts[0]);
                float time = float.Parse(parts[1]);
                
                return (stars, time);
            }
            
            return (0, 0);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading level progress: " + e.Message);
            return (0, 0);
        }
    }
}
