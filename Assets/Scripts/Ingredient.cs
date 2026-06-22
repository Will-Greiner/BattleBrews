using UnityEngine;
using System;

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

[System.Serializable]
public class IngredientRequirement
{
    public IngredientData ingredient;
    public int quantity = 1;
}
