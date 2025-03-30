using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure for level information
/// </summary>
[System.Serializable]
public class LevelData
{
    [Header("Level Identity")]
    public string LevelName;
    public string LevelPrefabName;
    public LevelTheme LevelTheme;
    
    [Header("Level Info")]
    [TextArea(3, 5)]
    public string LevelDescription;
    public int DifficultyRating; // 1-5 scale
    
    [Header("Level Requirements")]
    public string[] RequiredCompletedLevels;
    public int RequiredStars;
    
    [Header("Level Rewards")]
    public int StarsAvailable = 3;
    public string UnlocksAbility;
    public string UnlockedCustomization;
    
    [Header("Preview")]
    public Sprite LevelPreviewImage;
    
    // Constructors
    public LevelData() { }
    
    public LevelData(string name, string prefabName, string description, LevelTheme theme, int difficulty)
    {
        LevelName = name;
        LevelPrefabName = prefabName;
        LevelDescription = description;
        LevelTheme = theme;
        DifficultyRating = difficulty;
    }
    
    /// <summary>
    /// Create a default level data object
    /// </summary>
    public static LevelData CreateDefault(int levelIndex)
    {
        string levelNum = (levelIndex + 1).ToString();
        return new LevelData(
            "Level " + levelNum,
            "Level_" + levelNum,
            "A mysterious dream awaits exploration.",
            LevelTheme.Surreal,
            1
        );
    }
    
    /// <summary>
    /// Create sample level data for testing
    /// </summary>
    public static List<LevelData> CreateSampleLevels()
    {
        List<LevelData> levels = new List<LevelData>();
        
        // Level 1 - Tutorial/Childhood
        levels.Add(new LevelData(
            "Childhood Dream",
            "Level_1",
            "Explore a dream of your childhood home, where objects aren't quite as they seem.",
            LevelTheme.Nostalgic,
            1
        ));
        
        // Level 2 - Underwater
        levels.Add(new LevelData(
            "Ocean Depths",
            "Level_2",
            "Dive into an underwater dreamscape where gravity behaves differently.",
            LevelTheme.Underwater,
            2
        ));
        
        // Level 3 - Floating Islands
        levels.Add(new LevelData(
            "Floating Islands",
            "Level_3",
            "Navigate between floating islands where perspective changes everything.",
            LevelTheme.Surreal,
            2
        ));
        
        // Level 4 - Clockwork
        levels.Add(new LevelData(
            "Clockwork Dream",
            "Level_4",
            "Solve puzzles in a world of gears and time manipulation.",
            LevelTheme.Mechanical,
            3
        ));
        
        // Level 5 - Forest
        levels.Add(new LevelData(
            "Forest of Memories",
            "Level_5",
            "A shifting forest where past choices come back to haunt you.",
            LevelTheme.Nature,
            3
        ));
        
        return levels;
    }
    
    /// <summary>
    /// Check if player has met the requirements to play this level
    /// </summary>
    public bool AreRequirementsMet(List<string> unlockedLevels, int totalStars)
    {
        // Check star requirement
        if (totalStars < RequiredStars)
        {
            return false;
        }
        
        // Check required levels
        if (RequiredCompletedLevels != null && RequiredCompletedLevels.Length > 0)
        {
            foreach (string levelName in RequiredCompletedLevels)
            {
                if (!unlockedLevels.Contains(levelName))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
}
