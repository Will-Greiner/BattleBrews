using UnityEngine;

public class HandLogic : MonoBehaviour
{

    [Header("Hand Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float distanceFromCamera = 5f;
    public Camera camera;    
    
    [Space]
    [Header("Camera Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float rotationBarSize = 0.2f;
    public GameObject cameraEmpty;
    Vector2 mousePercent;
    [SerializeField] private float rotationAmount = 50f;
    private float currentY = 0f;

    [Space]
    [Header("Grab Settings")]
    [SerializeField] private float grabDistance = 3f;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private LayerMask layerToIgnore;
    private Rigidbody heldObject;
    private Rigidbody targetObject;
    private GameObject newObject;
    public bool holding = false;


    void Update()
    {
        //Move hand
        targetObject = null;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(-camera.transform.forward,camera.transform.position + camera.transform.forward * distanceFromCamera);
        float enter;

        //Raycast for pickup
        if (plane.Raycast(ray, out enter))
        {
            Vector3 targetPosition = ray.GetPoint(enter);
            transform.position = Vector3.MoveTowards(transform.position,targetPosition,moveSpeed * Time.deltaTime);
        }
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~layerToIgnore))
        {
            Debug.DrawRay(ray.origin,ray.direction * 100f,Color.red);
            if (hit.collider.CompareTag("Grabbable"))
            {
                if (Input.GetMouseButtonDown(0) && holding == false)
                {
                    hit.collider.GetComponent<GrabbableArea>().spawnNew();
                    newObject = Instantiate(hit.collider.GetComponent<GrabbableArea>().thisItem.prefabtoSpawn, grabPoint, true);
                    newObject.transform.position = grabPoint.position;
                    holding = true;
                    Debug.Log("HandLogic has found its target");   
                }
            }   
        }

        if(Input.GetMouseButtonUp(0) && holding == true)
        {
            holding = false;
            Destroy(newObject, 2f);
            newObject.GetComponent<Rigidbody>().useGravity = true;
            newObject.transform.parent = null;
            // add one time momentum
        }

        //Moving camera
        mousePercent = camera.ScreenToViewportPoint(Input.mousePosition);
        if (mousePercent.x > 1f - rotationBarSize)
        {
            currentY += rotationSpeed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, -rotationAmount, rotationAmount);
            cameraEmpty.transform.localRotation = Quaternion.Euler(0f, currentY, 0f);
            Debug.Log("Mouse is on the right side of the screen!");
        }
        else if (mousePercent.x < rotationBarSize)
        {
            currentY -= rotationSpeed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, -rotationAmount, rotationAmount);
            cameraEmpty.transform.localRotation =
            Quaternion.Euler(0f, currentY, 0f);
            Debug.Log("Mouse is on the left side of the screen!");
        }
    }
}