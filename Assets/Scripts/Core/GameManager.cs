using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager is the central control class for the game.
/// It manages game state, level progression, and coordinates other manager classes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState CurrentGameState = GameState.MainMenu;
    
    [Header("Manager References")]
    public LevelManager LevelManager;
    public UIManager UIManager;
    public NarrativeManager NarrativeManager;
    public AudioManager AudioManager;
    public SaveSystem SaveSystem;

    [Header("Game Settings")]
    public float GameVolume = 1.0f;
    public bool VibrationEnabled = true;
    
    // Track player progress
    [HideInInspector]
    public int CurrentLevelIndex = 0;
    [HideInInspector]
    public int TotalStars = 0;
    [HideInInspector]
    public List<string> UnlockedLevels = new List<string>();

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize the game systems
    /// </summary>
    private void InitializeGame()
    {
        // Ensure all required components are referenced
        if (LevelManager == null) LevelManager = GetComponentInChildren<LevelManager>();
        if (UIManager == null) UIManager = GetComponentInChildren<UIManager>();
        if (NarrativeManager == null) NarrativeManager = GetComponentInChildren<NarrativeManager>();
        if (AudioManager == null) AudioManager = GetComponentInChildren<AudioManager>();
        if (SaveSystem == null) SaveSystem = GetComponentInChildren<SaveSystem>();

        // Load saved game data
        LoadGameData();
        
        // Set application target frame rate
        Application.targetFrameRate = 60;
        
        Debug.Log("Game initialized successfully");
    }

    /// <summary>
    /// Load saved game data from device
    /// </summary>
    private void LoadGameData()
    {
        if (SaveSystem != null)
        {
            GameData data = SaveSystem.LoadGameData();
            if (data != null)
            {
                CurrentLevelIndex = data.CurrentLevelIndex;
                TotalStars = data.TotalStars;
                UnlockedLevels = data.UnlockedLevels;
                GameVolume = data.GameVolume;
                VibrationEnabled = data.VibrationEnabled;
                
                // Apply loaded settings
                if (AudioManager != null)
                {
                    AudioManager.SetVolume(GameVolume);
                }
            }
            else
            {
                // First time playing - unlock the first level
                UnlockedLevels.Add("Level_1");
            }
        }
    }

    /// <summary>
    /// Save current game data to device
    /// </summary>
    public void SaveGameData()
    {
        if (SaveSystem != null)
        {
            GameData data = new GameData
            {
                CurrentLevelIndex = CurrentLevelIndex,
                TotalStars = TotalStars,
                UnlockedLevels = UnlockedLevels,
                GameVolume = GameVolume,
                VibrationEnabled = VibrationEnabled
            };
            
            SaveSystem.SaveGameData(data);
        }
    }

    /// <summary>
    /// Start a new game
    /// </summary>
    public void StartNewGame()
    {
        CurrentLevelIndex = 0;
        ChangeGameState(GameState.Playing);
        LevelManager.LoadLevel(CurrentLevelIndex);
    }

    /// <summary>
    /// Continue the game from the last saved point
    /// </summary>
    public void ContinueGame()
    {
        ChangeGameState(GameState.Playing);
        LevelManager.LoadLevel(CurrentLevelIndex);
    }

    /// <summary>
    /// Load a specific level by index
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < LevelManager.AvailableLevels.Count)
        {
            CurrentLevelIndex = levelIndex;
            ChangeGameState(GameState.Playing);
            LevelManager.LoadLevel(levelIndex);
        }
    }

    /// <summary>
    /// Handle level completion
    /// </summary>
    public void CompleteLevel(int starsEarned)
    {
        TotalStars += starsEarned;
        
        // Unlock next level if available
        if (CurrentLevelIndex < LevelManager.AvailableLevels.Count - 1)
        {
            string nextLevelName = "Level_" + (CurrentLevelIndex + 2); // +2 because indices are 0-based
            if (!UnlockedLevels.Contains(nextLevelName))
            {
                UnlockedLevels.Add(nextLevelName);
            }
        }
        
        SaveGameData();
        ChangeGameState(GameState.Victory);
    }

    /// <summary>
    /// Change the current game state and notify other systems
    /// </summary>
    public void ChangeGameState(GameState newState)
    {
        CurrentGameState = newState;
        
        // Notify other systems about state change
        switch (newState)
        {
            case GameState.MainMenu:
                UIManager.ShowMainMenu();
                break;
            case GameState.Playing:
                UIManager.ShowGameUI();
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                UIManager.ShowPauseMenu();
                break;
            case GameState.Victory:
                UIManager.ShowVictoryScreen();
                break;
            case GameState.GameOver:
                UIManager.ShowGameOverScreen();
                break;
        }
    }

    /// <summary>
    /// Resume game from paused state
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        ChangeGameState(GameState.Playing);
    }

    /// <summary>
    /// Quit to main menu
    /// </summary>
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SaveGameData();
        ChangeGameState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Apply settings changes
    /// </summary>
    public void ApplySettings(float volume, bool vibration)
    {
        GameVolume = volume;
        VibrationEnabled = vibration;
        
        if (AudioManager != null)
        {
            AudioManager.SetVolume(GameVolume);
        }
        
        SaveGameData();
    }

    /// <summary>
    /// Restart the current level
    /// </summary>
    public void RestartLevel()
    {
        LevelManager.ReloadCurrentLevel();
        ChangeGameState(GameState.Playing);
    }

    /// <summary>
    /// Load the next level
    /// </summary>
    public void NextLevel()
    {
        if (CurrentLevelIndex < LevelManager.AvailableLevels.Count - 1)
        {
            CurrentLevelIndex++;
            LevelManager.LoadLevel(CurrentLevelIndex);
            ChangeGameState(GameState.Playing);
        }
        else
        {
            // All levels completed
            QuitToMainMenu();
        }
    }
}

/// <summary>
/// Enum to track different game states
/// </summary>
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Victory,
    GameOver
}

/// <summary>
/// Data structure to store game progress
/// </summary>
[System.Serializable]
public class GameData
{
    public int CurrentLevelIndex;
    public int TotalStars;
    public List<string> UnlockedLevels = new List<string>();
    public float GameVolume;
    public bool VibrationEnabled;
}
