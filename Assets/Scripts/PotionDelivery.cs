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

        if (OutcomeManager.Instance.State != GameState.RoundActive)
            return;

        Potion potion = hand.GetHeldObject().GetComponent<Potion>();

        if (potion == null)
            return;

        OutcomeManager.Instance.SubmitPotion(potion.potionData);

        hand.ClearHeldObject();
    }
}
