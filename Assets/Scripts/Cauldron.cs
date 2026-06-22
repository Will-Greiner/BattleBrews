using UnityEngine;
using System.Collections.Generic;

public class Cauldron : MonoBehaviour, IHandInteractable
{
    [SerializeField] RecipeDatabase allPossibleRecipes;
    [SerializeField] Transform potionSpawn;
    private List<IngredientData> addedIngredients = new List<IngredientData>();

    public void Interact(HandLogic hand)
    {
        // if (hand.isHolding)
        //     return;

        // hand.HoldObject(potionToSpawn.prefab);
        addedIngredients.Clear();
        Debug.Log("Emptied Cauldron");
    }

    public PotionData CheckForFulfilledRecipe()
    {
        foreach (PotionData potion in allPossibleRecipes.allRecipes)
        {
            if (IsRecipeFulfilled(addedIngredients, potion.requiredIngredients))
                return potion;
        }

        return null;
    }

    private bool IsRecipeFulfilled(List<IngredientData> added, List<IngredientRequirement> required)
    {
        int totalRequired = 0;

        foreach (IngredientRequirement requirement in required)
        {
            totalRequired += requirement.quantity;

            int addedAmount = 0;

            foreach (IngredientData ingredient in added)
            {
                if (ingredient == requirement.ingredient)
                    addedAmount++;
            }

            if (addedAmount < requirement.quantity)
            {
                return false;
            }
        }

        return added.Count == totalRequired;
    }

    public void AddIngredient(IngredientData ingredient)
    {
        addedIngredients.Add(ingredient);

        PotionData result = CheckForFulfilledRecipe();

        if (result != null)
            BrewPotion(result);
    }

    private void BrewPotion(PotionData potion)
    {
        addedIngredients.Clear();

        potion.isDiscovered = true;

        RecipeBook.Instance.RefreshBook();

        GameObject spawnedPotion = Instantiate(potion.prefab, potionSpawn.position, Quaternion.identity);
        Rigidbody rb = spawnedPotion.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Ingredient ingredient = other.GetComponent<Ingredient>();

        if (ingredient != null)
        {
            AddIngredient(ingredient.ingredient);

            Destroy(other.gameObject);
        }
    }
}
