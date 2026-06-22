using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientBookUI : MonoBehaviour
{
    [SerializeField] Image ingredientIcon;
    [SerializeField] TMP_Text quantity;
    [SerializeField] Sprite unknownIcon;

    public void Setup(IngredientRequirement requirement, bool recipeDiscovered)
    {
        quantity.text = requirement.quantity.ToString();

        if (recipeDiscovered)
        {
            ingredientIcon.sprite = requirement.ingredient.icon;
        }
        else
        {
            ingredientIcon.sprite = unknownIcon;
        }
    }
}
