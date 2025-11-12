using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager instance;
    List<Projectile> projectiles = new List<Projectile>();
    
    public List<Projectile> Projectiles { get => projectiles; set => projectiles = value; }
    
    private void Awake()
    {
       if(instance == null) instance = this;
       else Destroy(this.gameObject);
    }

    public void RegisterProjectile(Projectile projectile)
    {
        this.projectiles.Add(projectile);
    }
    
    public void UnregisterProjectile(Projectile projectile)
    {
        projectiles.Remove(projectile);
    }


    private void Update()
    {
        foreach (var projectile in this.projectiles)
        {
            projectile.ProjectileMovement();
        }
    }
}
