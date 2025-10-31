using System.Collections;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    protected Animator Animator;
    protected SpriteRenderer SpriteRenderer;
    protected int IsMovingHash;

    [SerializeField] public float moveSpeed;
    
    public CharacterStats Stats;

    protected virtual void Awake()
    {
        Animator = GetComponent<Animator>();
        IsMovingHash = Animator.StringToHash("isMoving");
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Stats = GetComponent<CharacterStats>();

        Stats.OnTakeDamage += FlashOnDamage;
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
