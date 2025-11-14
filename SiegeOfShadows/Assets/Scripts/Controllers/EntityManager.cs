using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class EntityManager : MonoBehaviour
{
    public static EntityManager instance;
    public PlayerController player;
    
    [Header("Projectile List")]
    List<Projectile> projectiles = new List<Projectile>();
    public List<Projectile> Projectiles { get => projectiles; set => projectiles = value; }
    
    [Header("Drop List")]
    List<Experience> experienceItems = new List<Experience>();
    TransformAccessArray expTAA;
    NativeList<float> expRadius;
    
    NativeArray<float2> expPos;
    NativeQueue<PickUp> hitQueue;
    
    public NativeArray<float> ExpRadiusesArray => expRadius.AsDeferredJobArray();
    
    private void Awake()
    {
       if(instance == null) instance = this;
       else Destroy(this.gameObject);
       
       player = FindAnyObjectByType<PlayerController>();
       
       expTAA   = new TransformAccessArray(0);
       expRadius = new NativeList<float>(Allocator.Persistent);
    }

    private void Update()
    {
        foreach (var projectile in this.projectiles)
        {
            projectile.ProjectileMovement();
        }
    }

    private void LateUpdate()
    {
        HandlePickUp();
    }


    void HandlePickUp()
    {
        var exps = experienceItems;
        int n = exps?.Count ?? 0;
        if (n == 0) return;
        
        expPos = new NativeArray<float2>(n, Allocator.TempJob);
        var copyJob = new CopyPositionsJob() { positions = expPos };
        JobHandle copyHandle = copyJob.Schedule(expTAA);
        
        hitQueue = new NativeQueue<PickUp>(Allocator.TempJob);
        var pickJob = new PickupJob()
        {
            itemPos             = expPos,
            itemRadius          = ExpRadiusesArray,
            playerPos          = (float2) new float2(player.transform.position.x, player.transform.position.y),
            playerPickupRadius = player.pickupRadius,
            hits               = hitQueue.AsParallelWriter()
        };
        JobHandle pickHandle = pickJob.Schedule(n, 64, copyHandle);
        pickHandle.Complete();
        
        var toRemove = new List<int>(128);
        while (hitQueue.TryDequeue(out var h))
            toRemove.Add(h.itemIndex);

        if (toRemove.Count > 0)
        {
            toRemove.Sort();
            int write = 0;
            for (int read = 1; read < toRemove.Count; read++)
                if (toRemove[read] != toRemove[write]) toRemove[++write] = toRemove[read];
            toRemove.RemoveRange(write + 1, toRemove.Count - (write + 1));
            toRemove.Sort((a,b) => b.CompareTo(a));

            foreach (int idx in toRemove)
            {
                if (idx < 0 || idx >= experienceItems.Count) continue;

                var orb = experienceItems[idx];
                if (orb == null) continue;
                
                orb.OnContact(player.stats as PlayerStats);
            }
        }
        
        hitQueue.Dispose();
        expPos.Dispose();
    }
    
    public void RegisterProjectile(Projectile projectile)
    {
        this.projectiles.Add(projectile);
    }
    
    public void UnregisterProjectile(Projectile projectile)
    {
        projectiles.Remove(projectile);
    }

    public void RegisterExperience(Experience exp)
    {
        experienceItems.Add(exp);
        expTAA.Add(exp.transform);
        expRadius.Add(exp.ExpRadius);
    }

    public void UnregisterExperience(Experience exp)
    {
        int idx = experienceItems.IndexOf(exp);
        if (idx < 0) return;

        expTAA.RemoveAtSwapBack(idx);

        int last = expRadius.Length - 1;
        if (idx != last) expRadius[idx] = expRadius[last];
        expRadius.RemoveAt(last);

        int lastE = experienceItems.Count - 1;
        experienceItems[idx] = experienceItems[lastE];
        experienceItems.RemoveAt(lastE);
    }
    
    void OnDestroy()
    {
        if (expTAA.isCreated) expTAA.Dispose();
        if (expRadius.IsCreated) expRadius.Dispose();
    }
}
