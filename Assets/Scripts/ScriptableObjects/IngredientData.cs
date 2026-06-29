using System.Security;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientData", menuName = "Scriptable Objects/IngredientData")]
public class IngredientData : ScriptableObject
{
    public Sprite icon;
    public string itemName;
    public GameObject prefab;
    public GameObject cookedPrefab;
    public GameObject crushedPrefab;
    public GameObject displayPrefab;
}
