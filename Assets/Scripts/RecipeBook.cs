using UnityEngine;
using System.Collections;

public class RecipeBook : MonoBehaviour, IHandInteractable
{
    public static RecipeBook Instance { get; private set; }

    [SerializeField] RecipeDatabase allRecipes;
    [SerializeField] Transform leftPageEntryParent;
    [SerializeField] Transform rightPageEntryParent;
    [SerializeField] int recipesPerPage = 3;
    [SerializeField] GameObject recipeEntryPrefab;

    [Header("Open/Close")]
    [SerializeField] Transform bookTransform;
    [SerializeField] Camera viewCamera;
    [SerializeField] Vector3 viewPositionOffset = new Vector3(0f, -0.2f, 1f);
    [SerializeField] Vector3 viewRotationOffset = new Vector3(0f, 180f, 0f);
    [SerializeField] Transform closedTransform;
    [SerializeField] float moveDuration = 0.75f;
    [SerializeField] Animator animator;
    [SerializeField] GameObject promptUI;
    [SerializeField] ObjectHighlight highlight;

    private bool isOpen;
    private bool isMoving;
    private Coroutine moveRoutine;

    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (bookTransform == null)
            bookTransform = transform;

        if (viewCamera == null)
            viewCamera = Camera.main;

        
        foreach (PotionData potion in allRecipes.allRecipes)
            SaveManager.LoadPotion(potion);

        RefreshBook();


        bookTransform.SetPositionAndRotation(
            closedTransform.position,
            closedTransform.rotation
        );

        if (animator != null)
            animator.applyRootMotion = false;
    }

    public void RefreshBook()
    {
        ClearPage(leftPageEntryParent);
        ClearPage(rightPageEntryParent);

        int totalVisibleRecipes = recipesPerPage * 2;
        int recipeCount = Mathf.Min(
            allRecipes.allRecipes.Count,
            totalVisibleRecipes
        );

        for (int i = 0; i < recipeCount; i++)
        {
            PotionData potion = allRecipes.allRecipes[i];

            Transform targetPage;
            int localIndex;

            if (i < recipesPerPage)
            {
                targetPage = leftPageEntryParent;
                localIndex = i;
            }
            else
            {
                targetPage = rightPageEntryParent;
                localIndex = i - recipesPerPage;
            }

            GameObject entryObject = Instantiate(
                recipeEntryPrefab,
                targetPage
            );

            RecipeBookEntry entry =
                entryObject.GetComponent<RecipeBookEntry>();

            if (entry != null)
                entry.Setup(potion);
        }
    }

    private void ClearPage(Transform pageParent)
    {
        if (pageParent == null)
            return;

        foreach (Transform child in pageParent)
            Destroy(child.gameObject);
    }

    public void Interact(HandLogic hand)
    {
        if (hand.isHolding || isOpen || isMoving)
            return;

        OpenBook();
    }

    private void OpenBook()
    {
        isOpen = true;

        if (HandLogic.Instance != null)
            HandLogic.Instance.inputLocked = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (animator != null)
            animator.SetTrigger("OpenBook");

        Vector3 targetPosition = viewCamera.transform.TransformPoint(viewPositionOffset);
        Quaternion targetRotation =
            viewCamera.transform.rotation * Quaternion.Euler(viewRotationOffset);

        StartMove(targetPosition, targetRotation);
    }

    public void CloseBook()
    {
        if (!isOpen || isMoving)
            return;

        isOpen = false;
        Cursor.visible = false;

        if (HandLogic.Instance != null)
            HandLogic.Instance.inputLocked = false;

        // Cursor.visible = false;

        if (animator != null)
            animator.SetTrigger("CloseBook");

        StartMove(closedTransform.position, closedTransform.rotation);
    }

    private void StartMove(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveBookRoutine(targetPosition, targetRotation));
    }

    private IEnumerator MoveBookRoutine(Vector3 targetPosition, Quaternion targetRotation)
    {
        isMoving = true;

        Vector3 startPosition = bookTransform.position;
        Quaternion startRotation = bookTransform.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / moveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            bookTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            bookTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        bookTransform.SetPositionAndRotation(targetPosition, targetRotation);

        isMoving = false;
        moveRoutine = null;
    }

    public string GetPrompt(HandLogic hand)
    {
        if (hand == null || !hand.isHolding)
            return "Lookup Recipes";

        return "";
    }

    [ContextMenu("Reset Potion Discoveries")]
    private void ResetDiscoveries()
    {
        foreach (PotionData potion in allRecipes.allRecipes)
            SaveManager.ResetPotion(potion);

        RefreshBook();
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<HandLogic>() != null)
        {
            if (promptUI)
                promptUI.SetActive(true);

            if (highlight)
                highlight.ShowHighlight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<HandLogic>() != null)
        {
            if (promptUI)
                promptUI.SetActive(false);

            if (highlight)
                highlight.HideHighlight();
        }
    }
}