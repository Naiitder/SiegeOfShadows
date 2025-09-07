using UnityEngine;

public class PlayerStats : CharacterStats
{
    private int level = 1;
    private int currentXp = 0;
    private int maxXp = 500;

    private void UpgradeLevel(int experience = 0)
    {
        level++;
        currentXp = experience;
        maxXp = (int)(maxXp*20/100)+maxXp;
        
        if (currentXp >= maxXp)
        {
            int diffXp = currentXp - maxXp;
            UpgradeLevel(diffXp);
        }
    }

    private void AddExperiencie(int experience)
    {
        currentXp += experience;
        if (currentXp >= maxXp)
        {
            int diffXp = currentXp - maxXp;
            UpgradeLevel(diffXp);
        }
    }
}
