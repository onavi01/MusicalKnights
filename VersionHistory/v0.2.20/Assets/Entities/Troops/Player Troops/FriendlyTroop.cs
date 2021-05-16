using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicalKnights.World;
using MusicalKnights.Entity.Commander;

namespace MusicalKnights.Entity.Troop {

    [ RequireComponent(typeof(Troop)) ]

    public class FriendlyTroop : MonoBehaviour {

        // INHERITED CLASSES
        [HideInInspector] public Troop troop;
        
        // INTERNAL METRONOME
        private Conductor _conductor;

        private void Start() {
            _conductor = Conductor.instance;

            //Inherit superclasses
            troop = GetComponent<Troop>();
        }

        private void Update() {

            int direction = 0;

            if ( troop.alliance == (int) Troop.Alliance.FRIEND ) {
                direction = 1;
            } else if ( troop.alliance == (int) Troop.Alliance.FOE ) {
                direction = -1;
            }
            
            //Do on every beat
            if ( _conductor.onBeat ) {

                //Follow player commands directly after player's turn
                if ( _conductor.measurePosInBeats == 0 && EnemyCommander.enemyTurn ) {
                    
                    //MARCH & CHARGE
                    if ( PlayerCommander.cmdIssued == Commands.MARCH_ID || PlayerCommander.cmdIssued == Commands.CHARGE_ID ) {
                        troop.MarchDefaultBehaviour(direction);
                        
                    //STEADY
                    } else if ( PlayerCommander.cmdIssued == Commands.STEADY_ID ) {
                        troop.SteadyDefaultBehaviour(direction);
                    
                    }
                    
                }
                
            }

        }
        
    }

}


