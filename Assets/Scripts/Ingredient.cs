using UnityEngine;

public class Ingredient : MonoBehaviour, IHandInteractable
{
    public IngredientData ingredient;

    public void Interact(HandLogic hand)
    {
        if (hand.isHolding)
            return;

        hand.PickUpExisitingObject(gameObject);
    }
}
