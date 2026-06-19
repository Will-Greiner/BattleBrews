using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PotionData", menuName = "Scriptable Objects/PotionData")]
public class PotionData : ScriptableObject
{
    public string potionID;
    public string potionName;
    public GameObject prefab;
    public List<IngredientData> requiredIngredients = new List<IngredientData>();
}
