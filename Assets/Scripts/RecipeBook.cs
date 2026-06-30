using UnityEngine;
using System.Collections;

public class RecipeBook : MonoBehaviour, IHandInteractable
{
    public static RecipeBook Instance { get; private set; }

    [SerializeField] RecipeDatabase allRecipes;
    [SerializeField] Transform recipeEntryParent;
    [SerializeField] GameObject recipeEntryPrefab;

    [Header("Open/Close")]
    [SerializeField] Transform bookTransform;
    [SerializeField] Camera viewCamera;
    [SerializeField] Vector3 viewPositionOffset = new Vector3(0f, -0.2f, 1f);
    [SerializeField] Vector3 viewRotationOffset = new Vector3(0f, 180f, 0f);
    [SerializeField] Transform closedTransform;
    [SerializeField] float moveDuration = 0.75f;
    [SerializeField] Animator animator;
    [SerializeField] GameObject bookUI;

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

        RefreshBook();

        bookUI.SetActive(false);

        bookTransform.SetPositionAndRotation(
            closedTransform.position,
            closedTransform.rotation
        );

        if (animator != null)
            animator.applyRootMotion = false;
    }

    public void RefreshBook()
    {
        foreach (Transform child in recipeEntryParent)
            Destroy(child.gameObject);

        foreach (PotionData potion in allRecipes.allRecipes)
        {
            GameObject entryObject = Instantiate(recipeEntryPrefab, recipeEntryParent);
            RecipeBookEntry entry = entryObject.GetComponent<RecipeBookEntry>();
            entry.Setup(potion);
        }
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

        bookUI.SetActive(false);

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

        bookUI.SetActive(isOpen);

        isMoving = false;
        moveRoutine = null;
    }
}