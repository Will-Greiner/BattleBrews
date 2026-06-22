using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PotionData", menuName = "Scriptable Objects/PotionData")]
public class PotionData : ScriptableObject
{
    public Sprite icon;
    public string potionName;
    public GameObject prefab;
    public List<IngredientRequirement> requiredIngredients;
    public bool isDiscovered;
}
