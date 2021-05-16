using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicalKnights.World {
    public class LoadGame : MonoBehaviour, IInspectorEditable {

        // GAME SETTINGS
        [Header("Game Settings")]
        [SerializeField] public int TARGET_FRAMERATE = 60;
        private void Start() {
            ValidateProperties();
            Application.targetFrameRate = TARGET_FRAMERATE;
        }

        public void ValidateProperties() {
            if ( TARGET_FRAMERATE <= 0 ) {
                TARGET_FRAMERATE = 60;
            }
        }

    }

}

