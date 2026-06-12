using UnityEngine;

public enum ObjectiveState { Locked, Active, Completed };

[CreateAssetMenu(fileName = "New Objective", menuName = "Objective System/Objective Data")]
public class ObjectiveData : ScriptableObject
{
    [Header("Identity")]
    public string objectiveID;
    public string description;

    [Header("Progress Settings")]
    public int requiredAmount = 1;
    public ObjectiveState state = ObjectiveState.Locked;

    [HideInInspector]
    public int currentAmount = 0;

    public bool IsCompleted => currentAmount >= requiredAmount;

    public void ResetProgress()
    {
        currentAmount = 0;
    }

    public void EvaluateProgress(int amount)
    {
        if (state != ObjectiveState.Active)
            return;

        // Prevent overachieving
        currentAmount = Mathf.Clamp(currentAmount + amount, 0, requiredAmount);

        // After currentAmount has been updated, check if we have now completed the objective
        if (IsCompleted)
        {
            // Pass through this objective data if we have reached the objective
            ObjectiveEvents.TriggerObjectiveCompleted(this);
        }
        else
        {
            // Pass through this objective data if we are not yet at the objective
            ObjectiveEvents.TriggerObjectiveUpdated(this);
        }
    }
}
