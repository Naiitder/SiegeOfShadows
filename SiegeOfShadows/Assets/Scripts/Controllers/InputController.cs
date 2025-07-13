using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public static InputController instance;
    
    PlayerInputActions inputActions;
    
    public Vector2 moveInput;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(this);
        
    }

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            
            inputActions.Movement.Movement.started += OnMove;
            inputActions.Movement.Movement.canceled += OnMove;
            inputActions.Movement.Movement.performed += OnMove;
        }
        inputActions.Enable();
    }
    
    private void OnDisable()
    {
        inputActions.Disable();
    }
    
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
