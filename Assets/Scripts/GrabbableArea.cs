using UnityEngine;

public class GrabbableArea : MonoBehaviour
{
    [SerializeField] public GrabbableItem thisItem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void spawnNew()
    {
        Debug.Log("This object is a GrabbableArea with a GrabbableItem: " + thisItem.itemID);
    }
}
