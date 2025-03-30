using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles dialogue display, timing, and character portraits
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject DialoguePanel;
    public Text SpeakerNameText;
    public Text DialogueText;
    public Image SpeakerPortrait;
    public Button ContinueButton;
    
    [Header("Dialogue Settings")]
    public float CharacterTypingSpeed = 0.05f;
    public float DialoguePauseDuration = 0.5f;
    public bool AutoAdvance = false;
    public float AutoAdvanceDelay = 3.0f;
    
    [Header("Narration UI")]
    public GameObject NarrationPanel;
    public Text NarrationText;
    public Button NarrationContinueButton;
    
    [Header("Visual Effects")]
    public Animation DialoguePanelAnimation;
    public Sprite[] CharacterPortraits;
    public Sprite[] EmotionIcons;
    public Image EmotionIndicator;
    
    [Header("Audio")]
    public AudioClip DialogueAppearSound;
    public AudioClip DialogueTypeSound;
    public AudioClip DialogueAdvanceSound;
    
    // Internal state
    private List<DialogueLine> _currentDialogueSequence;
    private int _currentDialogueIndex = 0;
    private bool _isTyping = false;
    private Coroutine _typingCoroutine;
    private Dictionary<string, Sprite> _portraitDictionary = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> _emotionDictionary = new Dictionary<string, Sprite>();

    private void Start()
    {
        // Initialize dialogue UI in hidden state
        if (DialoguePanel != null)
        {
            DialoguePanel.SetActive(false);
        }
        
        if (NarrationPanel != null)
        {
            NarrationPanel.SetActive(false);
        }
        
        // Set up continue button
        if (ContinueButton != null)
        {
            ContinueButton.onClick.AddListener(OnContinueButtonClicked);
        }
        
        if (NarrationContinueButton != null)
        {
            NarrationContinueButton.onClick.AddListener(OnNarrationContinueButtonClicked);
        }
        
        // Set up portrait dictionary for easier lookup
        if (CharacterPortraits != null)
        {
            for (int i = 0; i < CharacterPortraits.Length; i++)
            {
                if (CharacterPortraits[i] != null)
                {
                    string key = CharacterPortraits[i].name.ToLower();
                    _portraitDictionary[key] = CharacterPortraits[i];
                }
            }
        }
        
        // Set up emotion dictionary
        if (EmotionIcons != null)
        {
            for (int i = 0; i < EmotionIcons.Length; i++)
            {
                if (EmotionIcons[i] != null)
                {
                    string key = EmotionIcons[i].name.ToLower();
                    _emotionDictionary[key] = EmotionIcons[i];
                }
            }
        }
    }

    /// <summary>
    /// Start a sequence of dialogue lines
    /// </summary>
    public void StartDialogueSequence(List<DialogueLine> dialogueSequence)
    {
        if (dialogueSequence == null || dialogueSequence.Count == 0)
        {
            Debug.LogWarning("Attempted to start empty dialogue sequence");
            return;
        }
        
        // Store the sequence
        _currentDialogueSequence = dialogueSequence;
        _currentDialogueIndex = 0;
        
        // Show the first dialogue
        ShowDialogueLine(_currentDialogueSequence[_currentDialogueIndex]);
        
        Debug.Log("Started dialogue sequence with " + dialogueSequence.Count + " lines");
    }

    /// <summary>
    /// Show a single line of dialogue
    /// </summary>
    private void ShowDialogueLine(DialogueLine dialogueLine)
    {
        if (dialogueLine == null || DialoguePanel == null)
        {
            return;
        }
        
        // Show the dialogue panel
        DialoguePanel.SetActive(true);
        
        // Play panel animation if available
        if (DialoguePanelAnimation != null)
        {
            DialoguePanelAnimation.Play();
        }
        
        // Set speaker name
        if (SpeakerNameText != null)
        {
            SpeakerNameText.text = dialogueLine.SpeakerName;
        }
        
        // Clear previous dialogue text
        if (DialogueText != null)
        {
            DialogueText.text = "";
        }
        
        // Set speaker portrait
        if (SpeakerPortrait != null)
        {
            string portraitKey = dialogueLine.SpeakerName.ToLower().Replace(" ", "_");
            if (_portraitDictionary.ContainsKey(portraitKey))
            {
                SpeakerPortrait.sprite = _portraitDictionary[portraitKey];
                SpeakerPortrait.enabled = true;
            }
            else
            {
                SpeakerPortrait.enabled = false;
            }
        }
        
        // Set emotion indicator
        if (EmotionIndicator != null && !string.IsNullOrEmpty(dialogueLine.EmotionState))
        {
            string emotionKey = dialogueLine.EmotionState.ToLower();
            if (_emotionDictionary.ContainsKey(emotionKey))
            {
                EmotionIndicator.sprite = _emotionDictionary[emotionKey];
                EmotionIndicator.enabled = true;
            }
            else
            {
                EmotionIndicator.enabled = false;
            }
        }
        
        // Start typing effect
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        
        _typingCoroutine = StartCoroutine(TypeDialogueText(dialogueLine.DialogueText));
        
        // Play dialogue appear sound
        PlaySound(DialogueAppearSound);
        
        // Play voice clip if available
        if (dialogueLine.VoiceClip != null && GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlayVoiceClip(dialogueLine.VoiceClip);
        }
    }

    /// <summary>
    /// Show a narration text (textbox without speaker portrait or name)
    /// </summary>
    public void ShowNarration(string narrationText)
    {
        if (string.IsNullOrEmpty(narrationText) || NarrationPanel == null || NarrationText == null)
        {
            return;
        }
        
        // Show narration panel
        NarrationPanel.SetActive(true);
        
        // Start typing effect
        StartCoroutine(TypeNarrationText(narrationText));
        
        // Play sound
        PlaySound(DialogueAppearSound);
    }

    /// <summary>
    /// Type out dialogue text character by character
    /// </summary>
    private IEnumerator TypeDialogueText(string text)
    {
        if (DialogueText == null)
        {
            yield break;
        }
        
        _isTyping = true;
        
        // Clear text
        DialogueText.text = "";
        
        // Disable continue button while typing
        if (ContinueButton != null)
        {
            ContinueButton.interactable = false;
        }
        
        // Type out text character by character
        for (int i = 0; i < text.Length; i++)
        {
            DialogueText.text += text[i];
            
            // Play typing sound on certain characters
            if (text[i] != ' ' && text[i] != '.' && text[i] != ',' && text[i] != '!' && text[i] != '?')
            {
                PlaySound(DialogueTypeSound, 0.1f);
            }
            
            // Pause for punctuation
            if (text[i] == '.' || text[i] == '!' || text[i] == '?')
            {
                yield return new WaitForSeconds(DialoguePauseDuration);
            }
            else if (text[i] == ',')
            {
                yield return new WaitForSeconds(DialoguePauseDuration / 2);
            }
            else
            {
                yield return new WaitForSeconds(CharacterTypingSpeed);
            }
        }
        
        _isTyping = false;
        
        // Enable continue button
        if (ContinueButton != null)
        {
            ContinueButton.interactable = true;
        }
        
        // Auto advance if enabled
        if (AutoAdvance)
        {
            yield return new WaitForSeconds(AutoAdvanceDelay);
            if (!_isTyping)
            {
                AdvanceDialogue();
            }
        }
    }

    /// <summary>
    /// Type out narration text character by character
    /// </summary>
    private IEnumerator TypeNarrationText(string text)
    {
        if (NarrationText == null)
        {
            yield break;
        }
        
        _isTyping = true;
        
        // Clear text
        NarrationText.text = "";
        
        // Disable continue button while typing
        if (NarrationContinueButton != null)
        {
            NarrationContinueButton.interactable = false;
        }
        
        // Type out text character by character
        for (int i = 0; i < text.Length; i++)
        {
            NarrationText.text += text[i];
            
            // Play typing sound on certain characters
            if (text[i] != ' ' && text[i] != '.' && text[i] != ',' && text[i] != '!' && text[i] != '?')
            {
                PlaySound(DialogueTypeSound, 0.1f);
            }
            
            // Pause for punctuation
            if (text[i] == '.' || text[i] == '!' || text[i] == '?')
            {
                yield return new WaitForSeconds(DialoguePauseDuration);
            }
            else if (text[i] == ',')
            {
                yield return new WaitForSeconds(DialoguePauseDuration / 2);
            }
            else
            {
                yield return new WaitForSeconds(CharacterTypingSpeed);
            }
        }
        
        _isTyping = false;
        
        // Enable continue button
        if (NarrationContinueButton != null)
        {
            NarrationContinueButton.interactable = true;
        }
    }

    /// <summary>
    /// Handle continue button click
    /// </summary>
    private void OnContinueButtonClicked()
    {
        PlaySound(DialogueAdvanceSound);
        
        if (_isTyping)
        {
            // Skip to end of current text
            CompleteDiaglogueImmediately();
        }
        else
        {
            // Advance to next dialogue
            AdvanceDialogue();
        }
    }

    /// <summary>
    /// Complete current dialogue text immediately (skip typing)
    /// </summary>
    private void CompleteDiaglogueImmediately()
    {
        if (_isTyping && _currentDialogueSequence != null && _currentDialogueIndex < _currentDialogueSequence.Count)
        {
            StopCoroutine(_typingCoroutine);
            
            if (DialogueText != null)
            {
                DialogueText.text = _currentDialogueSequence[_currentDialogueIndex].DialogueText;
            }
            
            _isTyping = false;
            
            // Enable continue button
            if (ContinueButton != null)
            {
                ContinueButton.interactable = true;
            }
        }
    }

    /// <summary>
    /// Advance to the next dialogue line
    /// </summary>
    private void AdvanceDialogue()
    {
        if (_currentDialogueSequence == null)
        {
            // No dialogue sequence active
            HideDialoguePanel();
            return;
        }
        
        _currentDialogueIndex++;
        
        if (_currentDialogueIndex >= _currentDialogueSequence.Count)
        {
            // End of dialogue sequence
            EndDialogueSequence();
        }
        else
        {
            // Show next dialogue line
            ShowDialogueLine(_currentDialogueSequence[_currentDialogueIndex]);
        }
    }

    /// <summary>
    /// End the current dialogue sequence
    /// </summary>
    private void EndDialogueSequence()
    {
        HideDialoguePanel();
        _currentDialogueSequence = null;
        _currentDialogueIndex = 0;
    }

    /// <summary>
    /// Hide the dialogue panel
    /// </summary>
    private void HideDialoguePanel()
    {
        if (DialoguePanel != null)
        {
            DialoguePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Handle narration continue button click
    /// </summary>
    private void OnNarrationContinueButtonClicked()
    {
        PlaySound(DialogueAdvanceSound);
        
        if (_isTyping)
        {
            // Complete narration immediately
            if (NarrationText != null && NarrationContinueButton != null)
            {
                StopAllCoroutines();
                _isTyping = false;
                NarrationContinueButton.interactable = true;
            }
        }
        else
        {
            // Close narration panel
            if (NarrationPanel != null)
            {
                NarrationPanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Check if dialogue is currently active
    /// </summary>
    public bool IsDialogueActive()
    {
        return (DialoguePanel != null && DialoguePanel.activeSelf) || 
               (NarrationPanel != null && NarrationPanel.activeSelf);
    }

    /// <summary>
    /// Play a sound effect
    /// </summary>
    private void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null && GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlayUISound(clip, volume);
        }
    }
}
