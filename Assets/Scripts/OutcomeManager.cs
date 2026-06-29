using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public enum Outcome {Best, Mid, Worst};

public class OutcomeManager : MonoBehaviour
{
    public static OutcomeManager Instance {get; private set;}

    [SerializeField] BossScenario[] allScenarios;
    [SerializeField] int lives = 3;

    private readonly List<BossScenario> possibleScenarios = new();

    private BossScenario currentScenario;
    private PotionData requestedPotion;
    private int currentRound = 1;
    [SerializeField] TMP_Text roundText;

    public BossScenario CurrentScenario => currentScenario;
    public PotionData RequestedPotion => requestedPotion;
    public int CurrentRound => currentRound;
    public int Lives => lives;

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
    }

    private void Start()
    {
        StartRound();
    }

    private void StartRound()
    {
        roundText.text = "Round: " + currentRound.ToString();

        SelectScenario();
        SelectRequestedPotion();

        CharacterManager.Instance.GenerateCharacter();

        PotionRequestUI.Instance.ShowPotionRequest(currentScenario.bossName, requestedPotion.potionName, requestedPotion.icon);
    
        TimeManager.Instance.StartTimer(currentRound);
    }

    private void SelectScenario()
    {
        possibleScenarios.Clear();

        foreach (BossScenario scenario in allScenarios)
        {
            if (currentRound >= scenario.beginningRound &&
                (scenario.endRound == 0 || currentRound <= scenario.endRound))
            {
                possibleScenarios.Add(scenario);
            }
        }

        if (possibleScenarios.Count == 0)
        {
            Debug.LogError("No valid scenarios for round " + currentRound);
            currentScenario = null;
            return;
        }

        currentScenario = possibleScenarios[Random.Range(0, possibleScenarios.Count)];
    }

    private void SelectRequestedPotion()
    {
        if (currentScenario.bestOutcomePotions == null || currentScenario.bestOutcomePotions.Length == 0)
        {
            Debug.LogError("Current scenario has no requested potion options.");
            requestedPotion = null;
            return;
        }

        requestedPotion = currentScenario.bestOutcomePotions[Random.Range(0, currentScenario.bestOutcomePotions.Length)];
    }

    public void IncrementRound()
    {
        currentRound++;
        StartRound();
    }

    public Outcome EvaluateOutcome(PotionData givenPotion)
    {
        if (currentScenario.bestOutcomePotions.Contains(givenPotion))
            return Outcome.Best;

        if (currentScenario.worstOutcomePotions.Contains(givenPotion))
            return Outcome.Worst;

        // Roll a 50/50 value to determine if the okay result becomes good or bad
        return Random.value < 0.5f ? Outcome.Best : Outcome.Worst;
    }

    public void DetermineFighterFate(Outcome currentOutcome)
    {
        if (currentOutcome == Outcome.Worst)
        {
            lives--;

            if (lives <= 0)
            {
                Debug.Log("You Gamed Over");
                return;
            }
        }

        IncrementRound();
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
