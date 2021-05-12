using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MusicalKnights.World;

namespace MusicalKnights.UI {
    public class CommandString : MonoBehaviour {
        
        private Text str;

        //INTERNAL METRONOME
        private Conductor conductor;
        private int lastBeat = -1;
        
        //RENDERING
        public bool isAbovePlayer = false;
        private int count = 0;

        private void Start() {
            /*Load Conductor object*/
            conductor = Conductor.instance;
            str = GetComponent<Text>();

            if ( isAbovePlayer ) {
                str.text = PlayerCommander.cmdIssued;
            } else {
                str.text = EnemyCommander.cmdIssued;
            }
        }

        private void Update() {
            if ( isAbovePlayer ) {
                str.text = PlayerCommander.cmdIssued;
            } else {
                str.text = EnemyCommander.cmdIssued;
            }

            /*On every beat*/
            if ( lastBeat != conductor.currBeat ) {
                lastBeat = conductor.currBeat;
                count++;

                StartCoroutine( Fade() );

                /*Destroy after fading out completely*/
                if ( count == 5 ) {
                    Destroy(gameObject);
                }
            }
        }

        /*
        * Fade text opacity by 25%
        */
        private IEnumerator Fade() {
            float opacity = str.color.a - 0.25f;

            while ( str.color.a > opacity ) {
                while ( InputManager.gamePaused ) {
                    yield return null;
                }
                
                str.color += new Color(1f, 1f, 1f, -0.04f);
                yield return null;
            }
        }
    }

}