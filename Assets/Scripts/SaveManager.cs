using UnityEngine;

public static class SaveManager
{
    private const string PotionPrefix = "Potion_";

    public static void SavePotion(PotionData potion)
    {
        if (string.IsNullOrEmpty(potion.saveID))
        {
            Debug.LogError($"{potion.name} has no Save ID.");
            return;
        }

        PlayerPrefs.SetInt(
            PotionPrefix + potion.saveID,
            potion.isDiscovered ? 1 : 0
        );

        PlayerPrefs.Save();
    }

    public static void LoadPotion(PotionData potion)
    {
        if (string.IsNullOrEmpty(potion.saveID))
            return;

        potion.isDiscovered =
            PlayerPrefs.GetInt(
                PotionPrefix + potion.saveID,
                0
            ) == 1;
    }

    public static void ResetPotion(PotionData potion)
    {
        PlayerPrefs.DeleteKey(
            PotionPrefix + potion.saveID
        );

        potion.isDiscovered = false;
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteAll();
    }
}