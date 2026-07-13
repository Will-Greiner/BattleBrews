using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    [HideInInspector]
    public GameObject spawnedDisplayObject;
}

public class Cauldron : MonoBehaviour, IHandInteractable
{
    [Header("Recipes")]
    [SerializeField] RecipeDatabase allPossibleRecipes;

    [Header("Potion Results")]
    [SerializeField] PotionData grossPotion;
    [SerializeField] PotionData unstablePotion;

    [Header("Spawning")]
    [SerializeField] Transform potionSpawn;
    [SerializeField] GameObject splashPrefab;
    [SerializeField] Transform waterLocation;
    [SerializeField] GameObject goodPotionSpawn;
    [SerializeField] GameObject badPotionSpawn;
    [SerializeField] float spawnEffectLifetime = 1.5f;
    [SerializeField] float splashLifetime = 0.52f;

    [Header("Cauldron Limits")]
    [SerializeField] int maxIngredientTypes = 3;
    [SerializeField] int maxQuantityPerIngredient = 5;

    [Tooltip(
        "A gross potion can only be created after this many different " +
        "ingredient types have been added."
    )]
    [SerializeField] int uniqueIngredientsBeforeGrossPotion = 3;

    [Header("World Space 3D Display")]
    [SerializeField] GameObject uiRoot;
    [SerializeField] List<CauldronDisplaySlot> displaySlots = new();

    private readonly List<CauldronIngredientSlot> addedIngredients = new();

    [SerializeField] AudioSource source;
    [SerializeField] AudioClip splashClip;
    [SerializeField] AudioClip goodPotionCraftSound;
    [SerializeField] AudioClip badPotionCraftSound;

    public void Interact(HandLogic hand)
    {
        ClearCauldron();
    }

    public void AddIngredient(IngredientData ingredient)
    {
        if (ingredient == null)
            return;

        CauldronIngredientSlot existingSlot =
            GetSlotForIngredient(ingredient);

        if (existingSlot != null)
        {
            if (existingSlot.quantity >= maxQuantityPerIngredient)
            {
                Debug.Log(
                    $"Maximum quantity reached for {ingredient.name}.",
                    this
                );

                return;
            }

            existingSlot.quantity++;
        }
        else
        {
            if (addedIngredients.Count >= maxIngredientTypes)
            {
                Debug.Log(
                    $"The cauldron already contains " +
                    $"{maxIngredientTypes} different ingredient types.",
                    this
                );

                return;
            }

            addedIngredients.Add(new CauldronIngredientSlot
            {
                ingredient = ingredient,
                quantity = 1
            });
        }

        UpdateDisplay();
        EvaluateCauldron();
    }

    private void EvaluateCauldron()
    {
        if (allPossibleRecipes == null ||
            allPossibleRecipes.allRecipes == null)
        {
            Debug.LogError(
                "The cauldron has no RecipeDatabase assigned.",
                this
            );

            return;
        }

        PotionData exactRecipe = null;

        bool anyRecipeStillPossible = false;
        bool hasRecipeWithExactIngredientTypes = false;
        bool allExactTypeRecipesAreExceeded = true;

        foreach (PotionData potion in allPossibleRecipes.allRecipes)
        {
            if (potion == null || potion.requiredIngredients == null)
                continue;

            List<IngredientRequirement> requirements =
                potion.requiredIngredients;

            // Highest priority: exact type and quantity match.
            if (IsExactRecipe(requirements))
            {
                exactRecipe = potion;
                break;
            }

            // Can the current contents still become this recipe?
            if (CanStillCompleteRecipe(requirements))
                anyRecipeStillPossible = true;

            // Do the unique ingredient types exactly match this recipe?
            if (HasExactIngredientTypes(requirements))
            {
                hasRecipeWithExactIngredientTypes = true;

                // If even one recipe using these exact types has not
                // been exceeded, we should keep waiting.
                if (!HasExceededRecipe(requirements))
                    allExactTypeRecipesAreExceeded = false;
            }
        }

        if (exactRecipe != null)
        {
            BrewPotion(exactRecipe, true);
            return;
        }

        /*
         * Correct ingredient combination, but one or more quantities
         * are too high for every recipe using that combination.
         */
        if (hasRecipeWithExactIngredientTypes &&
            allExactTypeRecipesAreExceeded)
        {
            BrewPotion(unstablePotion, false);
            return;
        }

        /*
         * Wait when at least one recipe can still be completed from
         * the current ingredients and quantities.
         */
        if (anyRecipeStillPossible)
            return;

        /*
         * A gross potion requires enough different ingredient types,
         */
        if (addedIngredients.Count >= uniqueIngredientsBeforeGrossPotion)
        {
            BrewPotion(grossPotion, false);
        }
    }

