using System.Security;
using UnityEngine;

[CreateAssetMenu(fileName = "GrabbableItem", menuName = "Scriptable Objects/GrabbableItem")]
public class GrabbableItem : ScriptableObject
{
    [SerializeField] public string itemID;
    [SerializeField] public GameObject prefabtoSpawn;
}
