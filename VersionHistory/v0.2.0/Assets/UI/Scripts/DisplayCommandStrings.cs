using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.World;

namespace MusicalKnights.UI {
    public class DisplayCommandStrings : MonoBehaviour {

        //UI
        [SerializeField] public GameObject cmdStrObj;

        //INTERNAL METRONOME
        private Conductor conductor;
        private int lastBeat = -1;

        //COMMANDERS
        private GameObject playerObj, enemyObj;

        private void Start() {
            /*Load Conductor object*/
            conductor = Conductor.instance;

            /*Load Commander objects*/
            playerObj = GameObject.Find("Player");
            enemyObj = GameObject.Find("Adversary");
        }

        private void Update() {
            /*On every beat*/
            if ( lastBeat != conductor.currBeat ) {
                lastBeat = conductor.currBeat;

                /*Spawn command strings above respective commander's head*/
                if ( lastBeat == 0 && ( conductor.turnPos < PlayerCommander.PLAYER_TURN_START_THRESHOLD || conductor.turnPos > PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {
                    GameObject strObj = Instantiate( cmdStrObj, new Vector3( playerObj.transform.position.x, playerObj.transform.position.y + 1.2f, 0f ), playerObj.transform.rotation );
                    strObj.transform.SetParent(gameObject.transform.Find("Command Strings"));
                    strObj.GetComponent<CommandString>().isAbovePlayer = true;
                    
                } else if ( lastBeat == 0 && ( conductor.turnPos >= PlayerCommander.PLAYER_TURN_START_THRESHOLD && conductor.turnPos <= PlayerCommander.PLAYER_TURN_END_THRESHOLD ) ) {
                    GameObject strObj = Instantiate( cmdStrObj, new Vector3( enemyObj.transform.position.x, enemyObj.transform.position.y + 1.2f, 0f ), enemyObj.transform.rotation );
                    strObj.transform.SetParent(gameObject.transform.Find("Command Strings"));
                    strObj.GetComponent<CommandString>().isAbovePlayer = false;
                }
            }
        }
    }

}


