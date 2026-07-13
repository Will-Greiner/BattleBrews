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
    [SerializeField] private Animator animator;
    
    [Space]
    [Header("Camera Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float rotationSmoothTime = 0.2f;
    [SerializeField] private float rotationBarSize = 0.2f;
    public GameObject cameraEmpty;
    Vector2 mousePercent;
    [SerializeField] private float rotationAmount = 50f;

    private float targetY = 0f;
    private float currentY = 0f;
    private float rotationVelocity = 0f;

    [Space]
    [Header("Interact Settings")]
    [SerializeField] private float interactDistance = 30f;
    public Transform grabPoint;
    [SerializeField] private LayerMask layerToIgnore;
    private PotionDelivery currentDeliveryZone;
    private I_ItemReceiver currentItemReceiver;
    public AudioSource audioSource;

    [SerializeField] private string heldItemLayerName = "HeldItem";

    private int originalHeldLayer;
    private int heldItemLayer;

    private GameObject heldObject;
    public bool isHolding => heldObject != null;

    public bool inputLocked = false;

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
        if (inputLocked)
            return;

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        MoveHand(ray);

        handVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        HandleHandDistance(ray);

        if (Input.GetMouseButtonDown(0))
        {
            if (!isHolding)
                TryInteract(ray);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isHolding)
            {
                if (currentItemReceiver != null && currentItemReceiver.CanReceiveItem(this))
                    currentItemReceiver.ReceiveItem(this);
                else if (currentDeliveryZone != null)
                    currentDeliveryZone.Deliver(this);
                else
                    DropHeldObject();
            }
        }

        MoveCamera();
    }

    private void MoveCamera()
    {
        mousePercent = camera.ScreenToViewportPoint(Input.mousePosition);

        float input = 0f;

        if (mousePercent.x > 1f - rotationBarSize)
        {
            input = Mathf.InverseLerp(
                1f - rotationBarSize,
                1f,
                mousePercent.x
            );
        }
        else if (mousePercent.x < rotationBarSize)
        {
            input = -Mathf.InverseLerp(
                rotationBarSize,
                0f,
                mousePercent.x
            );
        }

        targetY += input * rotationSpeed * Time.deltaTime;
        targetY = Mathf.Clamp(targetY, -rotationAmount, rotationAmount);

        currentY = Mathf.SmoothDamp(
            currentY,
            targetY,
            ref rotationVelocity,
            rotationSmoothTime
        );

        cameraEmpty.transform.localRotation = Quaternion.Euler(0f, currentY, 0f);

        Vector3 euler = cameraEmpty.transform.localEulerAngles;

// Convert from 0-360 to -180-180
if (euler.y > 180f)
    euler.y -= 360f;

euler.y = Mathf.Clamp(euler.y, -10f, 10f);

transform.localRotation = Quaternion.Euler(euler);

        // if (mousePercent.x > 1f - rotationBarSize)
        // {
        //     targetY += rotationSpeed * Time.deltaTime;
        // }
        // else if (mousePercent.x < rotationBarSize)
        // {
        //     targetY -= rotationSpeed * Time.deltaTime;
        // }

        // targetY = Mathf.Clamp(targetY, -rotationAmount, rotationAmount);

        // currentY = Mathf.SmoothDamp(
        //     currentY,
        //     targetY,
        //     ref rotationVelocity,
        //     rotationSmoothTime
        // );

        // cameraEmpty.transform.localRotation = Quaternion.Euler(0f, currentY, 0f);
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

        animator.SetBool("isHolding", true);
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

        animator.SetBool("isHolding", false);

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

    public void EnterItemReceiverZone(I_ItemReceiver itemReceiver)
    {
        currentItemReceiver = itemReceiver;
    }

    public void ExitItemReceiverZone(I_ItemReceiver itemReceiver)
    {
        if (currentItemReceiver == itemReceiver)
        {
            currentItemReceiver = null;
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

    public void ResetHandAndCamera()
    {
        currentDistance = defaultDistanceFromCamera;

        targetY = 0f;
        currentY = 0f;
        rotationVelocity = 0f;

        if (cameraEmpty != null)
            cameraEmpty.transform.localRotation = Quaternion.identity;

        if (grabPoint != null)
            transform.position = grabPoint.position;

        lastPosition = transform.position;
        handVelocity = Vector3.zero;

        currentDeliveryZone = null;
        currentItemReceiver = null;
    }

    public void EnableInput()
    {
        inputLocked = false;

        Cursor.visible = false;
    }

    public void DisableInput()
    {
        inputLocked = true;

        Cursor.visible = true;
    }
}