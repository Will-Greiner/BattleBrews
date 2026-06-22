using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Outcome {Best, Mid, Worst};

public class OutcomeManager : MonoBehaviour
{
    public static OutcomeManager Instance {get; private set;}

    [SerializeField] BossScenario[] allScenarios;

    private List<BossScenario> possibleScenarios = new List<BossScenario>();

    private BossScenario currentScenario;
    private int currentRound = 1;
    [SerializeField] int lives = 3;

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

        InitializeRound();
        
    }

    private void Start()
    {
        TimeManager.Instance.StartTimer(currentRound);
    }

    private void InitializeRound()
    {
        Debug.Log("Round:" + currentRound);

        // Recalculate possible bosses each round
        possibleScenarios.Clear();

        foreach (BossScenario scenario in allScenarios)
        {
            // Filter out bosses based on a range --- 0 is infinity
            if (currentRound >= scenario.beginningRound && (scenario.endRound == 0 || currentRound <= scenario.endRound))
                possibleScenarios.Add(scenario);
        }

        if (possibleScenarios.Count > 0)
        {
            currentScenario = GetRandomScenario();
        }
    }

    public void IncrementRound()
    {
        currentRound++;
        TimeManager.Instance.StartTimer(currentRound);
        InitializeRound();
        CharacterManager.Instance.GenerateCharacter();
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

    public void DetermineFighterFate(Outcome currentOutcome)
    {
        if (currentOutcome == Outcome.Worst)
        {
            lives--;

            if (lives == 0)
            {
                Debug.Log("You Gamed Over");
            }
            else
            {
                IncrementRound();
            }
        }
        else 
        {
            IncrementRound();
        }
    }

    private BossScenario GetRandomScenario()
    {
        if (possibleScenarios == null || possibleScenarios.Count == 0)
        {
            Debug.LogError("There be no scenarios in the possible scenarios array");
            return null;
        }

        int randomIndex = Random.Range(0, possibleScenarios.Count);
        return possibleScenarios[randomIndex];
    }
}
