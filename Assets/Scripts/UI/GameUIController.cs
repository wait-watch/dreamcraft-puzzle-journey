using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the in-game UI elements during gameplay
/// </summary>
public class GameUIController : MonoBehaviour
{
    [Header("Game HUD Elements")]
    public Text ScoreText;
    public Text TimeRemainingText;
    public Slider ProgressBar;
    public Button PauseButton;
    
    [Header("Power UI Elements")]
    public GameObject PowersPanel;
    public Button ResizePowerButton;
    public Button GravityPowerButton;
    public Button TimePowerButton;
    public Image[] PowerChargeIndicators;
    
    [Header("Objective UI")]
    public Text ObjectiveText;
    public GameObject ObjectivePanel;
    public Button ObjectiveCloseButton;
    
    [Header("Tutorial Elements")]
    public GameObject TutorialOverlay;
    public Text TutorialText;
    public Button TutorialNextButton;
    
    [Header("Puzzle UI")]
    public GameObject HintButton;
    public GameObject HintPanel;
    public Text HintText;
    public Button HintCloseButton;

    private List<string> _tutorialSteps = new List<string>();
    private int _currentTutorialStep = 0;

    /// <summary>
    /// Initialize the game UI
    /// </summary>
    public void Initialize()
    {
        // Set up button listeners
        if (PauseButton != null)
        {
            PauseButton.onClick.RemoveAllListeners();
            PauseButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnPauseButtonClicked();
            });
        }
        
        // Set up power buttons
        SetupPowerButtons();
        
        // Set up objective panel
        if (ObjectiveCloseButton != null)
        {
            ObjectiveCloseButton.onClick.RemoveAllListeners();
            ObjectiveCloseButton.onClick.AddListener(() => {
                PlayButtonSound();
                ObjectivePanel.SetActive(false);
            });
        }
        
        // Show objective panel initially
        if (ObjectivePanel != null)
        {
            ObjectivePanel.SetActive(true);
            StartCoroutine(AutoHideObjectivePanel(5.0f));
        }
        
        // Set up hint panel
        if (HintButton != null)
        {
            HintButton.GetComponent<Button>().onClick.RemoveAllListeners();
            HintButton.GetComponent<Button>().onClick.AddListener(() => {
                PlayButtonSound();
                ShowHint();
            });
        }
        
        if (HintCloseButton != null)
        {
            HintCloseButton.onClick.RemoveAllListeners();
            HintCloseButton.onClick.AddListener(() => {
                PlayButtonSound();
                HintPanel.SetActive(false);
            });
        }
        
        // Set up tutorial if it's the first level
        if (GameManager.Instance.CurrentLevelIndex == 0)
        {
            SetupTutorial();
        }
        else
        {
            if (TutorialOverlay != null)
            {
                TutorialOverlay.SetActive(false);
            }
        }
        
        // Set initial UI values
        UpdateUI(0, 300, 3); // Default values
    }

    /// <summary>
    /// Update the UI elements with current game values
    /// </summary>
    public void UpdateUI(int score, float timeRemaining, int powerCharges)
    {
        // Update score
        if (ScoreText != null)
        {
            ScoreText.text = "Score: " + score;
        }
        
        // Update time
        if (TimeRemainingText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            TimeRemainingText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            // Change color based on time remaining
            if (timeRemaining <= 30)
            {
                TimeRemainingText.color = Color.red;
            }
            else if (timeRemaining <= 60)
            {
                TimeRemainingText.color = Color.yellow;
            }
            else
            {
                TimeRemainingText.color = Color.white;
            }
        }
        
        // Update power charges
        if (PowerChargeIndicators != null)
        {
            for (int i = 0; i < PowerChargeIndicators.Length; i++)
            {
                if (PowerChargeIndicators[i] != null)
                {
                    PowerChargeIndicators[i].enabled = (i < powerCharges);
                }
            }
        }
        
        // Update power buttons interactability
        UpdatePowerButtonsState(powerCharges > 0);
    }

    /// <summary>
    /// Set up the tutorial steps for the first level
    /// </summary>
    private void SetupTutorial()
    {
        if (TutorialOverlay == null || TutorialText == null || TutorialNextButton == null)
        {
            return;
        }
        
        // Create tutorial steps
        _tutorialSteps.Clear();
        _tutorialSteps.Add("Welcome to DreamCraft! In this game, you'll explore dream worlds and solve puzzles.");
        _tutorialSteps.Add("Use the joystick on the left to move your character around the dream world.");
        _tutorialSteps.Add("Tap on objects to interact with them. Some objects are part of puzzles.");
        _tutorialSteps.Add("You have special dream powers that can help you solve puzzles.");
        _tutorialSteps.Add("The resize power lets you make objects bigger or smaller.");
        _tutorialSteps.Add("The gravity power lets you change how gravity affects certain objects.");
        _tutorialSteps.Add("The time power lets you temporarily slow down time.");
        _tutorialSteps.Add("Your choices in the dream world will affect the story and lead to different endings.");
        _tutorialSteps.Add("Good luck on your journey through the dreamscape!");
        
        // Set up next button
        TutorialNextButton.onClick.RemoveAllListeners();
        TutorialNextButton.onClick.AddListener(() => {
            PlayButtonSound();
            NextTutorialStep();
        });
        
        // Start tutorial
        _currentTutorialStep = 0;
        TutorialOverlay.SetActive(true);
        ShowCurrentTutorialStep();
    }

    /// <summary>
    /// Show the current tutorial step
    /// </summary>
    private void ShowCurrentTutorialStep()
    {
        if (_currentTutorialStep < _tutorialSteps.Count)
        {
            TutorialText.text = _tutorialSteps[_currentTutorialStep];
        }
        
        // Update button text for last step
        if (_currentTutorialStep == _tutorialSteps.Count - 1)
        {
            TutorialNextButton.GetComponentInChildren<Text>().text = "Start Game";
        }
        else
        {
            TutorialNextButton.GetComponentInChildren<Text>().text = "Next";
        }
    }

    /// <summary>
    /// Advance to the next tutorial step
    /// </summary>
    private void NextTutorialStep()
    {
        _currentTutorialStep++;
        
        if (_currentTutorialStep < _tutorialSteps.Count)
        {
            ShowCurrentTutorialStep();
        }
        else
        {
            // End of tutorial
            TutorialOverlay.SetActive(false);
        }
    }

    /// <summary>
    /// Set up the power buttons
    /// </summary>
    private void SetupPowerButtons()
    {
        if (ResizePowerButton != null)
        {
            ResizePowerButton.onClick.RemoveAllListeners();
            ResizePowerButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnResizePowerButtonClicked();
            });
        }
        
        if (GravityPowerButton != null)
        {
            GravityPowerButton.onClick.RemoveAllListeners();
            GravityPowerButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnGravityPowerButtonClicked();
            });
        }
        
        if (TimePowerButton != null)
        {
            TimePowerButton.onClick.RemoveAllListeners();
            TimePowerButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnTimePowerButtonClicked();
            });
        }
    }

    /// <summary>
    /// Update the state of power buttons based on power availability
    /// </summary>
    private void UpdatePowerButtonsState(bool hasPowerCharges)
    {
        if (ResizePowerButton != null) ResizePowerButton.interactable = hasPowerCharges;
        if (GravityPowerButton != null) GravityPowerButton.interactable = hasPowerCharges;
        if (TimePowerButton != null) TimePowerButton.interactable = hasPowerCharges;
    }

    /// <summary>
    /// Handle pause button click
    /// </summary>
    private void OnPauseButtonClicked()
    {
        GameManager.Instance.ChangeGameState(GameState.Paused);
    }

    /// <summary>
    /// Handle resize power button click
    /// </summary>
    private void OnResizePowerButtonClicked()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ActivatePower(DreamPowerType.Resize);
        }
    }

    /// <summary>
    /// Handle gravity power button click
    /// </summary>
    private void OnGravityPowerButtonClicked()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ActivatePower(DreamPowerType.Gravity);
        }
    }

    /// <summary>
    /// Handle time power button click
    /// </summary>
    private void OnTimePowerButtonClicked()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ActivatePower(DreamPowerType.Time);
        }
    }

    /// <summary>
    /// Play button click sound
    /// </summary>
    private void PlayButtonSound()
    {
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.PlayUISound(GameManager.Instance.UIManager.ButtonClickSound);
        }
    }

    /// <summary>
    /// Auto-hide the objective panel after a delay
    /// </summary>
    private IEnumerator AutoHideObjectivePanel(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ObjectivePanel != null)
        {
            ObjectivePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Set the objective text
    /// </summary>
    public void SetObjective(string objective)
    {
        if (ObjectiveText != null)
        {
            ObjectiveText.text = objective;
        }
    }

    /// <summary>
    /// Show the hint panel with the current puzzle hint
    /// </summary>
    private void ShowHint()
    {
        if (HintPanel != null && HintText != null)
        {
            // Get the current puzzle hint from level
            Level currentLevel = FindObjectOfType<Level>();
            if (currentLevel != null)
            {
                string hint = currentLevel.GetCurrentPuzzleHint();
                HintText.text = hint;
                HintPanel.SetActive(true);
            }
        }
    }
}
