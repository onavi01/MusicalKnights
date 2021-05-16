using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MusicalKnights.World;

namespace MusicalKnights.UI {
    public class PauseMenuEventListener : MonoBehaviour {

        // INTERNAL METRONOME
        private Conductor _conductor;

        // UI
        private InputField _calibrationField;
        
        private void Start() {
            _conductor = Conductor.instance;

            //Load UI-related elements
            _calibrationField = GameObject.Find("Input Calibration").GetComponent<InputField>();
            _calibrationField.text = _conductor.playerInputOffset.ToString("N1");
        }

        public void UpdateInputCalibration( string offset ) {
            try {
                _conductor.playerInputOffset = float.Parse(offset);
            } catch (FormatException e) {
                Debug.Log(e);
            }
        }

        public void QuitGame() {
            Application.Quit();
        }
    }
    
}

