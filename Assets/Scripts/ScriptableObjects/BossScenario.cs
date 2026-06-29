using UnityEngine;

[CreateAssetMenu(fileName = "BossScenario", menuName = "Scriptable Objects/BossScenario")]
public class BossScenario : ScriptableObject
{
    public string bossName;
    public PotionData[] bestOutcomePotions;
    public PotionData[] worstOutcomePotions;
    public int beginningRound;
    public int endRound;
}
