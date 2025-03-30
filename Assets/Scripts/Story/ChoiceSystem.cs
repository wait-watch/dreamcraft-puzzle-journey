using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the presentation and processing of story choices
/// </summary>
public class ChoiceSystem : MonoBehaviour
{
    [Header("Choice UI")]
    public GameObject ChoicePanel;
    public Text ChoicePromptText;
    public Button[] ChoiceButtons;
    public Text[] ChoiceButtonTexts;
    
    [Header("Choice Animation")]
    public Animation ChoicePanelAnimation;
    public float ChoiceDisplayTime = 0.5f;
    
    [Header("Audio")]
    public AudioClip ChoiceAppearSound;
    public AudioClip ChoiceSelectSound;
    
    // Internal state
    private StoryChoice _currentChoice;
    private bool _isChoiceActive = false;

    private void Start()
    {
        // Hide choice panel initially
        if (ChoicePanel != null)
        {
            ChoicePanel.SetActive(false);
        }
        
        // Set up button listeners
        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            if (ChoiceButtons[i] != null)
            {
                int index = i; // Capture for lambda
                ChoiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
            }
        }
    }

    /// <summary>
    /// Present a choice to the player
    /// </summary>
    public void PresentChoice(StoryChoice choice)
    {
        if (choice == null || ChoicePanel == null)
        {
            Debug.LogError("Cannot present null choice or choice panel is missing");
            return;
        }
        
        _currentChoice = choice;
        _isChoiceActive = true;
        
        // Set up the choice UI
        if (ChoicePromptText != null)
        {
            ChoicePromptText.text = choice.ChoiceText;
        }
        
        // For this simplified implementation, we'll use a binary choice (yes/no)
        if (ChoiceButtonTexts.Length >= 2)
        {
            ChoiceButtonTexts[0].text = "Yes";
            ChoiceButtonTexts[1].text = "No";
        }
        
        // Show choice panel with animation
        ChoicePanel.SetActive(true);
        
        if (ChoicePanelAnimation != null)
        {
            ChoicePanelAnimation.Play();
        }
        
        // Play sound
        PlaySound(ChoiceAppearSound);
        
        // Pause the game while making choice
        Time.timeScale = 0f;
        
        Debug.Log("Presenting choice: " + choice.ChoiceID);
    }

    /// <summary>
    /// Handle choice selection
    /// </summary>
    private void OnChoiceSelected(int choiceIndex)
    {
        if (!_isChoiceActive || _currentChoice == null)
        {
            return;
        }
        
        _isChoiceActive = false;
        
        // Play sound
        PlaySound(ChoiceSelectSound);
        
        // Hide choice panel
        if (ChoicePanel != null)
        {
            ChoicePanel.SetActive(false);
        }
        
        // Resume game
        Time.timeScale = 1f;
        
        // Process choice outcome
        string outcomeChoiceID = _currentChoice.ChoiceID;
        if (choiceIndex == 1)
        {
            // For binary choices, append "_no" to the choice ID for the negative option
            outcomeChoiceID += "_no";
        }
        
        // Display dialogue if this choice triggers dialogue
        if (!string.IsNullOrEmpty(_currentChoice.TriggersDialogue))
        {
            DialogueSystem dialogueSystem = FindObjectOfType<DialogueSystem>();
            if (dialogueSystem != null)
            {
                DialogueLine line = new DialogueLine
                {
                    SpeakerName = "",
                    DialogueText = _currentChoice.TriggersDialogue,
                    EmotionState = "neutral"
                };
                
                List<DialogueLine> lines = new List<DialogueLine> { line };
                dialogueSystem.StartDialogueSequence(lines);
            }
        }
        
        // Process the choice outcome
        GameManager.Instance.NarrativeManager.ProcessChoiceOutcome(outcomeChoiceID);
        
        Debug.Log("Selected choice index: " + choiceIndex + " for choice: " + _currentChoice.ChoiceID);
    }

    /// <summary>
    /// Check if a choice is currently active
    /// </summary>
    public bool IsChoiceActive()
    {
        return _isChoiceActive;
    }

    /// <summary>
    /// Force close the choice UI (e.g., when pausing the game)
    /// </summary>
    public void CloseChoiceUI()
    {
        if (_isChoiceActive)
        {
            _isChoiceActive = false;
            
            if (ChoicePanel != null)
            {
                ChoicePanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Play a sound effect
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlayUISound(clip);
        }
    }

    /// <summary>
    /// Present a custom choice with specific options
    /// </summary>
    public void PresentCustomChoice(string promptText, string[] options, System.Action<int> callback)
    {
        if (ChoicePanel == null || options == null || options.Length == 0)
        {
            return;
        }
        
        // Set prompt text
        if (ChoicePromptText != null)
        {
            ChoicePromptText.text = promptText;
        }
        
        // Set up buttons
        int optionsToShow = Mathf.Min(options.Length, ChoiceButtons.Length);
        
        for (int i = 0; i < ChoiceButtons.Length; i++)
        {
            if (ChoiceButtons[i] != null)
            {
                if (i < optionsToShow)
                {
                    // Show and configure button
                    ChoiceButtons[i].gameObject.SetActive(true);
                    
                    if (ChoiceButtonTexts[i] != null)
                    {
                        ChoiceButtonTexts[i].text = options[i];
                    }
                    
                    // Set up callback
                    int index = i; // Capture for lambda
                    ChoiceButtons[i].onClick.RemoveAllListeners();
                    ChoiceButtons[i].onClick.AddListener(() => {
                        PlaySound(ChoiceSelectSound);
                        ChoicePanel.SetActive(false);
                        Time.timeScale = 1f;
                        _isChoiceActive = false;
                        
                        if (callback != null)
                        {
                            callback.Invoke(index);
                        }
                    });
                }
                else
                {
                    // Hide unused buttons
                    ChoiceButtons[i].gameObject.SetActive(false);
                }
            }
        }
        
        // Show choice panel
        ChoicePanel.SetActive(true);
        _isChoiceActive = true;
        
        // Play animation and sound
        if (ChoicePanelAnimation != null)
        {
            ChoicePanelAnimation.Play();
        }
        
        PlaySound(ChoiceAppearSound);
        
        // Pause game
        Time.timeScale = 0f;
    }
}
