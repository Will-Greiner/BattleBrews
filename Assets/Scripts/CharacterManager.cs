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
    [SerializeField] Texture2D[] eyeImages;
    [SerializeField] Texture2D[] pupilImages;
    [SerializeField] Texture2D[] mouthImages;
    [SerializeField] Material eyeMaterial;
    [SerializeField] Material mouthMaterial;

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

        int randomEyeIndex = Random.Range(0, eyeImages.Length);
        int randomPupilIndex = Random.Range(0, pupilImages.Length);
        int randomMouthIndex = Random.Range(0, mouthImages.Length);

        eyeMaterial.SetTexture("_Eye", eyeImages[randomEyeIndex]);
        eyeMaterial.SetTexture("_Pupil", pupilImages[randomPupilIndex]);
        mouthMaterial.SetTexture("_BaseMap", mouthImages[randomMouthIndex]);
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

        eyeMaterial.SetTexture("_Eye", eyeImages[0]);
        eyeMaterial.SetTexture("_Pupil", pupilImages[0]);
        mouthMaterial.SetTexture("_BaseMap", mouthImages[0]);
    }

}
