using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ArmorAbility")]
public class ArmorAbility : PassiveAbility
{
    public override void Initialize(int level)
    {
        int lvlIndex = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        GameController.instance.player.Stats.Armor += levels[lvlIndex].armor;
    }
}
