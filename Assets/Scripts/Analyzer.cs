using UnityEngine;
using System.Collections.Generic;

public class Analyzer : MonoBehaviour, IHandInteractable, I_ItemReceiver
{
    public static Analyzer Instance { get; private set; }

    [SerializeField] RecipeDatabase allRecipes;

    [Header("Input")]
    [SerializeField] int maxIngredients = 3;
    [SerializeField] Transform[] inputDisplayTransforms;

    [Header("Potion Results UI")]
    [SerializeField] GameObject analyzerUI;
    [SerializeField] Transform potionEntryTransform;
    [SerializeField] GameObject potionEntryPrefab;

    [Header("Transition")]
    [SerializeField] Transform analyzerCameraTarget;

    private List<IngredientData> providedIngredients = new();
    private List<GameObject> spawnedInputs = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Interact(HandLogic hand)
    {
        CameraTransitionManager.Instance.FocusOnTarget(analyzerCameraTarget);

        if (analyzerUI != null)
            analyzerUI.SetActive(true);
    }

    private bool HasOpenSlot()
    {
        if (providedIngredients.Count < maxIngredients)
            return true;

        return providedIngredients.Exists(item => item == null);
    }

    private void AddIngredient(IngredientData ingredient)
    {
        int slotIndex = providedIngredients.FindIndex(item => item == null);

        if (slotIndex == -1)
        {
            if (providedIngredients.Count >= maxIngredients)
                return;

            providedIngredients.Add(ingredient);
            slotIndex = providedIngredients.Count - 1;
        }
        else
        {
            providedIngredients[slotIndex] = ingredient;
        }

        SpawnInputDisplay(ingredient, slotIndex);
        RefreshPotionResults();
    }

    private void SpawnInputDisplay(IngredientData ingredient, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inputDisplayTransforms.Length)
            return;

        while (spawnedInputs.Count <= slotIndex)
            spawnedInputs.Add(null);

        if (spawnedInputs[slotIndex] != null)
            Destroy(spawnedInputs[slotIndex]);

        GameObject displayObject = Instantiate(
            ingredient.displayPrefab,
            inputDisplayTransforms[slotIndex]
        );

        displayObject.transform.localPosition = Vector3.zero;
        displayObject.transform.localRotation = Quaternion.identity;

        Rigidbody rb = displayObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        spawnedInputs[slotIndex] = displayObject;
    }

    private void RefreshPotionResults()
    {
        foreach (Transform child in potionEntryTransform)
            Destroy(child.gameObject);

        foreach (PotionData potion in GetMatchingRecipes(providedIngredients))
        {
            GameObject potionEntry = Instantiate(
                potionEntryPrefab,
                potionEntryTransform
            );

            potionEntry
                .GetComponent<AnalyzerPotionEntry>()
                .Setup(potion);
        }
    }

    public void ClearAnalyzer()
    {
        providedIngredients.Clear();

        foreach (GameObject display in spawnedInputs)
        {
            if (display != null)
                Destroy(display);
        }

        spawnedInputs.Clear();

        foreach (Transform child in potionEntryTransform)
            Destroy(child.gameObject);
    }

    public void ClearSlot(int slot)
    {
        if (slot < 0 || slot >= providedIngredients.Count)
            return;

        providedIngredients[slot] = null;

        if (slot < spawnedInputs.Count && spawnedInputs[slot] != null)
        {
            Destroy(spawnedInputs[slot]);
            spawnedInputs[slot] = null;
        }

        RefreshPotionResults();
    }

    public List<PotionData> GetMatchingRecipes(List<IngredientData> inputIngredients)
    {
        List<PotionData> matchingRecipes = new();

        List<IngredientData> validIngredients =
            inputIngredients.FindAll(ingredient => ingredient != null);

        foreach (PotionData potion in allRecipes.allRecipes)
        {
            if (DoRecipesMatch(validIngredients, potion.requiredIngredients))
                matchingRecipes.Add(potion);
        }

        return matchingRecipes;
    }

    private bool DoRecipesMatch(
        List<IngredientData> inputIngredients,
        List<IngredientRequirement> requiredIngredients
    )
    {
        foreach (IngredientData inputIngredient in inputIngredients)
        {
            IngredientRequirement matchingRequirement =
                requiredIngredients.Find(
                    requirement => requirement.ingredient == inputIngredient
                );

            if (matchingRequirement == null)
                return false;

            int inputAmount =
                inputIngredients.FindAll(
                    ingredient => ingredient == inputIngredient
                ).Count;

            if (inputAmount > matchingRequirement.quantity)
                return false;
        }

        return true;
    }

    public bool CanReceiveItem(HandLogic hand)
    {
        if (hand == null || !hand.isHolding)
            return false;

        if (!HasOpenSlot())
            return false;

        GameObject heldObject = hand.GetHeldObject();

        if (heldObject == null)
            return false;

        return heldObject.GetComponentInParent<Ingredient>() != null;
    }

    public void ReceiveItem(HandLogic hand)
    {
        if (!CanReceiveItem(hand))
            return;

        GameObject heldObject = hand.GetHeldObject();
        Ingredient ingredient = heldObject.GetComponentInParent<Ingredient>();

        AddIngredient(ingredient.ingredient);
        hand.ClearHeldObject();
    }

    public string GetPrompt(HandLogic hand)
    {
        if (hand == null || !hand.isHolding)
            return "Click to View";

        if (!HasOpenSlot())
            return "Analyzer full";

        GameObject heldObject = hand.GetHeldObject();

        if (heldObject == null ||
            heldObject.GetComponentInParent<Ingredient>() == null)
            return "Needs ingredient";

        return "Release to analyze";
    }
}