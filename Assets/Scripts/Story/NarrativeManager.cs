using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NarrativeManager handles the story progression, branching narratives, and player choices.
/// It maintains the player's story state and determines story outcomes based on choices.
/// </summary>
public class NarrativeManager : MonoBehaviour
{
    [Header("Dialogue System")]
    public DialogueSystem DialogueSystem;
    
    [Header("Choice System")]
    public ChoiceSystem ChoiceSystem;
    
    [Header("Story Data")]
    public TextAsset[] StoryScriptsPerLevel;
    
    [Header("Story Variables")]
    // These variables track player choices throughout the game
    public bool HasHelpedStranger = false;
    public bool HasSavedCompanion = false;
    public bool HasDiscoveredSecret = false;
    public int KindnessScore = 0;
    public int LogicScore = 0;
    public int CreativityScore = 0;

    // Dictionary to store level-specific narrative state
    private Dictionary<string, LevelNarrativeState> _levelNarrativeStates = new Dictionary<string, LevelNarrativeState>();
    
    // Current level narrative
    private LevelNarrativeState _currentLevelState;
    
    // Cached choice outcomes for the current level
    private Dictionary<string, StoryOutcome> _choiceOutcomes = new Dictionary<string, StoryOutcome>();

    private void Start()
    {
        // Initialize the dialogue and choice systems if not set
        if (DialogueSystem == null)
        {
            DialogueSystem = GetComponentInChildren<DialogueSystem>();
        }
        
        if (ChoiceSystem == null)
        {
            ChoiceSystem = GetComponentInChildren<ChoiceSystem>();
        }
        
        // Initialize with default values if no save data exists
        LoadNarrativeState();
    }

    /// <summary>
    /// Load narrative state from saved game data
    /// </summary>
    public void LoadNarrativeState()
    {
        // In a more complex implementation, this would load the narrative state from saved data
        // For now, we'll use default values
        HasHelpedStranger = false;
        HasSavedCompanion = false;
        HasDiscoveredSecret = false;
        KindnessScore = 0;
        LogicScore = 0;
        CreativityScore = 0;
    }

    /// <summary>
    /// Save narrative state to game data
    /// </summary>
    public void SaveNarrativeState()
    {
        // This would typically save to the game's save system
        // For now, we'll just log
        Debug.Log("Narrative state saved");
    }

    /// <summary>
    /// Setup the narrative for a specific level
    /// </summary>
    public void SetupLevelNarrative(int levelIndex)
    {
        if (levelIndex < 0 || StoryScriptsPerLevel == null || levelIndex >= StoryScriptsPerLevel.Length)
        {
            Debug.LogWarning("No story script available for level " + levelIndex);
            return;
        }
        
        // Get or create level state
        string levelKey = "Level_" + (levelIndex + 1);
        if (!_levelNarrativeStates.ContainsKey(levelKey))
        {
            _levelNarrativeStates[levelKey] = new LevelNarrativeState();
        }
        
        _currentLevelState = _levelNarrativeStates[levelKey];
        
        // Parse level story script
        ParseLevelStoryScript(StoryScriptsPerLevel[levelIndex]);
        
        // Set up initial dialogues if any
        if (_currentLevelState.IntroDialogue != null && _currentLevelState.IntroDialogue.Count > 0)
        {
            StartIntroSequence();
        }
        
        Debug.Log("Level narrative setup complete for level " + levelIndex);
    }

