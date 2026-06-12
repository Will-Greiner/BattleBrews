using UnityEngine;
using System;

public static class ObjectiveEvents
{
    public static event Action<ObjectiveData> OnObjectiveUpdated;
    public static event Action<ObjectiveData> OnObjectiveCompleted;

    public static void TriggerObjectiveUpdated(ObjectiveData objective)
    {
        OnObjectiveUpdated?.Invoke(objective);
    }

    public static void TriggerObjectiveCompleted(ObjectiveData objective)
    {
        OnObjectiveCompleted?.Invoke(objective);
    }
}
