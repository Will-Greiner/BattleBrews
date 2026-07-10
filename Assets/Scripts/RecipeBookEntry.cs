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

        foreach (Transform child in ingredientUIParent)
            Destroy(child.gameObject);

        foreach (IngredientRequirement requirement in potion.requiredIngredients)
        {
            GameObject ingredientUI = Instantiate(
                ingredientUIPrefab,
                ingredientUIParent
            );

            IngredientBookUI ingredientBookUI =
                ingredientUI.GetComponent<IngredientBookUI>();

            if (ingredientBookUI != null)
            {
                ingredientBookUI.Setup(
                    requirement,
                    potion.isDiscovered
                );
            }
        }
    }
}
