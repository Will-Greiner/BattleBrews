using UnityEngine;
using System.Collections.Generic;

public class Analyzer : MonoBehaviour, IHandInteractable
{
    public static Analyzer Instance {get; private set;}

    [SerializeField] RecipeDatabase allRecipes;

    [Header("Input")]
    [SerializeField] int maxIngredients = 3;
    [SerializeField] Transform[] inputDisplayTransforms;

    [Header("Potion Results UI")]
    [SerializeField] Transform potionEntryTransform;
    [SerializeField] GameObject potionEntryPrefab;

    private List<IngredientData> providedIngredients = new List<IngredientData>();
    private List<GameObject> spawnedInputs = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);
    }

    public void Interact(HandLogic hand)
    {
        ClearAnalyzer();
    }

    private void OnTriggerEnter(Collider other)
    {
        Ingredient inputIngredient = other.GetComponent<Ingredient>();

        if (inputIngredient == null)
            return;

        if (providedIngredients.Count >= maxIngredients)
            return;

        AddIngredient(inputIngredient.ingredient);
        
        Destroy(other.gameObject);
    }

    private void AddIngredient(IngredientData ingredient)
    {
        providedIngredients.Add(ingredient);

        SpawnInputDisplay(ingredient);
        RefreshPotionResults();
    }

    private void SpawnInputDisplay(IngredientData ingredient)
    {
        int slotIndex = providedIngredients.Count - 1;

        if (slotIndex < 0 || slotIndex >= inputDisplayTransforms.Length)
            return;

        GameObject displayObject = Instantiate(
            ingredient.prefab,
            inputDisplayTransforms[slotIndex].position,
            inputDisplayTransforms[slotIndex].rotation,
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

        spawnedInputs.Add(displayObject);
    }

    private void RefreshPotionResults()
    {
        foreach (Transform child in potionEntryTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (PotionData potion in GetMatchingRecipes(providedIngredients))
        {
            GameObject potionEntry =
                Instantiate(potionEntryPrefab, potionEntryTransform);

            potionEntry
                .GetComponent<AnalyzerPotionEntry>()
                .Setup(potion);
        }
    }

    private void ClearAnalyzer()
    {
        providedIngredients.Clear();

        foreach (GameObject display in spawnedInputs)
        {
            if (display != null)
                Destroy(display);
        }

        spawnedInputs.Clear();

        foreach (Transform child in potionEntryTransform)
        {
            Destroy(child.gameObject);
        }
    }

    public List<PotionData> GetMatchingRecipes(List<IngredientData> inputIngredients)
    {
        List<PotionData> matchingRecipes = new List<PotionData>();

        foreach (PotionData potion in allRecipes.allRecipes)
        {
            if (DoRecipesMatch(inputIngredients, potion.requiredIngredients))
            {
                matchingRecipes.Add(potion);
            }
        }

        return matchingRecipes;
    }

    private bool DoRecipesMatch(List<IngredientData> inputIngredients, List<IngredientRequirement> requiredIngredients)
    {
        foreach (IngredientData inputIngredient in inputIngredients)
        {
            IngredientRequirement matchingRequirement = requiredIngredients.Find(requirement => requirement.ingredient == inputIngredient);

            if (matchingRequirement == null)
                return false;

            int inputAmount = inputIngredients.FindAll(ingredient => ingredient == inputIngredient).Count;

            if (inputAmount > matchingRequirement.quantity)
                return false;
        }

        return true;
    }


}
