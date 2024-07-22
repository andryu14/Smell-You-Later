using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    [SerializeField] private GameObject PauseMenu;

    private bool gamePaused;


    public void Pause(InputAction.CallbackContext context) {
        if (context.performed) { // Whenever the "Esc" key is pressed
            if (gamePaused) {
                Resume();
            } else {
                Time.timeScale = 0f;
                PauseMenu.SetActive(true);
                gamePaused = true;
            }
        }
    }

    public void Resume() {
        Time.timeScale = 1f;
        PauseMenu.SetActive(false);
        gamePaused = false;
    }

    public void QuitToMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
