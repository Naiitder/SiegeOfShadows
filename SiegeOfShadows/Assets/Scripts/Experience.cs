using System;
using UnityEngine;

public class Experience : MonoBehaviour
{
    private int exp = 0;
    private float expRadius = 0.25f;
    
    public int Exp { get { return exp; } set { exp = value; } }
    public float ExpRadius { get { return expRadius; } set { expRadius = value; } }


    public void OnContact(PlayerStats stats)
    {
        stats.AddExperiencie(exp);
        Destroy(this.gameObject);
    }

    public void Initialize(int experience)
    {
        this.exp = experience;
        EntityManager.instance.RegisterExperience(this);
    }

    private void OnDestroy()
    {
        EntityManager.instance.UnregisterExperience(this);
    }
}
