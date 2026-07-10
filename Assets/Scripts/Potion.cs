using UnityEngine;

public class Potion : MonoBehaviour, IHandInteractable
{
    public PotionData potionData;

    public void Interact(HandLogic hand)
    {
        if (hand.isHolding)
            return;

        hand.PickUpExisitingObject(gameObject);
    }

    public string GetPrompt(HandLogic hand)
    {
        if (hand == null || !hand.isHolding)
            return potionData.potionName;

        return "";
    }
}
