using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundReportUI : MonoBehaviour
{
    public static RoundReportUI Instance {get; private set;}

    [SerializeField] GameObject panel;

    [SerializeField] TMP_Text gradeText;
    [SerializeField] string[] goodGrades;
    [SerializeField] string[] badGrades;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text bossText;
    [SerializeField] TMP_Text outcomeResultText;
    [SerializeField] TMP_Text potionResultText;
    [SerializeField] TMP_Text requestedPotionText;
    [SerializeField] TMP_Text givenPotionText;
    [SerializeField] TMP_Text livesText;
    [SerializeField] TMP_Text commentText;
    [SerializeField] string[] goodComments;
    [SerializeField] string[] badComments;

    [SerializeField] Button continueButton;

    [SerializeField] Animator animator;

    private void Awake()
    {
        Instance = this;
        
        panel.SetActive(false);

        continueButton.onClick.AddListener(() =>
        {
            Hide();
            HandLogic.Instance.EnableInput();
            OutcomeManager.Instance.ContinueAfterReport();
        });
    }

    public void ShowReport(Outcome outcome, BossScenario boss, PotionData requestedPotion, PotionData givenPotion, int remainingLives)
    {
        animator.SetTrigger("ReportOpen");

        panel.SetActive(true);

        int randomGradeIndex = Random.Range(0,5);

        gradeText.text = outcome == Outcome.Best ? goodGrades[randomGradeIndex] : badGrades[randomGradeIndex];

        resultText.text = outcome == Outcome.Best ? "Victory!" : "Defeat!";
        bossText.text = boss.bossName;
        outcomeResultText.text = outcome == Outcome.Best ? boss.goodBossResult : boss.badBossResult;
        potionResultText.text = outcome == Outcome.Best ? givenPotion.goodResult : givenPotion.badResult;
        requestedPotionText.text = requestedPotion.potionName;
        givenPotionText.text = givenPotion.potionName;
        livesText.text = remainingLives.ToString();

        int randomCommentIndex = Random.Range(0, goodComments.Length);

        commentText.text = outcome == Outcome.Best ? goodComments[randomCommentIndex] : badComments[randomCommentIndex];

        HandLogic.Instance.DisableInput();
    }

    public void ShowTimeoutReport(string bossName, string requestedPotion)
    {
        panel.SetActive(true);

        resultText.text = "Time's Up!";
        bossText.text = bossName;
        requestedPotionText.text = requestedPotion;
        givenPotionText.text = "None";

        HandLogic.Instance.DisableInput();
    }

    public void Hide()
    {
        animator.SetTrigger("ReportClose");
        panel.SetActive(false);
    }
}
