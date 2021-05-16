using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MusicalKnights.World;
using MusicalKnights.Entity.Commander;

namespace MusicalKnights.UI {
    public class CommandString : MonoBehaviour {

        // INTERNAL METRONOME
        private Conductor _conductor;

        // UI
        private Text _str;

        // RENDERING
        private int _numBeatsVisible = 0;
        [HideInInspector] public bool isAbovePlayer = false;


        private void Start() {
            _conductor = Conductor.instance;
            _str = GetComponent<Text>();

            if ( isAbovePlayer ) {
                _str.text = PlayerCommander.cmdIssued;
            } else {
                _str.text = EnemyCommander.cmdIssued;
            }
        }

        private void Update() {
            if ( isAbovePlayer ) {
                _str.text = PlayerCommander.cmdIssued;
            } else {
                _str.text = EnemyCommander.cmdIssued;
            }

            //Do on every beat
            if ( _conductor.onBeat ) {
                _numBeatsVisible++;

                StartCoroutine( Fade() );

                if ( _numBeatsVisible == 5 ) {
                    Destroy(gameObject);
                }
            }
        }

        /*
        * Fade text opacity by 25%
        */
        private IEnumerator Fade() {
            float opacity = _str.color.a - 0.25f;

            while ( _str.color.a > opacity ) {
                while ( InputManager.gamePaused ) {
                    yield return null;
                }
                
                _str.color += new Color(1f, 1f, 1f, -0.04f);
                yield return null;
            }
        }
    }

}