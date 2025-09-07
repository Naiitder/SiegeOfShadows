using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/BoxingPunch")]
public class BoxingPunchAbility : ProjectileAbility
{
    public override void InstantiateProjectile(Transform origin, Vector2 direction, int level)
    {
        direction.Normalize();

        GameObject proj = Instantiate(projectilePrefab, origin.position, Quaternion.identity);
        var projComponent = proj.GetComponent<Projectile>();
        
        int lvlIndex = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        projComponent.Initialize(direction, levels[lvlIndex].projectileSpeed, levels[lvlIndex].damage, levels[lvlIndex].projectileLifetime);
    }
}
