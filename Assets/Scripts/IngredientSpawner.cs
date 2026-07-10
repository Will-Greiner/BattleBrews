using UnityEngine;

public class IngredientSpawner : MonoBehaviour, IHandInteractable
{
    [SerializeField] public IngredientData ingredientToSpawn;
    [SerializeField] GameObject promptUI;
    [SerializeField] ObjectHighlight highlight;
    
    public void Interact(HandLogic hand)
    {
        if (hand.isHolding)
            return;

        hand.HoldObject(ingredientToSpawn.prefab);
    }

    public string GetPrompt(HandLogic hand)
    {
        if (hand == null || !hand.isHolding)
            return ingredientToSpawn.name;

        return "";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<HandLogic>() != null)
        {
            if (promptUI)
                promptUI.SetActive(true);

            if (highlight)
                highlight.ShowHighlight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<HandLogic>() != null)
        {
            if (promptUI)
                promptUI.SetActive(false);

            if (highlight)
                highlight.HideHighlight();
        }
    }
}
