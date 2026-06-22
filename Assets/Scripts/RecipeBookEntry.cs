using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeBookEntry : MonoBehaviour
{
    [SerializeField] Image potionIcon;
    [SerializeField] TMP_Text potionName;
    [SerializeField] Transform ingredientUIParent;
    [SerializeField] GameObject ingredientUIPrefab;



    public void Setup(PotionData potion)
    {
        potionIcon.sprite = potion.icon;
        potionName.text = potion.potionName;

        foreach (IngredientRequirement requirement in potion.requiredIngredients)
        {
            GameObject ingredientUI = Instantiate(ingredientUIPrefab, ingredientUIParent);
            ingredientUI.GetComponent<IngredientBookUI>().Setup(requirement, potion.isDiscovered);
        }
    }
}