    /// <summary>
    /// Parse a story script text asset into dialogue and choice data
    /// </summary>
    private void ParseLevelStoryScript(TextAsset storyScript)
    {
        if (storyScript == null)
        {
            Debug.LogError("Story script is null");
            return;
        }
        
        _currentLevelState.IntroDialogue = new List<DialogueLine>();
        _currentLevelState.OutroDialogue = new List<DialogueLine>();
        _currentLevelState.Choices = new List<StoryChoice>();
        _choiceOutcomes.Clear();
        
        // Parse the script text
        // In a real implementation, this would parse a JSON or custom format
        // For this implementation, we'll create some sample data
        
        // Create intro dialogue
        _currentLevelState.IntroDialogue.Add(new DialogueLine 
        { 
            SpeakerName = "Dream Guide", 
            DialogueText = "Welcome to the dream world. Your subconscious has manifested this place.",
            EmotionState = "neutral"
        });
        
        _currentLevelState.IntroDialogue.Add(new DialogueLine 
        { 
            SpeakerName = "Dream Guide", 
            DialogueText = "The puzzles you solve and choices you make here will reflect your inner self.",
            EmotionState = "curious"
        });
        
        // Create sample choices
        _currentLevelState.Choices.Add(new StoryChoice 
        { 
            ChoiceID = "help_stranger",
            ChoiceText = "Help the stranger in need",
            TriggersDialogue = "The stranger thanks you for your kindness."
        });
        
        _currentLevelState.Choices.Add(new StoryChoice 
        { 
            ChoiceID = "ignore_stranger",
            ChoiceText = "Ignore the stranger, focus on your goal",
            TriggersDialogue = "The stranger watches as you walk away."
        });
        
        // Create choice outcomes
        _choiceOutcomes["help_stranger"] = new StoryOutcome 
        { 
            OutcomeText = "You chose to help others even at your own expense. Your kindness shapes your dream world.",
            KindnessChange = 2,
            CreativityChange = 0,
            LogicChange = -1,
            UnlocksLevel = ""
        };
        
        _choiceOutcomes["ignore_stranger"] = new StoryOutcome 
        { 
            OutcomeText = "You chose to prioritize your goals. Your dream world becomes more focused but colder.",
            KindnessChange = -1,
            CreativityChange = 0,
            LogicChange = 2,
            UnlocksLevel = ""
        };
        
        // Create outro dialogue
        _currentLevelState.OutroDialogue.Add(new DialogueLine 
        { 
            SpeakerName = "Dream Guide", 
            DialogueText = "Your choices have consequences. The dream evolves with you.",
            EmotionState = "thoughtful"
        });
        
        Debug.Log("Story script parsed successfully");
    }

    /// <summary>
    /// Start the introductory dialogue sequence
    /// </summary>
    public void StartIntroSequence()
    {
        if (DialogueSystem != null && _currentLevelState != null && _currentLevelState.IntroDialogue.Count > 0)
        {
            DialogueSystem.StartDialogueSequence(_currentLevelState.IntroDialogue);
        }
    }

    /// <summary>
    /// Start the level ending dialogue sequence
    /// </summary>
    public void StartOutroSequence()
    {
        if (DialogueSystem != null && _currentLevelState != null && _currentLevelState.OutroDialogue.Count > 0)
        {
            DialogueSystem.StartDialogueSequence(_currentLevelState.OutroDialogue);
        }
    }

    /// <summary>
    /// Present a story choice to the player
    /// </summary>
    public void PresentChoice(string choiceID)
    {
        if (ChoiceSystem == null || _currentLevelState == null)
        {
            return;
        }
        
        // Find the choice in the current level
        StoryChoice choice = _currentLevelState.Choices.Find(c => c.ChoiceID == choiceID);
        if (choice == null)
        {
            Debug.LogWarning("Choice not found: " + choiceID);
            return;
        }
        
        // Present the choice
        ChoiceSystem.PresentChoice(choice);
    }