    private bool IsExactRecipe(
        List<IngredientRequirement> requirements
    )
    {
        if (requirements == null)
            return false;

        if (addedIngredients.Count != requirements.Count)
            return false;

        foreach (IngredientRequirement requirement in requirements)
        {
            if (requirement == null || requirement.ingredient == null)
                return false;

            CauldronIngredientSlot slot =
                GetSlotForIngredient(requirement.ingredient);

            if (slot == null)
                return false;

            if (slot.quantity != requirement.quantity)
                return false;
        }

        return true;
    }

    private bool CanStillCompleteRecipe(
        List<IngredientRequirement> requirements
    )
    {
        if (requirements == null)
            return false;

        /*
         * The cauldron cannot become this recipe if it already has
         * more unique ingredient types than the recipe requires.
         */
        if (addedIngredients.Count > requirements.Count)
            return false;

        foreach (CauldronIngredientSlot slot in addedIngredients)
        {
            IngredientRequirement matchingRequirement =
                GetRequirementForIngredient(
                    requirements,
                    slot.ingredient
                );

            // Current ingredient is not used by this recipe.
            if (matchingRequirement == null)
                return false;

            // Current quantity has already exceeded this recipe.
            if (slot.quantity > matchingRequirement.quantity)
                return false;
        }

        return true;
    }

    private bool HasExactIngredientTypes(
        List<IngredientRequirement> requirements
    )
    {
        if (requirements == null)
            return false;

        if (addedIngredients.Count != requirements.Count)
            return false;

        foreach (CauldronIngredientSlot slot in addedIngredients)
        {
            IngredientRequirement matchingRequirement =
                GetRequirementForIngredient(
                    requirements,
                    slot.ingredient
                );

            if (matchingRequirement == null)
                return false;
        }

        return true;
    }

    private bool HasExceededRecipe(
        List<IngredientRequirement> requirements
    )
    {
        if (!HasExactIngredientTypes(requirements))
            return false;

        foreach (IngredientRequirement requirement in requirements)
        {
            CauldronIngredientSlot slot =
                GetSlotForIngredient(requirement.ingredient);

            if (slot != null &&
                slot.quantity > requirement.quantity)
            {
                return true;
            }
        }

        return false;
    }

    private IngredientRequirement GetRequirementForIngredient(
        List<IngredientRequirement> requirements,
        IngredientData ingredient
    )
    {
        foreach (IngredientRequirement requirement in requirements)
        {
            if (requirement != null &&
                requirement.ingredient == ingredient)
            {
                return requirement;
            }
        }

        return null;
    }

    private CauldronIngredientSlot GetSlotForIngredient(
        IngredientData ingredient
    )
    {
        foreach (CauldronIngredientSlot slot in addedIngredients)
        {
            if (slot.ingredient == ingredient)
                return slot;
        }

        return null;
    }

