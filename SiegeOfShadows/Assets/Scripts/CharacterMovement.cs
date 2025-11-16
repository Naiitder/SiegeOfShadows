using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterMovement : MonoBehaviour
{
    protected Animator Animator;
    protected SpriteRenderer SpriteRenderer;
    protected int IsMovingHash;

    [SerializeField] public float moveSpeed;
    
    public CharacterStats stats;
    private Coroutine flashCoroutine;
    

    protected virtual void Awake()
    {
        Animator = GetComponent<Animator>();
        IsMovingHash = Animator.StringToHash("isMoving");
        SpriteRenderer = GetComponent<SpriteRenderer>();
        stats = GetComponent<CharacterStats>();
        stats.Initialize();
        
        stats.OnTakeDamage += FlashOnDamage;
    }


    private void FlashOnDamage()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(nameof(FlashCoroutine));
    }

    private IEnumerator FlashCoroutine()
    {
        SpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.35f);
        SpriteRenderer.color = Color.white;
        flashCoroutine = null;
    }
}
