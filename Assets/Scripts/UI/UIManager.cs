using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all UI screens and transitions between them
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Screen References")]
    public GameObject MainMenuScreen;
    public GameObject GameUIScreen;
    public GameObject PauseMenuScreen;
    public GameObject VictoryScreen;
    public GameObject GameOverScreen;
    public GameObject SettingsScreen;
    public GameObject LevelSelectScreen;
    
    [Header("UI Controllers")]
    public MainMenuController MainMenuController;
    public GameUIController GameUIController;
    public VictoryScreenController VictoryController;
    
    [Header("Transition Effects")]
    public Image FadePanel;
    public float FadeDuration = 0.5f;
    
    [Header("UI Audio")]
    public AudioClip ButtonClickSound;
    public AudioClip ScreenTransitionSound;

    private GameObject _currentActiveScreen;

    private void Start()
    {
        // Initialize with fade panel invisible
        if (FadePanel != null)
        {
            Color c = FadePanel.color;
            c.a = 0;
            FadePanel.color = c;
            FadePanel.gameObject.SetActive(true);
        }
        
        // Default to main menu on start
        ShowMainMenu();
    }

    /// <summary>
    /// Show the main menu screen
    /// </summary>
    public void ShowMainMenu()
    {
        SwitchToScreen(MainMenuScreen);
        
        // Initialize main menu if controller exists
        if (MainMenuController != null)
        {
            MainMenuController.Initialize();
        }
    }

    /// <summary>
    /// Show the in-game UI
    /// </summary>
    public void ShowGameUI()
    {
        SwitchToScreen(GameUIScreen);
        
        // Initialize game UI if controller exists
        if (GameUIController != null)
        {
            GameUIController.Initialize();
        }
    }

    /// <summary>
    /// Show the pause menu
    /// </summary>
    public void ShowPauseMenu()
    {
        PauseMenuScreen.SetActive(true);
        // We don't use SwitchToScreen here because we want to keep the game UI visible behind the pause menu
    }

    /// <summary>
    /// Hide the pause menu
    /// </summary>
    public void HidePauseMenu()
    {
        PauseMenuScreen.SetActive(false);
    }

    /// <summary>
    /// Show the victory screen
    /// </summary>
    public void ShowVictoryScreen()
    {
        SwitchToScreen(VictoryScreen);
        
        // Initialize victory screen if controller exists
        if (VictoryController != null)
        {
            LevelData currentLevel = GameManager.Instance.LevelManager.GetCurrentLevelData();
            VictoryController.ShowLevelResults(currentLevel);
        }
    }

    /// <summary>
    /// Show the game over screen
    /// </summary>
    public void ShowGameOverScreen()
    {
        SwitchToScreen(GameOverScreen);
    }

    /// <summary>
    /// Show the settings screen
    /// </summary>
    public void ShowSettingsScreen()
    {
        SettingsScreen.SetActive(true);
        // Similar to pause menu, we overlay this on current screen
    }

    /// <summary>
    /// Hide the settings screen
    /// </summary>
    public void HideSettingsScreen()
    {
        SettingsScreen.SetActive(false);
    }

    /// <summary>
    /// Show the level select screen
    /// </summary>
    public void ShowLevelSelectScreen()
    {
        SwitchToScreen(LevelSelectScreen);
        
        // Populate level select screen
        PopulateLevelSelectScreen();
    }

    /// <summary>
    /// Switch to a different UI screen with transition effect
    /// </summary>
    private void SwitchToScreen(GameObject targetScreen)
    {
        if (targetScreen == null)
        {
            Debug.LogError("Target screen is null");
            return;
        }

        // Hide all screens
        if (MainMenuScreen != null) MainMenuScreen.SetActive(false);
        if (GameUIScreen != null) GameUIScreen.SetActive(false);
        if (PauseMenuScreen != null) PauseMenuScreen.SetActive(false);
        if (VictoryScreen != null) VictoryScreen.SetActive(false);
        if (GameOverScreen != null) GameOverScreen.SetActive(false);
        if (SettingsScreen != null) SettingsScreen.SetActive(false);
        if (LevelSelectScreen != null) LevelSelectScreen.SetActive(false);
        
        // Show the target screen
        targetScreen.SetActive(true);
        _currentActiveScreen = targetScreen;
        
        // Play transition sound
        PlayUISound(ScreenTransitionSound);
    }

    /// <summary>
    /// Play a UI sound effect
    /// </summary>
    public void PlayUISound(AudioClip clip)
    {
        if (clip != null && GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlayUISound(clip);
        }
    }

    /// <summary>
    /// Start fade out transition effect
    /// </summary>
    public void StartFadeOut()
    {
        if (FadePanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }
    }

    /// <summary>
    /// Start fade in transition effect
    /// </summary>
    public void StartFadeIn()
    {
        if (FadePanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// Fade out coroutine
    /// </summary>
    private IEnumerator FadeOut()
    {
        FadePanel.gameObject.SetActive(true);
        
        float elapsedTime = 0;
        Color color = FadePanel.color;
        color.a = 0;
        FadePanel.color = color;
        
        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / FadeDuration);
            FadePanel.color = color;
            yield return null;
        }
        
        color.a = 1;
        FadePanel.color = color;
    }

    /// <summary>
    /// Fade in coroutine
    /// </summary>
    private IEnumerator FadeIn()
    {
        FadePanel.gameObject.SetActive(true);
        
        float elapsedTime = 0;
        Color color = FadePanel.color;
        color.a = 1;
        FadePanel.color = color;
        
        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1 - Mathf.Clamp01(elapsedTime / FadeDuration);
            FadePanel.color = color;
            yield return null;
        }
        
        color.a = 0;
        FadePanel.color = color;
        FadePanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// Populate the level select screen with available levels
    /// </summary>
    private void PopulateLevelSelectScreen()
    {
        // Implementation depends on your level select UI structure
        // This would typically involve creating level buttons for each level
        // and setting their state (locked/unlocked) based on player progress
        
        // For this example, we'll assume LevelSelectScreen has a predefined set of buttons
        // that we just need to update
        
        Transform levelButtonsContainer = LevelSelectScreen.transform.Find("LevelButtonsContainer");
        if (levelButtonsContainer != null)
        {
            Button[] levelButtons = levelButtonsContainer.GetComponentsInChildren<Button>(true);
            
            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelIndex = i; // Capture for lambda
                string levelName = "Level_" + (i + 1);
                bool isUnlocked = GameManager.Instance.UnlockedLevels.Contains(levelName);
                
                // Get button components
                Image buttonImage = levelButtons[i].GetComponent<Image>();
                Text buttonText = levelButtons[i].GetComponentInChildren<Text>();
                
                // Update button appearance based on locked/unlocked state
                if (isUnlocked)
                {
                    // Unlocked level
                    buttonImage.color = Color.white;
                    if (buttonText != null) buttonText.color = Color.black;
                    
                    // Set button click action
                    levelButtons[i].onClick.RemoveAllListeners();
                    levelButtons[i].onClick.AddListener(() => {
                        PlayUISound(ButtonClickSound);
                        GameManager.Instance.LoadLevel(levelIndex);
                    });
                    
                    // Add stars based on level completion
                    Transform starsContainer = levelButtons[i].transform.Find("StarsContainer");
                    if (starsContainer != null)
                    {
                        (int stars, float time) = GameManager.Instance.SaveSystem.LoadLevelProgress(i);
                        
                        // Update stars display
                        for (int s = 0; s < starsContainer.childCount; s++)
                        {
                            Image starImage = starsContainer.GetChild(s).GetComponent<Image>();
                            if (starImage != null)
                            {
                                starImage.color = (s < stars) ? Color.yellow : Color.gray;
                            }
                        }
                    }
                }
                else
                {
                    // Locked level
                    buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    if (buttonText != null) buttonText.color = Color.gray;
                    
                    // Disable button action
                    levelButtons[i].onClick.RemoveAllListeners();
                    levelButtons[i].interactable = false;
                    
                    // Hide stars for locked levels
                    Transform starsContainer = levelButtons[i].transform.Find("StarsContainer");
                    if (starsContainer != null)
                    {
                        for (int s = 0; s < starsContainer.childCount; s++)
                        {
                            Image starImage = starsContainer.GetChild(s).GetComponent<Image>();
                            if (starImage != null)
                            {
                                starImage.color = Color.gray;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Update UI elements during gameplay
    /// </summary>
    public void UpdateGameUI(int score, float timeRemaining, int powerCharges)
    {
        if (GameUIController != null)
        {
            GameUIController.UpdateUI(score, timeRemaining, powerCharges);
        }
    }
}
