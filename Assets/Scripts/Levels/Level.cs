using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum for different level themes
/// </summary>
public enum LevelTheme
{
    Nostalgic,
    Underwater,
    Surreal,
    Mechanical,
    Nature,
    Cyberpunk,
    Cosmic
}

/// <summary>
/// Base class for all level behaviors
/// </summary>
public class Level : MonoBehaviour
{
    [Header("Level Identity")]
    public string LevelID;
    public string LevelName;
    public LevelTheme Theme;
    
    [Header("Level Settings")]
    public float TimeLimit = 300.0f; // 5 minutes default
    public int MaxScore = 3000;
    public int DifficultyRating = 1;
    public bool IsTimeLimited = true;
    
    [Header("Level State")]
    public bool IsCompleted = false;
    public bool IsFailed = false;
    
    [Header("Puzzle Data")]
    public List<PuzzleBase> Puzzles = new List<PuzzleBase>();
    
    [Header("Story Elements")]
    public int StoryChoicePoints = 0;
    public string LevelDescription;
    public string StoryOutcomeText;
    
    [Header("Audio")]
    public AudioClip LevelMusic;
    public AudioClip AmbientSoundLoop;
    public AudioClip LevelCompletedSound;
    public AudioClip LevelFailedSound;
    
    // Runtime state
    private float _timeRemaining;
    private float _completionTime;
    private int _puzzlesCompleted = 0;
    private int _currentScore = 0;
    private float _levelStartTime;
    private bool _isLevelActive = false;
    private LevelData _levelData;

    private void Start()
    {
        // Get references to all puzzles in the level if not assigned manually
        if (Puzzles.Count == 0)
        {
            Puzzles.AddRange(GetComponentsInChildren<PuzzleBase>());
        }
        
        Debug.Log("Level started with " + Puzzles.Count + " puzzles");
    }

    /// <summary>
    /// Initialize the level with data
    /// </summary>
    public void InitializeLevel(LevelData data)
    {
        _levelData = data;
        
        // Set level properties from data
        LevelID = data.LevelPrefabName;
        LevelName = data.LevelName;
        Theme = data.LevelTheme;
        DifficultyRating = data.DifficultyRating;
        LevelDescription = data.LevelDescription;
        
        // Initialize level state
        _timeRemaining = TimeLimit;
        _levelStartTime = Time.time;
        _currentScore = 0;
        _puzzlesCompleted = 0;
        IsCompleted = false;
        IsFailed = false;
        _isLevelActive = true;
        
        // Start level music and ambient sound
        if (GameManager.Instance.AudioManager != null)
        {
            if (LevelMusic != null)
            {
                GameManager.Instance.AudioManager.PlayMusic(LevelMusic);
            }
            
            if (AmbientSoundLoop != null)
            {
                GameManager.Instance.AudioManager.PlayAmbientSound(AmbientSoundLoop);
            }
        }
        
        // Update UI with objective
        UpdateLevelUI();
        
        Debug.Log("Level initialized: " + LevelName);
    }

    private void Update()
    {
        if (!_isLevelActive)
        {
            return;
        }
        
        // Update time remaining if time-limited
        if (IsTimeLimited && !IsCompleted && !IsFailed)
        {
            _timeRemaining -= Time.deltaTime;
            
            // Check for time expired
            if (_timeRemaining <= 0)
            {
                _timeRemaining = 0;
                OnLevelFailed();
            }
            
            // Update UI with time
            UpdateLevelUI();
        }
    }

    /// <summary>
    /// Register a puzzle with the level
    /// </summary>
    public void RegisterPuzzle(PuzzleBase puzzle)
    {
        if (puzzle != null && !Puzzles.Contains(puzzle))
        {
            Puzzles.Add(puzzle);
        }
    }

    /// <summary>
    /// Handle puzzle completion
    /// </summary>
    public void OnPuzzleCompleted(PuzzleBase puzzle)
    {
        if (Puzzles.Contains(puzzle))
        {
            _puzzlesCompleted++;
            
            // Award score for puzzle completion
            int puzzleScore = CalculatePuzzleScore(puzzle);
            _currentScore += puzzleScore;
            
            Debug.Log("Puzzle completed. Score: " + puzzleScore + ", Total: " + _currentScore);
            
            // Update UI
            UpdateLevelUI();
            
            // Check if all puzzles are completed
            bool allPuzzlesCompleted = true;
            foreach (PuzzleBase p in Puzzles)
            {
                if (p != null && !p.IsCompleted)
                {
                    allPuzzlesCompleted = false;
                    break;
                }
            }
            
            // If all puzzles are completed, complete the level
            if (allPuzzlesCompleted)
            {
                OnLevelCompleted();
            }
        }
    }

