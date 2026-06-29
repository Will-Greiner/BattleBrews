using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PotionRequestUI : MonoBehaviour
{
    public static PotionRequestUI Instance {get; private set;}

    [SerializeField] GameObject requestPanel;
    [SerializeField] TMP_Text requestText;
    [SerializeField] TMP_Text potionNameText;
    [SerializeField] Image potionIcon;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowPotionRequest(string bossName, string potionName, Sprite icon)
    {
        requestPanel.SetActive(true);

        requestText.text = $"{bossName}";
        potionNameText.text = potionName;
        potionIcon.sprite = icon;
        potionIcon.enabled = icon != null;
    }

    public void HidePotionRequest()
    {
        requestPanel.SetActive(false);
    }
}
