using UnityEngine;
using System.Collections.Generic;

public class Analyzer : MonoBehaviour
{
    public static Analyzer Instance {get; private set;}

    [SerializeField] RecipeDatabase allRecipes;

    private List<IngredientData> providedIngredients = new List<IngredientData>();
    [SerializeField] int maxIngredients = 3;
    [SerializeField] Transform potionEntryTransform;
    [SerializeField] GameObject potionEntryPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);
    }

    // public void RefreshAnalyzer()
    // {
    //     foreach (PotionData potion in allRecipes.allRecipes)
    //     {
    //         foreach (IngredientRequirement requirement in potion.requiredIngredients)
    //         {
    //             if ()
    //         }
    //     }
    // }

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

    private void OnTriggerEnter(Collider other)
    {
        Ingredient inputIngredient = other.GetComponent<Ingredient>();

        if (inputIngredient != null)
        {
            if (providedIngredients.Count < maxIngredients)
            {
                providedIngredients.Add(inputIngredient.ingredient);
                foreach (PotionData potion in GetMatchingRecipes(providedIngredients))
                {
                    GameObject potionEntry = Instantiate(potionEntryPrefab, potionEntryTransform);
                    potionEntry.GetComponent<AnalyzerPotionEntry>().Setup(potion);
                }
            }
            Destroy(other.gameObject);
        }
    }
}
