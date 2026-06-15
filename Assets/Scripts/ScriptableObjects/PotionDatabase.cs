using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Objective System/Potion Database")]
public class PotionDatabase : ScriptableObject
{
    public List<PotionData> potions = new List<PotionData>();

    public PotionData GetRandomPotion()
    {
        if (potions == null || potions.Count == 0)
        {
            Debug.LogError("The potion database is empty");
            return null;
        }

        int randomIndex = Random.Range(0, potions.Count);
        return potions[randomIndex];
    }
}
