using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An object that can have its gravity altered by the player's gravity power
/// </summary>
public class GravityPuzzleObject : MonoBehaviour
{
    [Header("Gravity Settings")]
    public bool IsGravityAffectable = true;
    public float DefaultGravityScale = 1.0f;
    public bool CanInvertGravity = true;
    
    [Header("Puzzle Logic")]
    public float TargetGravityScale = -1.0f;
    public float GravityTolerance = 0.1f;
    
    [Header("Effects")]
    public ParticleSystem GravityChangeParticles;
    public TrailRenderer GravityTrail;
    public AudioClip GravityChangeSound;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnGravityChanged;
    public UnityEngine.Events.UnityEvent OnTargetGravityReached;
    
    private Rigidbody _rigidbody;
    private float _originalGravityScale;
    private bool _targetReached = false;
    private Vector3 _originalGravity;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        if (_rigidbody != null)
        {
            _originalGravityScale = _rigidbody.gravityScale;
        }
        
        _originalGravity = Physics.gravity;
        
        // Initialize trail if available
        if (GravityTrail != null)
        {
            GravityTrail.emitting = false;
        }
    }

    private void Update()
    {
        // Check if target gravity has been reached
        if (!_targetReached && IsGravityAffectable && _rigidbody != null)
        {
            if (Mathf.Abs(_rigidbody.gravityScale - TargetGravityScale) <= GravityTolerance)
            {
                _targetReached = true;
                OnTargetGravityReached.Invoke();
                
                // Play particles if available
                if (GravityChangeParticles != null)
                {
                    GravityChangeParticles.Play();
                }
                
                // Play sound if available
                if (GravityChangeSound != null && GameManager.Instance.AudioManager != null)
                {
                    GameManager.Instance.AudioManager.PlaySoundEffect(GravityChangeSound);
                }
                
                // Notify parent puzzle if this is part of a puzzle
                InteractableObject interactable = GetComponent<InteractableObject>();
                if (interactable != null && interactable.ParentPuzzle != null)
                {
                    interactable.OnInteraction.Invoke(interactable);
                }
            }
        }
        
        // Update trail if gravity is inverted
        if (GravityTrail != null && _rigidbody != null)
        {
            GravityTrail.emitting = _rigidbody.gravityScale < 0;
        }
    }

    /// <summary>
    /// Change the gravity scale of this object
    /// </summary>
    public void ChangeGravityScale(float newScale)
    {
        if (!IsGravityAffectable || _rigidbody == null)
        {
            return;
        }
        
        // Apply new gravity scale
        _rigidbody.gravityScale = newScale;
        
        // Play effects
        if (GravityChangeParticles != null)
        {
            GravityChangeParticles.Play();
        }
        
        // Play sound
        if (GravityChangeSound != null && GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlaySoundEffect(GravityChangeSound);
        }
        
        // Fire event
        OnGravityChanged.Invoke();
    }

    /// <summary>
    /// Toggle between normal and inverted gravity
    /// </summary>
    public void ToggleGravity()
    {
        if (!IsGravityAffectable || !CanInvertGravity || _rigidbody == null)
        {
            return;
        }
        
        // Invert current gravity scale
        float newScale = -_rigidbody.gravityScale;
        ChangeGravityScale(newScale);
    }

    /// <summary>
    /// Reset gravity to the original value
    /// </summary>
    public void ResetGravity()
    {
        if (_rigidbody != null)
        {
            _rigidbody.gravityScale = _originalGravityScale;
            _targetReached = false;
            
            // Stop trail
            if (GravityTrail != null)
            {
                GravityTrail.emitting = false;
            }
        }
    }

    /// <summary>
    /// Check if the object has reached the target gravity scale
    /// </summary>
    public bool IsAtTargetGravity()
    {
        if (_rigidbody == null) return false;
        
        return Mathf.Abs(_rigidbody.gravityScale - TargetGravityScale) <= GravityTolerance;
    }
}
