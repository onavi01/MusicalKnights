using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MusicalKnights.World;
using MusicalKnights.Entity.Commanders;

namespace MusicalKnights.UI {
    public class CmdStr : MonoBehaviour {

        // INTERNAL METRONOME
        private Conductor _conductor;

        // COMMANDERS
        private CommanderController _commanderController;
        private Commander _playerCommander;
        private Commander _adversaryCommander;

        // UI
        private Text _str;

        // RENDERING
        private int _numBeatsVisible = 0;
        [HideInInspector] public bool isAbovePlayer = false;


        private void Start() {
            _conductor = Conductor.instance;
            _str = GetComponent<Text>();

            //Load commander-related
            _commanderController = GameObject.Find("Commanders").GetComponent<CommanderController>();
            _playerCommander = _commanderController.playerObj.GetComponent<Commander>();
            _adversaryCommander = _commanderController.adversaryObj.GetComponent<Commander>();

            if ( isAbovePlayer ) {
                _str.text = _playerCommander.cmdIssued;
            } else {
                _str.text = _adversaryCommander.cmdIssued;
            }
        }

        private void Update() {
            if ( isAbovePlayer ) {
                _str.text = _playerCommander.cmdIssued;
            } else {
                _str.text = _adversaryCommander.cmdIssued;
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