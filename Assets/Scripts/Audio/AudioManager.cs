using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Handles all audio playback and management in the game
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource MusicSource;
    public AudioSource AmbientSource;
    public AudioSource SoundEffectsSource;
    public AudioSource UISource;
    public AudioSource VoiceSource;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float MasterVolume = 1.0f;
    [Range(0f, 1f)]
    public float MusicVolume = 0.8f;
    [Range(0f, 1f)]
    public float SFXVolume = 1.0f;
    [Range(0f, 1f)]
    public float AmbientVolume = 0.5f;
    [Range(0f, 1f)]
    public float UIVolume = 0.8f;
    [Range(0f, 1f)]
    public float VoiceVolume = 1.0f;
    
    [Header("Audio Mixer")]
    public AudioMixer AudioMixer;
    
    [Header("Sound Effects")]
    public AudioClip[] SoundEffects;
    
    [Header("UI Sounds")]
    public AudioClip ButtonClickSound;
    public AudioClip SliderSound;
    public AudioClip ErrorSound;
    public AudioClip SuccessSound;
    public AudioClip StarEarnedSound;
    
    [Header("Music")]
    public AudioClip MainMenuMusic;
    public AudioClip[] LevelMusic;
    public AudioClip VictoryMusic;
    public AudioClip GameOverMusic;
    
    // Dictionary to store sound effects for quick access
    private Dictionary<string, AudioClip> _soundEffectsDictionary = new Dictionary<string, AudioClip>();
    
    // Track current audio state
    private AudioClip _currentMusic;
    private AudioClip _currentAmbient;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        // Create audio sources if not assigned
        if (MusicSource == null)
        {
            MusicSource = CreateAudioSource("Music Source");
            MusicSource.loop = true;
            MusicSource.priority = 0;
        }
        
        if (AmbientSource == null)
        {
            AmbientSource = CreateAudioSource("Ambient Source");
            AmbientSource.loop = true;
            AmbientSource.priority = 10;
        }
        
        if (SoundEffectsSource == null)
        {
            SoundEffectsSource = CreateAudioSource("SFX Source");
            SoundEffectsSource.loop = false;
            SoundEffectsSource.priority = 128;
        }
        
        if (UISource == null)
        {
            UISource = CreateAudioSource("UI Source");
            UISource.loop = false;
            UISource.priority = 64;
        }
        
        if (VoiceSource == null)
        {
            VoiceSource = CreateAudioSource("Voice Source");
            VoiceSource.loop = false;
            VoiceSource.priority = 32;
        }
        
        // Build sound effects dictionary
        if (SoundEffects != null)
        {
            foreach (AudioClip clip in SoundEffects)
            {
                if (clip != null)
                {
                    _soundEffectsDictionary[clip.name] = clip;
                }
            }
        }
        
        // Add UI sounds to dictionary
        if (ButtonClickSound != null) _soundEffectsDictionary["button_click"] = ButtonClickSound;
        if (SliderSound != null) _soundEffectsDictionary["slider"] = SliderSound;
        if (ErrorSound != null) _soundEffectsDictionary["error"] = ErrorSound;
        if (SuccessSound != null) _soundEffectsDictionary["success"] = SuccessSound;
        if (StarEarnedSound != null) _soundEffectsDictionary["star_earned"] = StarEarnedSound;
    }

    private void Start()
    {
        // Apply initial volume settings
        ApplyVolumeSettings();
    }

    /// <summary>
    /// Create an audio source component
    /// </summary>
    private AudioSource CreateAudioSource(string name)
    {
        GameObject sourceObj = new GameObject(name);
        sourceObj.transform.parent = transform;
        return sourceObj.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Play background music
    /// </summary>
    public void PlayMusic(AudioClip musicClip, bool fade = true)
    {
        if (musicClip == null || MusicSource == null)
        {
            return;
        }
        
        // Don't play the same clip again
        if (_currentMusic == musicClip && MusicSource.isPlaying)
        {
            return;
        }
        
        _currentMusic = musicClip;
        
        if (fade)
        {
            // Crossfade to new music
            StartCoroutine(CrossFadeMusic(musicClip, 1.0f));
        }
        else
        {
            // Play immediately
            MusicSource.clip = musicClip;
            MusicSource.volume = MusicVolume * MasterVolume;
            MusicSource.Play();
        }
        
        Debug.Log("Playing music: " + musicClip.name);
    }

    /// <summary>
    /// Play ambient sound loop
    /// </summary>
    public void PlayAmbientSound(AudioClip ambientClip, bool fade = true)
    {
        if (ambientClip == null || AmbientSource == null)
        {
            return;
        }
        
        // Don't play the same clip again
        if (_currentAmbient == ambientClip && AmbientSource.isPlaying)
        {
            return;
        }
        
        _currentAmbient = ambientClip;
        
        if (fade)
        {
            // Crossfade to new ambient sound
            StartCoroutine(CrossFadeAmbient(ambientClip, 1.0f));
        }
        else
        {
            // Play immediately
            AmbientSource.clip = ambientClip;
            AmbientSource.volume = AmbientVolume * MasterVolume;
            AmbientSource.Play();
        }
        
        Debug.Log("Playing ambient sound: " + ambientClip.name);
    }

    /// <summary>
    /// Play a sound effect
    /// </summary>
    public void PlaySoundEffect(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null || SoundEffectsSource == null)
        {
            return;
        }
        
        SoundEffectsSource.PlayOneShot(clip, volume * SFXVolume * MasterVolume);
    }

    /// <summary>
    /// Play a sound effect by name
    /// </summary>
    public void PlaySoundEffectByName(string clipName, float volume = 1.0f)
    {
        if (string.IsNullOrEmpty(clipName) || !_soundEffectsDictionary.ContainsKey(clipName))
        {
            Debug.LogWarning("Sound effect not found: " + clipName);
            return;
        }
        
        PlaySoundEffect(_soundEffectsDictionary[clipName], volume);
    }

    /// <summary>
    /// Play a UI sound
    /// </summary>
    public void PlayUISound(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null || UISource == null)
        {
            return;
        }
        
        UISource.PlayOneShot(clip, volume * UIVolume * MasterVolume);
    }

    /// <summary>
    /// Play a voice clip
    /// </summary>
    public void PlayVoiceClip(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null || VoiceSource == null)
        {
            return;
        }
        
        VoiceSource.clip = clip;
        VoiceSource.volume = volume * VoiceVolume * MasterVolume;
        VoiceSource.Play();
    }

    /// <summary>
    /// Stop the currently playing music
    /// </summary>
    public void StopMusic(bool fade = true)
    {
        if (MusicSource == null)
        {
            return;
        }
        
        if (fade)
        {
            StartCoroutine(FadeOutMusic(1.0f));
        }
        else
        {
            MusicSource.Stop();
        }
        
        _currentMusic = null;
    }

    /// <summary>
    /// Pause all audio (for game pause)
    /// </summary>
    public void PauseAllAudio()
    {
        if (MusicSource != null && MusicSource.isPlaying)
        {
            MusicSource.Pause();
        }
        
        if (AmbientSource != null && AmbientSource.isPlaying)
        {
            AmbientSource.Pause();
        }
        
        if (SoundEffectsSource != null && SoundEffectsSource.isPlaying)
        {
            SoundEffectsSource.Pause();
        }
        
        if (VoiceSource != null && VoiceSource.isPlaying)
        {
            VoiceSource.Pause();
        }
    }

    /// <summary>
    /// Resume all audio (after game unpause)
    /// </summary>
    public void ResumeAllAudio()
    {
        if (MusicSource != null && !MusicSource.isPlaying && MusicSource.clip != null)
        {
            MusicSource.UnPause();
        }
        
        if (AmbientSource != null && !AmbientSource.isPlaying && AmbientSource.clip != null)
        {
            AmbientSource.UnPause();
        }
        
        if (SoundEffectsSource != null && !SoundEffectsSource.isPlaying && SoundEffectsSource.clip != null)
        {
            SoundEffectsSource.UnPause();
        }
        
        if (VoiceSource != null && !VoiceSource.isPlaying && VoiceSource.clip != null)
        {
            VoiceSource.UnPause();
        }
    }

    /// <summary>
    /// Set the volume level
    /// </summary>
    public void SetVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
    }

    /// <summary>
    /// Set specific volume level
    /// </summary>
    public void SetVolumeType(string volumeType, float volume)
    {
        volume = Mathf.Clamp01(volume);
        
        switch (volumeType.ToLower())
        {
            case "master":
                MasterVolume = volume;
                break;
            case "music":
                MusicVolume = volume;
                break;
            case "sfx":
                SFXVolume = volume;
                break;
            case "ambient":
                AmbientVolume = volume;
                break;
            case "ui":
                UIVolume = volume;
                break;
            case "voice":
                VoiceVolume = volume;
                break;
        }
        
        ApplyVolumeSettings();
    }

    /// <summary>
    /// Apply all volume settings to audio sources
    /// </summary>
    private void ApplyVolumeSettings()
    {
        if (MusicSource != null)
        {
            MusicSource.volume = MusicVolume * MasterVolume;
        }
        
        if (AmbientSource != null)
        {
            AmbientSource.volume = AmbientVolume * MasterVolume;
        }
        
        // SFX and UI volume is applied at the time of playing
        
        if (VoiceSource != null)
        {
            VoiceSource.volume = VoiceVolume * MasterVolume;
        }
        
        // Set mixer values if available
        if (AudioMixer != null)
        {
            float masterDb = MasterVolume > 0.001f ? 20.0f * Mathf.Log10(MasterVolume) : -80.0f;
            float musicDb = MusicVolume > 0.001f ? 20.0f * Mathf.Log10(MusicVolume) : -80.0f;
            float sfxDb = SFXVolume > 0.001f ? 20.0f * Mathf.Log10(SFXVolume) : -80.0f;
            float ambientDb = AmbientVolume > 0.001f ? 20.0f * Mathf.Log10(AmbientVolume) : -80.0f;
            float uiDb = UIVolume > 0.001f ? 20.0f * Mathf.Log10(UIVolume) : -80.0f;
            float voiceDb = VoiceVolume > 0.001f ? 20.0f * Mathf.Log10(VoiceVolume) : -80.0f;
            
            AudioMixer.SetFloat("MasterVolume", masterDb);
            AudioMixer.SetFloat("MusicVolume", musicDb);
            AudioMixer.SetFloat("SFXVolume", sfxDb);
            AudioMixer.SetFloat("AmbientVolume", ambientDb);
            AudioMixer.SetFloat("UIVolume", uiDb);
            AudioMixer.SetFloat("VoiceVolume", voiceDb);
        }
    }

    /// <summary>
    /// Crossfade to new music
    /// </summary>
    private IEnumerator CrossFadeMusic(AudioClip newClip, float fadeDuration)
    {
        // Cancel any existing fade
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        float startVolume = MusicSource.volume;
        float timer = 0;
        
        // Fade out current music
        while (timer < fadeDuration / 2)
        {
            timer += Time.deltaTime;
            MusicSource.volume = Mathf.Lerp(startVolume, 0, timer / (fadeDuration / 2));
            yield return null;
        }
        
        // Switch to new clip
        MusicSource.clip = newClip;
        MusicSource.Play();
        
        // Reset timer for fade in
        timer = 0;
        float targetVolume = MusicVolume * MasterVolume;
        
        // Fade in new music
        while (timer < fadeDuration / 2)
        {
            timer += Time.deltaTime;
            MusicSource.volume = Mathf.Lerp(0, targetVolume, timer / (fadeDuration / 2));
            yield return null;
        }
        
        // Ensure final volume is correct
        MusicSource.volume = targetVolume;
    }

    /// <summary>
    /// Crossfade to new ambient sound
    /// </summary>
    private IEnumerator CrossFadeAmbient(AudioClip newClip, float fadeDuration)
    {
        float startVolume = AmbientSource.volume;
        float timer = 0;
        
        // Fade out current ambient
        while (timer < fadeDuration / 2)
        {
            timer += Time.deltaTime;
            AmbientSource.volume = Mathf.Lerp(startVolume, 0, timer / (fadeDuration / 2));
            yield return null;
        }
        
        // Switch to new clip
        AmbientSource.clip = newClip;
        AmbientSource.Play();
        
        // Reset timer for fade in
        timer = 0;
        float targetVolume = AmbientVolume * MasterVolume;
        
        // Fade in new ambient
        while (timer < fadeDuration / 2)
        {
            timer += Time.deltaTime;
            AmbientSource.volume = Mathf.Lerp(0, targetVolume, timer / (fadeDuration / 2));
            yield return null;
        }
        
        // Ensure final volume is correct
        AmbientSource.volume = targetVolume;
    }

    /// <summary>
    /// Fade out music
    /// </summary>
    private IEnumerator FadeOutMusic(float fadeDuration)
    {
        float startVolume = MusicSource.volume;
        float timer = 0;
        
        // Fade out
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            MusicSource.volume = Mathf.Lerp(startVolume, 0, timer / fadeDuration);
            yield return null;
        }
        
        // Stop music
        MusicSource.Stop();
        MusicSource.clip = null;
    }

    /// <summary>
    /// Get a sound effect by name
    /// </summary>
    public AudioClip GetSoundEffect(string name)
    {
        if (_soundEffectsDictionary.ContainsKey(name))
        {
            return _soundEffectsDictionary[name];
        }
        
        Debug.LogWarning("Sound effect not found: " + name);
        return null;
    }
}
