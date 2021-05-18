using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.World;
using MusicalKnights.Entity.Commanders;

namespace MusicalKnights.UI {
    public class Border : MonoBehaviour, IInspectorEditable {

        // INTERNAL METRONOME
        private Conductor _conductor;

        // COMMANDERS
        private CommanderController _commanderController;

        // RENDERING
        private Shader _shaderGUIText; //the GUI Text Shader is used to allow for "easy grayscaling"
        private SpriteRenderer _rend;

        [Header("Properties")]
        public float minAlpha = 0.35f;
        public float maxAlpha = 1f;
        public float fadeSpd = 0.02f;

        private void Start() {
            _conductor = Conductor.instance;

            //Load controllers
            _commanderController = GameObject.Find("Commanders").GetComponent<CommanderController>();

            //Rendering
            _shaderGUIText = Shader.Find("GUI/Text Shader");
            _rend = GetComponent<SpriteRenderer>();
            _rend.material.shader = _shaderGUIText;
            _rend.color = new Color( 0f, 0f, 0f, minAlpha );

            ValidateProperties();
        }

        private void Update() {
            //Do on every beat
            if ( _conductor.onBeat ) {
                StartCoroutine( FlashBorder(_commanderController.playerTurn) );
            }
        }

        public void ValidateProperties() {
            if ( minAlpha >= maxAlpha || maxAlpha <= minAlpha ) {
                minAlpha = 0.35f;
                maxAlpha = 1f;
            } if ( fadeSpd <= 0 ) {
                fadeSpd = 0.02f;
            }
        }

        /*
        * Makes the GUI border flash according to whose turn it is
        */
        private IEnumerator FlashBorder( bool playerTurn ) {
            float opacity = maxAlpha;

            if ( playerTurn ) {
                GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, opacity );
            } else {
                GetComponent<SpriteRenderer>().color = new Color( 0.5f, 0.5f, 0.5f, opacity );
            }

            yield return new WaitForSeconds(0.05f);

            //Fade border back to min opacity
            while ( opacity > minAlpha ) {

                while ( InputManager.gamePaused ) {
                    yield return null;
                }
                
                opacity -= fadeSpd;
                
                if ( playerTurn ) {
                    GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, opacity );
                } else {
                    GetComponent<SpriteRenderer>().color = new Color( 0.5f, 0.5f, 0.5f, opacity );
                }

                yield return null;
            }
        }

    }

}


