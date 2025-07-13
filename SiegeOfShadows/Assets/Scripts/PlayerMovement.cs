using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Animator _animator;
    private int _isMovingHash;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _isMovingHash = Animator.StringToHash("IsMoving");
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        
    }
}
