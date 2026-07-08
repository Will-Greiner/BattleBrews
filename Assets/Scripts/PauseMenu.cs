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
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Q))
        {if (isPaused)
            {
                Resume();
                Cursor.visible = false;
            }
            else
                Pause();
        }
        #else
            if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
                Cursor.visible = false;
            }
            else
                Pause();
            
        }
        #endif
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
        Cursor.visible = true;
        isPaused = false;
        pauseMenuUI.SetActive(false);

        if (handLogic != null)
            handLogic.inputLocked = true;

        TimeManager.Instance.HideClock();

        cameraTransitionManager.ReturnToMainMenu();
    }
}