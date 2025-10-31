using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected Vector2 direction;
    public float speed;
    protected int damage;
    
    [Header("Hit")]
    public float hitRadius = 0.08f; 
    public bool destroyOnHit = false;
    
    private static readonly List<EnemyMovement> _candidates = new(32);
    
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
        
        float sweep = Mathf.Max(hitRadius, 0.05f);
        var em = EnemyManager.instance;
        if (em != null)
        {
            _candidates.Clear();
            em.QueryEnemiesAlongSegment(curPos, nextPos, sweep + em.GetEnemyHitRadius(null), _candidates);
            
            for (int i = 0; i < _candidates.Count; i++)
            {
                var enemy = _candidates[i];
                if (!enemy) continue;

                Vector2 c = enemy.transform ? (Vector2)enemy.transform.position : Vector2.positiveInfinity;
                float R = (em != null ? em.GetEnemyHitRadius(enemy) : 0.3f) + hitRadius;

                if (IntersectsSegmentCircle(curPos, nextPos, c, R))
                {
                    enemy.Stats?.TakeDamage(damage);

                    if (destroyOnHit)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }
        
        transform.position = nextPos;
        
        if (sprite != null)
        {
            sprite.flipY = direction.sqrMagnitude >= 0.01f;
        }
    }

    private static bool IntersectsSegmentCircle(Vector2 A, Vector2 B, Vector2 C, float R)
    {
        Vector2 AB = B - A;
        float ab2 = Vector2.Dot(AB, AB);
        if (ab2 < 1e-12f) return (C - A).sqrMagnitude <= R * R;

        float t = Mathf.Clamp01(Vector2.Dot(C - A, AB) / ab2);
        Vector2 P = A + t * AB; 
        return (C - P).sqrMagnitude <= R * R;
    }
    
    private void OnDestroy()
    {
        if (ProjectileManager.instance != null)
        {
            ProjectileManager.instance.UnregisterProjectile(this);
        }
    }
    
}
