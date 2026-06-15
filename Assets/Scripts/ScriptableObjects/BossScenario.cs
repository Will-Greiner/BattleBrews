using UnityEngine;

[CreateAssetMenu(fileName = "BossScenario", menuName = "Scriptable Objects/BossScenario")]
public class BossScenario : ScriptableObject
{
    public string bossID;
    public PotionData[] bestOutcomePotions;
    public PotionData[] worstOutcomePotions;
}
