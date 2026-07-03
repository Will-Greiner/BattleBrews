using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] HandLogic handLogic;
    [SerializeField] CameraTransitionManager cameraTransitionManager;

    private bool isPaused;

    private void Start()
    {
        pauseMenuUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused){
                Cursor.visible = true;
                Resume();
            }
            else
                Pause();
        }
    }

    public void Pause()
    {
        Cursor.visible = true;
        isPaused = true;
        pauseMenuUI.SetActive(true);

        TimeManager.Instance.PauseTimer();

        if (handLogic != null)
            handLogic.inputLocked = true;
    }

    public void Resume()
    {
        Cursor.visible = false;
        isPaused = false;
        pauseMenuUI.SetActive(false);

        TimeManager.Instance.ResumeTimer();

        if (handLogic != null)
            handLogic.inputLocked = false;
    }

    public void ReturnToMainMenu()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);

        if (handLogic != null)
            handLogic.inputLocked = true;

        TimeManager.Instance.HideClock();

        cameraTransitionManager.ReturnToMainMenu();
    }
}