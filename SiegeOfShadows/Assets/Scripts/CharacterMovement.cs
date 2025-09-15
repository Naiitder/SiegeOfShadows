using System.Collections;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    protected Animator Animator;
    protected SpriteRenderer SpriteRenderer;
    protected int IsMovingHash;

    [SerializeField] public float moveSpeed;
    [SerializeField] protected Rigidbody2D rb;
    
    public CharacterStats Stats;

    protected virtual void Awake()
    {
        Animator = GetComponent<Animator>();
        IsMovingHash = Animator.StringToHash("isMoving");
        rb = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Stats = GetComponent<CharacterStats>();

        Stats.OnTakeDamage += FlashOnDamage;
    }

    protected void UpdateAnimation()
    {
        if(rb.linearVelocity.x < 0) SpriteRenderer.flipX = false;
        else if (rb.linearVelocity.x > 0) SpriteRenderer.flipX = true;
            
        if(rb.linearVelocity.magnitude > 0) Animator.SetBool(IsMovingHash, true);
        else Animator.SetBool(IsMovingHash, false);
    }

    private void FlashOnDamage()
    {
        StartCoroutine(nameof(FlashCoroutine));
    }

    private IEnumerator FlashCoroutine()
    {
        SpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        SpriteRenderer.color = Color.white;
    }
}
