using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicalKnights.UI {
    public class InputManager : MonoBehaviour {

        //UI
        private GameObject pauseMenu, controlList, debugInfo;
        public static bool gamePaused = false;
        public static bool helpToggled = true;
        public static bool debugToggled = true;

        private void Start() {
            /*Load UI elements*/
            pauseMenu = GameObject.Find("Pause Menu");
            controlList = GameObject.Find("Controls");
            debugInfo = GameObject.Find("Debug Info");

            /*Toggle default UI visibilities*/
            pauseMenu.SetActive(gamePaused);
            controlList.SetActive(helpToggled);
            debugInfo.SetActive(debugToggled);
        }

        private void Update() {
            if ( Input.GetButtonDown("Pause") ) {
                if ( !gamePaused ) {
                    PauseGame();
                } else {
                    ResumeGame();
                }

                gamePaused = !gamePaused;
                pauseMenu.SetActive(gamePaused);
            }
            
            if ( !gamePaused ) {
                if ( Input.GetButtonDown("Help") ) {
                    helpToggled = !helpToggled;
                    controlList.SetActive(helpToggled);
                }

                if ( Input.GetButtonDown("Debug") ) {
                    debugToggled = !debugToggled;
                    debugInfo.SetActive(debugToggled);
                }
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

}

