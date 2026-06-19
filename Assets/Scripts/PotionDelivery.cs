using UnityEngine;

public class PotionDelivery : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        HandLogic.Instance.EnterDeliveryZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        HandLogic.Instance.ExitDeliveryZone(this);
    }

    public void Deliver(HandLogic hand)
    {
        if (!hand.isHolding)
            return;

        Potion potion = hand.GetHeldObject().GetComponent<Potion>();

        if (potion == null)
            return;

        Debug.Log(OutcomeManager.Instance.EvaluateOutcome(potion.potionData));

        CharacterManager.Instance.GenerateCharacter();

        hand.ClearHeldObject();
    }
}
