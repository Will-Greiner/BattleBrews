using TMPro;
using UnityEngine;

public class HeldItemInputZone : MonoBehaviour
{
    [SerializeField] MonoBehaviour receiverSource;
    [SerializeField] TMP_Text promptText;

    private I_ItemReceiver receiver;
    private bool handInside;

    private void Awake()
    {
        receiver = receiverSource as I_ItemReceiver;

        if (receiver == null)
            Debug.LogError(name + " receiverSource must implement I_ItemReceiver.");
    }

    private void Start()
    {
        HidePrompt();
    }

    private void Update()
    {
        if (!handInside || receiver == null)
            return;

        HandLogic hand = HandLogic.Instance;

        if (hand == null)
            return;

        ShowPrompt(receiver.GetPrompt(hand));
    }

    private void OnTriggerEnter(Collider other)
    {
        HandLogic hand = other.GetComponentInParent<HandLogic>();

        if (hand == null)
            return;

        handInside = true;
        hand.EnterItemReceiverZone(receiver);
    }

    private void OnTriggerExit(Collider other)
    {
        HandLogic hand = other.GetComponentInParent<HandLogic>();

        if (hand == null)
            return;

        handInside = false;
        hand.ExitItemReceiverZone(receiver);
        HidePrompt();
    }

    private void ShowPrompt(string message)
    {
        if (promptText == null)
            return;

        promptText.text = message;
        promptText.gameObject.SetActive(!string.IsNullOrEmpty(message));
    }

    private void HidePrompt()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }
}