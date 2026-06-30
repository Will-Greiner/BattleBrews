public interface I_ItemReceiver
{
    bool CanReceiveItem(HandLogic hand);
    void ReceiveItem(HandLogic hand);
    string GetPrompt(HandLogic hand);
}
