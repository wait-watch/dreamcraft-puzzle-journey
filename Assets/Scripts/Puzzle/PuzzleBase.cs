using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base class for all puzzles in the game
/// </summary>
public class PuzzleBase : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public string PuzzleID;
    public string PuzzleName;
    [TextArea(3, 5)]
    public string PuzzleDescription;
    public bool IsActive = false;
    public bool IsCompleted = false;
    
    [Header("Hints")]
    [TextArea(3, 5)]
    public string[] Hints;
    
    [Header("Puzzle Components")]
    public List<InteractableObject> PuzzleObjects = new List<InteractableObject>();
    
    [Header("Events")]
    public UnityEvent OnPuzzleActivated;
    public UnityEvent OnPuzzleCompleted;
    public UnityEvent OnPuzzleReset;
    
    [Header("Audio")]
    public AudioClip PuzzleActivatedSound;
    public AudioClip PuzzleCompletedSound;
    public AudioClip PuzzleResetSound;
    
    protected int _currentState = 0;
    protected int _requiredStates = 1;
    protected Level _parentLevel;

    protected virtual void Start()
    {
        // Find parent level
        _parentLevel = GetComponentInParent<Level>();
        
        // Register this puzzle with the level
        if (_parentLevel != null)
        {
            _parentLevel.RegisterPuzzle(this);
        }
        
        // Initialize puzzle objects
        foreach (InteractableObject obj in PuzzleObjects)
        {
            if (obj != null)
            {
                obj.ParentPuzzle = this;
                obj.OnInteraction.AddListener(OnPuzzleObjectInteraction);
            }
        }
        
        // If the puzzle should be active from the start
        if (IsActive)
        {
            ActivatePuzzle();
        }
    }

    /// <summary>
    /// Activate the puzzle
    /// </summary>
    public virtual void ActivatePuzzle()
    {
        if (IsActive || IsCompleted)
        {
            return;
        }
        
        IsActive = true;
        
        // Enable all puzzle objects
        foreach (InteractableObject obj in PuzzleObjects)
        {
            if (obj != null)
            {
                obj.IsInteractable = true;
            }
        }
        
        // Trigger activation event
        OnPuzzleActivated.Invoke();
        
        // Play activation sound
        PlaySound(PuzzleActivatedSound);
        
        // Update UI with puzzle objective
        UpdatePuzzleObjective();
        
        Debug.Log("Puzzle '" + PuzzleName + "' activated");
    }

    /// <summary>
    /// Handle interaction with puzzle objects
    /// </summary>
    protected virtual void OnPuzzleObjectInteraction(InteractableObject interactedObject)
    {
        // To be implemented by specific puzzle types
        // This will usually update the puzzle state and check if it's solved
    }

    /// <summary>
    /// Check if the puzzle is completed
    /// </summary>
    protected virtual bool CheckPuzzleCompletion()
    {
        return _currentState >= _requiredStates;
    }

    /// <summary>
    /// Complete the puzzle
    /// </summary>
    public virtual void CompletePuzzle()
    {
        if (IsCompleted)
        {
            return;
        }
        
        IsCompleted = true;
        IsActive = false;
        
        // Disable interaction with puzzle objects
        foreach (InteractableObject obj in PuzzleObjects)
        {
            if (obj != null)
            {
                obj.IsInteractable = false;
            }
        }
        
        // Trigger completion event
        OnPuzzleCompleted.Invoke();
        
        // Play completion sound
        PlaySound(PuzzleCompletedSound);
        
        // Notify level of puzzle completion
        if (_parentLevel != null)
        {
            _parentLevel.OnPuzzleCompleted(this);
        }
        
        Debug.Log("Puzzle '" + PuzzleName + "' completed");
    }

    /// <summary>
    /// Reset the puzzle
    /// </summary>
    public virtual void ResetPuzzle()
    {
        IsCompleted = false;
        _currentState = 0;
        
        // Reset all puzzle objects
        foreach (InteractableObject obj in PuzzleObjects)
        {
            if (obj != null)
            {
                obj.ResetObject();
            }
        }
        
        // Trigger reset event
        OnPuzzleReset.Invoke();
        
        // Play reset sound
        PlaySound(PuzzleResetSound);
        
        Debug.Log("Puzzle '" + PuzzleName + "' reset");
    }

    /// <summary>
    /// Play a sound effect
    /// </summary>
    protected void PlaySound(AudioClip clip)
    {
        if (clip != null && GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlaySoundEffect(clip);
        }
    }

    /// <summary>
    /// Update the UI with the current puzzle objective
    /// </summary>
    protected void UpdatePuzzleObjective()
    {
        if (GameManager.Instance.UIManager != null)
        {
            GameUIController gameUI = FindObjectOfType<GameUIController>();
            if (gameUI != null)
            {
                gameUI.SetObjective(PuzzleDescription);
            }
        }
    }

    /// <summary>
    /// Get a hint for the current puzzle
    /// </summary>
    public string GetHint()
    {
        if (Hints == null || Hints.Length == 0)
        {
            return "No hints available for this puzzle.";
        }
        
        // Return different hints based on puzzle progress
        float progress = (float)_currentState / _requiredStates;
        int hintIndex = Mathf.Clamp(Mathf.FloorToInt(progress * Hints.Length), 0, Hints.Length - 1);
        
        return Hints[hintIndex];
    }
    
    /// <summary>
    /// Increments the puzzle state and checks for completion
    /// </summary>
    protected void IncrementState()
    {
        _currentState++;
        
        if (CheckPuzzleCompletion())
        {
            CompletePuzzle();
        }
    }
}
