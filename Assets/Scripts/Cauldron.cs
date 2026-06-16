using UnityEngine;

public class Cauldron : MonoBehaviour, IHandInteractable
{
    [SerializeField] PotionData potionToSpawn;

    public void Interact(HandLogic hand)
    {
        if (hand.isHolding)
            return;

        hand.HoldObject(potionToSpawn.prefab);
    }
}
