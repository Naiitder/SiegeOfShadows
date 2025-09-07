using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ability")]
public class Ability : ScriptableObject
{
    public string abilityName;
    public Sprite abilityIcon;
    
    public List<AbilityLevelData> levels;
    
    public float GetCooldown(int level)
    {
        int lvlIndex = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        return levels[lvlIndex].cooldown;
    }

}
