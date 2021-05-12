using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.World;

namespace MusicalKnights.UI {
    public class Border : MonoBehaviour, IInspectorEditable {

        //INTERNAL METRONOME
        private Conductor conductor;
        private int lastBeat = -1;

        //RENDERING
        private Shader shaderGUIText; //we'll use the GUI Text Shader to allow for "easy grayscaling"
        private SpriteRenderer rend;

        [Header("Properties")]
        public float MIN_ALPHA = 0.35f;
        public float MAX_ALPHA = 1f;
        public float FADE_SPD = 0.02f;

        private void Start() {
            /*Load Conductor object*/
            conductor = Conductor.instance;

            /*Rendering*/
            shaderGUIText = Shader.Find("GUI/Text Shader");
            rend = GetComponent<SpriteRenderer>();
            rend.material.shader = shaderGUIText;
            rend.color = new Color( 0f, 0f, 0f, MIN_ALPHA );

            ValidateProperties();
        }

        private void Update() {
            /*On every beat*/
            if ( lastBeat != conductor.currBeat ) {
                lastBeat = conductor.currBeat;
                StartCoroutine( FlashBorder(PlayerCommander.playerTurn) );
            }
        }

        public void ValidateProperties() {
            if ( MIN_ALPHA >= MAX_ALPHA || MAX_ALPHA <= MIN_ALPHA ) {
                MIN_ALPHA = 0.35f;
                MAX_ALPHA = 1f;
            } if ( FADE_SPD <= 0 ) {
                FADE_SPD = 0.02f;
            }
        }

        /*
        * Flashes the GUI border based on the current commander's turn
        */
        private IEnumerator FlashBorder( bool playerTurn ) {
            float opacity = MAX_ALPHA;

            /*Gray on adversary's turn, black on player's turn*/
            if ( playerTurn ) {
                GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, opacity );
            } else {
                GetComponent<SpriteRenderer>().color = new Color( 0.5f, 0.5f, 0.5f, opacity );
            }

            yield return new WaitForSeconds(0.05f);

            /*Fade border back to min opacity*/
            while ( opacity > MIN_ALPHA ) {

                while ( InputManager.gamePaused ) {
                    yield return null;
                }
                
                opacity -= FADE_SPD;
                
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


