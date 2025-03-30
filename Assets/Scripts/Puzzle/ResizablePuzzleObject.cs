using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An object that can be resized by the player's resize power
/// </summary>
public class ResizablePuzzleObject : MonoBehaviour
{
    [Header("Resize Settings")]
    public bool IsResizable = true;
    public float MinScale = 0.2f;
    public float MaxScale = 3.0f;
    public bool MaintainMass = true;
    
    [Header("Puzzle Logic")]
    public float TargetScale = 1.0f;
    public float ScaleTolerance = 0.1f;
    
    [Header("Effects")]
    public ParticleSystem ResizeParticles;
    public AudioClip ResizeSound;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnResizeStart;
    public UnityEngine.Events.UnityEvent OnResizeEnd;
    public UnityEngine.Events.UnityEvent OnTargetScaleReached;
    
    private Vector3 _originalScale;
    private Rigidbody _rigidbody;
    private float _originalMass;
    private bool _targetReached = false;

    private void Start()
    {
        _originalScale = transform.localScale;
        _rigidbody = GetComponent<Rigidbody>();
        
        if (_rigidbody != null)
        {
            _originalMass = _rigidbody.mass;
        }
    }

    private void Update()
    {
        // Check if target scale has been reached
        if (!_targetReached && IsResizable)
        {
            float currentScaleFactor = transform.localScale.magnitude / _originalScale.magnitude;
            
            if (Mathf.Abs(currentScaleFactor - TargetScale) <= ScaleTolerance)
            {
                _targetReached = true;
                OnTargetScaleReached.Invoke();
                
                // Play particles if available
                if (ResizeParticles != null)
                {
                    ResizeParticles.Play();
                }
                
                // Play sound if available
                if (ResizeSound != null && GameManager.Instance.AudioManager != null)
                {
                    GameManager.Instance.AudioManager.PlaySoundEffect(ResizeSound);
                }
                
                // Notify parent puzzle if this is part of a puzzle
                InteractableObject interactable = GetComponent<InteractableObject>();
                if (interactable != null && interactable.ParentPuzzle != null)
                {
                    interactable.OnInteraction.Invoke(interactable);
                }
            }
        }
    }

    /// <summary>
    /// Resizes the object to a specific scale factor
    /// </summary>
    public void ResizeToScale(float scaleFactor)
    {
        if (!IsResizable)
        {
            return;
        }
        
        // Clamp scale factor within allowed range
        scaleFactor = Mathf.Clamp(scaleFactor, MinScale, MaxScale);
        
        // Calculate new scale
        Vector3 newScale = _originalScale * scaleFactor;
        
        // Apply scale
        transform.localScale = newScale;
        
        // Adjust mass if needed
        if (MaintainMass && _rigidbody != null)
        {
            // Mass scales with volume (cube of scale factor)
            float volumeFactor = Mathf.Pow(scaleFactor, 3);
            _rigidbody.mass = _originalMass * volumeFactor;
        }
        
        // Check if we should fire resize start event
        if (!OnResizeStart.GetPersistentEventCount().Equals(0))
        {
            OnResizeStart.Invoke();
        }
    }

    /// <summary>
    /// Makes the object increase in size
    /// </summary>
    public void IncreaseSize(float amount)
    {
        if (!IsResizable)
        {
            return;
        }
        
        // Calculate current scale factor
        float currentScaleFactor = transform.localScale.x / _originalScale.x;
        
        // Apply new scale factor
        ResizeToScale(currentScaleFactor + amount);
    }

    /// <summary>
    /// Makes the object decrease in size
    /// </summary>
    public void DecreaseSize(float amount)
    {
        if (!IsResizable)
        {
            return;
        }
        
        // Calculate current scale factor
        float currentScaleFactor = transform.localScale.x / _originalScale.x;
        
        // Apply new scale factor
        ResizeToScale(currentScaleFactor - amount);
    }

    /// <summary>
    /// Reset the object to its original scale
    /// </summary>
    public void ResetToOriginalScale()
    {
        if (!IsResizable)
        {
            return;
        }
        
        transform.localScale = _originalScale;
        
        if (_rigidbody != null)
        {
            _rigidbody.mass = _originalMass;
        }
        
        _targetReached = false;
        
        // Fire resize end event
        OnResizeEnd.Invoke();
    }

    /// <summary>
    /// Check if the object is at or near the target scale
    /// </summary>
    public bool IsAtTargetScale()
    {
        float currentScaleFactor = transform.localScale.magnitude / _originalScale.magnitude;
        return Mathf.Abs(currentScaleFactor - TargetScale) <= ScaleTolerance;
    }
}
