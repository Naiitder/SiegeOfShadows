using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected Vector2 direction;
    public float speed;
    public float lifetime = 3f;
    protected float rotationSpeed = 360f;
    protected int damage;
    
    SpriteRenderer sprite;
    
    public virtual void Initialize(Vector2 dir, float spd, int damage, float lifetime)
    {
        this.direction = dir;
        this.speed = spd;
        this.damage = damage;
        this.lifetime = lifetime;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        
        
        sprite = GetComponentInChildren<SpriteRenderer>();
        
        ProjectileManager.instance.RegisterProjectile(this);
        Destroy(gameObject, lifetime);
    }

    public virtual void ProjectileMovement()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        
        if (sprite != null)
        {
            if(direction.magnitude < 0.1f) sprite.flipY = false;
            else sprite.flipY = true; 
        }
    }

    private void OnDestroy()
    {
        if (ProjectileManager.instance != null)
        {
            ProjectileManager.instance.UnregisterProjectile(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<CharacterStats>().TakeDamage(damage);
        }
    }
}
