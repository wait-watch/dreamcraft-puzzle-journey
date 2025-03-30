using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages level loading, transitions, and level-specific data
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Level Configuration")]
    public List<LevelData> AvailableLevels = new List<LevelData>();
    
    [Header("Level Settings")]
    public float LevelTransitionTime = 1.0f;
    
    private int _currentLevelIndex = 0;
    private Level _currentLevel;

    private void Start()
    {
        // Initialize level list if empty
        if (AvailableLevels.Count == 0)
        {
            Debug.LogWarning("No levels configured in LevelManager. Adding default levels.");
            
            // Add sample initial levels
            AvailableLevels.Add(new LevelData
            {
                LevelName = "Childhood Dream",
                LevelPrefabName = "Level_1",
                LevelDescription = "Explore a dream of your childhood home, where objects aren't quite as they seem.",
                LevelTheme = LevelTheme.Nostalgic,
                DifficultyRating = 1
            });
            
            AvailableLevels.Add(new LevelData
            {
                LevelName = "Ocean Depths",
                LevelPrefabName = "Level_2",
                LevelDescription = "Dive into an underwater dreamscape where gravity behaves differently.",
                LevelTheme = LevelTheme.Underwater,
                DifficultyRating = 2
            });
            
            AvailableLevels.Add(new LevelData
            {
                LevelName = "Floating Islands",
                LevelPrefabName = "Level_3",
                LevelDescription = "Navigate between floating islands where perspective changes everything.",
                LevelTheme = LevelTheme.Surreal,
                DifficultyRating = 2
            });
            
            AvailableLevels.Add(new LevelData
            {
                LevelName = "Clockwork Dream",
                LevelPrefabName = "Level_4",
                LevelDescription = "Solve puzzles in a world of gears and time manipulation.",
                LevelTheme = LevelTheme.Mechanical,
                DifficultyRating = 3
            });
            
            AvailableLevels.Add(new LevelData
            {
                LevelName = "Forest of Memories",
                LevelPrefabName = "Level_5",
                LevelDescription = "A shifting forest where past choices come back to haunt you.",
                LevelTheme = LevelTheme.Nature,
                DifficultyRating = 3
            });
        }
    }

    /// <summary>
    /// Load a level by index
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= AvailableLevels.Count)
        {
            Debug.LogError("Invalid level index: " + levelIndex);
            return;
        }

        _currentLevelIndex = levelIndex;
        StartCoroutine(LoadLevelAsync(AvailableLevels[levelIndex].LevelPrefabName));
    }
    
    /// <summary>
    /// Reload the current level
    /// </summary>
    public void ReloadCurrentLevel()
    {
        LoadLevel(_currentLevelIndex);
    }

    /// <summary>
    /// Asynchronously load a level with transition effects
    /// </summary>
    private IEnumerator LoadLevelAsync(string levelName)
    {
        // Start fade out transition
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.StartFadeOut();
        }

        yield return new WaitForSeconds(LevelTransitionTime / 2);

        // Load the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
        
        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Wait a frame to ensure all objects are initialized
        yield return null;
        
        // Find the Level component in the loaded scene
        _currentLevel = FindObjectOfType<Level>();
        
        if (_currentLevel == null)
        {
            Debug.LogError("No Level component found in the loaded scene: " + levelName);
        }
        else
        {
            _currentLevel.InitializeLevel(AvailableLevels[_currentLevelIndex]);
            
            // Notify NarrativeManager about the new level
            if (GameManager.Instance.NarrativeManager != null)
            {
                GameManager.Instance.NarrativeManager.SetupLevelNarrative(_currentLevelIndex);
            }
        }

        // Start fade in transition
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.StartFadeIn();
        }

        // Set the current level index in GameManager
        GameManager.Instance.CurrentLevelIndex = _currentLevelIndex;

        Debug.Log("Level loaded: " + levelName);
    }

    /// <summary>
    /// Check if a level is unlocked by name
    /// </summary>
    public bool IsLevelUnlocked(string levelName)
    {
        return GameManager.Instance.UnlockedLevels.Contains(levelName);
    }

    /// <summary>
    /// Complete the current level with stars earned
    /// </summary>
    public void CompleteCurrentLevel(int starsEarned)
    {
        if (_currentLevel != null)
        {
            _currentLevel.OnLevelCompleted();
        }
        
        GameManager.Instance.CompleteLevel(starsEarned);
    }

    /// <summary>
    /// Get the current level data
    /// </summary>
    public LevelData GetCurrentLevelData()
    {
        if (_currentLevelIndex >= 0 && _currentLevelIndex < AvailableLevels.Count)
        {
            return AvailableLevels[_currentLevelIndex];
        }
        return null;
    }
    
    /// <summary>
    /// Fail the current level
    /// </summary>
    public void FailCurrentLevel()
    {
        if (_currentLevel != null)
        {
            _currentLevel.OnLevelFailed();
        }
        
        GameManager.Instance.ChangeGameState(GameState.GameOver);
    }
}
