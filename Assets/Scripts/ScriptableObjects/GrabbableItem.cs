using System.Security;
using UnityEngine;

[CreateAssetMenu(fileName = "GrabbableItem", menuName = "Scriptable Objects/GrabbableItem")]
public class GrabbableItem : ScriptableObject
{
    public string itemID;
    public string itemName;
    public GameObject prefabtoSpawn;
}
