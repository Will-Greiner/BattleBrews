using UnityEngine;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class CauldronIngredientSlot
{
    public IngredientData ingredient;
    public int quantity;
}

[System.Serializable]
public class CauldronDisplaySlot
{
    public Transform prefabParent;
    public TMP_Text quantityText;

    [HideInInspector] public GameObject spawnedDisplayObject;
}

public class Cauldron : MonoBehaviour, IHandInteractable
{
    [Header("Recipes")]
    [SerializeField] RecipeDatabase allPossibleRecipes;

    [Header("Spawning")]
    [SerializeField] Transform potionSpawn;
    [SerializeField] GameObject splashPrefab;
    [SerializeField] Transform waterLocation;
    [SerializeField] GameObject goodPotionSpawn;
    [SerializeField] GameObject badPotionSpawn;

    [Header("Cauldron Limits")]
    [SerializeField] int maxIngredientTypes = 3;
    [SerializeField] int maxQuantityPerIngredient = 5;

    [Header("World Space 3D Display")]
    [SerializeField] GameObject uiRoot;
    [SerializeField] List<CauldronDisplaySlot> displaySlots = new();

    private List<CauldronIngredientSlot> addedIngredients = new();

    public void Interact(HandLogic hand)
    {
        ClearCauldron();
    }

    private void ClearCauldron()
    {
        addedIngredients.Clear();
        UpdateDisplay();
        Debug.Log("Emptied Cauldron");
    }

    public void AddIngredient(IngredientData ingredient)
    {
        if (ingredient == null)
            return;

        CauldronIngredientSlot existingSlot = GetSlotForIngredient(ingredient);

        if (existingSlot != null)
        {
            if (existingSlot.quantity >= maxQuantityPerIngredient)
            {
                Debug.Log("Max quantity reached for " + ingredient.name);
                return;
            }

            existingSlot.quantity++;
        }
        else
        {
            if (addedIngredients.Count >= maxIngredientTypes)
            {
                Debug.Log("Cauldron already has 3 ingredient types.");
                return;
            }

            addedIngredients.Add(new CauldronIngredientSlot
            {
                ingredient = ingredient,
                quantity = 1
            });
        }

        UpdateDisplay();

        PotionData result = CheckForFulfilledRecipe();

        if (result != null)
            BrewPotion(result);
    }

    private CauldronIngredientSlot GetSlotForIngredient(IngredientData ingredient)
    {
        foreach (CauldronIngredientSlot slot in addedIngredients)
        {
            if (slot.ingredient == ingredient)
                return slot;
        }

        return null;
    }

    public PotionData CheckForFulfilledRecipe()
    {
        foreach (PotionData potion in allPossibleRecipes.allRecipes)
        {
            if (IsRecipeFulfilled(potion.requiredIngredients))
                return potion;
        }

        return null;
    }

    private bool IsRecipeFulfilled(List<IngredientRequirement> required)
    {
        if (addedIngredients.Count != required.Count)
            return false;

        foreach (IngredientRequirement requirement in required)
        {
            CauldronIngredientSlot slot = GetSlotForIngredient(requirement.ingredient);

            if (slot == null)
                return false;

            if (slot.quantity != requirement.quantity)
                return false;
        }

        return true;
    }

    private void BrewPotion(PotionData potion)
    {
        ClearCauldron();

        potion.isDiscovered = true;
        RecipeBook.Instance.RefreshBook();

        GameObject effect = Instantiate(goodPotionSpawn, potionSpawn.position, Quaternion.identity);
        Destroy(effect, 1.5f);
        GameObject spawnedPotion = Instantiate(
            potion.prefab,
            potionSpawn.position,
            Quaternion.identity
        );

        Rigidbody rb = spawnedPotion.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < displaySlots.Count; i++)
        {
            ClearDisplaySlot(displaySlots[i]);
        }

        for (int i = 0; i < addedIngredients.Count && i < displaySlots.Count; i++)
        {
            SetDisplaySlot(displaySlots[i], addedIngredients[i]);
        }

        if (uiRoot != null)
            uiRoot.SetActive(addedIngredients.Count > 0);
    }

    private void SetDisplaySlot(CauldronDisplaySlot displaySlot, CauldronIngredientSlot ingredientSlot)
    {
        if (ingredientSlot.ingredient.displayPrefab != null && displaySlot.prefabParent != null)
        {
            GameObject spawned = Instantiate(
                ingredientSlot.ingredient.displayPrefab,
                displaySlot.prefabParent
            );

            spawned.transform.localPosition = Vector3.zero;
            spawned.transform.localRotation = Quaternion.identity;
            spawned.transform.localScale = Vector3.one;

            displaySlot.spawnedDisplayObject = spawned;
        }

        if (displaySlot.quantityText != null)
            displaySlot.quantityText.text = ingredientSlot.quantity.ToString();
    }

    private void ClearDisplaySlot(CauldronDisplaySlot displaySlot)
    {
        if (displaySlot.spawnedDisplayObject != null)
            Destroy(displaySlot.spawnedDisplayObject);

        displaySlot.spawnedDisplayObject = null;

        if (displaySlot.quantityText != null)
            displaySlot.quantityText.text = "";
    }

    private void OnTriggerEnter(Collider other)
    {
        Ingredient ingredient = other.GetComponentInParent<Ingredient>();

        Vector3 entryObjectPos = other.transform.position;
        GameObject SplashEffect = Instantiate(splashPrefab, entryObjectPos, Quaternion.identity, waterLocation.transform);
        Destroy(SplashEffect, 0.52f);

        if (ingredient != null)
        {
            AddIngredient(ingredient.ingredient);
            Destroy(ingredient.gameObject);
        }
    }

    public string GetPrompt(HandLogic hand)
    {
        if (hand == null || hand.isHolding)
            return "Drop Ingredient";

        return "";
    }
}