using UnityEngine;

public class IngredientSpawner : MonoBehaviour, IHandInteractable
{
    [SerializeField] public IngredientData ingredientToSpawn;
    
    public void Interact(HandLogic hand)
    {
        if (hand.isHolding)
            return;

        hand.HoldObject(ingredientToSpawn.prefab);
    }
}
