using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicalKnights.UI {
    public class InputManager : MonoBehaviour {

        // UI
        private GameObject _pauseMenu;
        private GameObject _cmdList;
        private GameObject _debugInfo;

        public static bool gamePaused = false;
        public static bool helpToggled = true;
        public static bool debugToggled = true;

        private void Start() {
            _pauseMenu = GameObject.Find("Pause Menu");
            _cmdList = GameObject.Find("Commands");
            _debugInfo = GameObject.Find("Debug Info");

            //Toggle default UI visibilities
            _pauseMenu.SetActive(gamePaused);
            _cmdList.SetActive(helpToggled);
            _debugInfo.SetActive(debugToggled);
        }

        private void Update() {

            if ( Input.GetButtonDown("Pause") ) {
                if ( !gamePaused ) {
                    PauseGame();
                } else {
                    ResumeGame();
                }

                gamePaused = !gamePaused;
                _pauseMenu.SetActive(gamePaused);
            }
            
            if ( !gamePaused ) {
                if ( Input.GetButtonDown("Help") ) {
                    helpToggled = !helpToggled;
                    _cmdList.SetActive(helpToggled);
                }

                if ( Input.GetButtonDown("Debug") ) {
                    debugToggled = !debugToggled;
                    _debugInfo.SetActive(debugToggled);
                }
            }

        }

        /*
        * Halts the game & all audio tracks
        */
        public void PauseGame() {
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        /*
        * Resumes the game & al audio tracks
        */
        public void ResumeGame() {
            Time.timeScale = 1;
            AudioListener.pause = false;
        }
        
    }

}

