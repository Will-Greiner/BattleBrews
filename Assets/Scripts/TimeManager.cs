using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance {get; private set;}

    [SerializeField] int startingTimeInSeconds = 120;
    [SerializeField] int secondsLostPerRound = 30;

    [SerializeField] TMP_Text timerText;
    [SerializeField] GameObject clockUI;

    private int currentAvailableTime;
    private float timer;
    private bool timerRunning;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    public void StartTimer(int roundNumber)
    {
        timerRunning = true;
        currentAvailableTime = Mathf.Max(30, startingTimeInSeconds - ((roundNumber - 1) * secondsLostPerRound));
        timer = 0f;
        timerText.text = GetTimeString();
    }

    public void StopTimer()
    {
        timerRunning = false;
        currentAvailableTime = 0;
        timer = 0f;

        timerText.text = "00:00";
    }

    public void PauseTimer()
    {
        timerRunning = false;
    }

    public void ResumeTimer()
    {
        timerRunning = true;
    }

    public string GetTimeString()
    {
        int minutes = currentAvailableTime / 60;
        int seconds = currentAvailableTime % 60;

        return $"{minutes:00}:{seconds:00}";
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        if (currentAvailableTime <= 0)
        {
            timerRunning = false;

            // Handle timer running out
            OutcomeManager.Instance.DetermineFighterFate(Outcome.Worst);

            return;
        }

        timer += Time.deltaTime;

        if (timer >= 1f)
        {
            timer -= 1f; 
            currentAvailableTime--;

            timerText.text = GetTimeString();
        }
    }

    public void ShowClock()
    {
        clockUI.SetActive(true);
    }

    public void HideClock()
    {
        clockUI.SetActive(false);
    }
}
