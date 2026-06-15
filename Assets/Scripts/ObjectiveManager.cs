using System.Collections.Generic;
using UnityEngine;

public enum Outcome {Best, Mid, Worst};

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance {get; private set;}

    [SerializeField] List<ObjectiveData> objectivesList = new List<ObjectiveData>();

    private int currentObjectiveIndex = 0;

    [SerializeField] BossScenario[] possibleScenarios;
    [SerializeField] PotionData[] availablePotions;

    private BossScenario currentScenario;
    private PotionData submittedPotion;

    private void Awake()
    {
        if ( Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // InitializeSequence();
        
    }

    private void InitializeSequence()
    {
        if (possibleScenarios != null)
        {
            currentScenario = GetRandomScenario();
        }
    }

    private Outcome EvaluateOutcome()
    {
        for (int i = 0; i <= currentScenario.bestOutcomePotions.Length; i++)
        {
            if (submittedPotion == currentScenario.bestOutcomePotions[i])
                return Outcome.Best;
        }

        for (int i = 0; i <= currentScenario.worstOutcomePotions.Length; i++)
        {
            if (submittedPotion == currentScenario.worstOutcomePotions[i])
                return Outcome.Worst;
        }

        return Outcome.Mid;
    }

    // private void OnEnable()
    // {
    //     ObjectiveEvents.OnObjectiveCompleted += HandleObjectiveCompleted;
    // }

    // private void OnDisable()
    // {
    //     ObjectiveEvents.OnObjectiveCompleted -= HandleObjectiveCompleted;
    // }

    // private void InitializeSequence()
    // {
    //     if (objectivesList.Count == 0) 
    //         return;

    //     // Start the game without save data
    //     foreach (var objective in objectivesList)
    //     {
    //         objective.ResetProgress();
    //     }

    //     // Start at 0;
    //     currentObjectiveIndex = 0;
    //     objectivesList[currentObjectiveIndex].state = ObjectiveState.Active;

    //     ObjectiveEvents.TriggerObjectiveUpdated(objectivesList[currentObjectiveIndex]);
    // }

    // public void AdvanceProgress(string id, int amount)
    // {
    //     ObjectiveData target = objectivesList.Find(objective => objective.objectiveID == id);
    //     if (target != null)
    //     {
    //         target.EvaluateProgress(amount);
    //     }
    // }

    // private void HandleObjectiveCompleted(ObjectiveData completedObjective)
    // {
    //     if (objectivesList[currentObjectiveIndex] == completedObjective)
    //         UnlockNextObjective();
    // }

    // private void UnlockNextObjective()
    // {
    //     currentObjectiveIndex++;

    //     // Check if there are any more objectives
    //     if (currentObjectiveIndex >= objectivesList.Count)
    //     {
    //         //Objectives exhausted
    //     }

    //     // Activate next objective
    //     ObjectiveData nextObjective = objectivesList[currentObjectiveIndex];
    //     nextObjective.state = ObjectiveState.Active;

    //     // Trigger Update Event
    //     ObjectiveEvents.TriggerObjectiveUpdated(nextObjective);
    // }


    private BossScenario GetRandomScenario()
    {
        if (possibleScenarios == null || possibleScenarios.Length == 0)
        {
            Debug.LogError("There be no scenarios in the possible scenarios array");
            return null;
        }

        int randomIndex = Random.Range(0, possibleScenarios.Length);
        return possibleScenarios[randomIndex];
    }
}
