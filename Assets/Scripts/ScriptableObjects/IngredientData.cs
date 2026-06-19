using System.Security;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientData", menuName = "Scriptable Objects/IngredientData")]
public class IngredientData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public GameObject prefab;
}
