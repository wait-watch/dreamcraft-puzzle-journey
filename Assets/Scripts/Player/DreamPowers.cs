using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Types of dream powers the player can use
/// </summary>
public enum DreamPowerType
{
    Resize,
    Gravity,
    Time
}

/// <summary>
/// Handles dream manipulation powers that the player can use to solve puzzles
/// </summary>
public class DreamPowers : MonoBehaviour
{
    [Header("Power Settings")]
    public float PowerDuration = 10.0f;
    public float PowerEffectRadius = 5.0f;
    public LayerMask AffectableLayers;
    
    [Header("Resize Power")]
    public float MinResizeScale = 0.5f;
    public float MaxResizeScale = 2.0f;
    public float ResizeSpeed = 1.0f;
    
    [Header("Gravity Power")]
    public float GravityModifier = -1.0f;
    public float GravityTransitionTime = 0.5f;
    
    [Header("Time Power")]
    public float TimeSlowFactor = 0.3f;
    
    [Header("Visual Effects")]
    public GameObject ResizePowerVFX;
    public GameObject GravityPowerVFX;
    public GameObject TimePowerVFX;
    public ParticleSystem PowerActivationParticles;
    
    [Header("Audio")]
    public AudioClip ResizePowerSound;
    public AudioClip GravityPowerSound;
    public AudioClip TimePowerSound;
    
    // Private variables
    private DreamPowerType _activePower = DreamPowerType.Resize;
    private bool _isPowerActive = false;
    private float _powerTimeRemaining = 0f;
    private GameObject _activeVFX;
    private List<Rigidbody> _affectedObjects = new List<Rigidbody>();
    private Dictionary<Rigidbody, Vector3> _originalScales = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, float> _originalGravityScales = new Dictionary<Rigidbody, float>();

    private void Update()
    {
        // Only update if a power is active
        if (_isPowerActive)
        {
            _powerTimeRemaining -= Time.deltaTime;
            
            // End power when time runs out
            if (_powerTimeRemaining <= 0)
            {
                DeactivateCurrentPower();
            }
            else
            {
                // Continue power effect
                UpdateActivePower();
            }
        }
    }

    /// <summary>
    /// Activate a dream power
    /// </summary>
    public void ActivatePower(DreamPowerType powerType)
    {
        // Deactivate any current power
        if (_isPowerActive)
        {
            DeactivateCurrentPower();
        }
        
        // Set the new power
        _activePower = powerType;
        _isPowerActive = true;
        _powerTimeRemaining = PowerDuration;
        
        // Show visual effect
        DestroyActiveVFX();
        CreatePowerVFX(powerType);
        
        // Play power activation particles
        if (PowerActivationParticles != null)
        {
            PowerActivationParticles.Play();
        }
        
        // Play power sound
        PlayPowerSound(powerType);
        
        // Find objects that can be affected
        FindAffectableObjects();
        
        // Initialize power
        InitializePower(powerType);
        
        Debug.Log("Activated " + powerType + " power");
    }

    /// <summary>
    /// Initialize power-specific settings when activated
    /// </summary>
    private void InitializePower(DreamPowerType powerType)
    {
        switch (powerType)
        {
            case DreamPowerType.Resize:
                // Store original scales
                _originalScales.Clear();
                foreach (Rigidbody rb in _affectedObjects)
                {
                    if (rb != null)
                    {
                        _originalScales[rb] = rb.transform.localScale;
                    }
                }
                break;
                
            case DreamPowerType.Gravity:
                // Store original gravity settings
                _originalGravityScales.Clear();
                foreach (Rigidbody rb in _affectedObjects)
                {
                    if (rb != null)
                    {
                        _originalGravityScales[rb] = rb.gravityScale;
                    }
                }
                break;
                
            case DreamPowerType.Time:
                // Set time scale
                Time.timeScale = TimeSlowFactor;
                Time.fixedDeltaTime = 0.02f * TimeSlowFactor;
                break;
        }
    }

    /// <summary>
    /// Update active power effect
    /// </summary>
    private void UpdateActivePower()
    {
        switch (_activePower)
        {
            case DreamPowerType.Resize:
                UpdateResizePower();
                break;
                
            case DreamPowerType.Gravity:
                UpdateGravityPower();
                break;
                
            case DreamPowerType.Time:
                // Time power is handled by Time.timeScale
                break;
        }
    }

