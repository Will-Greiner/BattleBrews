using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnalyzerPotionEntry : MonoBehaviour
{
    [SerializeField] Image potionIcon;
    [SerializeField] TMP_Text potionName;

    public void Setup(PotionData potion)
    {
        potionIcon.sprite = potion.icon;
        potionName.text = potion.name;
    }
}
