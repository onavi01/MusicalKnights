using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.UI;
using MusicalKnights.World;

namespace MusicalKnights.Entity.Commanders {
    public class Commander : MonoBehaviour {

        // INTERNAL METRONOME
        private Conductor _conductor;

        // BOP (TEMP)
        private float _originScale = 0;
        private float _bopMin = 0;

        // COMMANDER STATS & PARAMETERS
        public enum Alliance {
            FRIEND,
            FOE
        }
        [HideInInspector] public int alliance = 0;
        [HideInInspector] public CommanderController controller;

        [HideInInspector] public string cmdIssued = "...";

        // RENDERING
        public SpriteRenderer rend;

        private void Start() {
            _conductor = Conductor.instance;

            _originScale = transform.localScale.y;
            _bopMin = transform.localScale.y - 0.3f;
            rend = GetComponent<SpriteRenderer>();

            //Load controllers
            controller = transform.parent.gameObject.GetComponent<CommanderController>();
        }

        private void Update() {
            //Do on every beat
            if ( _conductor.onBeat ) {
                StartCoroutine(Bop());
            }
        }

        private IEnumerator Bop() {
            /*Squish*/
            while ( transform.localScale.y > _bopMin ) {
                /*Halt on game pause*/
                while ( InputManager.gamePaused ) {
                    yield return null;
                }

                transform.localScale += new Vector3( 0, -0.03f, 0);
                yield return null;
            }

            /*Stretch*/
            while ( transform.localScale.y < _originScale ) {
                /*Halt on game pause*/
                while ( InputManager.gamePaused ) {
                    yield return null;
                }
                
                transform.localScale += new Vector3( 0, 0.03f, 0);
                yield return null;
            }
        }

    }
}

