using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the main menu UI elements and interactions
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu UI Elements")]
    public Button PlayButton;
    public Button LevelSelectButton;
    public Button SettingsButton;
    public Button CreditsButton;
    public Button QuitButton;
    
    [Header("Main Menu Visuals")]
    public Image GameLogo;
    public ParticleSystem DreamParticles;
    public Animation LogoAnimation;

    [Header("Audio")]
    public AudioClip MenuMusic;
    public AudioClip ButtonHoverSound;
    
    [Header("Dream Background")]
    public Image BackgroundImage;
    public float BackgroundTransitionSpeed = 3.0f;
    public Color[] BackgroundColors;
    
    private int _currentColorIndex = 0;

    /// <summary>
    /// Initialize the main menu
    /// </summary>
    public void Initialize()
    {
        // Set up button listeners
        if (PlayButton != null)
        {
            PlayButton.onClick.RemoveAllListeners();
            PlayButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnPlayButtonClicked();
            });
        }
        
        if (LevelSelectButton != null)
        {
            LevelSelectButton.onClick.RemoveAllListeners();
            LevelSelectButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnLevelSelectButtonClicked();
            });
        }
        
        if (SettingsButton != null)
        {
            SettingsButton.onClick.RemoveAllListeners();
            SettingsButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnSettingsButtonClicked();
            });
        }
        
        if (CreditsButton != null)
        {
            CreditsButton.onClick.RemoveAllListeners();
            CreditsButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnCreditsButtonClicked();
            });
        }
        
        if (QuitButton != null)
        {
            QuitButton.onClick.RemoveAllListeners();
            QuitButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnQuitButtonClicked();
            });
        }
        
        // Play menu music
        if (GameManager.Instance.AudioManager != null && MenuMusic != null)
        {
            GameManager.Instance.AudioManager.PlayMusic(MenuMusic);
        }
        
        // Start dream particles
        if (DreamParticles != null)
        {
            DreamParticles.Play();
        }
        
        // Start logo animation
        if (LogoAnimation != null)
        {
            LogoAnimation.Play();
        }
        
        // Start background color transition
        if (BackgroundImage != null && BackgroundColors != null && BackgroundColors.Length > 0)
        {
            StartCoroutine(AnimateBackgroundColor());
        }
        
        // Update button states based on game progress
        UpdateButtonStates();
    }

    /// <summary>
    /// Update button states based on game progress
    /// </summary>
    private void UpdateButtonStates()
    {
        // Enable level select only if at least one level has been completed
        if (LevelSelectButton != null)
        {
            bool hasProgress = GameManager.Instance.SaveSystem.SaveDataExists();
            LevelSelectButton.interactable = hasProgress;
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
    /// Handle play button click
    /// </summary>
    private void OnPlayButtonClicked()
    {
        // Check if there's a saved game to continue
        if (GameManager.Instance.SaveSystem.SaveDataExists())
        {
            // Show continue/new game dialog
            ShowContinueNewGameDialog();
        }
        else
        {
            // No saved game, start new game directly
            GameManager.Instance.StartNewGame();
        }
    }

    /// <summary>
    /// Show dialog to continue or start new game
    /// </summary>
    private void ShowContinueNewGameDialog()
    {
        // Find dialog in children
        Transform dialogTransform = transform.Find("ContinueNewGameDialog");
        if (dialogTransform != null)
        {
            GameObject dialog = dialogTransform.gameObject;
            dialog.SetActive(true);
            
            // Set up button listeners
            Button continueButton = dialog.transform.Find("ContinueButton").GetComponent<Button>();
            Button newGameButton = dialog.transform.Find("NewGameButton").GetComponent<Button>();
            Button cancelButton = dialog.transform.Find("CancelButton").GetComponent<Button>();
            
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(() => {
                    PlayButtonSound();
                    dialog.SetActive(false);
                    GameManager.Instance.ContinueGame();
                });
            }
            
            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveAllListeners();
                newGameButton.onClick.AddListener(() => {
                    PlayButtonSound();
                    dialog.SetActive(false);
                    GameManager.Instance.StartNewGame();
                });
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(() => {
                    PlayButtonSound();
                    dialog.SetActive(false);
                });
            }
        }
        else
        {
            // Fallback if dialog not found
            GameManager.Instance.ContinueGame();
        }
    }

    /// <summary>
    /// Handle level select button click
    /// </summary>
    private void OnLevelSelectButtonClicked()
    {
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.ShowLevelSelectScreen();
        }
    }

    /// <summary>
    /// Handle settings button click
    /// </summary>
    private void OnSettingsButtonClicked()
    {
        if (GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.ShowSettingsScreen();
        }
    }

    /// <summary>
    /// Handle credits button click
    /// </summary>
    private void OnCreditsButtonClicked()
    {
        // Find credits panel in children
        Transform creditsTransform = transform.Find("CreditsPanel");
        if (creditsTransform != null)
        {
            GameObject creditsPanel = creditsTransform.gameObject;
            creditsPanel.SetActive(true);
            
            // Set up close button
            Button closeButton = creditsPanel.transform.Find("CloseButton").GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => {
                    PlayButtonSound();
                    creditsPanel.SetActive(false);
                });
            }
        }
    }

    /// <summary>
    /// Handle quit button click
    /// </summary>
    private void OnQuitButtonClicked()
    {
        // Show quit confirmation dialog
        Transform quitDialogTransform = transform.Find("QuitConfirmationDialog");
        if (quitDialogTransform != null)
        {
            GameObject quitDialog = quitDialogTransform.gameObject;
            quitDialog.SetActive(true);
            
            // Set up dialog buttons
            Button confirmButton = quitDialog.transform.Find("ConfirmButton").GetComponent<Button>();
            Button cancelButton = quitDialog.transform.Find("CancelButton").GetComponent<Button>();
            
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(() => {
                    PlayButtonSound();
                    Application.Quit();
                    
                    // This will help with testing in Unity Editor
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    #endif
                });
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(() => {
                    PlayButtonSound();
                    quitDialog.SetActive(false);
                });
            }
        }
        else
        {
            // Fallback if dialog not found
            Application.Quit();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
    
    /// <summary>
    /// Animate background color transition
    /// </summary>
    private IEnumerator AnimateBackgroundColor()
    {
        if (BackgroundColors.Length < 2)
        {
            yield break;
        }
        
        while (true)
        {
            // Get current and target colors
            Color currentColor = BackgroundImage.color;
            int nextColorIndex = (_currentColorIndex + 1) % BackgroundColors.Length;
            Color targetColor = BackgroundColors[nextColorIndex];
            
            // Transition between colors
            float elapsedTime = 0;
            
            while (elapsedTime < BackgroundTransitionSpeed)
            {
                BackgroundImage.color = Color.Lerp(currentColor, targetColor, elapsedTime / BackgroundTransitionSpeed);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Ensure final color is exactly the target
            BackgroundImage.color = targetColor;
            _currentColorIndex = nextColorIndex;
            
            // Wait before starting next transition
            yield return new WaitForSeconds(1.0f);
        }
    }
}
