using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public enum Outcome {Best, Mid, Worst};
public enum GameState {MainMenu, RoundStarting, RoundActive, RoundResolving, GameOver}

public class OutcomeManager : MonoBehaviour
{
    public static OutcomeManager Instance {get; private set;}

    [Header("Game")]
    [SerializeField] BossScenario[] allScenarios;
    [SerializeField] int startingLives = 3;
    [SerializeField] float roundResultDelay = 2f;

    [Header("UI")]
    [SerializeField] TMP_Text roundText;
    [SerializeField] GameObject gameOverScreen;

    private readonly List<BossScenario> possibleScenarios = new();

    private BossScenario currentScenario;
    private PotionData requestedPotion;
    private int currentRound = 1;
    private int lives;

    private GameState state = GameState.MainMenu;

    public BossScenario CurrentScenario => currentScenario;
    public PotionData RequestedPotion => requestedPotion;
    public int CurrentRound => currentRound;
    public int Lives => lives;
    public GameState State => state;

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
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        state = GameState.MainMenu;

        currentRound = 0;
        lives = startingLives;
        currentScenario = null;
        requestedPotion = null;

        TimeManager.Instance.HideClock();
        CharacterManager.Instance.ClearCharacter();
        PotionRequestUI.Instance.HidePotionRequest();

        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
    }

    public void StartGame()
    {
        if (state != GameState.MainMenu && state != GameState.GameOver)
            return;

        currentRound = 1;
        lives = startingLives;

        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        TimeManager.Instance.ShowClock();

        StartRound();
    }

    private void StartRound()
    {
        state = GameState.RoundStarting;

        UpdateUI();

        SelectScenario();
        SelectRequestedPotion();

        CharacterManager.Instance.GenerateCharacter();

        PotionRequestUI.Instance.ShowPotionRequest(currentScenario.bossName, requestedPotion.potionName, requestedPotion.icon);
    
        TimeManager.Instance.StartTimer(currentRound);

        state = GameState.RoundActive;
    }

    public void SubmitPotion(PotionData potion)
    {
        if (state != GameState.RoundActive)
            return;

        Outcome outcome = EvaluateOutcome(potion);
        ResolveRound(outcome, potion);
    }

    public void TimeExpired()
    {
        if (state != GameState.RoundActive)
            return;

        state = GameState.RoundResolving;

        TimeManager.Instance.PauseTimer();

        lives--;

        UpdateUI();

        RoundReportUI.Instance.ShowTimeoutReport(currentScenario.bossName, requestedPotion.potionName);
    }

    private void ResolveRound(Outcome outcome, PotionData givenPotion = null)
    {
        if (state != GameState.RoundActive)
            return;

        state = GameState.RoundResolving;

        TimeManager.Instance.PauseTimer();

        if (outcome == Outcome.Worst)
            lives--;

        UpdateUI();

        string givenPotionName = givenPotion != null ? givenPotion.potionName : "None";

        RoundReportUI.Instance.ShowReport(outcome, currentScenario.bossName, requestedPotion.potionName, givenPotionName);
    }

    private void EndGame()
    {
        state = GameState.GameOver;

        TimeManager.Instance.StopTimer();
        TimeManager.Instance.HideClock();

        CharacterManager.Instance.ClearCharacter();
        PotionRequestUI.Instance.HidePotionRequest();

        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);
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

    private void UpdateUI()
    {
        if (roundText != null)
            roundText.text = "Round: " + currentRound;
    }

    public void ContinueAfterReport()
    {
        if (state != GameState.RoundResolving)
            return;

        if (lives <= 0)
        {
            EndGame();
            return;
        }

        currentRound++;
        StartRound();
    }
}
