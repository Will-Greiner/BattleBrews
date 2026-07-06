using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnalyzerPotionEntry : MonoBehaviour
{
    [SerializeField] Image potionIcon;
    [SerializeField] TMP_Text potionName;

    public void Setup(PotionData potion)
    {
        if (potion == null)
            return;

        if (potionIcon != null)
            potionIcon.sprite = potion.icon;

        if (potionName != null)
            potionName.text = potion.potionName;
    }
}