    private void BrewPotion(
        PotionData potion,
        bool isSuccessful
    )
    {
        if (potion == null)
        {
            Debug.LogError(
                isSuccessful
                    ? "The successful potion result is not assigned."
                    : "The failed potion result is not assigned.",
                this
            );

            return;
        }

        if (potion.prefab == null)
        {
            Debug.LogError(
                $"{potion.name} does not have a potion prefab assigned.",
                potion
            );

            return;
        }

        if (potionSpawn == null)
        {
            Debug.LogError(
                "The cauldron has no potion spawn assigned.",
                this
            );

            return;
        }

        ClearCauldron();

        if (!potion.isDiscovered)
        {
            potion.isDiscovered = true;
            SaveManager.SavePotion(potion);

            if (RecipeBook.Instance != null)
                RecipeBook.Instance.RefreshBook();
        }

        SpawnBrewEffect(isSuccessful);

        GameObject spawnedPotion = Instantiate(
            potion.prefab,
            potionSpawn.position,
            potionSpawn.rotation
        );

        Rigidbody rb = spawnedPotion.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void SpawnBrewEffect(bool isSuccessful)
    {
        GameObject effectPrefab =
            isSuccessful
                ? goodPotionSpawn
                : badPotionSpawn;

        if (effectPrefab == null || potionSpawn == null)
            return;

        GameObject effect = Instantiate(
            effectPrefab,
            potionSpawn.position,
            potionSpawn.rotation
        );

        if (isSuccessful)
            source.PlayOneShot(goodPotionCraftSound);
        else
            source.PlayOneShot(badPotionCraftSound);

        Destroy(effect, spawnEffectLifetime);
    }

    private void ClearCauldron()
    {
        addedIngredients.Clear();
        UpdateDisplay();

        Debug.Log("Emptied Cauldron", this);
    }

    private void UpdateDisplay()
    {
        foreach (CauldronDisplaySlot displaySlot in displaySlots)
            ClearDisplaySlot(displaySlot);

        int visibleSlotCount = Mathf.Min(
            addedIngredients.Count,
            displaySlots.Count
        );

        for (int i = 0; i < visibleSlotCount; i++)
        {
            SetDisplaySlot(
                displaySlots[i],
                addedIngredients[i]
            );
        }

        if (uiRoot != null)
            uiRoot.SetActive(addedIngredients.Count > 0);
    }

    private void SetDisplaySlot(
        CauldronDisplaySlot displaySlot,
        CauldronIngredientSlot ingredientSlot
    )
    {
        if (displaySlot == null || ingredientSlot == null)
            return;

        IngredientData ingredient = ingredientSlot.ingredient;

        if (ingredient != null &&
            ingredient.displayPrefab != null &&
            displaySlot.prefabParent != null)
        {
            GameObject spawned = Instantiate(
                ingredient.displayPrefab,
                displaySlot.prefabParent
            );

            spawned.transform.localPosition = Vector3.zero;
            spawned.transform.localRotation = Quaternion.identity;
            spawned.transform.localScale = Vector3.one;

            displaySlot.spawnedDisplayObject = spawned;
        }

        if (displaySlot.quantityText != null)
        {
            displaySlot.quantityText.text =
                ingredientSlot.quantity.ToString();
        }
    }

    private void ClearDisplaySlot(
        CauldronDisplaySlot displaySlot
    )
    {
        if (displaySlot == null)
            return;

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
        source.PlayOneShot(splashClip);
        GameObject SplashEffect = Instantiate(splashPrefab, entryObjectPos, Quaternion.identity, waterLocation.transform);
        Destroy(SplashEffect, 0.52f);

        if (ingredient != null)
        {
            AddIngredient(ingredient.ingredient);
            Destroy(ingredient.gameObject);
        }
    }

    private void SpawnSplash(Vector3 position)
    {
        if (splashPrefab == null)
            return;

        Transform splashParent =
            waterLocation != null
                ? waterLocation
                : transform;

        GameObject splashEffect = Instantiate(
            splashPrefab,
            position,
            Quaternion.identity,
            splashParent
        );

        Destroy(splashEffect, splashLifetime);
    }

    public string GetPrompt(HandLogic hand)
    {
        if (hand == null || hand.isHolding)
            return "Drop Ingredient";

        return "";
    }
}