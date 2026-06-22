using UnityEngine;

public class RecipeBook : MonoBehaviour, IHandInteractable
{
    public static RecipeBook Instance {get; private set;}

    [SerializeField] RecipeDatabase allRecipes;
    [SerializeField] Transform recipeEntryParent;
    [SerializeField] GameObject recipeEntryPrefab;

    [Header("Open/Close")]
    [SerializeField] Vector3 viewPositionOffset = new Vector3(0f,-0.2f,1f);
    [SerializeField] Vector3 viewRotationOffset = new Vector3(0f,180f,0f);
    [SerializeField] Transform closedTransform;
    [SerializeField] float openSpeed = 3f;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isOpen = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);

        RefreshBook();

        targetPosition = closedTransform.position;
        targetRotation = closedTransform.rotation;
    }

    public void RefreshBook()
    {
        foreach (Transform child in recipeEntryParent)
        {
            Destroy(child.gameObject);
        }

        foreach (PotionData potion in allRecipes.allRecipes)
        {
                GameObject entryObject = Instantiate(recipeEntryPrefab, recipeEntryParent);

                RecipeBookEntry entry = entryObject.GetComponent<RecipeBookEntry>();

                entry.Setup(potion);   
        }
    }

    private void OpenBook()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, openSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, openSpeed * Time.deltaTime);
    }

    public void CloseBook()
    {
        isOpen = false;
        targetPosition = closedTransform.position;
        targetRotation = closedTransform.rotation;

        if (HandLogic.Instance != null)
            HandLogic.Instance.inputLocked = false;
    }

    private void Update()
    {
        OpenBook();
    }

    public void Interact(HandLogic hand)
    {
        if (hand.isHolding)
            return;

        isOpen = true;

        targetPosition = Camera.main.transform.TransformPoint(viewPositionOffset);
        targetRotation = Camera.main.transform.rotation * Quaternion.Euler(viewRotationOffset);
        
        hand.inputLocked = true;

        if (isOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
