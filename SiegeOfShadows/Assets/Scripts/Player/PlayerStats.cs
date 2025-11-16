using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : CharacterStats
{
    [SerializeField] private int level = 1;
    [SerializeField] private int currentXp = 0;
    [SerializeField] private int maxXp = 500;
    
    [SerializeField] public Slider healthSlider;
    [SerializeField] public Slider easeHealthSlider;
    
    [SerializeField] public Slider expSlider;
    [SerializeField] public Slider easeExpSlider;
    
    public int Level {get {return level;}}
    public int CurrentXp { get { return currentXp; } }
    public int MaxXp { get { return maxXp; } }

    private void UpgradeLevel(int experience = 0)
    {
        level++;
        currentXp = experience;
        maxXp = (int)(maxXp*20/100)+maxXp;
        UpdateExpSlider(true);
        
        if (currentXp >= maxXp)
        {
            int diffXp = currentXp - maxXp;
            UpdateExpSlider();
            UpgradeLevel(diffXp);
        }
    }

    public void AddExperiencie(int experience)
    {
        currentXp += experience;
        UpdateExpSlider();
        if (currentXp >= maxXp)
        {
            int diffXp = currentXp - maxXp;
            UpgradeLevel(diffXp);
        }
    }

    private void UpdateHealthSlider(bool changeEaseValue = false)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            if (easeHealthSlider != null && changeEaseValue)
            {
                easeHealthSlider.maxValue = maxHealth;
                easeHealthSlider.value = currentHealth;   
            }
        }
    }
    
    private void UpdateExpSlider(bool changeEaseValue = false)
    {
        if (expSlider != null)
        {
            expSlider.maxValue = maxXp;
            expSlider.value = currentXp;
            if (easeExpSlider != null && changeEaseValue)
            {
                easeExpSlider.maxValue = maxXp;
                easeExpSlider.value = currentXp;   
            }
        }
    }

    
    public void UpdateEaseSliders()
    {
        if (Math.Abs(easeHealthSlider.value - healthSlider.value) > 0.1f)
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, healthSlider.value, 0.05f);        
        
        if (Math.Abs(easeExpSlider.value - expSlider.value) > 0.1f)
            easeExpSlider.value = Mathf.Lerp(easeExpSlider.value, expSlider.value, 0.05f);
    }

    public override void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg);
        UpdateHealthSlider();
    }

    public override void Initialize()
    {
        base.Initialize();
        UpdateHealthSlider(true);
        UpdateExpSlider(true);
    }
}
