using System.Collections;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    protected Animator Animator;
    protected SpriteRenderer SpriteRenderer;
    protected int IsMovingHash;

    [SerializeField] public float moveSpeed;
    
    public CharacterStats Stats;
    
    [SerializeField] protected Rigidbody2D rb;
    public Rigidbody2D Rb { get { return rb; } set { rb = value; } }

    protected virtual void Awake()
    {
        Animator = GetComponent<Animator>();
        IsMovingHash = Animator.StringToHash("isMoving");
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Stats = GetComponent<CharacterStats>();

        Stats.OnTakeDamage += FlashOnDamage;
        rb = GetComponent<Rigidbody2D>();
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
