using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MusicalKnights.World;

namespace MusicalKnights.UI {
    public class PauseMenuEventListener : MonoBehaviour {
        //CONDUCTOR
        Conductor conductor;

        //UI
        InputField calibrationField;
        
        private void Start() {
            /*Load Conductor object*/
            conductor = Conductor.instance;

            /*Load UI elements*/
            calibrationField = GameObject.Find("Input Calibration").GetComponent<InputField>();
            calibrationField.text = conductor.playerInputOffset.ToString("N1");
        }

        public void UpdateInputCalibration( string offset ) {
            try {
                conductor.playerInputOffset = float.Parse(offset);
            } catch (FormatException e) {
                Debug.Log(e);
            }
        }

        public void QuitGame() {
            Application.Quit();
        }
    }
}