    /// <summary>
    /// Process the outcome of a player choice
    /// </summary>
    public void ProcessChoiceOutcome(string choiceID)
    {
        if (!_choiceOutcomes.ContainsKey(choiceID))
        {
            Debug.LogWarning("No outcome defined for choice: " + choiceID);
            return;
        }
        
        StoryOutcome outcome = _choiceOutcomes[choiceID];
        
        // Apply changes to player stats
        KindnessScore += outcome.KindnessChange;
        LogicScore += outcome.LogicChange;
        CreativityScore += outcome.CreativityChange;
        
        // Special flags based on choice ID
        if (choiceID == "help_stranger")
        {
            HasHelpedStranger = true;
        }
        else if (choiceID == "save_companion")
        {
            HasSavedCompanion = true;
        }
        else if (choiceID == "discover_secret")
        {
            HasDiscoveredSecret = true;
        }
        
        // Display outcome text if available
        if (!string.IsNullOrEmpty(outcome.OutcomeText) && DialogueSystem != null)
        {
            DialogueSystem.ShowNarration(outcome.OutcomeText);
        }
        
        // Unlock new level if specified
        if (!string.IsNullOrEmpty(outcome.UnlocksLevel))
        {
            if (!GameManager.Instance.UnlockedLevels.Contains(outcome.UnlocksLevel))
            {
                GameManager.Instance.UnlockedLevels.Add(outcome.UnlocksLevel);
                GameManager.Instance.SaveGameData();
            }
        }
        
        // Save narrative state
        SaveNarrativeState();
        
        Debug.Log("Processed choice outcome for: " + choiceID);
    }

    /// <summary>
    /// Get the story outcome text for the current level based on choices
    /// </summary>
    public string GetStoryOutcome()
    {
        // This would typically generate an outcome based on the player's choices
        // For now, we'll generate based on the kindness/logic/creativity scores
        
        if (KindnessScore > LogicScore && KindnessScore > CreativityScore)
        {
            return "Your compassionate nature guided your journey through the dream world. The connections you made have created a harmonious outcome.";
        }
        else if (LogicScore > KindnessScore && LogicScore > CreativityScore)
        {
            return "Your analytical mind found efficient solutions to the dream's challenges. Your logical approach has created a structured outcome.";
        }
        else if (CreativityScore > KindnessScore && CreativityScore > LogicScore)
        {
            return "Your creative spirit found unexpected paths through the dream world. Your imaginative approach has created a vibrant outcome.";
        }
        else
        {
            return "Your balanced approach to the dream world's challenges has created a complex but stable outcome.";
        }
    }

    /// <summary>
    /// Check if player has made specific choices that unlock hidden content
    /// </summary>
    public bool HasUnlockedHiddenContent()
    {
        // Example: Secret ending requires all three special flags
        return HasHelpedStranger && HasSavedCompanion && HasDiscoveredSecret;
    }
    
    /// <summary>
    /// Get the dominant trait of the player based on their scores
    /// </summary>
    public string GetDominantTrait()
    {
        if (KindnessScore >= LogicScore && KindnessScore >= CreativityScore)
        {
            return "Compassionate";
        }
        else if (LogicScore >= KindnessScore && LogicScore >= CreativityScore)
        {
            return "Analytical";
        }
        else
        {
            return "Creative";
        }
    }
}

/// <summary>
/// Represents the narrative state of a specific level
/// </summary>
[System.Serializable]
public class LevelNarrativeState
{
    public List<DialogueLine> IntroDialogue;
    public List<DialogueLine> OutroDialogue;
    public List<StoryChoice> Choices;
    public List<string> MadeChoices = new List<string>();
}

/// <summary>
/// Represents a line of dialogue in the game
/// </summary>
[System.Serializable]
public class DialogueLine
{
    public string SpeakerName;
    public string DialogueText;
    public string EmotionState;
    public AudioClip VoiceClip;
}

/// <summary>
/// Represents a choice the player can make in the story
/// </summary>
[System.Serializable]
public class StoryChoice
{
    public string ChoiceID;
    public string ChoiceText;
    public string TriggersDialogue;
    public string[] RequiredFlags;
}

/// <summary>
/// Represents the outcome of a choice
/// </summary>
[System.Serializable]
public class StoryOutcome
{
    public string OutcomeText;
    public int KindnessChange;
    public int LogicChange;
    public int CreativityChange;
    public string UnlocksLevel;
}
