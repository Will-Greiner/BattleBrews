using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Scriptable Objects/RecipeDatabase")]
public class RecipeDatabase : ScriptableObject
{
    public List<PotionData> allRecipes;
}
