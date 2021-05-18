﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.World;
using MusicalKnights.Entity.Commanders;

namespace MusicalKnights.Entity.Troop {

    [ RequireComponent(typeof(Troop)) ]

    public class FriendlyTroop : MonoBehaviour {

        // COMMANDERS
        private CommanderController _commanderController;

        // INHERITED CLASSES
        [HideInInspector] public Troop troop;
        
        // INTERNAL METRONOME
        private Conductor _conductor;

        private void Start() {
            _conductor = Conductor.instance;

            //Load controllers
            _commanderController = GameObject.Find("Commanders").GetComponent<CommanderController>();

            //Inherit superclasses
            troop = GetComponent<Troop>();
            troop.commanderObj = troop.commanderController.playerObj;
            troop.commander = troop.commanderObj.GetComponent<Commander>();
            troop.alliance = troop.commander.alliance;
        }

        private void Update() {

            int direction = 0;

            if ( troop.alliance == (int) Commander.Alliance.FRIEND ) {
                direction = 1;
            } else if ( troop.alliance == (int) Commander.Alliance.FOE ) {
                direction = -1;
            }
            
            //Do on every beat
            if ( _conductor.onBeat ) {

                //Follow player commands directly after player's turn
                if ( _conductor.measurePosInBeats == 0 && _commanderController.enemyTurn ) {
                    
                    //MARCH & CHARGE
                    if ( troop.commander.cmdIssued == Commands.MARCH_ID || troop.commander.cmdIssued == Commands.CHARGE_ID ) {
                        troop.MarchDefaultBehaviour(direction);
                        
                    //STEADY
                    } else if ( troop.commander.cmdIssued == Commands.STEADY_ID ) {
                        troop.SteadyDefaultBehaviour(direction);
                    
                    }
                    
                }
                
            }

        }
        
    }

}


