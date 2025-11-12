using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float projectileHitCooldown = 0.35f;
    protected Vector2 direction;
    public float speed;
    [SerializeField] protected int damage;
    [SerializeField] protected float radius;
    
    public int Damage { get => damage; set => damage = value; }
    public float Radius { get => radius; set => radius = value; }
    
    SpriteRenderer sprite;
    
    public virtual void Initialize(Vector2 dir, float spd, int dmg, float lifetime)
    {
        this.direction = dir;
        this.speed = spd;
        this.damage = dmg;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        
        
        sprite = GetComponentInChildren<SpriteRenderer>();
        
        ProjectileManager.instance.RegisterProjectile(this);
        Destroy(gameObject, lifetime);
    }

    public virtual void ProjectileMovement()
    {
        Vector2 curPos = transform.position;
        Vector2 nextPos = curPos + direction * speed * Time.deltaTime;
        
        transform.position = nextPos;
        
        if (sprite != null)
        {
            sprite.flipY = direction.sqrMagnitude >= 0.01f;
        }
    }
    
    private void OnDestroy()
    {
        if (ProjectileManager.instance != null)
        {
            ProjectileManager.instance.UnregisterProjectile(this);
        }
    }
    
}
