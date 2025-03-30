using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the player character, movement, and dream powers
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float MoveSpeed = 5.0f;
    public float RotationSpeed = 10.0f;
    public Joystick MoveJoystick;
    public float JoystickDeadzone = 0.1f;
    
    [Header("Character Components")]
    public Animator CharacterAnimator;
    public Transform ModelTransform;
    
    [Header("Interaction")]
    public float InteractionRange = 2.0f;
    public LayerMask InteractableLayers;
    public GameObject InteractionIndicator;
    
    [Header("Powers")]
    public DreamPowers DreamPowersController;
    public int PowerCharges = 3;
    public float PowerCooldown = 5.0f;
    
    // Private variables
    private Rigidbody _rigidbody;
    private Vector3 _movementDirection;
    private bool _isMoving = false;
    private InteractableObject _currentInteractable = null;
    private bool _isPowerActive = false;
    private bool _isInteracting = false;

    private void Start()
    {
        // Get required components
        _rigidbody = GetComponent<Rigidbody>();
        
        // Make sure we have a dream powers controller
        if (DreamPowersController == null)
        {
            DreamPowersController = GetComponent<DreamPowers>();
            if (DreamPowersController == null)
            {
                DreamPowersController = gameObject.AddComponent<DreamPowers>();
            }
        }
        
        // Initial UI update for power charges
        UpdateUIWithPowerCharges();
    }

    private void Update()
    {
        // Only process input if the game is in playing state
        if (GameManager.Instance.CurrentGameState != GameState.Playing)
        {
            if (CharacterAnimator != null)
            {
                CharacterAnimator.SetBool("IsMoving", false);
            }
            return;
        }
        
        // Process joystick input for movement
        ProcessMovementInput();
        
        // Check for interactable objects
        CheckForInteractables();
        
        // Update animations
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // Apply movement in fixed update for consistent physics
        ApplyMovement();
    }

    /// <summary>
    /// Process joystick input for movement
    /// </summary>
    private void ProcessMovementInput()
    {
        if (MoveJoystick != null)
        {
            // Get input from joystick
            float horizontalInput = MoveJoystick.Horizontal;
            float verticalInput = MoveJoystick.Vertical;
            
            // Apply deadzone
            if (Mathf.Abs(horizontalInput) < JoystickDeadzone)
            {
                horizontalInput = 0;
            }
            
            if (Mathf.Abs(verticalInput) < JoystickDeadzone)
            {
                verticalInput = 0;
            }
            
            // Create movement vector
            _movementDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
            _isMoving = _movementDirection.magnitude > 0.1f;
        }
        else
        {
            // Fallback to keyboard input for testing
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            
            _movementDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
            _isMoving = _movementDirection.magnitude > 0.1f;
        }
    }

    /// <summary>
    /// Apply movement to the character
    /// </summary>
    private void ApplyMovement()
    {
        if (_isMoving && !_isInteracting)
        {
            // Move the character
            Vector3 movement = _movementDirection * MoveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(_rigidbody.position + movement);
            
            // Rotate the character to face movement direction
            if (_movementDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_movementDirection);
                ModelTransform.rotation = Quaternion.Slerp(ModelTransform.rotation, targetRotation, RotationSpeed * Time.fixedDeltaTime);
            }
        }
    }

    /// <summary>
    /// Check for interactable objects near the player
    /// </summary>
    private void CheckForInteractables()
    {
        // Don't check if already interacting
        if (_isInteracting)
        {
            return;
        }
        
        // Cast a sphere to detect interactable objects
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, InteractionRange, InteractableLayers);
        
        // Find the closest interactable
        float closestDistance = float.MaxValue;
        InteractableObject closestInteractable = null;
        
        foreach (Collider collider in hitColliders)
        {
            InteractableObject interactable = collider.GetComponent<InteractableObject>();
            if (interactable != null && interactable.IsInteractable)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }
        
        // Update current interactable
        if (_currentInteractable != closestInteractable)
        {
            // Remove highlight from previous interactable
            if (_currentInteractable != null)
            {
                _currentInteractable.SetHighlighted(false);
            }
            
            _currentInteractable = closestInteractable;
            
            // Highlight new interactable
            if (_currentInteractable != null)
            {
                _currentInteractable.SetHighlighted(true);
            }
        }
        
        // Show interaction indicator
        if (InteractionIndicator != null)
        {
            InteractionIndicator.SetActive(_currentInteractable != null);
        }
        
        // Handle touch input for interaction
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Check if touch is not on UI
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                // Handle interaction
                InteractWithCurrentObject();
            }
        }
        
        // Handle mouse input for interaction (for testing)
        if (Input.GetMouseButtonDown(0))
        {
            // Check if click is not on UI
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                // Handle interaction
                InteractWithCurrentObject();
            }
        }
    }

    /// <summary>
    /// Interact with the currently highlighted object
    /// </summary>
    private void InteractWithCurrentObject()
    {
        if (_currentInteractable != null)
        {
            _isInteracting = true;
            
            // Move towards the interactable if needed
            if (Vector3.Distance(transform.position, _currentInteractable.transform.position) > InteractionRange * 0.5f)
            {
                StartCoroutine(MoveToInteractable(_currentInteractable));
            }
            else
            {
                // Interact immediately if close enough
                _currentInteractable.Interact(this);
                
                if (CharacterAnimator != null)
                {
                    CharacterAnimator.SetTrigger("Interact");
                }
                
                _isInteracting = false;
            }
        }
    }

    /// <summary>
    /// Move towards an interactable before interacting
    /// </summary>
    private IEnumerator MoveToInteractable(InteractableObject interactable)
    {
        Vector3 targetPosition = interactable.InteractionPoint != null ? 
            interactable.InteractionPoint.position : 
            interactable.transform.position;
        
        // Calculate direction to interactable
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        directionToTarget.y = 0; // Keep movement on the ground plane
        
        // Rotate to face the interactable
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        
        // Move towards the interactable
        float distance = Vector3.Distance(transform.position, targetPosition);
        while (distance > InteractionRange * 0.5f && interactable != null)
        {
            // Update distance
            distance = Vector3.Distance(transform.position, targetPosition);
            
            // Rotate towards target
            ModelTransform.rotation = Quaternion.Slerp(ModelTransform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            
            // Move towards target
            _rigidbody.MovePosition(Vector3.MoveTowards(_rigidbody.position, targetPosition, MoveSpeed * Time.deltaTime));
            
            if (CharacterAnimator != null)
            {
                CharacterAnimator.SetBool("IsMoving", true);
            }
            
            yield return null;
        }
        
        // Face the interactable
        ModelTransform.rotation = targetRotation;
        
        if (CharacterAnimator != null)
        {
            CharacterAnimator.SetBool("IsMoving", false);
            CharacterAnimator.SetTrigger("Interact");
        }
        
        // Wait for animation
        yield return new WaitForSeconds(0.5f);
        
        // Interact with the object
        if (interactable != null)
        {
            interactable.Interact(this);
        }
        
        _isInteracting = false;
    }

    /// <summary>
    /// Update character animations based on movement
    /// </summary>
    private void UpdateAnimations()
    {
        if (CharacterAnimator != null)
        {
            CharacterAnimator.SetBool("IsMoving", _isMoving && !_isInteracting);
        }
    }

    /// <summary>
    /// Activate a dream power
    /// </summary>
    public void ActivatePower(DreamPowerType powerType)
    {
        if (_isPowerActive || PowerCharges <= 0)
        {
            return;
        }
        
        _isPowerActive = true;
        PowerCharges--;
        
        // Update UI
        UpdateUIWithPowerCharges();
        
        // Activate the power
        DreamPowersController.ActivatePower(powerType);
        
        // Start cooldown
        StartCoroutine(PowerCooldownCoroutine());
    }

    /// <summary>
    /// Coroutine for power cooldown
    /// </summary>
    private IEnumerator PowerCooldownCoroutine()
    {
        yield return new WaitForSeconds(PowerCooldown);
        _isPowerActive = false;
    }

    /// <summary>
    /// Update UI with current power charges
    /// </summary>
    private void UpdateUIWithPowerCharges()
    {
        if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
        {
            GameManager.Instance.UIManager.UpdateGameUI(0, 0, PowerCharges);
        }
    }

    /// <summary>
    /// Add a power charge
    /// </summary>
    public void AddPowerCharge()
    {
        PowerCharges = Mathf.Min(PowerCharges + 1, 3);
        UpdateUIWithPowerCharges();
    }
    
    /// <summary>
    /// Check if player has any power charges
    /// </summary>
    public bool HasPowerCharges()
    {
        return PowerCharges > 0;
    }
}
