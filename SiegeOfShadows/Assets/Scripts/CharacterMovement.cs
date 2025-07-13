using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    protected Animator _animator;
    protected SpriteRenderer _spriteRenderer;
    protected int _isMovingHash;

    [SerializeField] protected float moveSpeed;
    [SerializeField] protected Rigidbody2D rb;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _isMovingHash = Animator.StringToHash("isMoving");
        rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected void UpdateAnimation()
    {
        if(rb.linearVelocity.x < 0) _spriteRenderer.flipX = false;
        else if (rb.linearVelocity.x > 0) _spriteRenderer.flipX = true;
            
        if(rb.linearVelocity.magnitude > 0) _animator.SetBool(_isMovingHash, true);
        else _animator.SetBool(_isMovingHash, false);
    }
}
