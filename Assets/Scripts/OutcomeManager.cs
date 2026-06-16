using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Outcome {Best, Mid, Worst};

public class OutcomeManager : MonoBehaviour
{
    public static OutcomeManager Instance {get; private set;}

    [SerializeField] BossScenario[] possibleScenarios;

    private BossScenario currentScenario;

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

        InitializeSequence();
        
    }

    private void InitializeSequence()
    {
        if (possibleScenarios != null)
        {
            currentScenario = GetRandomScenario();
        }
    }

    public Outcome EvaluateOutcome(PotionData givenPotion)
    {
        if (currentScenario.bestOutcomePotions.Contains(givenPotion))
            return Outcome.Best;

        if (currentScenario.worstOutcomePotions.Contains(givenPotion))
            return Outcome.Worst;

        // Roll a 50/50 value to determine if the okay result becomes good or bad
        bool isGood = Random.value < 0.5f;
        return isGood ? Outcome.Best : Outcome.Worst;
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
