using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Represents an object that the player can interact with in the dream world
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public bool IsInteractable = true;
    public Transform InteractionPoint;
    public float HighlightIntensity = 0.3f;
    
    [Header("Interaction Properties")]
    public string ObjectName;
    [TextArea(2, 4)]
    public string InteractionDescription;
    
    [Header("Visual Feedback")]
    public Material DefaultMaterial;
    public Material HighlightMaterial;
    public Material InteractedMaterial;
    
    [Header("Audio")]
    public AudioClip InteractionSound;
    
    [Header("Animation")]
    public Animation InteractionAnimation;
    public string AnimationTriggerName;
    
    [Header("Events")]
    public UnityEvent<InteractableObject> OnInteraction;
    
    // Reference to parent puzzle
    [HideInInspector]
    public PuzzleBase ParentPuzzle;
    
    // State tracking
    [HideInInspector]
    public bool HasBeenInteracted = false;
    
    private Renderer _renderer;
    private Material _originalMaterial;
    private bool _isHighlighted = false;
    private Animator _animator;

    private void Start()
    {
        // Get required components
        _renderer = GetComponent<Renderer>();
        _animator = GetComponent<Animator>();
        
        // Store original material if using renderer
        if (_renderer != null && _renderer.material != null)
        {
            _originalMaterial = _renderer.material;
        }
        
        // Set default interaction point if not specified
        if (InteractionPoint == null)
        {
            InteractionPoint = transform;
        }
    }

    /// <summary>
    /// Player interacts with this object
    /// </summary>
    public virtual void Interact(PlayerController player)
    {
        if (!IsInteractable)
        {
            return;
        }
        
        // Mark as interacted
        HasBeenInteracted = true;
        
        // Play interaction animation if available
        if (InteractionAnimation != null)
        {
            InteractionAnimation.Play();
        }
        
        // Trigger animator if available
        if (_animator != null && !string.IsNullOrEmpty(AnimationTriggerName))
        {
            _animator.SetTrigger(AnimationTriggerName);
        }
        
        // Change material if available
        if (_renderer != null && InteractedMaterial != null)
        {
            _renderer.material = InteractedMaterial;
        }
        
        // Play interaction sound
        PlayInteractionSound();
        
        // Invoke the interaction event
        OnInteraction.Invoke(this);
        
        Debug.Log("Player interacted with: " + ObjectName);
    }

    /// <summary>
    /// Set whether this object is highlighted (for nearby interactable objects)
    /// </summary>
    public virtual void SetHighlighted(bool highlighted)
    {
        if (_isHighlighted == highlighted)
        {
            return;
        }
        
        _isHighlighted = highlighted;
        
        // Change material based on highlight state
        if (_renderer != null)
        {
            if (highlighted && HighlightMaterial != null)
            {
                _renderer.material = HighlightMaterial;
            }
            else if (!HasBeenInteracted)
            {
                _renderer.material = _originalMaterial;
            }
            else if (InteractedMaterial != null)
            {
                _renderer.material = InteractedMaterial;
            }
        }
        else
        {
            // For objects without renderer, we can still add a highlight effect
            Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer childRenderer in childRenderers)
            {
                if (highlighted)
                {
                    // Add emission to shader properties
                    foreach (Material mat in childRenderer.materials)
                    {
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            Color baseColor = mat.HasProperty("_Color") ? mat.color : Color.white;
                            mat.EnableKeyword("_EMISSION");
                            mat.SetColor("_EmissionColor", baseColor * HighlightIntensity);
                        }
                    }
                }
                else
                {
                    // Remove emission
                    foreach (Material mat in childRenderer.materials)
                    {
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            mat.DisableKeyword("_EMISSION");
                            mat.SetColor("_EmissionColor", Color.black);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Reset this object to its initial state
    /// </summary>
    public virtual void ResetObject()
    {
        HasBeenInteracted = false;
        
        // Reset material
        if (_renderer != null && _originalMaterial != null)
        {
            _renderer.material = _originalMaterial;
        }
        
        // Reset animation
        if (_animator != null)
        {
            _animator.Rebind();
        }
    }

    /// <summary>
    /// Play interaction sound effect
    /// </summary>
    protected void PlayInteractionSound()
    {
        if (InteractionSound != null && GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlaySoundEffect(InteractionSound);
        }
    }
}
