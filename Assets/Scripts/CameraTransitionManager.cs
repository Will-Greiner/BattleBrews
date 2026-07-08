using System.Collections;
using UnityEngine;

public class CameraTransitionManager : MonoBehaviour
{
    public static CameraTransitionManager Instance { get; private set; }

    [Header("Cameras")]
    [SerializeField] Camera menuCamera;
    [SerializeField] Camera gameplayCamera;

    [Header("Fixed Camera Targets")]
    [SerializeField] Transform mainMenuTarget;
    [SerializeField] Transform gameplayTarget;

    [Header("Gameplay References")]
    [SerializeField] HandLogic handLogic;

    [Header("Settings")]
    [SerializeField] float gameTransitionDuration = 2f;
    [SerializeField] float focusTransitionDuration = 1f;

    private Coroutine transitionRoutine;

    private Vector3 focusReturnPosition;
    private Quaternion focusReturnRotation;
    private bool isInFocusView;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ShowMainMenuInstant();
    }

    // Button: Start Game
    public void StartGame()
    {
        StartTransition(StartGameRoutine());
    }

    // Button: Return To Main Menu
    public void ReturnToMainMenu()
    {
        StartTransition(ReturnToMainMenuRoutine());
    }

    // Button/Object: Focus analyzer, recipe book, etc.
    public void FocusOnTarget(Transform target)
    {
        if (target == null)
            return;

        StartTransition(FocusOnTargetRoutine(target));
    }

    // Button: Back from analyzer, recipe book, etc.
    public void ReturnFromFocus()
    {
        if (!isInFocusView)
            return;

        StartTransition(ReturnFromFocusRoutine());
    }

    private void StartTransition(IEnumerator routine)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(routine);
    }

    private IEnumerator StartGameRoutine()
    {
        LockGameplayInput(true);

        menuCamera.enabled = true;
        gameplayCamera.enabled = false;

        menuCamera.transform.SetPositionAndRotation(
            mainMenuTarget.position,
            mainMenuTarget.rotation
        );

        yield return MoveCamera(
            menuCamera.transform,
            gameplayTarget.position,
            gameplayTarget.rotation,
            gameTransitionDuration
        );

        gameplayCamera.transform.SetPositionAndRotation(
            gameplayTarget.position,
            gameplayTarget.rotation
        );

        menuCamera.enabled = false;
        gameplayCamera.enabled = true;

        OutcomeManager.Instance.StartGame();

        LockGameplayInput(false);

        transitionRoutine = null;
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        LockGameplayInput(true);

        menuCamera.enabled = true;
        gameplayCamera.enabled = false;

        menuCamera.transform.SetPositionAndRotation(
            gameplayCamera.transform.position,
            gameplayCamera.transform.rotation
        );

        yield return MoveCamera(
            menuCamera.transform,
            mainMenuTarget.position,
            mainMenuTarget.rotation,
            gameTransitionDuration
        );

        ResetGameplayCamera();

        OutcomeManager.Instance.ShowMainMenu();

        menuCamera.enabled = true;
        gameplayCamera.enabled = false;

        transitionRoutine = null;
    }

    private IEnumerator FocusOnTargetRoutine(Transform target)
    {
        LockGameplayInput(true);

        focusReturnPosition = gameplayCamera.transform.position;
        focusReturnRotation = gameplayCamera.transform.rotation;
        isInFocusView = true;

        menuCamera.enabled = true;
        gameplayCamera.enabled = false;

        menuCamera.transform.SetPositionAndRotation(
            focusReturnPosition,
            focusReturnRotation
        );

        yield return MoveCamera(
            menuCamera.transform,
            target.position,
            target.rotation,
            focusTransitionDuration
        );

        transitionRoutine = null;
    }

    private IEnumerator ReturnFromFocusRoutine()
    {
        LockGameplayInput(true);

        menuCamera.enabled = true;
        gameplayCamera.enabled = false;

        yield return MoveCamera(
            menuCamera.transform,
            focusReturnPosition,
            focusReturnRotation,
            focusTransitionDuration
        );

        gameplayCamera.transform.SetPositionAndRotation(
            focusReturnPosition,
            focusReturnRotation
        );

        menuCamera.enabled = false;
        gameplayCamera.enabled = true;

        isInFocusView = false;

        LockGameplayInput(false);

        transitionRoutine = null;
    }

    private IEnumerator MoveCamera(
        Transform cameraTransform,
        Vector3 targetPosition,
        Quaternion targetRotation,
        float duration
    )
    {
        Vector3 startPosition = cameraTransform.position;
        Quaternion startRotation = cameraTransform.rotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            cameraTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        cameraTransform.SetPositionAndRotation(targetPosition, targetRotation);
    }

    private void ShowMainMenuInstant()
    {
        LockGameplayInput(true);

        menuCamera.enabled = true;
        gameplayCamera.enabled = false;

        menuCamera.transform.SetPositionAndRotation(
            mainMenuTarget.position,
            mainMenuTarget.rotation
        );

        ResetGameplayCamera();
    }

    private void ResetGameplayCamera()
    {
        gameplayCamera.transform.SetPositionAndRotation(
            gameplayTarget.position,
            gameplayTarget.rotation
        );

        if (handLogic != null)
            handLogic.ResetHandAndCamera();
    }

    private void LockGameplayInput(bool locked)
    {
        if (handLogic != null)
            handLogic.inputLocked = locked;
    }
}