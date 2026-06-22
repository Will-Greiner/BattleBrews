using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance {get; private set;}

    [SerializeField] GameObject[] hatObjects;
    [SerializeField] GameObject[] hairObjects;
    [SerializeField] GameObject[] facialHairObjects;
    [SerializeField] GameObject[] armorObjects;

    [SerializeField] Transform[] spawnTransforms;
    [SerializeField] Transform spawnParent;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);
    }

    private void Start()
    {
        GenerateCharacter();
    }

    public void GenerateCharacter()
    {
        ClearCharacter();

        // Combine arrays into another array
        GameObject[][] allArrays = new GameObject[][] {hatObjects, hairObjects, facialHairObjects, armorObjects};
        
        // Cycle through each array and select a random object
        // for (int i = 0; i < allArrays.Length; i++)
        // {
        //     int randomIndex = Random.Range(0, allArrays[i].Length);

        //     Instantiate(allArrays[i][randomIndex], spawnTransforms[i].position, Quaternion.identity, spawnParent);
        // }
    }

    public void ClearCharacter()
    {
        // Remove character from scene
        foreach (Transform child in spawnParent)
        {
            Destroy(child.gameObject);
        }
    }

}
