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

    [Header("UI")]
    [SerializeField] TMP_Text roundText;
    [SerializeField] GameObject gameOverScreen;

    [Header("Outcome Particles")]
    [SerializeField] private ParticleSystem bestOutcomeParticles;
    [SerializeField] private ParticleSystem worstOutcomeParticles;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bestClip;
    [SerializeField] private AudioClip worstClip;

    [Header("Round Timing")]
    [SerializeField] private float reportCardDelay = 2f;
    [SerializeField] GameObject thanksWindow;

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
        if (thanksWindow != null)
            thanksWindow.SetActive(false);

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        state = GameState.MainMenu;

        StopAllOutcomeParticles();

        currentRound = 0;
        lives = startingLives;
        currentScenario = null;
        requestedPotion = null;

        thanksWindow.SetActive(false);

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

        thanksWindow.SetActive(false);

        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        TimeManager.Instance.ShowClock();

        StartRound();
    }

    private void StartRound()
    {
        StartCoroutine(StartRoundRoutine());
    }

    private IEnumerator StartRoundRoutine()
    {
        state = GameState.RoundStarting;

        StopAllOutcomeParticles();

        UpdateUI();

        SelectScenario();
        SelectRequestedPotion();

        CharacterManager.Instance.GenerateCharacter();

        if (CharacterManager.Instance.CurrentFighter != null)
            yield return CharacterManager.Instance.CurrentFighter.WalkIn();

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

        StartCoroutine(TimeExpiredRoutine());
    }

    private IEnumerator TimeExpiredRoutine()
    {
        state = GameState.RoundResolving;

        TimeManager.Instance.PauseTimer();

        lives--;

        UpdateUI();

        PotionRequestUI.Instance.animator.SetTrigger("HideUI");
        PotionRequestUI.Instance.HidePotionRequest();

        if (CharacterManager.Instance.CurrentFighter != null)
            yield return CharacterManager.Instance.CurrentFighter.WalkOut();

        RoundReportUI.Instance.ShowTimeoutReport(
            currentScenario.bossName,
            requestedPotion.potionName
        );
    }

    private void ResolveRound(Outcome outcome, PotionData givenPotion = null)
    {
        if (state != GameState.RoundActive)
            return;

        StartCoroutine(ResolveRoundRoutine(outcome, givenPotion));
    }

    private IEnumerator ResolveRoundRoutine(Outcome outcome, PotionData potion)
    {
        state = GameState.RoundResolving;

        TimeManager.Instance.PauseTimer();

        if (outcome == Outcome.Worst)
            lives--;

        UpdateUI();

        PotionRequestUI.Instance.animator.SetTrigger("HideUI");
        PotionRequestUI.Instance.HidePotionRequest();

        if (CharacterManager.Instance.CurrentFighter != null)
            yield return CharacterManager.Instance.CurrentFighter.WalkOut();

        // Character is now offscreen
        PlayOutcomeEffects(outcome);

        // Let the player see/hear the effect
        yield return new WaitForSeconds(reportCardDelay);


        RoundReportUI.Instance.ShowReport(
            outcome,
            currentScenario,
            requestedPotion,
            potion,
            lives
        );
    }

    private void StopAllOutcomeParticles()
    {
        StopParticleSystem(bestOutcomeParticles);
        StopParticleSystem(worstOutcomeParticles);
    }

    private void StopParticleSystem(ParticleSystem particleSystem)
    {
        if (particleSystem == null)
            return;

        particleSystem.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }

    private void PlayOutcomeEffects(Outcome outcome)
{
        switch (outcome)
        {
            case Outcome.Best:
                if (bestOutcomeParticles != null)
                    bestOutcomeParticles.Play();

                if (bestClip != null)
                    audioSource.PlayOneShot(bestClip);
                break;

            case Outcome.Worst:
                if (worstOutcomeParticles != null)
                    worstOutcomeParticles.Play();

                if (worstClip != null)
                    audioSource.PlayOneShot(worstClip);
                break;
        }
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

        if (currentRound == 3)
        {
            ShowThanksWindow();
            return;
        }

        currentRound++;
        StartRound();
    }

    private void ShowThanksWindow()
    {
        if (thanksWindow == null)
        {
            Debug.LogWarning(
                "Round three window is not assigned. Continuing normally."
            );

            currentRound++;
            StartRound();
            return;
        }

        state = GameState.RoundResolving;

        TimeManager.Instance.PauseTimer();
        HandLogic.Instance.DisableInput();

        thanksWindow.SetActive(true);
    }

    public void ContinueAfterThanksWindow()
    {
        if (thanksWindow != null)
            thanksWindow.SetActive(false);

        currentRound++;

        HandLogic.Instance.EnableInput();

        StartRound();
    }

}
