using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour {
    GameObject pauseMenu;
    public static bool gamePaused = false;

    void Start() {
        pauseMenu = GameObject.Find("Pause Menu");
        pauseMenu.SetActive(false);
    }

    void Update() {
        if ( Input.GetButtonDown("Pause") ) {
            if ( !gamePaused ) {
                PauseGame();
            } else {
                ResumeGame();
            }

            gamePaused = !gamePaused;
            pauseMenu.SetActive(gamePaused);
        }
    }

    public void PauseGame() {
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void ResumeGame() {
        Time.timeScale = 1;
        AudioListener.pause = false;
    }
}
