using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundReportUI : MonoBehaviour
{
    public static RoundReportUI Instance {get; private set;}

    [SerializeField] GameObject panel;

    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text bossText;
    [SerializeField] TMP_Text requestedPotionText;
    [SerializeField] TMP_Text givenPotionText;

    [SerializeField] Button continueButton;

    private void Awake()
    {
        Instance = this;
        
        panel.SetActive(false);

        continueButton.onClick.AddListener(() =>
        {
            Hide();
            OutcomeManager.Instance.ContinueAfterReport();
        });
    }

    public void ShowReport(Outcome outcome, string bossName, string requestedPotion, string givenPotion)
    {
        panel.SetActive(true);

        resultText.text = outcome == Outcome.Best ? "Victory!" : "Defeat!";
        bossText.text = "Opponent: " + bossName;
        requestedPotionText.text = "Requested Potion: " + requestedPotion;
        givenPotionText.text = "Given Potion: " + givenPotion;
    }

    public void ShowTimeoutReport(string bossName, string requestedPotion)
    {
        panel.SetActive(true);

        resultText.text = "Time's Up!";
        bossText.text = "Opponent: " + bossName;
        requestedPotionText.text = "Requested Potion: " + requestedPotion;
        givenPotionText.text = "Given Potion: None";
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
