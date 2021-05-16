using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.World;
using MusicalKnights.Entity.Commander;

namespace MusicalKnights.UI {
    public class DisplayCommandStrings : MonoBehaviour {

        // INTERNAL METRONOME
        private Conductor _conductor;

        // UI
        public GameObject cmdStrObj;

        // COMMANDERS
        private GameObject _playerObj;
        private GameObject _adversaryObj;

        private void Start() {
            _conductor = Conductor.instance;
            _playerObj = GameObject.Find("Player");
            _adversaryObj = GameObject.Find("Adversary");
        }

        private void Update() {

            //Do on every beat
            if ( _conductor.onBeat ) {

                //Spawn command strings above respective commander's head
                if ( _conductor.measurePosInBeats == 0 && _conductor.loopPosInBeats > 3 ) {
                    GameObject strObj = null;

                    if ( EnemyCommander.enemyTurn ) {
                        strObj = Instantiate( cmdStrObj, new Vector3( _playerObj.transform.position.x, _playerObj.transform.position.y + 1.2f, 0f ), _playerObj.transform.rotation );
                        strObj.GetComponent<CommandString>().isAbovePlayer = true;
                    } else if ( PlayerCommander.playerTurn ) {
                        strObj = Instantiate( cmdStrObj, new Vector3( _adversaryObj.transform.position.x, _adversaryObj.transform.position.y + 1.2f, 0f ), _adversaryObj.transform.rotation );
                        strObj.GetComponent<CommandString>().isAbovePlayer = false;      
                    }

                    strObj.transform.SetParent(transform);
                }

            }

        }
    }

}


