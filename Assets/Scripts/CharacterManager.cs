using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance {get; private set;}

    [SerializeField] GameObject[] hatObjects;
    [SerializeField] GameObject[] hairObjects;
    [SerializeField] GameObject[] facialHairObjects;
    [SerializeField] GameObject[] armorObjects;
    [SerializeField] GameObject[] backObjects;

    [SerializeField] Transform[] spawnTransforms;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    public FighterAnimationController CurrentFighter {get; private set;}

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);
    }

    public void GenerateCharacter()
    {
        ClearCharacter();

        // Combine arrays into another array
        GameObject[][] allArrays = new GameObject[][] {hatObjects, hairObjects, facialHairObjects, armorObjects, backObjects};
        
        // Cycle through each array and select a random object
        for (int i = 0; i < allArrays.Length; i++)
        {
            if (allArrays[i].Length > 0)
            {
                int randomIndex = Random.Range(0, allArrays[i].Length);

                GameObject spawned = Instantiate(allArrays[i][randomIndex], spawnTransforms[i].position, spawnTransforms[i].rotation, spawnTransforms[i]);
            
                if (CurrentFighter == null)
                    CurrentFighter = spawned.GetComponentInParent<FighterAnimationController>();
            }
        }
    }

    public void ClearCharacter()
    {
        CurrentFighter = null;

        // Remove character from scene
        for (int i = 0; i < spawnTransforms.Length; i++)
        {
            foreach (Transform child in spawnTransforms[i])
            {
                Destroy(child.gameObject);
            }
        }
    }

}