    /// <summary>
    /// Update the resize power effect
    /// </summary>
    private void UpdateResizePower()
    {
        // Get touch or click input for resizing objects
        bool isIncreasing = false;
        bool isDecreasing = false;
        
        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                if (touch.deltaPosition.y > 0)
                {
                    isIncreasing = true;
                }
                else if (touch.deltaPosition.y < 0)
                {
                    isDecreasing = true;
                }
            }
        }
        
        // Mouse input for testing
        if (Input.GetMouseButton(0))
        {
            if (Input.GetAxis("Mouse Y") > 0)
            {
                isIncreasing = true;
            }
            else if (Input.GetAxis("Mouse Y") < 0)
            {
                isDecreasing = true;
            }
        }
        
        // Apply resize to affected objects
        foreach (Rigidbody rb in _affectedObjects)
        {
            if (rb != null)
            {
                ResizablePuzzleObject resizable = rb.GetComponent<ResizablePuzzleObject>();
                if (resizable != null && resizable.IsResizable)
                {
                    Vector3 originalScale = _originalScales[rb];
                    Vector3 currentScale = rb.transform.localScale;
                    
                    if (isIncreasing)
                    {
                        // Increase size
                        Vector3 newScale = currentScale * (1 + ResizeSpeed * Time.deltaTime);
                        
                        // Clamp to max size
                        float scaleFactor = Mathf.Min(newScale.x / originalScale.x, 
                                                 newScale.y / originalScale.y, 
                                                 newScale.z / originalScale.z);
                        
                        if (scaleFactor <= MaxResizeScale)
                        {
                            rb.transform.localScale = newScale;
                        }
                    }
                    else if (isDecreasing)
                    {
                        // Decrease size
                        Vector3 newScale = currentScale * (1 - ResizeSpeed * Time.deltaTime);
                        
                        // Clamp to min size
                        float scaleFactor = Mathf.Max(newScale.x / originalScale.x, 
                                                 newScale.y / originalScale.y, 
                                                 newScale.z / originalScale.z);
                        
                        if (scaleFactor >= MinResizeScale)
                        {
                            rb.transform.localScale = newScale;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Update the gravity power effect
    /// </summary>
    private void UpdateGravityPower()
    {
        // Get touch or click for toggling gravity direction
        bool toggleGravity = false;
        
        // Touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                toggleGravity = true;
            }
        }
        
        // Mouse input for testing
        if (Input.GetMouseButtonDown(0))
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                toggleGravity = true;
            }
        }
        
        if (toggleGravity)
        {
            // Cast a ray to find which object to affect
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100f, AffectableLayers))
            {
                Rigidbody hitRigidbody = hit.collider.GetComponent<Rigidbody>();
                if (hitRigidbody != null)
                {
                    GravityPuzzleObject gravityObject = hitRigidbody.GetComponent<GravityPuzzleObject>();
                    if (gravityObject != null && gravityObject.IsGravityAffectable)
                    {
                        // Toggle gravity direction
                        float newGravityScale = -hitRigidbody.gravityScale;
                        hitRigidbody.gravityScale = newGravityScale;
                        
                        // Play effect at hit point
                        PlayGravityToggleEffect(hit.point);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Deactivate the current power and restore normal state
    /// </summary>
    private void DeactivateCurrentPower()
    {
        if (!_isPowerActive)
        {
            return;
        }
        
        // Power-specific cleanup
        switch (_activePower)
        {
            case DreamPowerType.Resize:
                // Objects keep their new size
                break;
                
            case DreamPowerType.Gravity:
                // Restore original gravity settings
                foreach (Rigidbody rb in _affectedObjects)
                {
                    if (rb != null && _originalGravityScales.ContainsKey(rb))
                    {
                        rb.gravityScale = _originalGravityScales[rb];
                    }
                }
                break;
                
            case DreamPowerType.Time:
                // Restore normal time scale
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = 0.02f;
                break;
        }
        
        // Destroy VFX
        DestroyActiveVFX();
        
        // Reset variables
        _isPowerActive = false;
        _powerTimeRemaining = 0f;
        _affectedObjects.Clear();
        _originalScales.Clear();
        _originalGravityScales.Clear();
        
        Debug.Log("Deactivated " + _activePower + " power");
    }

    /// <summary>
    /// Find objects that can be affected by the current power
    /// </summary>
    private void FindAffectableObjects()
    {
        _affectedObjects.Clear();
        
        // Find objects within radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, PowerEffectRadius, AffectableLayers);
        
        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Check power-specific requirements
                switch (_activePower)
                {
                    case DreamPowerType.Resize:
                        if (collider.GetComponent<ResizablePuzzleObject>() != null)
                        {
                            _affectedObjects.Add(rb);
                        }
                        break;
                        
                    case DreamPowerType.Gravity:
                        if (collider.GetComponent<GravityPuzzleObject>() != null)
                        {
                            _affectedObjects.Add(rb);
                        }
                        break;
                        
                    case DreamPowerType.Time:
                        // Time affects all objects via Time.timeScale
                        _affectedObjects.Add(rb);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Create visual effect for the active power
    /// </summary>
    private void CreatePowerVFX(DreamPowerType powerType)
    {
        GameObject vfxPrefab = null;
        
        switch (powerType)
        {
            case DreamPowerType.Resize:
                vfxPrefab = ResizePowerVFX;
                break;
                
            case DreamPowerType.Gravity:
                vfxPrefab = GravityPowerVFX;
                break;
                
            case DreamPowerType.Time:
                vfxPrefab = TimePowerVFX;
                break;
        }
        
        if (vfxPrefab != null)
        {
            _activeVFX = Instantiate(vfxPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    /// <summary>
    /// Destroy any active VFX
    /// </summary>
    private void DestroyActiveVFX()
    {
        if (_activeVFX != null)
        {
            Destroy(_activeVFX);
            _activeVFX = null;
        }
    }

    /// <summary>
    /// Play sound effect for power activation
    /// </summary>
    private void PlayPowerSound(DreamPowerType powerType)
    {
        AudioClip clip = null;
        
        switch (powerType)
        {
            case DreamPowerType.Resize:
                clip = ResizePowerSound;
                break;
                
            case DreamPowerType.Gravity:
                clip = GravityPowerSound;
                break;
                
            case DreamPowerType.Time:
                clip = TimePowerSound;
                break;
        }
        
        if (clip != null && GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlaySoundEffect(clip);
        }
    }

    /// <summary>
    /// Play visual effect when gravity is toggled on an object
    /// </summary>
    private void PlayGravityToggleEffect(Vector3 position)
    {
        if (GravityPowerVFX != null)
        {
            GameObject effect = Instantiate(GravityPowerVFX, position, Quaternion.identity);
            Destroy(effect, 2.0f);
        }
        
        if (GravityPowerSound != null && GameManager.Instance.AudioManager != null)
        {
            GameManager.Instance.AudioManager.PlaySoundEffect(GravityPowerSound);
        }
    }
}