    /// <summary>
    /// Calculate score for a completed puzzle
    /// </summary>
    private int CalculatePuzzleScore(PuzzleBase puzzle)
    {
        // Base score depends on puzzle completion
        int baseScore = 500;
        
        // Adjust score based on time (faster completion = more points)
        float timeFactor = 1.0f;
        if (IsTimeLimited && TimeLimit > 0)
        {
            timeFactor = Mathf.Clamp01(_timeRemaining / TimeLimit);
        }
        
        // Formula: Base score + time bonus
        int finalScore = baseScore + Mathf.RoundToInt(baseScore * timeFactor);
        
        return finalScore;
    }

    /// <summary>
    /// Handle level completion
    /// </summary>
    private void OnLevelCompleted()
    {
        if (IsCompleted)
        {
            return;
        }
        
        IsCompleted = true;
        _isLevelActive = false;
        _completionTime = Time.time - _levelStartTime;
        
        // Play completion sound
        if (LevelCompletedSound != null && GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlaySoundEffect(LevelCompletedSound);
        }
        
        // Show level outro dialogue if available
        if (GameManager.Instance.NarrativeManager != null)
        {
            GameManager.Instance.NarrativeManager.StartOutroSequence();
        }
        
        // Calculate final score with time bonus
        int timeBonus = 0;
        if (IsTimeLimited && TimeLimit > 0)
        {
            float timeRatio = _timeRemaining / TimeLimit;
            timeBonus = Mathf.RoundToInt(1000 * timeRatio);
            _currentScore += timeBonus;
        }
        
        Debug.Log("Level completed! Time: " + _completionTime + "s, Score: " + _currentScore);
        
        // Notify GameManager of level completion
        GameManager.Instance.CompleteLevel(CalculateStars());
    }

    /// <summary>
    /// Handle level failure
    /// </summary>
    public void OnLevelFailed()
    {
        if (IsFailed || IsCompleted)
        {
            return;
        }
        
        IsFailed = true;
        _isLevelActive = false;
        
        // Play failure sound
        if (LevelFailedSound != null && GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlaySoundEffect(LevelFailedSound);
        }
        
        Debug.Log("Level failed!");
        
        // Notify GameManager of level failure
        GameManager.Instance.LevelManager.FailCurrentLevel();
    }

    /// <summary>
    /// Calculate stars earned for level completion (0-3)
    /// </summary>
    public int CalculateStars()
    {
        // Base criteria for stars:
        // 1 star: Completed level
        // 2 stars: Good score or good time
        // 3 stars: Great score and good time
        
        int stars = 1; // Always get at least 1 star for completion
        
        // Score-based stars
        float scoreRatio = (float)_currentScore / MaxScore;
        if (scoreRatio >= 0.8f) // 80% or higher of max score
        {
            stars = 3;
        }
        else if (scoreRatio >= 0.5f) // 50% or higher of max score
        {
            stars = 2;
        }
        
        // Time can reduce stars
        if (IsTimeLimited && TimeLimit > 0)
        {
            float timeRatio = _timeRemaining / TimeLimit;
            
            if (timeRatio < 0.2f && stars > 1) // Less than 20% time remaining
            {
                stars--; // Lose a star for cutting it too close
            }
        }
        
        return stars;
    }

    /// <summary>
    /// Update the UI with current level info
    /// </summary>
    private void UpdateLevelUI()
    {
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.UpdateGameUI(_currentScore, _timeRemaining, 0); // 0 is a placeholder for power charges
        }
    }

    /// <summary>
    /// Get the completion time for the level
    /// </summary>
    public float GetCompletionTime()
    {
        return _completionTime;
    }

    /// <summary>
    /// Get the current score
    /// </summary>
    public int GetScore()
    {
        return _currentScore;
    }

    /// <summary>
    /// Get the current level story outcome
    /// </summary>
    public string GetStoryOutcome()
    {
        // If a specific outcome is set, use that
        if (!string.IsNullOrEmpty(StoryOutcomeText))
        {
            return StoryOutcomeText;
        }
        
        // Otherwise, get outcome from NarrativeManager
        if (GameManager.Instance.NarrativeManager != null)
        {
            return GameManager.Instance.NarrativeManager.GetStoryOutcome();
        }
        
        // Fallback text
        return "You've completed the dream challenge, but the meaning remains elusive...";
    }

    /// <summary>
    /// Get a hint for the current puzzle
    /// </summary>
    public string GetCurrentPuzzleHint()
    {
        // Find first incomplete puzzle
        foreach (PuzzleBase puzzle in Puzzles)
        {
            if (puzzle != null && !puzzle.IsCompleted && puzzle.IsActive)
            {
                return puzzle.GetHint();
            }
        }
        
        return "Try exploring the dream world for clues.";
    }
}
