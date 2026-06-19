using UnityEngine;

public class HandLogic : MonoBehaviour
{

    [Header("Hand Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float defaultDistanceFromCamera = 5f;
    [SerializeField] private float distanceSmoothSpeed = 5f;
    private float currentDistance;
    public Camera camera;    
    private Vector3 lastPosition;
    private Vector3 handVelocity;
    [SerializeField] private float throwMultiplier = 0.3f;
    [SerializeField] private float maxThrowSpeed = 10f;
    
    [Space]
    [Header("Camera Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float rotationBarSize = 0.2f;
    public GameObject cameraEmpty;
    Vector2 mousePercent;
    [SerializeField] private float rotationAmount = 50f;
    private float currentY = 0f;

    [Space]
    [Header("Interact Settings")]
    [SerializeField] private float interactDistance = 30f;
    public Transform grabPoint;
    [SerializeField] private LayerMask layerToIgnore;
    private PotionDelivery currentDeliveryZone;

    [SerializeField] private string heldItemLayerName = "HeldItem";

    private int originalHeldLayer;
    private int heldItemLayer;

    private GameObject heldObject;
    public bool isHolding => heldObject != null;

    public static HandLogic Instance {get; private set;}

    private void Awake()
    {
        if ( Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        currentDistance = defaultDistanceFromCamera;
        lastPosition = transform.position;

        heldItemLayer = LayerMask.NameToLayer(heldItemLayerName);
    }


    void Update()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        
        MoveHand(ray);
        
        // Momentum Calculation
        handVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        HandleHandDistance(ray);

        if (Input.GetMouseButtonDown(0))
        {
            // If not holding anything, see if we can interact/pickup something
            if (!isHolding)
            {
                TryInteract(ray);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            // If we have something and are in the delivery zone, deliver it otherwise drop it
            if (isHolding)
            {
                if (currentDeliveryZone != null)
                {
                    currentDeliveryZone.Deliver(this);
                }
                else
                {
                    DropHeldObject();
                }
            }
        }
        
        MoveCamera();
    }

    private void MoveCamera()
    {
        //Moving camera
        mousePercent = camera.ScreenToViewportPoint(Input.mousePosition);
        if (mousePercent.x > 1f - rotationBarSize)
        {
            currentY += rotationSpeed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, -rotationAmount, rotationAmount);
            cameraEmpty.transform.localRotation = Quaternion.Euler(0f, currentY, 0f);
        }
        else if (mousePercent.x < rotationBarSize)
        {
            currentY -= rotationSpeed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, -rotationAmount, rotationAmount);
            cameraEmpty.transform.localRotation =
            Quaternion.Euler(0f, currentY, 0f);
        }
    }

    private void MoveHand(Ray ray)
    {
        Plane plane = new Plane(-camera.transform.forward,camera.transform.position + camera.transform.forward * currentDistance);

        //Raycast for pickup
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 targetPosition = ray.GetPoint(enter);
            transform.position = Vector3.MoveTowards(transform.position,targetPosition,moveSpeed * Time.deltaTime);
        }
    }

    private void HandleHandDistance(Ray ray)
    {
        // Calculate distance from camera to be whatever is under the mouse cursor.
        float targetDistance = defaultDistanceFromCamera;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~layerToIgnore))
        {
            Debug.DrawRay(ray.origin,ray.direction * interactDistance,Color.red);

            HandDistanceZone zone = hit.collider.GetComponent<HandDistanceZone>();

            if (zone != null)
                targetDistance = zone.handDistance;
                    
        }

        // Smoothly update the distance
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, distanceSmoothSpeed * Time.deltaTime);

    }

    private bool TryInteract(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~layerToIgnore))
        {
            IHandInteractable interactable = hit.collider.GetComponentInParent<IHandInteractable>();

            if (interactable != null)
            {
                interactable.Interact(this);
                return true;
            }
        }

        return false;
    }

    public void HoldObject(GameObject prefab)
    {
        if (isHolding)
            return;

        heldObject = Instantiate(prefab, grabPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        // Change item to correct layer
        originalHeldLayer = heldObject.layer;
        SetLayerRecursively(heldObject, heldItemLayer);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public void DropHeldObject()
    {
        if (!isHolding)
            return;

        GameObject droppedObject = heldObject;
        heldObject = null;

        droppedObject.transform.parent = null;
        SetLayerRecursively(droppedObject, originalHeldLayer);

        Rigidbody rb = droppedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            
            // Throw it
            Vector3 throwVelocity = handVelocity * throwMultiplier;
            throwVelocity = Vector3.ClampMagnitude(throwVelocity, maxThrowSpeed);

            rb.linearVelocity = throwVelocity;
        }
    }

    public void PickUpExisitingObject(GameObject objectToPickUp)
    {
        if (isHolding)
            return;

        heldObject = objectToPickUp;

        heldObject.transform.SetParent(grabPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        originalHeldLayer = heldObject.layer;
        SetLayerRecursively(heldObject, heldItemLayer);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public GameObject GetHeldObject()
    {
        return heldObject;
    }

    public void ClearHeldObject()
    {
        if (heldObject != null)
        {
            SetLayerRecursively(heldObject, originalHeldLayer);
            Destroy(heldObject);
            heldObject = null;
        }
    }

    public void EnterDeliveryZone(PotionDelivery deliveryZone)
    {
        currentDeliveryZone = deliveryZone;
    }

    public void ExitDeliveryZone(PotionDelivery deliveryZone)
    {
        if (currentDeliveryZone == deliveryZone)
        {
            currentDeliveryZone = null;
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}