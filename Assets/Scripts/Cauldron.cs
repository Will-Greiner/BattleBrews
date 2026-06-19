using UnityEngine;
using System.Collections.Generic;

public class Cauldron : MonoBehaviour, IHandInteractable
{
    [SerializeField] List<PotionData> allPossibleRecipes = new List<PotionData>();
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
        foreach (PotionData potion in allPossibleRecipes)
        {
            if (IsRecipeFulfilled(addedIngredients, potion.requiredIngredients))
                return potion;
        }

        return null;
    }

    private bool IsRecipeFulfilled(List<IngredientData> added, List<IngredientData> required)
    {
        // If there aren't enough ingredients added, return false
        if (added.Count != required.Count)
            return false;

        // Add the added ingredient into a duplicate array to check progress
        List<IngredientData> addedCopy = new List<IngredientData>(added);

        foreach (IngredientData requiredIngredient in required)
        {
            // If there is no matching required Ingredient to remove from the duplicate array, then the recipe is incorrect
            // aka If we can't remove a matching ingredient from the array, then return false.
            if (!addedCopy.Remove(requiredIngredient))
                return false;
        }

        // If the addCopy array is empty, then the recipe is fulfilled
        return true;
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
