using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.World;
using MusicalKnights.Entity.Commanders;

namespace MusicalKnights.UI {
    public class CmdStrController : MonoBehaviour {

        // INTERNAL METRONOME
        private Conductor _conductor;

        // UI
        public GameObject cmdStrObj;

        // COMMANDERS
        private CommanderController _commanderController;

        private void Start() {
            _conductor = Conductor.instance;
            
            //Load controllers
            _commanderController = GameObject.Find("Commanders").GetComponent<CommanderController>();
        }

        private void Update() {

            //Do on every beat
            if ( _conductor.onBeat ) {

                //Spawn command strings above respective commander's head
                if ( _conductor.measurePosInBeats == 0 && _conductor.loopPosInBeats > 3 ) {
                    GameObject strObj = null;

                    if ( _commanderController.enemyTurn ) {
                        strObj = Instantiate( cmdStrObj, new Vector3( _commanderController.playerObj.transform.position.x, _commanderController.playerObj.transform.position.y + 1.2f, 0f ), _commanderController.playerObj.transform.rotation );
                        strObj.GetComponent<CmdStr>().isAbovePlayer = true;
                    } else if ( _commanderController.playerTurn ) {
                        strObj = Instantiate( cmdStrObj, new Vector3( _commanderController.adversaryObj.transform.position.x, _commanderController.adversaryObj.transform.position.y + 1.2f, 0f ), _commanderController.adversaryObj.transform.rotation );
                        strObj.GetComponent<CmdStr>().isAbovePlayer = false;      
                    }

                    strObj.transform.SetParent(transform);
                }

            }

        }
    }

}


