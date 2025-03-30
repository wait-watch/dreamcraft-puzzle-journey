using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the victory screen elements and animations
/// </summary>
public class VictoryScreenController : MonoBehaviour
{
    [Header("Victory Screen Elements")]
    public Text LevelNameText;
    public Text CompletionTimeText;
    public Text ScoreText;
    public GameObject StarsContainer;
    public Image[] StarImages;
    public Button NextLevelButton;
    public Button RetryButton;
    public Button MainMenuButton;
    
    [Header("Narrative Elements")]
    public Text StoryOutcomeText;
    public Image StoryChoiceImage;
    
    [Header("Animations")]
    public Animation StarAnimation;
    public Animation ScoreAnimation;
    public ParticleSystem VictoryParticles;

    /// <summary>
    /// Initialize the victory screen
    /// </summary>
    private void OnEnable()
    {
        // Set up button listeners
        if (NextLevelButton != null)
        {
            NextLevelButton.onClick.RemoveAllListeners();
            NextLevelButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnNextLevelButtonClicked();
            });
        }
        
        if (RetryButton != null)
        {
            RetryButton.onClick.RemoveAllListeners();
            RetryButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnRetryButtonClicked();
            });
        }
        
        if (MainMenuButton != null)
        {
            MainMenuButton.onClick.RemoveAllListeners();
            MainMenuButton.onClick.AddListener(() => {
                PlayButtonSound();
                OnMainMenuButtonClicked();
            });
        }
        
        // Start victory particles
        if (VictoryParticles != null)
        {
            VictoryParticles.Play();
        }
    }

    /// <summary>
    /// Show the level results on the victory screen
    /// </summary>
    public void ShowLevelResults(LevelData levelData)
    {
        if (levelData == null) return;
        
        // Set level name
        if (LevelNameText != null)
        {
            LevelNameText.text = levelData.LevelName;
        }
        
        // Get completion stats from the current level
        Level currentLevel = FindObjectOfType<Level>();
        if (currentLevel != null)
        {
            // Set completion time
            if (CompletionTimeText != null)
            {
                float completionTime = currentLevel.GetCompletionTime();
                int minutes = Mathf.FloorToInt(completionTime / 60);
                int seconds = Mathf.FloorToInt(completionTime % 60);
                CompletionTimeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
            }
            
            // Set score
            if (ScoreText != null)
            {
                int score = currentLevel.GetScore();
                ScoreText.text = "Score: " + score;
                
                // Animate score counting up
                StartCoroutine(AnimateScoreCounter(score));
            }
            
            // Set stars based on performance
            if (StarImages != null && StarImages.Length > 0)
            {
                int stars = currentLevel.CalculateStars();
                
                // Save level progress
                GameManager.Instance.SaveSystem.SaveLevelProgress(
                    GameManager.Instance.CurrentLevelIndex, 
                    stars, 
                    currentLevel.GetCompletionTime()
                );
                
                // Show stars animation with delay
                StartCoroutine(AnimateStars(stars));
            }
            
            // Set story outcome text
            if (StoryOutcomeText != null)
            {
                string outcome = currentLevel.GetStoryOutcome();
                StoryOutcomeText.text = outcome;
            }
        }
        
        // Check if this is the last level
        if (NextLevelButton != null)
        {
            bool isLastLevel = (GameManager.Instance.CurrentLevelIndex >= 
                               GameManager.Instance.LevelManager.AvailableLevels.Count - 1);
            NextLevelButton.gameObject.SetActive(!isLastLevel);
        }
    }
    
    /// <summary>
    /// Animate the stars appearing with a delay
    /// </summary>
    private IEnumerator AnimateStars(int starsCount)
    {
        // Hide all stars initially
        for (int i = 0; i < StarImages.Length; i++)
        {
            StarImages[i].gameObject.SetActive(false);
        }
        
        // Reveal stars one by one
        for (int i = 0; i < Mathf.Min(starsCount, StarImages.Length); i++)
        {
            yield return new WaitForSeconds(0.5f);
            
            StarImages[i].gameObject.SetActive(true);
            
            // Play star animation if available
            Animation anim = StarImages[i].GetComponent<Animation>();
            if (anim != null)
            {
                anim.Play();
            }
            
            // Play sound
            PlayStarSound();
        }
    }
    
    /// <summary>
    /// Animate the score counter
    /// </summary>
    private IEnumerator AnimateScoreCounter(int finalScore)
    {
        int currentCount = 0;
        float countDuration = 1.5f;
        float countSpeed = finalScore / countDuration;
        
        while (currentCount < finalScore)
        {
            currentCount += Mathf.CeilToInt(countSpeed * Time.deltaTime);
            currentCount = Mathf.Min(currentCount, finalScore);
            
            ScoreText.text = "Score: " + currentCount;
            
            yield return null;
        }
    }

    /// <summary>
    /// Handle next level button click
    /// </summary>
    private void OnNextLevelButtonClicked()
    {
        GameManager.Instance.NextLevel();
    }

    /// <summary>
    /// Handle retry button click
    /// </summary>
    private void OnRetryButtonClicked()
    {
        GameManager.Instance.RestartLevel();
    }

    /// <summary>
    /// Handle main menu button click
    /// </summary>
    private void OnMainMenuButtonClicked()
    {
        GameManager.Instance.QuitToMainMenu();
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
    /// Play star earned sound
    /// </summary>
    private void PlayStarSound()
    {
        AudioClip starSound = GameManager.Instance.AudioManager.GetSoundEffect("star_earned");
        if (starSound != null)
        {
            GameManager.Instance.AudioManager.PlayUISound(starSound);
        }
    }
}
